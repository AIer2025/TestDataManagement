using TestDataManagement.Api.Models;
using TestDataManagement.Api.Repositories;

namespace TestDataManagement.Api.Services;

public class TestDataService : ITestDataService
{
    private readonly ITestDataRepository _repository;

    public TestDataService(ITestDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> CreateTestDataAsync(TestDataCreateDto dto)
    {
        if (!ValidateTestData(dto, out var errorMessage))
        {
            throw new ArgumentException(errorMessage);
        }
        return await _repository.CreateAsync(dto);
    }

    public async Task<bool> UpdateTestDataAsync(TestDataUpdateDto dto)
    {
        if (!ValidateTestData(dto, out var errorMessage))
        {
            throw new ArgumentException(errorMessage);
        }
        return await _repository.UpdateAsync(dto);
    }

    public async Task<bool> DeleteTestDataAsync(long testId)
    {
        return await _repository.DeleteAsync(testId);
    }

    public async Task<TestData?> GetTestDataByIdAsync(long testId)
    {
        return await _repository.GetByIdAsync(testId);
    }

    public async Task<(List<TestData> Items, int TotalCount)> GetTestDataListAsync(TestDataQuery query)
    {
        return await _repository.GetListAsync(query);
    }

    public bool ValidateTestData(TestDataCreateDto dto, out string errorMessage)
    {
        errorMessage = string.Empty;

        // 基本验证
        if (dto.ModuleId <= 0)
        {
            errorMessage = "模组ID无效";
            return false;
        }

        if (dto.Quantity <= 0)
        {
            errorMessage = "样本数量必须大于0";
            return false;
        }

        // 根据删失类型验证数据完整性
        switch (dto.CensoringType)
        {
            case 0: // 完全数据
                if (!dto.FailureTime.HasValue || dto.FailureTime.Value <= 0)
                {
                    errorMessage = "完全数据必须提供有效的失效时间";
                    return false;
                }
                break;

            case 1: // 右删失数据
                if (!dto.FailureTime.HasValue || dto.FailureTime.Value <= 0)
                {
                    errorMessage = "右删失数据必须提供截止时间(failure_time)";
                    return false;
                }
                break;

            case 2: // 区间删失数据
                if (!dto.FailureTime.HasValue || !dto.LastInspectionTime.HasValue)
                {
                    errorMessage = "区间删失数据必须同时提供上界(failure_time)和下界(last_inspection_time)";
                    return false;
                }
                if (dto.LastInspectionTime.Value <= 0)
                {
                    errorMessage = "区间删失数据的下界(last_inspection_time)必须大于0";
                    return false;
                }
                if (dto.FailureTime.Value <= dto.LastInspectionTime.Value)
                {
                    errorMessage = "区间删失数据的上界必须大于下界";
                    return false;
                }
                break;

            case 3: // 左删失数据
                if (!dto.FailureTime.HasValue || dto.FailureTime.Value <= 0)
                {
                    errorMessage = "左删失数据必须提供首次检测时间(failure_time)";
                    return false;
                }
                break;

            default:
                errorMessage = "无效的删失类型,必须是0-3之间的整数";
                return false;
        }

        // 验证湿度范围
        if (dto.Humidity.HasValue && (dto.Humidity.Value < 0 || dto.Humidity.Value > 100))
        {
            errorMessage = "湿度必须在0-100之间";
            return false;
        }

        return true;
    }
}