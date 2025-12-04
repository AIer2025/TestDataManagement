using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Repositories;

public interface ITestDataRepository
{
    Task<long> CreateAsync(TestDataCreateDto dto);
    Task<bool> UpdateAsync(TestDataUpdateDto dto);
    Task<bool> DeleteAsync(long testId);
    Task<TestData?> GetByIdAsync(long testId);
    Task<(List<TestData> Items, int TotalCount)> GetListAsync(TestDataQuery query);
}