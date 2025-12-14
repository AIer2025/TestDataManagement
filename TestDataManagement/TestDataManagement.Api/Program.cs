using TestDataManagement.Api.Repositories;
using TestDataManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 获取数据库连接字符串
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection")
    ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");

// 注册仓储和服务 - 使用工厂方法正确注入连接字符串
builder.Services.AddScoped<ITestDataRepository>(sp => new TestDataRepository(connectionString));
builder.Services.AddScoped<ITestDataService, TestDataService>();
builder.Services.AddScoped<IModuleRepository>(sp => new ModuleRepository(connectionString));
builder.Services.AddScoped<IModuleService, ModuleService>();

// 注册 Weibull 分析服务
builder.Services.AddScoped<IWeibullAnalysisService, WeibullAnalysisService>();

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// 添加静态文件支持
builder.Services.AddDirectoryBrowser();

var app = builder.Build(); //注释测试

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 启用静态文件
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();