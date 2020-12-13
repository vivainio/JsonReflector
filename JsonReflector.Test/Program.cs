using NFluent;
using ReflectorServer;
using System.Text.Json;
using System.Threading.Tasks;
using TrivialTestRunner;

namespace JsonReflector.Test
{
    class Tests
    {
        [Case]
        public void TestSession()
        {
            var s = new Session();
            var got = s.GetInstance(typeof(DemoDispatchClass));


        }
        [Case]
        public void TestDispatch()
        {
            byte[] json(string s) => s.Replace('\'', '"').AsUtf();

            var disp = new Dispatcher();
            disp.RegisterTypes(new[] { typeof(DemoDispatchClass) });
            Check.That(disp.Describe().AsString()).Contains("DemoDispatchClass");
            int cc = DemoClassDependency.CallCount;
            var ses = new Session();
            //ses.SetInstance(new DemoClassDependency());
            var okCall = json(@" ['DemoDispatchClass', 'TargetMethod',  1, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");
            var okRet = disp.DispatchJson(okCall, ses).AsString();
            Check.That(okRet).Contains("legal response");

            // raises exception
            var failCall = json(@" ['DemoDispatchClass', 'TargetMethod',  2, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");
            var failRet = disp.DispatchJson(failCall, ses).AsString();
            Check.That(failRet).Contains("For shame!").And.Contains("Exception:");

        }


    }
    class Program
    {
        
        static async Task<int> Main(string[] args)
        {

            TRunner.AddTests<Tests>();
            await TRunner.RunTestsAsync();
            TRunner.ReportAll();
            return TRunner.ExitStatus;
        }
    }
}
