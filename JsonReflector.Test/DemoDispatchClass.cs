﻿using System;
using System.Collections.Generic;

namespace JsonReflector.Test
{
    public class DemoDispatchClass
    {
        public class DemoNestedType
        {
            public ICollection<string> Whoa { get; set; }
        }

        public DemoNestedType TargetMethod(int a, string b, List<string> lstring, int[] c, DemoNestedType complex)
        {
            if (a == 1)
            {
                return new DemoNestedType
                {
                    Whoa = new[] { "legal response" }
                };
            }
            throw new ArgumentException("This only allows a to be 1. For shame!");
        }
    }
}
