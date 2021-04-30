using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Omni.ImageConverter
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<ConversionSettings> _settings;

        private const string ConvertedFolder = "converted";
        private const string ErrorFolder = "error";
        private const string OriginalsFolder = "originals";

        public Worker(ILogger<Worker> logger, IOptions<ConversionSettings> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConfigureFileSystemWatcher();
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void ConfigureFileSystemWatcher()
        {
            Directory.CreateDirectory(_settings.Value.FolderPath);
            
            var fileSystemWatcher = new FileSystemWatcher(_settings.Value.FolderPath)
            {
                NotifyFilter = NotifyFilters.FileName
            };
            
            fileSystemWatcher.Created += OnFileAdded;
            fileSystemWatcher.IncludeSubdirectories = false;
            fileSystemWatcher.EnableRaisingEvents = true;

            foreach (var filter in _settings.Value.ConvertFrom.Select(i => Path.ChangeExtension("*", i)))
            {
                fileSystemWatcher.Filters.Add(filter);
            }

            Directory.CreateDirectory(Path.Join(_settings.Value.FolderPath, ConvertedFolder));
            Directory.CreateDirectory(Path.Join(_settings.Value.FolderPath, ErrorFolder));
            Directory.CreateDirectory(Path.Join(_settings.Value.FolderPath, OriginalsFolder));
        }
        
        private void OnFileAdded(object sender, FileSystemEventArgs eventArgs)
        {
            try
            {
                var magickFormat = Enum.Parse<MagickFormat>(_settings.Value.ConvertTo.Replace(".", ""), true);
                
                using var image = new MagickImage(eventArgs.FullPath) {Format = magickFormat};
                
                File.WriteAllBytes(Path.Join(_settings.Value.FolderPath, ConvertedFolder, Path.ChangeExtension(eventArgs.Name, _settings.Value.ConvertTo)), image.ToByteArray());

                File.Move(eventArgs.FullPath, Path.Join(_settings.Value.FolderPath, OriginalsFolder, eventArgs.Name));
                
                _logger.LogInformation($"Successfully converted {eventArgs.Name}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"There was an error converting {eventArgs.FullPath}");
                File.Move(eventArgs.FullPath, Path.Join(_settings.Value.FolderPath, ErrorFolder, eventArgs.Name));
            }
        }
    }
}
