using ChurchApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationLogging();

builder.Services.AddApplicationDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddApplicationSwagger();
builder.Services.AddApplicationAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddApplicationHealthChecks();

var app = builder.Build();

app.ApplyDatabaseMigrations();
app.UseApplicationPipeline();

app.Run();
