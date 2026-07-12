//using CodebaseAssistant.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using CodebaseAssistant.Infrastructure.Configuration;
//using CodebaseAssistant.Infrastructure.Services;
//using CodebaseAssistant.Application.Interfaces;
//var builder = WebApplication.CreateBuilder(args);
////using CodebaseAssistant.Infrastructure.DependencyInjection;
//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<CodebaseAssistantDbContext>(options =>
//{
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"));
//});
//// Inline infrastructure registrations to avoid ambiguity between compiled refs
//builder.Services.Configure<UploadSettings>(
//    builder.Configuration.GetSection("UploadSettings"));

//builder.Services.AddScoped<IRepositoryService, RepositoryService>();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using CodebaseAssistant.Infrastructure.DependencyInjection;
using CodebaseAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CodebaseAssistantDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register all Infrastructure services
builder.Services.AddInfrastructureServices(
    builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();