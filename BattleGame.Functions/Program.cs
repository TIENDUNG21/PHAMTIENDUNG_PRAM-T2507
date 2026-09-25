using BattleGame.Functions.Data;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton<IBattleGameRepository>(_ =>
{
    var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")
        ?? throw new InvalidOperationException("App setting 'SqlConnectionString' is missing.");
    return new BattleGameRepository(connectionString);
});

builder.Build().Run();
