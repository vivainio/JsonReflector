using System;
using System.Collections.Generic;

namespace JsonReflector
{
    public class DemoClassDependency
    {
        public static int CallCount = 0;
        public DemoClassDependency()
        {
            CallCount++;

        }
    }
    public class DemoDispatchClass
    {
        private DemoClassDependency myDep;

        public DemoDispatchClass(DemoClassDependency dep)
        {
            this.myDep = dep;
        }
        public class DemoNestedType
        {
            public ICollection<string> Whoa { get; set; }
        }

        public bool Ping()
        {
            return true;
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
