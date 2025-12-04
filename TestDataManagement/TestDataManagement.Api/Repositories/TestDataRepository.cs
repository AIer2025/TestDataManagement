using MySqlConnector;
using System.Text;
using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Repositories;

public class TestDataRepository : ITestDataRepository
{
    private readonly string _connectionString;

    public TestDataRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreateAsync(TestDataCreateDto dto)
    {
        // 根据删失类型自动设置关联字段
        var isCensored = dto.CensoringType == 1;
        var stateFlag = dto.CensoringType == 1 ? 'S' : 'F';
        var lastInspectionTime = dto.CensoringType == 2 ? dto.LastInspectionTime : 0;

        const string sql = @"
            INSERT INTO tb_test_data (
                module_id, test_time, test_value, test_unit, test_type, 
                test_cycle, quantity, failure_time, last_inspection_time, 
                failure_mode, subset_id, is_censored, censoring_type, 
                state_flag, temperature, humidity, operator, remarks
            ) VALUES (
                @ModuleId, @TestTime, @TestValue, @TestUnit, @TestType,
                @TestCycle, @Quantity, @FailureTime, @LastInspectionTime,
                @FailureMode, @SubsetId, @IsCensored, @CensoringType,
                @StateFlag, @Temperature, @Humidity, @Operator, @Remarks
            );
            SELECT LAST_INSERT_ID();";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ModuleId", dto.ModuleId);
        command.Parameters.AddWithValue("@TestTime", dto.TestTime);
        command.Parameters.AddWithValue("@TestValue", dto.TestValue);
        command.Parameters.AddWithValue("@TestUnit", dto.TestUnit ?? "hours");
        command.Parameters.AddWithValue("@TestType", dto.TestType);
        command.Parameters.AddWithValue("@TestCycle", (object?)dto.TestCycle ?? DBNull.Value);
        command.Parameters.AddWithValue("@Quantity", dto.Quantity);
        command.Parameters.AddWithValue("@FailureTime", (object?)dto.FailureTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastInspectionTime", lastInspectionTime ?? 0);
        command.Parameters.AddWithValue("@FailureMode", (object?)dto.FailureMode ?? DBNull.Value);
        command.Parameters.AddWithValue("@SubsetId", (object?)dto.SubsetId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCensored", isCensored);
        command.Parameters.AddWithValue("@CensoringType", dto.CensoringType);
        command.Parameters.AddWithValue("@StateFlag", stateFlag);
        command.Parameters.AddWithValue("@Temperature", (object?)dto.Temperature ?? DBNull.Value);
        command.Parameters.AddWithValue("@Humidity", (object?)dto.Humidity ?? DBNull.Value);
        command.Parameters.AddWithValue("@Operator", (object?)dto.Operator ?? DBNull.Value);
        command.Parameters.AddWithValue("@Remarks", (object?)dto.Remarks ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }

    public async Task<bool> UpdateAsync(TestDataUpdateDto dto)
    {
        // 根据删失类型自动设置关联字段
        var isCensored = dto.CensoringType == 1;
        var stateFlag = dto.CensoringType == 1 ? 'S' : 'F';
        var lastInspectionTime = dto.CensoringType == 2 ? dto.LastInspectionTime : 0;

        const string sql = @"
            UPDATE tb_test_data SET
                module_id = @ModuleId,
                test_time = @TestTime,
                test_value = @TestValue,
                test_unit = @TestUnit,
                test_type = @TestType,
                test_cycle = @TestCycle,
                quantity = @Quantity,
                failure_time = @FailureTime,
                last_inspection_time = @LastInspectionTime,
                failure_mode = @FailureMode,
                subset_id = @SubsetId,
                is_censored = @IsCensored,
                censoring_type = @CensoringType,
                state_flag = @StateFlag,
                temperature = @Temperature,
                humidity = @Humidity,
                operator = @Operator,
                remarks = @Remarks
            WHERE test_id = @TestId";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TestId", dto.TestId);
        command.Parameters.AddWithValue("@ModuleId", dto.ModuleId);
        command.Parameters.AddWithValue("@TestTime", dto.TestTime);
        command.Parameters.AddWithValue("@TestValue", dto.TestValue);
        command.Parameters.AddWithValue("@TestUnit", dto.TestUnit ?? "hours");
        command.Parameters.AddWithValue("@TestType", dto.TestType);
        command.Parameters.AddWithValue("@TestCycle", (object?)dto.TestCycle ?? DBNull.Value);
        command.Parameters.AddWithValue("@Quantity", dto.Quantity);
        command.Parameters.AddWithValue("@FailureTime", (object?)dto.FailureTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastInspectionTime", lastInspectionTime ?? 0);
        command.Parameters.AddWithValue("@FailureMode", (object?)dto.FailureMode ?? DBNull.Value);
        command.Parameters.AddWithValue("@SubsetId", (object?)dto.SubsetId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCensored", isCensored);
        command.Parameters.AddWithValue("@CensoringType", dto.CensoringType);
        command.Parameters.AddWithValue("@StateFlag", stateFlag);
        command.Parameters.AddWithValue("@Temperature", (object?)dto.Temperature ?? DBNull.Value);
        command.Parameters.AddWithValue("@Humidity", (object?)dto.Humidity ?? DBNull.Value);
        command.Parameters.AddWithValue("@Operator", (object?)dto.Operator ?? DBNull.Value);
        command.Parameters.AddWithValue("@Remarks", (object?)dto.Remarks ?? DBNull.Value);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long testId)
    {
        const string sql = "DELETE FROM tb_test_data WHERE test_id = @TestId";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TestId", testId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<TestData?> GetByIdAsync(long testId)
    {
        const string sql = @"
            SELECT td.*, m.module_name, m.module_code
            FROM tb_test_data td
            INNER JOIN tb_module m ON td.module_id = m.module_id
            WHERE td.test_id = @TestId";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TestId", testId);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToTestData(reader);
        }
        return null;
    }

    public async Task<(List<TestData> Items, int TotalCount)> GetListAsync(TestDataQuery query)
    {
        var whereClause = new StringBuilder("WHERE 1=1");
        var parameters = new List<MySqlParameter>();

        if (query.ModuleId.HasValue)
        {
            whereClause.Append(" AND td.module_id = @ModuleId");
            parameters.Add(new MySqlParameter("@ModuleId", query.ModuleId.Value));
        }

        if (!string.IsNullOrEmpty(query.TestType))
        {
            whereClause.Append(" AND td.test_type = @TestType");
            parameters.Add(new MySqlParameter("@TestType", query.TestType));
        }

        if (query.CensoringType.HasValue)
        {
            whereClause.Append(" AND td.censoring_type = @CensoringType");
            parameters.Add(new MySqlParameter("@CensoringType", query.CensoringType.Value));
        }

        if (query.StartDate.HasValue)
        {
            whereClause.Append(" AND td.test_time >= @StartDate");
            parameters.Add(new MySqlParameter("@StartDate", query.StartDate.Value));
        }

        if (query.EndDate.HasValue)
        {
            whereClause.Append(" AND td.test_time <= @EndDate");
            parameters.Add(new MySqlParameter("@EndDate", query.EndDate.Value));
        }

        // 查询总数
        var countSql = $"SELECT COUNT(*) FROM tb_test_data td {whereClause}";
        
        // 查询数据
        var offset = (query.PageIndex - 1) * query.PageSize;
        var dataSql = $@"
            SELECT td.*, m.module_name, m.module_code
            FROM tb_test_data td
            INNER JOIN tb_module m ON td.module_id = m.module_id
            {whereClause}
            ORDER BY td.test_time DESC
            LIMIT @Limit OFFSET @Offset";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // 获取总数
        await using var countCommand = new MySqlCommand(countSql, connection);
        countCommand.Parameters.AddRange(parameters.ToArray());
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

        // 获取数据列表
        var items = new List<TestData>();
        await using var dataCommand = new MySqlCommand(dataSql, connection);
        dataCommand.Parameters.AddRange(parameters.Select(p => new MySqlParameter(p.ParameterName, p.Value)).ToArray());
        dataCommand.Parameters.AddWithValue("@Limit", query.PageSize);
        dataCommand.Parameters.AddWithValue("@Offset", offset);

        await using var reader = await dataCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(MapToTestData(reader));
        }

        return (items, totalCount);
    }

    private static TestData MapToTestData(MySqlDataReader reader)
    {
        return new TestData
        {
            TestId = reader.GetInt64("test_id"),
            ModuleId = reader.GetInt32("module_id"),
            BatchId = reader.IsDBNull(reader.GetOrdinal("batch_id")) ? null : reader.GetInt32("batch_id"),
            TestTime = reader.GetDateTime("test_time"),
            TestValue = reader.GetDecimal("test_value"),
            TestUnit = reader.IsDBNull(reader.GetOrdinal("test_unit")) ? null : reader.GetString("test_unit"),
            TestType = reader.GetString("test_type"),
            TestCycle = reader.IsDBNull(reader.GetOrdinal("test_cycle")) ? null : reader.GetInt32("test_cycle"),
            Quantity = reader.GetInt32("quantity"),
            FailureTime = reader.IsDBNull(reader.GetOrdinal("failure_time")) ? null : reader.GetDecimal("failure_time"),
            LastInspectionTime = reader.IsDBNull(reader.GetOrdinal("last_inspection_time")) ? null : reader.GetDecimal("last_inspection_time"),
            FailureMode = reader.IsDBNull(reader.GetOrdinal("failure_mode")) ? null : reader.GetString("failure_mode"),
            SubsetId = reader.IsDBNull(reader.GetOrdinal("subset_id")) ? null : reader.GetString("subset_id"),
            IsCensored = reader.GetBoolean("is_censored"),
            CensoringType = reader.GetInt32("censoring_type"),
            StateFlag = reader.GetChar("state_flag"),
            Temperature = reader.IsDBNull(reader.GetOrdinal("temperature")) ? null : reader.GetDecimal("temperature"),
            Humidity = reader.IsDBNull(reader.GetOrdinal("humidity")) ? null : reader.GetDecimal("humidity"),
            Operator = reader.IsDBNull(reader.GetOrdinal("operator")) ? null : reader.GetString("operator"),
            Remarks = reader.IsDBNull(reader.GetOrdinal("remarks")) ? null : reader.GetString("remarks"),
            CreateTime = reader.GetDateTime("create_time"),
            ModuleName = reader.GetString("module_name"),
            ModuleCode = reader.GetString("module_code")
        };
    }
}