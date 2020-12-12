using ReflectorServer;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonReflector.Test
{
    public class PageObject
    {
        public string TargetMethod(int a, string b, List<string> lstring, int[] c)
        {
            return "ok";
        }

    }
    class Program
    {
        
        static void Main(string[] args)
        {
            var m = Dispatcher.ResolveMethod("JsonReflector.Test.PageObject,JsonReflector.Test", "TargetMethod");



            var s2 = System.Text.Encoding.UTF8.GetBytes(@" ['hello', 1, '12', ['nested'], [2,3]".Replace('\'', '"'));
            var rd = new Utf8JsonReader(s2);
            rd.Read(); // startarray
            rd.Read(); // function
            var func = rd.GetString();
            var createdArgs = Dispatcher.PopulateArguments(m, ref rd);
            var instance = new PageObject();
            m.Invoke(instance, createdArgs);



        }
    }
}
