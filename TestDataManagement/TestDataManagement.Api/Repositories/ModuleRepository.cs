using MySqlConnector;
using TestDataManagement.Api.Models;

namespace TestDataManagement.Api.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly string _connectionString;

    public ModuleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Module>> GetAllActiveAsync()
    {
        const string sql = @"
            SELECT m.*, p.platform_name, s.system_name
            FROM tb_module m
            INNER JOIN tb_platform p ON m.platform_id = p.platform_id
            INNER JOIN tb_system s ON p.system_id = s.system_id
            WHERE m.is_active = 1
            ORDER BY m.module_code";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        var modules = new List<Module>();
        while (await reader.ReadAsync())
        {
            modules.Add(MapToModule(reader));
        }
        return modules;
    }

    public async Task<Module?> GetByIdAsync(int moduleId)
    {
        const string sql = @"
            SELECT m.*, p.platform_name, s.system_name
            FROM tb_module m
            INNER JOIN tb_platform p ON m.platform_id = p.platform_id
            INNER JOIN tb_system s ON p.system_id = s.system_id
            WHERE m.module_id = @ModuleId";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ModuleId", moduleId);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToModule(reader);
        }
        return null;
    }

    private static Module MapToModule(MySqlDataReader reader)
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
            SystemName = reader.GetString("system_name")
        };
    }
}