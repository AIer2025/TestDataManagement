using MySqlConnector;
using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Repositories;

/// <summary>
/// 主数据仓储实现
/// </summary>
public class MasterDataRepository : IMasterDataRepository
{
    private readonly string _connectionString;

    public MasterDataRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    #region 实验员主数据

    public async Task<List<TestOperator>> GetAllOperatorsAsync()
    {
        const string sql = @"
            SELECT id_operator, operator_name, operator_mobile, operator_mail, operator_department_id
            FROM tb_test_operator
            ORDER BY id_operator";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        var list = new List<TestOperator>();
        while (await reader.ReadAsync())
        {
            list.Add(MapToOperator(reader));
        }
        return list;
    }

    public async Task<TestOperator?> GetOperatorByIdAsync(int id)
    {
        const string sql = @"
            SELECT id_operator, operator_name, operator_mobile, operator_mail, operator_department_id
            FROM tb_test_operator
            WHERE id_operator = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToOperator(reader);
        }
        return null;
    }

    public async Task<int> CreateOperatorAsync(TestOperator entity)
    {
        const string sql = @"
            INSERT INTO tb_test_operator (operator_name, operator_mobile, operator_mail, operator_department_id)
            VALUES (@Name, @Mobile, @Mail, @DeptId);
            SELECT LAST_INSERT_ID();";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", entity.OperatorName);
        command.Parameters.AddWithValue("@Mobile", entity.OperatorMobile ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Mail", entity.OperatorMail ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DeptId", entity.OperatorDepartmentId ?? (object)DBNull.Value);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateOperatorAsync(TestOperator entity)
    {
        const string sql = @"
            UPDATE tb_test_operator 
            SET operator_name = @Name, 
                operator_mobile = @Mobile, 
                operator_mail = @Mail, 
                operator_department_id = @DeptId
            WHERE id_operator = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", entity.IdOperator);
        command.Parameters.AddWithValue("@Name", entity.OperatorName);
        command.Parameters.AddWithValue("@Mobile", entity.OperatorMobile ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Mail", entity.OperatorMail ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DeptId", entity.OperatorDepartmentId ?? (object)DBNull.Value);
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteOperatorAsync(int id)
    {
        const string sql = "DELETE FROM tb_test_operator WHERE id_operator = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    private static TestOperator MapToOperator(MySqlDataReader reader)
    {
        return new TestOperator
        {
            IdOperator = reader.GetInt32("id_operator"),
            OperatorName = reader.GetString("operator_name"),
            OperatorMobile = reader.IsDBNull(reader.GetOrdinal("operator_mobile")) ? null : reader.GetString("operator_mobile"),
            OperatorMail = reader.IsDBNull(reader.GetOrdinal("operator_mail")) ? null : reader.GetString("operator_mail"),
            OperatorDepartmentId = reader.IsDBNull(reader.GetOrdinal("operator_department_id")) ? null : reader.GetInt32("operator_department_id")
        };
    }

    #endregion

    #region 公司(系统)主数据

    public async Task<List<SystemEntity>> GetAllSystemsAsync()
    {
        const string sql = @"
            SELECT s.*, 
                   CASE WHEN EXISTS(SELECT 1 FROM tb_platform p WHERE p.system_id = s.system_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_system s
            ORDER BY s.system_id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        var list = new List<SystemEntity>();
        while (await reader.ReadAsync())
        {
            list.Add(MapToSystem(reader));
        }
        return list;
    }

    public async Task<SystemEntity?> GetSystemByIdAsync(int id)
    {
        const string sql = @"
            SELECT s.*, 
                   CASE WHEN EXISTS(SELECT 1 FROM tb_platform p WHERE p.system_id = s.system_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_system s
            WHERE s.system_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToSystem(reader);
        }
        return null;
    }

    public async Task<int> CreateSystemAsync(SystemEntity entity)
    {
        const string sql = @"
            INSERT INTO tb_system (system_code, system_name, description, location, install_date, warranty_end_date, is_active, created_by, updated_by)
            VALUES (@Code, @Name, @Desc, @Location, @InstallDate, @WarrantyDate, @IsActive, @CreatedBy, @UpdatedBy);
            SELECT LAST_INSERT_ID();";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Code", entity.SystemCode);
        command.Parameters.AddWithValue("@Name", entity.SystemName);
        command.Parameters.AddWithValue("@Desc", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Location", entity.Location ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InstallDate", entity.InstallDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@WarrantyDate", entity.WarrantyEndDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);
        command.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy ?? "admin");
        command.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy ?? "admin");
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateSystemAsync(SystemEntity entity)
    {
        const string sql = @"
            UPDATE tb_system 
            SET system_code = @Code, 
                system_name = @Name, 
                description = @Desc, 
                location = @Location,
                install_date = @InstallDate,
                warranty_end_date = @WarrantyDate,
                is_active = @IsActive,
                updated_by = @UpdatedBy
            WHERE system_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", entity.SystemId);
        command.Parameters.AddWithValue("@Code", entity.SystemCode);
        command.Parameters.AddWithValue("@Name", entity.SystemName);
        command.Parameters.AddWithValue("@Desc", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Location", entity.Location ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InstallDate", entity.InstallDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@WarrantyDate", entity.WarrantyEndDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);
        command.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy ?? "admin");
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteSystemAsync(int id)
    {
        const string sql = "DELETE FROM tb_system WHERE system_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> IsSystemReferencedAsync(int systemId)
    {
        const string sql = "SELECT COUNT(*) FROM tb_platform WHERE system_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", systemId);
        
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private static SystemEntity MapToSystem(MySqlDataReader reader)
    {
        return new SystemEntity
        {
            SystemId = reader.GetInt32("system_id"),
            SystemCode = reader.GetString("system_code"),
            SystemName = reader.GetString("system_name"),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
            Location = reader.IsDBNull(reader.GetOrdinal("location")) ? null : reader.GetString("location"),
            InstallDate = reader.IsDBNull(reader.GetOrdinal("install_date")) ? null : reader.GetDateTime("install_date"),
            WarrantyEndDate = reader.IsDBNull(reader.GetOrdinal("warranty_end_date")) ? null : reader.GetDateTime("warranty_end_date"),
            CreateTime = reader.GetDateTime("create_time"),
            UpdateTime = reader.GetDateTime("update_time"),
            IsActive = reader.GetBoolean("is_active"),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetString("created_by"),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetString("updated_by"),
            IsReferenced = reader.GetInt32("is_referenced") == 1
        };
    }

    #endregion

    #region 平台主数据

    public async Task<List<Platform>> GetAllPlatformsAsync()
    {
        const string sql = @"
            SELECT p.*, s.system_name,
                   CASE WHEN EXISTS(SELECT 1 FROM tb_module m WHERE m.platform_id = p.platform_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_platform p
            INNER JOIN tb_system s ON p.system_id = s.system_id
            ORDER BY p.platform_id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        var list = new List<Platform>();
        while (await reader.ReadAsync())
        {
            list.Add(MapToPlatform(reader));
        }
        return list;
    }

    public async Task<Platform?> GetPlatformByIdAsync(int id)
    {
        const string sql = @"
            SELECT p.*, s.system_name,
                   CASE WHEN EXISTS(SELECT 1 FROM tb_module m WHERE m.platform_id = p.platform_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_platform p
            INNER JOIN tb_system s ON p.system_id = s.system_id
            WHERE p.platform_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToPlatform(reader);
        }
        return null;
    }

    public async Task<List<Platform>> GetPlatformsBySystemIdAsync(int systemId)
    {
        const string sql = @"
            SELECT p.*, s.system_name,
                   CASE WHEN EXISTS(SELECT 1 FROM tb_module m WHERE m.platform_id = p.platform_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_platform p
            INNER JOIN tb_system s ON p.system_id = s.system_id
            WHERE p.system_id = @SystemId
            ORDER BY p.platform_id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SystemId", systemId);
        await using var reader = await command.ExecuteReaderAsync();
        
        var list = new List<Platform>();
        while (await reader.ReadAsync())
        {
            list.Add(MapToPlatform(reader));
        }
        return list;
    }

    public async Task<int> CreatePlatformAsync(Platform entity)
    {
        const string sql = @"
            INSERT INTO tb_platform (system_id, platform_code, platform_name, platform_type, serial_number, description, install_date, is_active, created_by, updated_by)
            VALUES (@SystemId, @Code, @Name, @Type, @Serial, @Desc, @InstallDate, @IsActive, @CreatedBy, @UpdatedBy);
            SELECT LAST_INSERT_ID();";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SystemId", entity.SystemId);
        command.Parameters.AddWithValue("@Code", entity.PlatformCode);
        command.Parameters.AddWithValue("@Name", entity.PlatformName);
        command.Parameters.AddWithValue("@Type", entity.PlatformType ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Serial", entity.SerialNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Desc", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InstallDate", entity.InstallDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);
        command.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy ?? "admin");
        command.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy ?? "admin");
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdatePlatformAsync(Platform entity)
    {
        const string sql = @"
            UPDATE tb_platform 
            SET system_id = @SystemId,
                platform_code = @Code, 
                platform_name = @Name, 
                platform_type = @Type,
                serial_number = @Serial,
                description = @Desc,
                install_date = @InstallDate,
                is_active = @IsActive,
                updated_by = @UpdatedBy
            WHERE platform_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", entity.PlatformId);
        command.Parameters.AddWithValue("@SystemId", entity.SystemId);
        command.Parameters.AddWithValue("@Code", entity.PlatformCode);
        command.Parameters.AddWithValue("@Name", entity.PlatformName);
        command.Parameters.AddWithValue("@Type", entity.PlatformType ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Serial", entity.SerialNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Desc", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InstallDate", entity.InstallDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);
        command.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy ?? "admin");
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeletePlatformAsync(int id)
    {
        const string sql = "DELETE FROM tb_platform WHERE platform_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> IsPlatformReferencedAsync(int platformId)
    {
        const string sql = "SELECT COUNT(*) FROM tb_module WHERE platform_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", platformId);
        
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private static Platform MapToPlatform(MySqlDataReader reader)
    {
        return new Platform
        {
            PlatformId = reader.GetInt32("platform_id"),
            SystemId = reader.GetInt32("system_id"),
            PlatformCode = reader.GetString("platform_code"),
            PlatformName = reader.GetString("platform_name"),
            PlatformType = reader.IsDBNull(reader.GetOrdinal("platform_type")) ? null : reader.GetString("platform_type"),
            SerialNumber = reader.IsDBNull(reader.GetOrdinal("serial_number")) ? null : reader.GetString("serial_number"),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
            InstallDate = reader.IsDBNull(reader.GetOrdinal("install_date")) ? null : reader.GetDateTime("install_date"),
            CreateTime = reader.GetDateTime("create_time"),
            UpdateTime = reader.GetDateTime("update_time"),
            IsActive = reader.GetBoolean("is_active"),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetString("created_by"),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetString("updated_by"),
            SystemName = reader.GetString("system_name"),
            IsReferenced = reader.GetInt32("is_referenced") == 1
        };
    }

    #endregion

    #region 模块主数据

    public async Task<List<Module>> GetAllModulesAsync()
    {
        const string sql = @"
            SELECT m.*, p.platform_name, s.system_name,
                   CASE WHEN EXISTS(SELECT 1 FROM tb_test_data td WHERE td.module_id = m.module_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_module m
            INNER JOIN tb_platform p ON m.platform_id = p.platform_id
            INNER JOIN tb_system s ON p.system_id = s.system_id
            ORDER BY m.module_id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        var list = new List<Module>();
        while (await reader.ReadAsync())
        {
            list.Add(MapToModuleWithRef(reader));
        }
        return list;
    }

    public async Task<List<Module>> GetModulesByPlatformIdAsync(int platformId)
    {
        const string sql = @"
            SELECT m.*, p.platform_name, s.system_name,
                   CASE WHEN EXISTS(SELECT 1 FROM tb_test_data td WHERE td.module_id = m.module_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_module m
            INNER JOIN tb_platform p ON m.platform_id = p.platform_id
            INNER JOIN tb_system s ON p.system_id = s.system_id
            WHERE m.platform_id = @PlatformId
            ORDER BY m.module_id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PlatformId", platformId);
        await using var reader = await command.ExecuteReaderAsync();
        
        var list = new List<Module>();
        while (await reader.ReadAsync())
        {
            list.Add(MapToModuleWithRef(reader));
        }
        return list;
    }

    public async Task<Module?> GetModuleByIdAsync(int id)
    {
        const string sql = @"
            SELECT m.*, p.platform_name, s.system_name,
                   CASE WHEN EXISTS(SELECT 1 FROM tb_test_data td WHERE td.module_id = m.module_id) THEN 1 ELSE 0 END AS is_referenced
            FROM tb_module m
            INNER JOIN tb_platform p ON m.platform_id = p.platform_id
            INNER JOIN tb_system s ON p.system_id = s.system_id
            WHERE m.module_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToModuleWithRef(reader);
        }
        return null;
    }

    public async Task<int> CreateModuleAsync(Module entity)
    {
        const string sql = @"
            INSERT INTO tb_module (platform_id, module_code, module_name, module_type, manufacturer, model_number, serial_number, manufacture_date, rated_life, description, is_active, created_by, updated_by)
            VALUES (@PlatformId, @Code, @Name, @Type, @Manufacturer, @ModelNumber, @SerialNumber, @ManufactureDate, @RatedLife, @Desc, @IsActive, @CreatedBy, @UpdatedBy);
            SELECT LAST_INSERT_ID();";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@PlatformId", entity.PlatformId);
        command.Parameters.AddWithValue("@Code", entity.ModuleCode);
        command.Parameters.AddWithValue("@Name", entity.ModuleName);
        command.Parameters.AddWithValue("@Type", entity.ModuleType ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Manufacturer", entity.Manufacturer ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ModelNumber", entity.ModelNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SerialNumber", entity.SerialNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ManufactureDate", entity.ManufactureDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@RatedLife", entity.RatedLife ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Desc", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);
        command.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy ?? "admin");
        command.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy ?? "admin");
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateModuleAsync(Module entity)
    {
        const string sql = @"
            UPDATE tb_module 
            SET platform_id = @PlatformId,
                module_code = @Code, 
                module_name = @Name, 
                module_type = @Type,
                manufacturer = @Manufacturer,
                model_number = @ModelNumber,
                serial_number = @SerialNumber,
                manufacture_date = @ManufactureDate,
                rated_life = @RatedLife,
                description = @Desc,
                is_active = @IsActive,
                updated_by = @UpdatedBy
            WHERE module_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", entity.ModuleId);
        command.Parameters.AddWithValue("@PlatformId", entity.PlatformId);
        command.Parameters.AddWithValue("@Code", entity.ModuleCode);
        command.Parameters.AddWithValue("@Name", entity.ModuleName);
        command.Parameters.AddWithValue("@Type", entity.ModuleType ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Manufacturer", entity.Manufacturer ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ModelNumber", entity.ModelNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SerialNumber", entity.SerialNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ManufactureDate", entity.ManufactureDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@RatedLife", entity.RatedLife ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Desc", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", entity.IsActive);
        command.Parameters.AddWithValue("@UpdatedBy", entity.UpdatedBy ?? "admin");
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteModuleAsync(int id)
    {
        const string sql = "DELETE FROM tb_module WHERE module_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> IsModuleReferencedAsync(int moduleId)
    {
        const string sql = "SELECT COUNT(*) FROM tb_test_data WHERE module_id = @Id";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", moduleId);
        
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private static Module MapToModuleWithRef(MySqlDataReader reader)
    {
        return new Module
        {
            ModuleId = reader.GetInt32("module_id"),
            PlatformId = reader.GetInt32("platform_id"),
            ModuleCode = reader.GetString("module_code"),
            ModuleName = reader.GetString("module_name"),
            ModuleType = reader.IsDBNull(reader.GetOrdinal("module_type")) ? null : reader.GetString("module_type"),
            Manufacturer = reader.IsDBNull(reader.GetOrdinal("manufacturer")) ? null : reader.GetString("manufacturer"),
            ModelNumber = reader.IsDBNull(reader.GetOrdinal("model_number")) ? null : reader.GetString("model_number"),
            SerialNumber = reader.IsDBNull(reader.GetOrdinal("serial_number")) ? null : reader.GetString("serial_number"),
            ManufactureDate = reader.IsDBNull(reader.GetOrdinal("manufacture_date")) ? null : reader.GetDateTime("manufacture_date"),
            RatedLife = reader.IsDBNull(reader.GetOrdinal("rated_life")) ? null : reader.GetInt32("rated_life"),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
            IsActive = reader.GetBoolean("is_active"),
            PlatformName = reader.GetString("platform_name"),
            SystemName = reader.GetString("system_name"),
            IsReferenced = reader.GetInt32("is_referenced") == 1,
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetString("created_by"),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetString("updated_by")
        };
    }

    #endregion
}
