using SkyRoute.Core.DependencyInjection;
using SkyRoute.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSkyRouteCore();
builder.Services.AddSkyRouteInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
