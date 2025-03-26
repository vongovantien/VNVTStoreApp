using FW.WAPI.Core.Configuration;
using FW.WAPI.Core.General;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace FW.WAPI.Core.Infrastructure.Logger
{
    public class CoreLogger : ICoreLogger
    {
        private readonly ILogger _logger;
        private readonly IStartupCoreOptions _startupCoreOptions;
        private readonly IConfiguration _configuration;

        public CoreLogger(IConfiguration configuration, ILogger<CoreLogger> logger, IStartupCoreOptions startupCoreOptions)
        {
            _logger = logger;
            _startupCoreOptions = startupCoreOptions;
            _configuration = configuration;
        }

        public void Log(string message)
        {
            try
            {
                var serviceName = _configuration.GetSection("ServiceName").Value;
                var logSouce = new { ServiceName = serviceName, Message = message };
                _logger.Log(_startupCoreOptions.LogLevel, JsonUtilities.ConvertObjectToJson(logSouce));
            }
            catch { }
        }

        public void Log(LogLevel logLevel, string message)
        {
            try
            {
                var serviceName = _configuration.GetSection("ServiceName").Value;
                var logSouce = new { ServiceName = serviceName, Message = message };
                _logger.Log(logLevel, JsonUtilities.ConvertObjectToJson(logSouce));
            }
            catch { }
        }

        [Obsolete("Function is obsolete")]
        public void Log(LogLevel logLevel, string message, params object[] args)
        {
            try
            {
                var serviceName = _configuration.GetSection("ServiceName").Value;
                var logSouce = new { ServiceName = serviceName, Message = string.Format(message, args) };
                _logger.Log(logLevel, JsonUtilities.ConvertObjectToJson(logSouce));
            }
            catch { }
        }

        public void LogAudit(string functionCode, string userCode, string action, string content, string description = null)
        {
            throw new NotImplementedException();
        }
    }

    public interface ICoreLogger
    {
        void Log(string message);
        void Log(LogLevel logLevel, string message);
        void Log(LogLevel logLevel, string message, params object[] args);
        void LogAudit(string functionCode, string userCode,
            string action, string content, string description = null);
    }
}
