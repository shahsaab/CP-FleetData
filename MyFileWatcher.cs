namespace CP_FleetDataJob
{
    public class MyFileWatcher : IMyFileWatcher
    {
        private string _mainDirectory;
        private string _errorDirectory;
        private string _successDirectory;
        private string _fileFilter = "*.csv";
        FileSystemWatcher _fileSystemWatcher;
        ILogger<MyFileWatcher> _logger;
        CustomLogs _customLogs;
        IServiceProvider _serviceProvider;

        public MyFileWatcher(ILogger<MyFileWatcher> logger, IServiceProvider serviceProvider, CustomLogs customLogs)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json").Build();

            _mainDirectory = configuration.GetSection("FilePaths").GetValue<string>("main");
            _errorDirectory = configuration.GetSection("FilePaths").GetValue<string>("fail");
            _successDirectory = configuration.GetSection("FilePaths").GetValue<string>("success");

            _logger = logger;
            if (!Directory.Exists(_mainDirectory))
                Directory.CreateDirectory(_mainDirectory);
            
            _fileSystemWatcher = new FileSystemWatcher(_mainDirectory, _fileFilter);
            _serviceProvider = serviceProvider;
            _customLogs = customLogs;
        }

        public void Start()
        {
            _fileSystemWatcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            _fileSystemWatcher.Created += _fileSystemWatcher_Created;

            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.IncludeSubdirectories = true;

            _logger.LogInformation($"File Watching has started for directory {_mainDirectory}");
        }

        private async void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var consumerService = scope.ServiceProvider.GetRequiredService<IFileConsumerService>();
                    bool success = await consumerService.ConsumeFile(e.FullPath);

                    string destination = success
                        ? Path.Combine(_successDirectory, $"{DateTime.Now:yyyyMMddHHmmss_}{e.Name}")
                        : Path.Combine(_errorDirectory, $"{DateTime.Now:yyyyMMddHHmmss_}{e.Name}");

                    File.Move(e.FullPath, destination);

                    string status = success ? "Success" : "Error";
                    await _customLogs.LogError($"File processed with status: {status}", e.Name, status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Task failed with error: {ex.Message}");
                await _customLogs.LogError(ex.Message, e.Name);
                File.Move(e.FullPath, Path.Combine(_errorDirectory, $"{DateTime.Now:yyyyMMddHHmmss_}{e.Name}"));
            }
        }

    }
}
