using SkyRoute.Core.Extensions;
using SkyRoute.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddSkyRouteCore();
builder.Services.AddSkyRouteInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddPolicy(FrontendCorsPolicy, policy =>
	{
		var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
			?? ["http://localhost:4200"];

		policy.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
