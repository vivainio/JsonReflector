using ReflectorServer;
using System.Text.Json;

namespace JsonReflector.Test
{
    class Program
    {
        
        static void Main(string[] args)
        {
            byte[] json(string s) => s.Replace('\'', '"').AsUtf();

            var disp = new Dispatcher();
            var instance = new DemoDispatchClass();
            disp.AddInstance(instance);

            var okCall = json(@" ['DemoDispatchClass', 'TargetMethod',  1, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");

            var okRet = disp.DispatchJson(okCall).AsString();

            // raises exception
            var failCall = json(@" ['DemoDispatchClass', 'TargetMethod',  2, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");
            var failRet = disp.DispatchJson(failCall).AsString();
        }
    }
}
