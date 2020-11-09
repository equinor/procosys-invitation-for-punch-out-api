using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests

{
    [TestClass]
    public abstract class TestBase
    {
        protected static TestFactory TestFactory;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            if (TestFactory == null)
            {
                TestFactory = new TestFactory();
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (TestFactory != null)
            {
                TestFactory.Dispose();
                TestFactory = null;
            }
        }

        public HttpClient AnonymousClient(string plant) => TestFactory.GetClientForPlant(TestFactory.AnonymousUser, plant);
        public HttpClient LibraryAdminClient(string plant) => TestFactory.GetClientForPlant(TestFactory.LibraryAdminUser, plant);
        public HttpClient PlannerClient(string plant) => TestFactory.GetClientForPlant(TestFactory.PlannerUser, plant);
        public HttpClient ViewerClient(string plant) => TestFactory.GetClientForPlant(TestFactory.ViewerUser, plant);
        public HttpClient AuthenticatedHackerClient(string plant) => TestFactory.GetClientForPlant(TestFactory.HackerUser, plant);
    }
}
