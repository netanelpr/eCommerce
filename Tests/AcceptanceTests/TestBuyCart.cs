﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using eCommerce.Auth;
using eCommerce.Adapters;
using eCommerce.Business;
using eCommerce.Business.CombineRules;
using eCommerce.Business.Discounts;
using eCommerce.Business.Purchases;
using eCommerce.Common;
using eCommerce.Service;
using eCommerce.Service.StorePolicies;
using NUnit.Framework;
using Tests.AuthTests;
using Tests.Business.Mokups;
using Tests.Service;

namespace Tests.AcceptanceTests
{
    /// <summary>
    /// <UC>
    /// Purchase the whole cart
    /// </UC>
    /// <Req>
    /// 2.9
    /// </Req>
    /// </summary>
    [TestFixture]
    public class TestBuyCart
    {
        private IAuthService _auth;
        private IStoreService _store;
        private ICartService _cart;
        private IUserService _user;
        private string store_name = "Borca";
        private bool shouldTearDown = false;
        private string IvanLoginToken;


        [SetUp]
        //The process:
        // Policy -> Discounts -> GetItems -> Pay -> Supply -> History -> :-)
        public void SetUp()
        {
            PaymentProxy.AssignPaymentService(new mokPaymentService(true,true,true));
            SupplyProxy.AssignSupplyService(new mokSupplyService(true,true));
                _auth = new AuthService();
            _store = new StoreService();
            _cart = new CartService();
            _user = new UserService();
            StoreRepository SR = new StoreRepository();
            TRegisteredUserRepo RP = new TRegisteredUserRepo();
            UserAuth UA = UserAuth.CreateInstanceForTests(RP);
            IRepository<IUser> UR = new RegisteredUsersRepository();

            _auth = AuthService.CreateUserServiceForTests(UA, UR, SR);
            _store = StoreService.CreateUserServiceForTests(UA, UR, SR);
            _cart = CartService.CreateUserServiceForTests(UA, UR, SR);
            MemberInfo Ivan = new MemberInfo("Ivan11", "Ivan@gmail.com", "Ivan Park",
                DateTime.ParseExact("19/04/2005", "dd/MM/yyyy", CultureInfo.InvariantCulture), "hazait 14");
            string token = _auth.Connect();
            _auth.Register(token, Ivan, "qwerty123");
            Result<string> IvanLogInResult = _auth.Login(token, "Ivan11", "qwerty123", ServiceUserRole.Member);
            IItem product = new SItem("Tara milk", store_name, 10, "dairy",
                new List<string> {"dairy", "milk", "Tara"}, (double) 5.4);
            IItem product2 = new SItem("Chocolate milk", store_name, 200, "Sweet",
                new List<string> {"dairy", "milk", "sweet"}, (double) 3.5);
            _store.OpenStore(IvanLogInResult.Value, store_name);
            _store.AddNewItemToStore(IvanLogInResult.Value, product);
            _store.AddNewItemToStore(IvanLogInResult.Value, product2);
            token = _auth.Logout(IvanLogInResult.Value).Value;
            _auth.Disconnect(token);
        }
        [TearDown]
        public void Teardown()
        {
            

            if (shouldTearDown)
            {
                string token = _auth.Logout(IvanLoginToken).Value;
                _auth.Disconnect(token);
            }
        }

        [Test]
        [Order(0)]
        // Policy -> Discounts -> GetItems -> Pay -> Supply -> History -> :-)
        public void TestBuyCartSuccess()
        {
            string token = _auth.Connect();
            string ITEM_NAME = "Tara milk";
            //To check later
            
            _cart.AddItemToCart(token, ITEM_NAME, store_name, 5);
            Result purchaseResult = _cart.PurchaseCart(token, new PaymentInfo("Ivan11","123456789","1234567890123456","12/34","123","address"));
            Assert.True(purchaseResult.IsSuccess, purchaseResult.Error);
            _auth.Disconnect(token);
        }
        
