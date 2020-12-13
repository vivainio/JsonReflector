using ReflectorServer;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace JsonReflector.Test
{
    public class NestedType
    {
        public ICollection<string> Whoa { get; set; }
    }

    public static class Utf8BytesExtensions
    {
        public static string AsString(this byte[] bytes) => System.Text.Encoding.UTF8.GetString(bytes);
        public static byte[] AsUtf(this string s) => System.Text.Encoding.UTF8.GetBytes(s);

    }
    public class PageObject
    {
        public NestedType TargetMethod(int a, string b, List<string> lstring, int[] c, NestedType complex)
        {
            if (a == 1)
            {
                return new NestedType
                {
                    Whoa = new[] { "legal response" }
                };
            }
            throw new ArgumentException("This only allows a to be 1. For shame!");
        }

    }
    class Program
    {
        
        static void Main(string[] args)
        {
            var m = Dispatcher.ResolveMethod("JsonReflector.Test.PageObject,JsonReflector.Test", "TargetMethod");

            byte[] json(string s) => s.Replace('\'', '"').AsUtf();

            var disp = new Dispatcher();
            var instance = new PageObject();
            disp.AddInstance(instance);

            var okCall = json(@" ['PageObject', 'TargetMethod',  1, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");

            var okRet = disp.DispatchJson(okCall).AsString();

            // raises exception
            var failCall = json(@" ['PageObject', 'TargetMethod',  2, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");
            var failRet = disp.DispatchJson(failCall).AsString();
        }
    }
}
