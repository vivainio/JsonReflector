using ReflectorServer;
using System.Text.Json;

namespace JsonReflector.Test
{
    class Program
    {
        
        public static void TestSession()
        {
            var s = new Session();
            var got = s.GetInstance(typeof(DemoDispatchClass));


        }
        static void Main(string[] args)
        {
            TestSession();
            byte[] json(string s) => s.Replace('\'', '"').AsUtf();

            var disp = new Dispatcher();
            disp.RegisterTypes(new[] { typeof(DemoDispatchClass) });

            var okCall = json(@" ['DemoDispatchClass', 'TargetMethod',  1, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");

            var okRet = disp.DispatchJson(okCall).AsString();

            // raises exception
            var failCall = json(@" ['DemoDispatchClass', 'TargetMethod',  2, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");
            var failRet = disp.DispatchJson(failCall).AsString();
        }
    }
}