        [Test]
        [TestCase(5)]
        public void TestBuyCartFailurePolicy(int amount)
        {
            string token = _auth.Connect();
            string ITEM_NAME = "Vodka";
            string CATEGORY_NAME = "Alcohol";
            Result<string> IvanLogInResult = _auth.Login(token, "Ivan11", "qwerty123", ServiceUserRole.Member);
            Assert.True(IvanLogInResult.IsSuccess,IvanLogInResult.Error);
            this.IvanLoginToken = IvanLogInResult.Value;
            this.shouldTearDown = true;
            
            IItem vodka = new SItem(ITEM_NAME, store_name, 20, CATEGORY_NAME,
                new List<string> {"Alcohol"}, (double) 70);
            _store.AddNewItemToStore(IvanLogInResult.Value, vodka);
            
            //To check later
            Result<SPurchaseHistory> historyResult = _store.GetPurchaseHistoryOfStore(IvanLogInResult.Value, store_name);
            Assert.True(historyResult.IsSuccess,historyResult.Error);
            int countStoreHistory = historyResult.Value.Records.Count;
            int userHistory = _user.GetPurchaseHistory(IvanLogInResult.Value).Value.Records.Count;
            int countRealPaymentHits = PaymentProxy.REAL_HITS;
            int countRealRefund = PaymentProxy.REAL_REFUNDS;
            int countRealSupplyHits = SupplyProxy.REAL_HITS;
            var resGetItem = _store.GetItem(IvanLogInResult.Value, store_name, ITEM_NAME);
            Assert.True(resGetItem.IsSuccess,resGetItem.Error);
            int itemsInStock = resGetItem.Value.Amount;

            SRuleInfo ruleInfoAge = new SRuleInfo(RuleType.Age, "18", "", "", Comperators.SMALLER_EQUALS);
            SRuleNode ruleNodeAge = new SRuleNode(SRuleNodeType.Leaf, ruleInfoAge);
            
            SRuleInfo alcoRule = new SRuleInfo(RuleType.Category, CATEGORY_NAME, "", "");
            SRuleNode ruleNodeAlco = new SRuleNode(SRuleNodeType.Leaf, alcoRule);
            
            SRuleNode ruleCombiAnd = new SRuleNode(SRuleNodeType.Composite,ruleNodeAlco,ruleNodeAge,Combinations.AND);

            var resAddToPolicy=_store.AddRuleToStorePolicy(IvanLogInResult.Value, store_name,ruleCombiAnd);
            
            Assert.True(resAddToPolicy.IsSuccess);
            
            token = _auth.Logout(IvanLogInResult.Value).Value;
            _auth.Disconnect(token);
            
            
            MemberInfo Jake = new MemberInfo("Jake11", "Ivan@gmail.com", "Jake Park",
                DateTime.ParseExact("19/04/2015", "dd/MM/yyyy", CultureInfo.InvariantCulture), "hazait 14");
            string tokenJake = _auth.Connect();
            _auth.Register(tokenJake, Jake, "qwerty123");
            Result<string> JakeLogInResult = _auth.Login(tokenJake, "Jake11", "qwerty123", ServiceUserRole.Member);
            
            
            var resAddItemToCart = _cart.AddItemToCart(JakeLogInResult.Value, ITEM_NAME, store_name, amount);
            Assert.True(resAddItemToCart.IsSuccess, resAddItemToCart.Error);
            
            Result purchaseResult = _cart.PurchaseCart(JakeLogInResult.Value,
                new PaymentInfo("Ivan11", "123456789", "1234567890123456", "12/34", "123", "address"));
            
            Assert.False(purchaseResult.IsSuccess);
            Assert.True(purchaseResult.Error.Contains("<Policy>"),purchaseResult.Error);
            token = _auth.Logout(JakeLogInResult.Value).Value;
            _auth.Disconnect(token);
            
            token = _auth.Connect();
            IvanLogInResult = _auth.Login(token, "Ivan11", "qwerty123", ServiceUserRole.Member);
            Assert.True(IvanLogInResult.IsSuccess,IvanLogInResult.Error);
            
            //Make sure nothing changed
            
            Assert.AreEqual(itemsInStock,_store.GetItem(IvanLogInResult.Value,store_name,ITEM_NAME).Value.Amount);
            
            Assert.AreEqual(countStoreHistory,historyResult.Value.Records.Count);
            Assert.AreEqual(userHistory,_user.GetPurchaseHistory(IvanLogInResult.Value).Value.Records.Count);
            Assert.AreEqual(countRealPaymentHits,PaymentProxy.REAL_HITS);
            Assert.AreEqual(countRealRefund,PaymentProxy.REAL_REFUNDS);
            Assert.AreEqual(countRealSupplyHits,SupplyProxy.REAL_HITS);
            token = _auth.Logout(IvanLogInResult.Value).Value;
            _auth.Disconnect(token);
        }
        
