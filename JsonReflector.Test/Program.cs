using ReflectorServer;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonReflector.Test
{
    public class PageObject
    {
        public string TargetMethod(int a, string b, int[] c)
        {
            return "ok";
        }

    }
    class Program
    {
        
        static void Main(string[] args)
        {
            var s = @" {
                'Args': [1,'12', [1,1]]
                }".Replace('\'', '"');
          
            
            var o = JsonSerializer.Deserialize<InvokeDto>(s);
            var m = Dispatcher.ResolveMethod("JsonReflector.Test.PageObject,JsonReflector.Test", "TargetMethod");

            var createdArgs = Dispatcher.PopulateArguments(m, o.Args);
            var instance = new PageObject();
            m.Invoke(instance, createdArgs);
            Console.WriteLine("Hello World!");
        }
    }
}
