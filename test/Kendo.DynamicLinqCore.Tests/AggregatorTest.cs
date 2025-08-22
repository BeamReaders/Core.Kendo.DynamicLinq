using System.Collections.Generic;
using Kendo.DynamicLinqCore.Tests.Data;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json;

namespace Kendo.DynamicLinqCore.Tests
{
    public class AggregatorTest : IClassFixture<MockContext>
    {
        private MockContext _dbContext;

        public static IEnumerable<DataSourceRequest> DataSourceRequestWithAggregateSalarySum
        {
            get
            {
                #if NETCOREAPP3_1
                yield return JsonSerializer.Deserialize<DataSourceRequest>("{\"take\":10,\"skip\":0,\"aggregate\":[{\"field\":\"Salary\",\"aggregate\":\"sum\"}]}", jsonSerializerOptions);
                #else
                yield return JsonConvert.DeserializeObject<DataSourceRequest>("{\"take\":10,\"skip\":0,\"aggregate\":[{\"field\":\"Salary\",\"aggregate\":\"sum\"}]}");
                #endif
            }
        }

        public static IEnumerable<DataSourceRequest> DataSourceRequestWithManyAggregates
        {
            get
            {
                #if NETCOREAPP3_1
                yield return JsonSerializer.Deserialize<DataSourceRequest>("{\"take\":10,\"skip\":0,\"aggregate\":[{\"field\":\"Salary\",\"aggregate\":\"sum\"},{\"field\":\"Salary\",\"aggregate\":\"average\"},{\"field\":\"Number\",\"aggregate\":\"max\"}]}", jsonSerializerOptions);
                #else
                yield return JsonConvert.DeserializeObject<DataSourceRequest>("{\"take\":10,\"skip\":0,\"aggregate\":[{\"field\":\"Salary\",\"aggregate\":\"sum\"},{\"field\":\"Salary\",\"aggregate\":\"average\"},{\"field\":\"Number\",\"aggregate\":\"max\"}]}");
                #endif
            }
        }

        public AggregatorTest(MockContext mockContext)
        {
            _dbContext = mockContext;// MockContext.GetDefaultInMemoryDbContext();
        }

        [Fact]
        public void InputParameter_DecimalSum_CheckResultObjectString()
        {
            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(10, 0, null, null, new[]
            {
                new Aggregator
                {
                    Aggregate = "sum",
                    Field = "Salary"
                }
            }, null);

            object expectedObject = "{ Salary = { sum = 14850 } }";
            expectedObject.Should().BeEquivalentTo(result.Aggregates.ToString());
        }

        [Theory]
        [InlineData("{\"take\":10,\"skip\":0,\"aggregate\":[{\"field\":\"Salary\",\"aggregate\":\"sum\"}]}")]
        public void InputDataSourceRequest_DecimalSum_CheckResultObjectString(string filters)
        {
            DataSourceRequest dataSourceRequest = JsonConvert.DeserializeObject<DataSourceRequest>(filters);

            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(dataSourceRequest);

            object expectedObject = "{ Salary = { sum = 14850 } }";
            expectedObject.Should().BeEquivalentTo(result.Aggregates.ToString());
        }

        [Theory]
        [InlineData("{\"take\":10,\"skip\":0,\"aggregate\":[{\"field\":\"Salary\",\"aggregate\":\"sum\"}]}")]

        public void InputDataSourceRequest_DecimalSum_CheckResultSum(string filters)
        {
            DataSourceRequest dataSourceRequest = JsonConvert.DeserializeObject<DataSourceRequest>(filters);

            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(dataSourceRequest);
            var salaryAggregates = result.Aggregates.GetType().GetProperty("Salary")?.GetValue(result.Aggregates, null);
            var salarySum = salaryAggregates?.GetType().GetProperty("sum")?.GetValue(salaryAggregates, null);

            const decimal expectedSalarySum = 14850;
            expectedSalarySum.Should().Be((decimal)salarySum);

            const decimal incorrectSalarySum = 9999;
            incorrectSalarySum.Should().NotBe((decimal)salarySum);
        }

        [Fact]
        public void InputParameter_ManyAggregators_CheckResultObjectString()
        {
            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(10, 0, null, null, new[]
            {
                new Aggregator
                {
                    Aggregate = "sum",
                    Field = "Salary"
                },
                new Aggregator
                {
                    Aggregate = "average",
                    Field = "Salary"
                },
                new Aggregator
                {
                    Aggregate = "max",
                    Field = "Number"
                },
            }, null);

            object expectedObject = "{ Salary = { sum = 14850, average = 2970 }, Number = { max = 6 } }";
            expectedObject.Should().BeEquivalentTo(result.Aggregates.ToString());
        }

        [Theory]
        [InlineData("{\"take\":10,\"skip\":0,\"aggregate\":[{\"field\":\"Salary\",\"aggregate\":\"sum\"},{\"field\":\"Salary\",\"aggregate\":\"average\"},{\"field\":\"Number\",\"aggregate\":\"max\"}]}")]
        public void InputDataSourceRequest_ManyAggregators_CheckResultObjectString(string filters)
        {
            DataSourceRequest dataSourceRequest = JsonConvert.DeserializeObject<DataSourceRequest>(filters);

            var result = _dbContext.Employee.AsQueryable().ToDataSourceResult(dataSourceRequest);

            object expectedObject = "{ Salary = { sum = 14850, average = 2970 }, Number = { max = 6 } }";
            expectedObject.Should().BeEquivalentTo(result.Aggregates.ToString());
        }
    }
}