        [TestCase(10)]
        [TestCase(15)]
        [TestCase(-1)]
        [Order(1)]
        [Test]
        // Policy -> Discounts -> GetItems -> X
        public void TestBuyCartFailureGetItems(int amount)
        {
            string token = _auth.Connect();
            string ITEM_NAME = "Tara milk";
            
            Result<string> IvanLogInResult = _auth.Login(token, "Ivan11", "qwerty123", ServiceUserRole.Member);
            Assert.True(IvanLogInResult.IsSuccess,IvanLogInResult.Error);
            this.IvanLoginToken = IvanLogInResult.Value;
            this.shouldTearDown = true;
            
            //To check later
            Result<SPurchaseHistory> historyResult = _store.GetPurchaseHistoryOfStore(IvanLogInResult.Value, store_name);
            Assert.True(historyResult.IsSuccess,historyResult.Error);
            int countStoreHistory = historyResult.Value.Records.Count;
            int userHistory = _user.GetPurchaseHistory(IvanLogInResult.Value).Value.Records.Count;
            int countRealPaymentHits = PaymentProxy.REAL_HITS;
            int countRealRefund = PaymentProxy.REAL_REFUNDS;
            int countRealSupplyHits = SupplyProxy.REAL_HITS;
            var resGetItem = _store.GetItem(IvanLogInResult.Value, store_name, ITEM_NAME);
            Assert.True(resGetItem.IsSuccess,resGetItem.Error);
            int itemsInStock = resGetItem.Value.Amount;

            var resAddItemToCart = _cart.AddItemToCart(IvanLogInResult.Value, ITEM_NAME, store_name, amount);
            Assert.False(resAddItemToCart.IsSuccess, resAddItemToCart.Error);
            this.shouldTearDown = false;
            
            token = _auth.Logout(IvanLoginToken).Value;
            _auth.Disconnect(token);
        }
        
