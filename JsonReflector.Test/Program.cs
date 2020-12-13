using System.Text.Json;
using System.Threading.Tasks;
using TrivialTestRunner;

namespace JsonReflector.Test
{
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
