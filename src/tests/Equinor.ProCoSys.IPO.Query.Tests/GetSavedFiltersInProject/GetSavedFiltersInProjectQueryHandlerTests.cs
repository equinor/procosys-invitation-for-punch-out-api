using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Query.GetSavedFiltersInProject;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.Tests.GetSavedFiltersInProject
{
    [TestClass]
    public class GetSavedFiltersInProjectQueryHandlerTests : ReadOnlyTestsBaseInMemory
    {
        private GetSavedFiltersInProjectQuery _query;
        private GetSavedFiltersInProjectQuery _queryWithNullProject;


        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                _query = new GetSavedFiltersInProjectQuery(ProjectName);
                _queryWithNullProject = new GetSavedFiltersInProjectQuery(null);
            }
        }

        [TestMethod]
        public async Task HandleGetSavedFiltersInProjectQuery_ShouldReturnOkResult()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider);
            var dut = new GetSavedFiltersInProjectQueryHandler(context, _currentUserProvider);
            var result = await dut.Handle(_query, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task HandleGetSavedFiltersWithNullProjectQuery_ShouldReturnOkResult()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider);
            var dut = new GetSavedFiltersInProjectQueryHandler(context, _currentUserProvider);
            var result = await dut.Handle(_queryWithNullProject, default);

            Assert.AreEqual(ResultType.Ok, result.ResultType);
        }

        [TestMethod]
        public async Task HandleGetSavedFiltersInProjectQuery_ShouldReturnCorrectSavedFilters()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider);
            var dut = new GetSavedFiltersInProjectQueryHandler(context, _currentUserProvider);

            var result = await dut.Handle(_query, default);
            var savedFilter = result.Data.Single(sf => sf.Id == _savedFilter.Id);

            Assert.AreEqual(2, result.Data.Count);
            Assert.AreEqual(_savedFilter.Id, savedFilter.Id);
            Assert.AreEqual(_savedFilter.Title, savedFilter.Title);
            Assert.AreEqual(_savedFilter.Criteria, savedFilter.Criteria);
            Assert.AreEqual(_savedFilter.DefaultFilter, savedFilter.DefaultFilter);
        }

        [TestMethod]
        public async Task HandleGetSavedFiltersInProjectQuery_ShouldReturnEmptyListOfSavedFilters_ForProjectWithoutSavedFilters()
        {
            await using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider);
            var dut = new GetSavedFiltersInProjectQueryHandler(context, _currentUserProvider);

            var result = await dut.Handle(new GetSavedFiltersInProjectQuery("Unknownproject"), default);
            Assert.AreEqual(0, result.Data.Count);
        }
    }
}
