using Microsoft.AspNetCore.Mvc;
using TestDataManagement.Api.Models;
using TestDataManagement.Api.Services;

namespace TestDataManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestDataController : ControllerBase
{
    private readonly ITestDataService _service;
    private readonly ILogger<TestDataController> _logger;

    public TestDataController(ITestDataService service, ILogger<TestDataController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// 创建测试数据
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<long>>> Create([FromBody] TestDataCreateDto dto)
    {
        try
        {
            var testId = await _service.CreateTestDataAsync(dto);
            return Ok(ApiResponse<long>.SuccessResult(testId, "数据录入成功"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "数据验证失败");
            return BadRequest(ApiResponse<long>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建测试数据失败");
            return StatusCode(500, ApiResponse<long>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 更新测试数据
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(long id, [FromBody] TestDataUpdateDto dto)
    {
        try
        {
            if (id != dto.TestId)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("ID不匹配"));
            }

            var result = await _service.UpdateTestDataAsync(dto);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "更新成功"));
            }
            return NotFound(ApiResponse<bool>.ErrorResult("数据不存在"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "数据验证失败");
            return BadRequest(ApiResponse<bool>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新测试数据失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 删除测试数据
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        try
        {
            var result = await _service.DeleteTestDataAsync(id);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResult(true, "删除成功"));
            }
            return NotFound(ApiResponse<bool>.ErrorResult("数据不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除测试数据失败");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取单条测试数据
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TestData>>> GetById(long id)
    {
        try
        {
            var data = await _service.GetTestDataByIdAsync(id);
            if (data != null)
            {
                return Ok(ApiResponse<TestData>.SuccessResult(data));
            }
            return NotFound(ApiResponse<TestData>.ErrorResult("数据不存在"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取测试数据失败");
            return StatusCode(500, ApiResponse<TestData>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 查询测试数据列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TestData>>>> GetList([FromQuery] TestDataQuery query)
    {
        try
        {
            var (items, totalCount) = await _service.GetTestDataListAsync(query);
            return Ok(ApiResponse<List<TestData>>.SuccessResult(items, totalCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询测试数据失败");
            return StatusCode(500, ApiResponse<List<TestData>>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取所有操作员列表
    /// </summary>
    [HttpGet("operators")]
    public async Task<ActionResult<ApiResponse<List<TestOperator>>>> GetOperators()
    {
        try
        {
            var operators = await _service.GetOperatorsAsync();
            return Ok(ApiResponse<List<TestOperator>>.SuccessResult(operators));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取操作员列表失败");
            return StatusCode(500, ApiResponse<List<TestOperator>>.ErrorResult("服务器内部错误"));
        }
    }

    /// <summary>
    /// 获取所有子集列表
    /// </summary>
    [HttpGet("subsets")]
    public async Task<ActionResult<ApiResponse<List<TestSubset>>>> GetSubsets()
    {
        try
        {
            var subsets = await _service.GetSubsetsAsync();
            return Ok(ApiResponse<List<TestSubset>>.SuccessResult(subsets));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取子集列表失败");
            return StatusCode(500, ApiResponse<List<TestSubset>>.ErrorResult("服务器内部错误"));
        }
    }
}