using Kendo.DynamicLinqCore.Tests.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FluentAssertions;
using Xunit;
using Newtonsoft.Json;


namespace Kendo.DynamicLinqCore.Tests
{
    public class FilterTest : IClassFixture<MockContext>
    {
        private MockContext _dbContext;

        public FilterTest(MockContext mockContext)
        {
            _dbContext = mockContext;// MockContext.GetDefaultInMemoryDbContext();
        }

        [Fact]
        public void InputParameter_SubPropertyContains_CheckResultCount()
        {
            var result = _dbContext.Employee.Include(x => x.Company).AsQueryable().ToDataSourceResult(10, 0, null, new Filter
            {
                Field = "Company.Name",
                Value = "Microsoft",
                Operator = "contains",
                Logic = "and"
            });

            result.Total.Should().Be(2);

            var result2 = _dbContext.Employee.AsQueryable().ToDataSourceResult(10, 0, null, new Filter
            {
                Filters = new[]
                {
                    new Filter
                    {
                        Field ="Company.Name",
                        Operator = "contains",
                        Value = "Microsoft"
                    }
                },
                Logic = "and"
            });

            result2.Total.Should().Be(2);
        }

        [Fact]
        public void InputDataSourceRequest_DecimalGreaterAndLess_CheckResultCount()
        {
            // source string = {"take":20,"skip":0,"filter":{"logic":"and","filters":[{"field":"Salary","operator":"gt","value":999.00},{"field":"Salary","operator":"lt","value":6000.00}]}}

            var request = JsonConvert.DeserializeObject<DataSourceRequest>("{\"take\":20,\"skip\":0,\"filter\":{\"logic\":\"and\",\"filters\":[{\"field\":\"Salary\",\"operator\":\"gt\",\"value\":999.00},{\"field\":\"Salary\",\"operator\":\"lt\",\"value\":6000.00}]}}");

            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(request);
            result.Total.Should().Be(4);
        }

        [Fact]
        public void InputDataSourceRequest_DoubleGreaterAndLessEqual_CheckResultCount()
        {
            // source string = {"take":20,"skip":0,"filter":{"logic":"and","filters":[{"field":"Weight","operator":"gt","value":48},{"field":"Weight","operator":"lt","value":69.2}]}}

            var request = JsonConvert.DeserializeObject<DataSourceRequest>("{\"take\":20,\"skip\":0,\"filter\":{\"logic\":\"and\",\"filters\":[{\"field\":\"Weight\",\"operator\":\"gt\",\"value\":48},{\"field\":\"Weight\",\"operator\":\"lte\",\"value\":69.2}]}}");

            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(request);
            result.Total.Should().Be(3);
        }

        [Fact]
        public void InputDataSourceRequest_ManyConditions_CheckResultCount()
        {
            // source string = {\"take\":10,\"skip\":0,\"filter\":{\"logic\":\"and\",\"filters\":[{\"logic\":\"or\",\"filters\":[{\"field\":\"Birthday\",\"operator\":\"eq\",\"value\":\"1986-10-09T16:00:00.000Z\"},{\"field\":\"Birthday\",\"operator\":\"eq\",\"value\":\"1976-11-05T16:00:00.000Z\"}]},{\"logic\":\"and\",\"filters\":[{\"field\":\"Salary\",\"operator\":\"gte\",\"value\":1000},{\"field\":\"Salary\",\"operator\":\"lte\",\"value\":6000}]}]}}

            var request = JsonConvert.DeserializeObject<DataSourceRequest>("{\"take\":10,\"skip\":0,\"filter\":{\"logic\":\"and\",\"filters\":[{\"logic\":\"and\",\"filters\":[{\"field\":\"Birthday\",\"operator\":\"lt\",\"value\":\"1986-10-11T16:00:00.000Z\"},{\"field\":\"Birthday\",\"operator\":\"gt\",\"value\":\"1976-11-05T16:00:00.000Z\"}]},{\"logic\":\"and\",\"filters\":[{\"field\":\"Salary\",\"operator\":\"gte\",\"value\":1000},{\"field\":\"Salary\",\"operator\":\"lte\",\"value\":6000}]}]}}");

            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(request);
            result.Total.Should().Be(2);
        }

    }
}