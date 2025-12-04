using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Services;

public interface ITestDataService
{
    Task<long> CreateTestDataAsync(TestDataCreateDto dto);
    Task<bool> UpdateTestDataAsync(TestDataUpdateDto dto);
    Task<bool> DeleteTestDataAsync(long testId);
    Task<TestData?> GetTestDataByIdAsync(long testId);
    Task<(List<TestData> Items, int TotalCount)> GetTestDataListAsync(TestDataQuery query);
    bool ValidateTestData(TestDataCreateDto dto, out string errorMessage);
}