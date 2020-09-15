using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace systemdsensordaemon
{
  public class Worker : BackgroundService
  {
    enum ConfigStatusEnum {
      ConfigNotFound,
      ConfigParseError,
      ConfigOk
    }

    private readonly ILogger<Worker> _logger;
    private DaemonConfig config;
    private BigInteger pollCount = 0;
    private const String configurationFile = "/etc/samid.conf";
    public Worker(ILogger<Worker> logger)
    {
      _logger = logger;
      
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {

      FileInfo fileInfo = new FileInfo(configurationFile);

      _logger.LogInformation("samid worker starting...");
      _logger.LogInformation("samid reading configuration file from "+ configurationFile);
      switch (ReadConfig(fileInfo)) {
        case ConfigStatusEnum.ConfigOk:
          _logger.LogInformation("samid configuration file OK.");
          break;
        case ConfigStatusEnum.ConfigNotFound:
          _logger.LogInformation("samid configuration file NOT FOUND.");
          break;
        case ConfigStatusEnum.ConfigParseError:
          _logger.LogInformation("samid configuration file PARSE ERROR.");
          break;
      }
      return base.StartAsync(cancellationToken);      
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      if (config != null) {
        _logger.LogInformation("samid running every: " + config.pollingFrequency + " seconds.");
        _logger.LogInformation("samid outputs to: " + config.outputServer);
        _logger.LogInformation("samid logging to journalctl: " + config.isLogging + ".");

        while (!stoppingToken.IsCancellationRequested) {
          if (config.isLogging) {
            if (config.sources != null) {
              config.sources.ForEach(
              (source) => {

              });
            }            
          }

          _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
          await Task.Delay(config.pollingFrequency * 1000, stoppingToken);
        }
      }       
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("samid POLLING STOPPED.");
      _logger.LogInformation("samid poll count at {time} was " + pollCount.ToString(), DateTimeOffset.Now);
      return base.StopAsync(cancellationToken);
    }

    private ConfigStatusEnum ReadConfig(FileInfo config) {
      JSchemaGenerator jSchemaGenerator = new JSchemaGenerator();
      JSchema configShema = jSchemaGenerator.Generate(typeof(DaemonConfig));
      if (config.Exists) {
        try {
          JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(File.ReadAllText(config.FullName)));
          JSchemaValidatingReader jSchemaValidatingReader = new JSchemaValidatingReader(jsonTextReader);
          JsonSerializer serializer = new JsonSerializer();
          this.config = serializer.Deserialize<DaemonConfig>(jSchemaValidatingReader);
          return ConfigStatusEnum.ConfigOk;
        } catch (Exception) {
          return ConfigStatusEnum.ConfigParseError;
        }
      } else {
        return ConfigStatusEnum.ConfigNotFound;
      }
    }
  }
}
