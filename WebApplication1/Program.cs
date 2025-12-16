using System.Text.Json.Serialization;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);
builder.SetupDatabase();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.InitializeRepositories();
builder.Services.InitializeServices();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ExceptionHandlingMiddleware>();

await app.MigrateAsync();

await app.RunAsync();

public partial class Program { }