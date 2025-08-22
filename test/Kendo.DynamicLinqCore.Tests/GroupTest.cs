using System.Linq.Dynamic.Core;
using FluentAssertions;
using Kendo.DynamicLinqCore.Tests.Data;
using Newtonsoft.Json;
using Xunit;

namespace Kendo.DynamicLinqCore.Tests
{
    public class GroupTest: IClassFixture<MockContext>
    {
        private MockContext _dbContext;


        public GroupTest(MockContext mockContext)
        {
            _dbContext = mockContext;// MockContext.GetDefaultInMemoryDbContext();
        }

        [Fact]
        public void DataSourceRequest_EnumField_GroupedCount()
        {
            // source string = {"take":20,"skip":0,"sort":[{"field":"Number","dir":"desc"}],"group":[{"field":"Gender"}]}

            #if NETCOREAPP3_1
                var request = JsonSerializer.Deserialize<DataSourceRequest>("{\"take\":20,\"skip\":0,\"sort\":[{\"field\":\"Number\",\"dir\":\"desc\"}],\"group\":[{\"field\":\"Gender\"}]}", jsonSerializerOptions);
            #else
                var request = JsonConvert.DeserializeObject<DataSourceRequest>("{\"take\":20,\"skip\":0,\"sort\":[{\"field\":\"Number\",\"dir\":\"desc\"}],\"group\":[{\"field\":\"Gender\"}]}");
            #endif

            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(request);
            var groupItems = result.Groups.ToDynamicList().Count;
            groupItems.Should().Be(3);
        }

    }
}