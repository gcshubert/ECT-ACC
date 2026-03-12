using ECT.ACC.Api.Services;
using ECT.ACC.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
//recommended by Claude for Swagger functionality
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ECTDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ECTDatabase")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // default Vite port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IScenarioService, ScenarioService>();
builder.Services.AddScoped<IDeficitAnalysisService, DeficitAnalysisService>();
builder.Services.AddScoped<IScenarioConfigurationService, ScenarioConfigurationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    ////Claude recommended: 
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactClient");

app.UseAuthorization();


app.MapControllers();

// Seed domain + template data
await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<ECTDbContext>();
await db.SeedDomainsAsync();

app.Run();
