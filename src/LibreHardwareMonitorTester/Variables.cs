using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitorTester
{
    internal static class Variables
    {
        /// <summary>
        /// Application info
        /// </summary>
        internal static string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version?.Major}.{Assembly.GetExecutingAssembly().GetName().Version?.Minor}.{Assembly.GetExecutingAssembly().GetName().Version?.Build}.{Assembly.GetExecutingAssembly().GetName().Version?.Revision}";

        /// <summary>
        /// Local IO
        /// </summary>
        internal static string StartupPath { get; } = AppDomain.CurrentDomain.BaseDirectory;
        internal static string LogPath { get; } = Path.Combine(StartupPath, "logs");

        internal static IHardware GpuCard { get; set; }
    }
}
