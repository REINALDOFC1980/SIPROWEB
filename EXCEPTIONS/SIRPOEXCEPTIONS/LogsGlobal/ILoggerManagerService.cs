using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIRPOEXCEPTIONS.Log
{
    public interface ILoggerManagerService
    {
        void LogInfo(string message, params object[]  args);  // 🔥 Sobrecarga
        void LogWarn(string message);
        void LogDebug(string message);
        void LogError(string message);
    }
}

