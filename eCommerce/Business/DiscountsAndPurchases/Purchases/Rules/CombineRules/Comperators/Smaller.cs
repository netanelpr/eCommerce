﻿using System;

namespace eCommerce.Business.CombineRules
{
    public class Smaller: Compare
    {
        public override int GetResult(IComparable a, IComparable b)
        {
            return b.CompareTo(a) < 0 ? 1 : -1;
        }
    }
}