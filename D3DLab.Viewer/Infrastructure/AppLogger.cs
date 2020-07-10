using D3DLab.ECS;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Text;

using WPFLab;

namespace D3DLab.Viewer.Infrastructure {
    class AppLogger : IAppLogger, ILabLogger {
        readonly Logger log;
        public AppLogger() {
            var config = new LoggingConfiguration();
            var logfile = new FileTarget("logfile") { FileName = "lab.log" };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
            log = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Debug(string message) {
            log.Debug(message);
        }
        public void Error(Exception exception, string message) {
            log.Error(exception, message);
        }
        public void Error(Exception exception) {
            log.Error(exception);
        }
        public void Error(string message) {
            log.Error(message);
        }
        public void Info(string message) {
            log.Info(message);
        }
        public void Warn(string message) {
            log.Warn(message);
        }
    }
}
