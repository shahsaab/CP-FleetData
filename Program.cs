using CP_FleetDataJob;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDbContext<FleetDbContext>();
        services.AddSingleton<CustomLogs>();
        services.AddHostedService<Worker>();
        services.AddSingleton<IMyFileWatcher,MyFileWatcher>();
        services.AddScoped<IFileConsumerService,FileConsumerService>();
        
    })
    .Build();

await host.RunAsync();
