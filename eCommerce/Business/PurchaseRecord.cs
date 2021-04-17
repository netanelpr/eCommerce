﻿using System;
using eCommerce.Business.Service;

namespace eCommerce.Business
{
    public class PurchaseRecord
    {
        private StoreInfo _storeInfo;
        private BasketInfo _basketInfo;
        private DateTime _dateTime;
        public PurchaseRecord(IStore store, IBasket basket, DateTime now)
        {
            this._storeInfo = new StoreInfo(store);
            this._basketInfo = new BasketInfo(basket);
            this._dateTime = now;
        }

        public StoreInfo GetStoreInfo()
        {
            return _storeInfo;
        }

        public BasketInfo GetBasketInfo()
        {
            return _basketInfo;
        }

        public DateTime GetDate()
        {
            return _dateTime;
        }
    }
}