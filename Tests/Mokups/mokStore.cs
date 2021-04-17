﻿using System.Collections.Generic;
using eCommerce.Business;
using eCommerce.Business.Service;
using eCommerce.Common;

namespace Tests.Mokups
{
    public class mokStore: IStore
    {
        public string storeName { get; set; }
        public Item item;
        public ICart cart;
        public mokStore(string storeName)
        {
            this.storeName = storeName;
            this.item = item;
        }
        
        public IList<Item> GetAllItems()
        {
            throw new System.NotImplementedException();
        }

        public Result<Item> GetItem(ItemInfo item)
        {
            return Result.Ok<Item>(this.item);
        }

        public List<Item> SearchItem(string stringSearch)
        {
            throw new System.NotImplementedException();
        }

        public List<Item> SearchItemWithPriceFilter(string stringSearch, int startPrice, int endPrice)
        {
            throw new System.NotImplementedException();
        }

        public List<Item> SearchItemWithCategoryFilter(string stringSearch, string category)
        {
            throw new System.NotImplementedException();
        }

        public Result AddBasketToStore(IBasket basket)
        {
            throw new System.NotImplementedException();
        }

        public Result CalculateBasketPrices(IBasket basket)
        {
            throw new System.NotImplementedException();
        }

        public Result CatchAllBasketProducts(IBasket basket)
        {
            throw new System.NotImplementedException();
        }

        public Result FinishPurchaseOfBasket(IBasket basket)
        {
            throw new System.NotImplementedException();
        }

        public Result FinishPurchaseOfItems(ItemInfo itemInfo)
        {
            throw new System.NotImplementedException();
        }

        public Result AddItemToStore(ItemInfo newItem, IUser user)
        {
            throw new System.NotImplementedException();
        }

        public Result EditItemToStore(ItemInfo newItem, IUser user)
        {
            throw new System.NotImplementedException();
        }

        public Result RemoveItemToStore(ItemInfo newItem, IUser user)
        {
            throw new System.NotImplementedException();
        }

        public Result AppointNewOwner(IUser user, OwnerAppointment ownerAppointment)
        {
            throw new System.NotImplementedException();
        }

        public Result AppointNewManager(IUser user, ManagerAppointment managerAppointment)
        {
            throw new System.NotImplementedException();
        }

        public Result<IList<PurchaseRecord>> GetPurchaseHistory(IUser user)
        {
            throw new System.NotImplementedException();
        }

        public Result EnterBasketToHistory(IBasket basket)
        {
            throw new System.NotImplementedException();
        }

        public string GetStoreName()
        {
            return this.storeName;
        }

        public bool CheckWithPolicy(PurchaseStrategyName purchaseStrategy)
        {
            throw new System.NotImplementedException();
        }

        public bool TryAddNewCartToStore(Cart cart)
        {
            throw new System.NotImplementedException();
        }

        public Result ConnectNewBasketToStore(Basket newBasket)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckConnectionToCart(ICart cart)
        {
            return cart == this.cart;
        }

        public Result<double> CheckDiscount(Basket basket)
        {
            throw new System.NotImplementedException();
        }
    }
}