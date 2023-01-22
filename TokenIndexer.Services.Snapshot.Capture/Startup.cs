using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using TokenIndexer.Domain.Initialization;
using TokenIndexer.Services.Snapshot.Capture;

[assembly: FunctionsStartup(typeof(Startup))]
namespace TokenIndexer.Services.Snapshot.Capture;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;

        builder.Services.AddRpcClient(configuration);
        builder.Services.AddNftApiClient(configuration);
        builder.Services.AddMongoDb(configuration);
        builder.Services.AddApplicationInsights(configuration);
        
        var domainAssembly = Assembly.Load("TokenIndexer.Domain");
        builder.Services.AddAutoMapper(domainAssembly);
        
        builder.Services.Scan(scan =>
            scan.FromAssemblies(domainAssembly)
                .AddClasses()
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithTransientLifetime());
    }
}