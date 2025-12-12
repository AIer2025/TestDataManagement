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
        // censoring_type: 0=完全数据, 1=右删失, 2=区间删失, 3=左删失
        // is_censored: 只有右删失(1)时为true
        // state_flag: 只有右删失(1)时为'S'(Suspension), 其他都是'F'(Failure)
        // last_inspection_time: 只有区间删失(2)时才需要>0的值
        var isCensored = dto.CensoringType == 1;
        var stateFlag = dto.CensoringType == 1 ? 'S' : 'F';
        
        // 区间删失时使用传入的LastInspectionTime，其他类型强制为0
        decimal? lastInspectionTime;
        if (dto.CensoringType == 2)
        {
            // 区间删失必须有有效的下界值
            lastInspectionTime = dto.LastInspectionTime ?? 0;
            if (lastInspectionTime <= 0)
            {
                throw new ArgumentException("区间删失数据的前次检测时间(last_inspection_time)必须大于0");
            }
        }
        else
        {
            lastInspectionTime = 0;
        }

        const string sql = @"
            INSERT INTO tb_test_data (
                module_id, test_time, test_value, test_unit, test_type, 
                test_cycle, quantity, failure_time, last_inspection_time, 
                failure_mode, subset_id, is_censored, censoring_type, 
                state_flag, temperature, humidity, id_operator, remarks
            ) VALUES (
                @ModuleId, @TestTime, @TestValue, @TestUnit, @TestType,
                @TestCycle, @Quantity, @FailureTime, @LastInspectionTime,
                @FailureMode, @SubsetId, @IsCensored, @CensoringType,
                @StateFlag, @Temperature, @Humidity, @IdOperator, @Remarks
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
        command.Parameters.AddWithValue("@TestCycle", dto.TestCycle ?? 1);
        command.Parameters.AddWithValue("@Quantity", dto.Quantity);
        command.Parameters.AddWithValue("@FailureTime", (object?)dto.FailureTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastInspectionTime", lastInspectionTime ?? 0);
        command.Parameters.AddWithValue("@FailureMode", (object?)dto.FailureMode ?? DBNull.Value);
        command.Parameters.AddWithValue("@SubsetId", dto.SubsetId);
        command.Parameters.AddWithValue("@IsCensored", isCensored);
        command.Parameters.AddWithValue("@CensoringType", dto.CensoringType);
        command.Parameters.AddWithValue("@StateFlag", stateFlag);
        command.Parameters.AddWithValue("@Temperature", dto.Temperature ?? 20);
        command.Parameters.AddWithValue("@Humidity", dto.Humidity ?? 60);
        command.Parameters.AddWithValue("@IdOperator", dto.IdOperator);
        command.Parameters.AddWithValue("@Remarks", dto.Remarks ?? "请输入备注说明~~~!!!");

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }

    public async Task<bool> UpdateAsync(TestDataUpdateDto dto)
    {
        // 根据删失类型自动设置关联字段
        // censoring_type: 0=完全数据, 1=右删失, 2=区间删失, 3=左删失
        // is_censored: 只有右删失(1)时为true
        // state_flag: 只有右删失(1)时为'S'(Suspension), 其他都是'F'(Failure)
        // last_inspection_time: 只有区间删失(2)时才需要>0的值
        var isCensored = dto.CensoringType == 1;
        var stateFlag = dto.CensoringType == 1 ? 'S' : 'F';
        
        // 区间删失时使用传入的LastInspectionTime，其他类型强制为0
        decimal? lastInspectionTime;
        if (dto.CensoringType == 2)
        {
            // 区间删失必须有有效的下界值
            lastInspectionTime = dto.LastInspectionTime ?? 0;
            if (lastInspectionTime <= 0)
            {
                throw new ArgumentException("区间删失数据的前次检测时间(last_inspection_time)必须大于0");
            }
        }
        else
        {
            lastInspectionTime = 0;
        }

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
                id_operator = @IdOperator,
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
        command.Parameters.AddWithValue("@TestCycle", dto.TestCycle ?? 1);
        command.Parameters.AddWithValue("@Quantity", dto.Quantity);
        command.Parameters.AddWithValue("@FailureTime", (object?)dto.FailureTime ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastInspectionTime", lastInspectionTime ?? 0);
        command.Parameters.AddWithValue("@FailureMode", (object?)dto.FailureMode ?? DBNull.Value);
        command.Parameters.AddWithValue("@SubsetId", dto.SubsetId);
        command.Parameters.AddWithValue("@IsCensored", isCensored);
        command.Parameters.AddWithValue("@CensoringType", dto.CensoringType);
        command.Parameters.AddWithValue("@StateFlag", stateFlag);
        command.Parameters.AddWithValue("@Temperature", dto.Temperature ?? 20);
        command.Parameters.AddWithValue("@Humidity", dto.Humidity ?? 60);
        command.Parameters.AddWithValue("@IdOperator", dto.IdOperator);
        command.Parameters.AddWithValue("@Remarks", dto.Remarks ?? "请输入备注说明~~~!!!");

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
            SELECT td.*, m.module_name, m.module_code, 
                   op.operator_name, ss.subset_name
            FROM tb_test_data td
            INNER JOIN tb_module m ON td.module_id = m.module_id
            LEFT JOIN tb_test_operator op ON td.id_operator = op.id_operator
            LEFT JOIN tb_test_subset ss ON td.subset_id = ss.subset_id
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
            SELECT td.*, m.module_name, m.module_code,
                   op.operator_name, ss.subset_name
            FROM tb_test_data td
            INNER JOIN tb_module m ON td.module_id = m.module_id
            LEFT JOIN tb_test_operator op ON td.id_operator = op.id_operator
            LEFT JOIN tb_test_subset ss ON td.subset_id = ss.subset_id
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
            SubsetId = reader.IsDBNull(reader.GetOrdinal("subset_id")) ? 1 : reader.GetInt32("subset_id"),
            // 修复：使用GetByte读取tinyint(1)类型，避免被误读为布尔值
            IsCensored = reader.GetByte("is_censored") != 0,
            CensoringType = reader.GetByte("censoring_type"),
            StateFlag = reader.GetChar("state_flag"),
            Temperature = reader.IsDBNull(reader.GetOrdinal("temperature")) ? null : reader.GetDecimal("temperature"),
            Humidity = reader.IsDBNull(reader.GetOrdinal("humidity")) ? null : reader.GetDecimal("humidity"),
            IdOperator = reader.IsDBNull(reader.GetOrdinal("id_operator")) ? 1 : reader.GetInt32("id_operator"),
            Remarks = reader.IsDBNull(reader.GetOrdinal("remarks")) ? null : reader.GetString("remarks"),
            CreateTime = reader.GetDateTime("create_time"),
            ModuleName = reader.GetString("module_name"),
            ModuleCode = reader.GetString("module_code"),
            OperatorName = reader.IsDBNull(reader.GetOrdinal("operator_name")) ? null : reader.GetString("operator_name"),
            SubsetName = reader.IsDBNull(reader.GetOrdinal("subset_name")) ? null : reader.GetString("subset_name")
        };
    }

    // 获取所有操作员列表
    public async Task<List<TestOperator>> GetOperatorsAsync()
    {
        const string sql = "SELECT id_operator, operator_name, operator_mobile, operator_mail, operator_department_id FROM tb_test_operator ORDER BY id_operator";
        
        var operators = new List<TestOperator>();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            operators.Add(new TestOperator
            {
                IdOperator = reader.GetInt32("id_operator"),
                OperatorName = reader.GetString("operator_name"),
                OperatorMobile = reader.IsDBNull(reader.GetOrdinal("operator_mobile")) ? null : reader.GetString("operator_mobile"),
                OperatorMail = reader.IsDBNull(reader.GetOrdinal("operator_mail")) ? null : reader.GetString("operator_mail"),
                OperatorDepartmentId = reader.IsDBNull(reader.GetOrdinal("operator_department_id")) ? null : reader.GetInt32("operator_department_id")
            });
        }
        
        return operators;
    }

    // 获取所有子集列表
    public async Task<List<TestSubset>> GetSubsetsAsync()
    {
        const string sql = "SELECT subset_id, subset_name FROM tb_test_subset ORDER BY subset_id";
        
        var subsets = new List<TestSubset>();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            subsets.Add(new TestSubset
            {
                SubsetId = reader.GetInt32("subset_id"),
                SubsetName = reader.GetString("subset_name")
            });
        }
        
        return subsets;
    }
}
