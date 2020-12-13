using NFluent;
using ReflectorServer;
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
            Check.That(got).IsNotNull();
        }

        static byte[] Json(string s) => s.Replace('\'', '"').AsUtf();

        Dispatcher PrepareDispatcher()
        {
            var disp = new Dispatcher();
            disp.RegisterTypes(new[] { typeof(DemoDispatchClass) });
            return disp;
        }

        [Case]
        public void SessionResolveDep()
        {
            var disp = PrepareDispatcher();
            var ses = new Session();
            int cc = DemoClassDependency.CallCount;
            var ret = disp.DispatchJson(Json("['DemoDispatchClass', 'Ping']"), ses).AsString();
            Check.That(ret).IsEqualTo("true");
            Check.That(DemoClassDependency.CallCount).IsEqualTo(cc + 1);
        }

        [Case]
        public void SessionResolveDepWithAlreadyExistingInstance()
        {
            var disp = PrepareDispatcher();
            var ses = new Session();
            var dep = new DemoClassDependency();
            int cc = DemoClassDependency.CallCount;
            ses.SetInstance(dep);
            var ret = disp.DispatchJson(Json("['DemoDispatchClass', 'Ping']"), ses).AsString();
            Check.That(ret).IsEqualTo("true");
            Check.That(DemoClassDependency.CallCount).IsEqualTo(cc);
        }

        [Case]
        public void TestDispatch()
        {

            var disp = PrepareDispatcher();
            Check.That(disp.Describe().AsString()).Contains("DemoDispatchClass");
            int cc = DemoClassDependency.CallCount;
            var ses = new Session();
            var okCall = Json(@" ['DemoDispatchClass', 'TargetMethod',  1, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");
            var okRet = disp.DispatchJson(okCall, ses).AsString();
            Check.That(okRet).Contains("legal response");
            var failCall = Json(@" ['DemoDispatchClass', 'TargetMethod',  2, '12', ['nested'], [2,3], { 'Whoa' : ['deep value 1', 'deep2'] } ");
            var failRet = disp.DispatchJson(failCall, ses).AsString();
            Check.That(failRet).Contains("For shame!").And.Contains("Exception:");

        }


    }
}
