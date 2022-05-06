using System.Globalization;
using LibreHardwareMonitor.Hardware;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace LibreHardwareMonitorTester
{
    public class Program
    {
        public static async Task Main()
        {
            try
            {
                // prepare console
                Console.Title = "LibreHardwareMonitor Tester";

                Console.WriteLine("");
                Console.WriteLine($"LibreHardwareMonitor Tester [{Variables.Version}]");
                Console.WriteLine("");
                Console.WriteLine("This application is provided as-is for testing purposes by LAB02 Research");
                Console.WriteLine("Usecase: [Aidan-Chey], [https://github.com/LAB02-Research/HASS.Agent/issues/46]");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("This application will continuously fetch GPU load and -temperature sensors");
                Console.WriteLine("");

                // prepare a serilog logger
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Async(a =>
                        a.File(Path.Combine(Variables.LogPath, $"{DateTime.Now:yyyy-MM-dd}_lhmtester_.log"),
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 10000000,
                            retainedFileCountLimit: 10,
                            rollOnFileSizeLimit: true,
                            buffered: true,
                            flushToDiskInterval: TimeSpan.FromMilliseconds(150)))
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate:
                        "[{Timestamp:MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .Enrich.FromLogContext()
                    .CreateLogger();

                var gpuFound = SetGpuCard();
                if (!gpuFound) return;

                Log.Information("Beginning perpetual sensor querying ..\r\n");

                while (true)
                {
                    Variables.GpuCard.Update();

                    ProcessLoadSensor(out var errorOccured);
                    if (errorOccured) return;

                    await Task.Delay(50);

                    ProcessTemperatureSensor(out errorOccured);
                    if (errorOccured) return;

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            catch (AccessViolationException ex)
            {
                Log.Information("");
                Log.Fatal(ex, "AccessViolationException occured: {err}", ex.Message);
                Log.Information("");
            }
            catch (Exception ex)
            {
                Log.Information("");
                Log.Fatal(ex, "Exception occured: {err}", ex.Message);
                Log.Information("");
            }
            finally
            {
                Log.CloseAndFlush();
                
                await Task.Delay(250);

                Console.WriteLine("");
                Console.WriteLine("Application completed, press any key to exit ..");

                Console.ReadKey(true);
            }
        }

        private static void ProcessLoadSensor(out bool errorOccured)
        {
            errorOccured = false;

            try
            {
                var loadSensor = Variables.GpuCard.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load);

                if (loadSensor == null)
                {
                    Log.Error("[LOADSENSOR] Sensor not found");
                    return;
                }

                if (loadSensor.Value == null)
                {
                    Log.Error("[LOADSENSOR] Sensor contains null value");
                    return;
                }

                if (!loadSensor.Value.HasValue)
                {
                    Log.Error("[LOADSENSOR] Sensor contains no value");
                    return;
                }

                var value = loadSensor.Value.Value.ToString("#.##", CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(value)) value = "0";

                Console.WriteLine($"[LOADSENSOR] Value: {value}");
            }
            catch (AccessViolationException ex)
            {
                Log.Information("");
                Log.Fatal(ex, "[LOADSENSOR] AccessViolationException occured: {err}", ex.Message);
                Log.Information("");

                Log.Information("[LOADSENSOR] Trying to reload GPU card ..");

                var reloaded = SetGpuCard();
                if (!reloaded) errorOccured = true;
            }
            catch (Exception ex)
            {
                Log.Information("");
                Log.Fatal(ex, "[LOADSENSOR] Exception occured: {err}", ex.Message);
                Log.Information("");

                Log.Information("[LOADSENSOR] Trying to reload GPU card ..");

                var reloaded = SetGpuCard();
                if (!reloaded) errorOccured = true;
            }
        }

        private static void ProcessTemperatureSensor(out bool errorOccured)
        {
            errorOccured = false;

            try
            {
                var tempSensor = Variables.GpuCard.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);

                if (tempSensor == null)
                {
                    Log.Error("[TEMPSENSOR] Sensor not found");
                    return;
                }

                if (tempSensor.Value == null)
                {
                    Log.Error("[TEMPSENSOR] Sensor contains null value");
                    return;
                }

                if (!tempSensor.Value.HasValue)
                {
                    Log.Error("[TEMPSENSOR] Sensor contains no value");
                    return;
                }

                var value = tempSensor.Value.Value.ToString("#.##", CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(value)) value = "0";

                Console.WriteLine($"[TEMPSENSOR] Value: {value}");
            }
            catch (AccessViolationException ex)
            {
                Log.Information("");
                Log.Fatal(ex, "[TEMPSENSOR] AccessViolationException occured: {err}", ex.Message);
                Log.Information("");

                Log.Information("[TEMPSENSOR] Trying to reload GPU card ..");

                var reloaded = SetGpuCard();
                if (!reloaded) errorOccured = true;
            }
            catch (Exception ex)
            {
                Log.Information("");
                Log.Fatal(ex, "[TEMPSENSOR] Exception occured: {err}", ex.Message);
                Log.Information("");

                Log.Information("[TEMPSENSOR] Trying to reload GPU card ..");

                var reloaded = SetGpuCard();
                if (!reloaded) errorOccured = true;
            }
        }

        private static bool SetGpuCard()
        {
            try
            {
                Log.Information("[HARDWARE] Fetching PC hardware info ..");

                var computer = new Computer
                {
                    IsCpuEnabled = false,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = false,
                    IsMotherboardEnabled = false,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = false,
                };

                computer.Open();

                Log.Information("[HARDWARE] Fetching GPU card ..");

                Variables.GpuCard = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel);

                computer.Close();

                if (Variables.GpuCard == null)
                {
                    Log.Error("[HARDWARE] No GPU card found, unable to monitor");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Information("");
                Log.Fatal(ex, "[HARDWARE] Exception occured while fetching the GPUY card: {err}", ex.Message);
                Log.Information("");

                Log.Error("[HARDWARE] Failed to retrieve a GPU card, unable to continue");
                return false;
            }
        }
    }
}