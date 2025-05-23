using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIRPOEXCEPTIONS.Log
{
    public class LoggerManagerService : ILoggerManagerService
    {
        private readonly ILogger<LoggerManagerService> _logger;

        public LoggerManagerService(ILogger<LoggerManagerService> logger)
        {
            _logger = logger;
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }


        public void LogError(string message)
        {
            _logger.LogError(message);
        }


        public void LogInfo(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }


        public void LogWarn(string message)
        {
            _logger.LogWarning(message);
        }

    }
}