        //missing item
        //payment fail
        //supply fail
        [TestCase(10)]
        [Order(2)]
        // Policy -> Discounts -> GetItems -> Pay -> X
        public void TestBuyCartPaymentFail(int amount)
        {
            string token = _auth.Connect();
            string ITEM_NAME = "Chocolate milk";
            Result<string> IvanLogInResult = _auth.Login(token, "Ivan11", "qwerty123", ServiceUserRole.Member);
            Assert.True(IvanLogInResult.IsSuccess,IvanLogInResult.Error);
            
            //To check later
            Result<SPurchaseHistory> historyResult = _store.GetPurchaseHistoryOfStore(IvanLogInResult.Value, store_name);
            int countStoreHistory = historyResult.Value.Records.Count;
            int userHistory = _user.GetPurchaseHistory(IvanLogInResult.Value).Value.Records.Count;
            int countRealPaymentHits = PaymentProxy.REAL_HITS;
            int countRealRefund = PaymentProxy.REAL_REFUNDS;
            int countRealSupplyHits = SupplyProxy.REAL_HITS;
            var resGetItem = _store.GetItem(IvanLogInResult.Value, store_name, ITEM_NAME);
            Assert.True(resGetItem.IsSuccess,resGetItem.Error);
            int itemsInStock = resGetItem.Value.Amount;
            
            PaymentProxy.AssignPaymentService(new mokPaymentService(false,false,true));
            
            var resAddCart=_cart.AddItemToCart(IvanLogInResult.Value, ITEM_NAME, store_name, amount);
            Assert.True(resAddCart.IsSuccess,resAddCart.Error);
            Result purchaseResult = _cart.PurchaseCart(IvanLogInResult.Value,
                new PaymentInfo("Ivan11", "123456789", "1234567890123456", "12/34", "123", "address"));
            
            Assert.False(purchaseResult.IsSuccess);
            
            //Make sure nothing changed
            Assert.True(purchaseResult.Error.Contains("<Payment>"),purchaseResult.Error);
            Assert.AreEqual(itemsInStock,_store.GetItem(IvanLogInResult.Value,store_name,ITEM_NAME).Value.Amount);
            
            Assert.AreEqual(countStoreHistory,historyResult.Value.Records.Count);
            Assert.AreEqual(userHistory,_user.GetPurchaseHistory(IvanLogInResult.Value).Value.Records.Count);
            Assert.AreEqual(countRealPaymentHits,PaymentProxy.REAL_HITS);
            Assert.AreEqual(countRealRefund,PaymentProxy.REAL_REFUNDS);
            Assert.AreEqual(countRealSupplyHits,SupplyProxy.REAL_HITS);
            
            token = _auth.Logout(IvanLoginToken).Value;
            _auth.Disconnect(token);
        }
        
        
        
        
        // Policy -> Discounts -> GetItems -> Pay -> Supply -> X
        [TestCase(10)]
        [Order(3)]
        public void TestBuyCartSupplyProcessFail(int amount)
        {
            string token = _auth.Connect();
            string ITEM_NAME = "Chocolate milk";
            
            Result<string> IvanLogInResult = _auth.Login(token, "Ivan11", "qwerty123", ServiceUserRole.Member);
            Assert.True(IvanLogInResult.IsSuccess,IvanLogInResult.Error);
            this.shouldTearDown = true;
            this.IvanLoginToken = IvanLogInResult.Value;
            
            Result<SPurchaseHistory> historyResult = _store.GetPurchaseHistoryOfStore(IvanLogInResult.Value, store_name);
            int countStoreHistory = historyResult.Value.Records.Count;
            int userHistory = _user.GetPurchaseHistory(IvanLogInResult.Value).Value.Records.Count;
            int countRealPaymentHits = PaymentProxy.REAL_HITS;
            int countRealRefund = PaymentProxy.REAL_REFUNDS;
            int countRealSupplyHits = SupplyProxy.REAL_HITS;
            var resGetItem = _store.GetItem(IvanLogInResult.Value, store_name, ITEM_NAME);
            Assert.True(resGetItem.IsSuccess,resGetItem.Error);
            int itemsInStock = resGetItem.Value.Amount;
            
            
            PaymentProxy.AssignPaymentService(new mokPaymentService(true,true,true));
            SupplyProxy.AssignSupplyService(new mokSupplyService(true,false));
            
            var resAddCart=_cart.AddItemToCart(IvanLogInResult.Value, "Chocolate milk", store_name, amount);
            Assert.True(resAddCart.IsSuccess,resAddCart.Error);
            Result purchaseResult = _cart.PurchaseCart(IvanLogInResult.Value,
                new PaymentInfo("Ivan11", "123456789", "1234567890123456", "12/34", "123", "address"));

            Assert.False(purchaseResult.IsSuccess);
            Assert.AreEqual(itemsInStock,_store.GetItem(IvanLogInResult.Value,store_name,ITEM_NAME).Value.Amount);
            Assert.True(purchaseResult.Error.Contains("<Supply>"),purchaseResult.Error);
            Assert.AreEqual(countStoreHistory,historyResult.Value.Records.Count);
            Assert.AreEqual(userHistory,_user.GetPurchaseHistory(IvanLogInResult.Value).Value.Records.Count);
            Assert.AreEqual(countRealPaymentHits+1,PaymentProxy.REAL_HITS);
            Assert.GreaterOrEqual(countRealRefund+1,PaymentProxy.REAL_REFUNDS);
            Assert.AreEqual(countRealSupplyHits,SupplyProxy.REAL_HITS);
            
            _auth.Logout(IvanLogInResult.Value);
        }
        
        
        
        
    }
}