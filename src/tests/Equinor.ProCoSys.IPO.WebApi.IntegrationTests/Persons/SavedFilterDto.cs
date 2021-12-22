using System;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Persons
{
    public class SavedFilterDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Criteria { get; set; }
        public bool DefaultFilter { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string RowVersion { get; set; }
    }
}
