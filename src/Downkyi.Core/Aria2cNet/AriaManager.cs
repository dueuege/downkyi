using System.Diagnostics;
using Downkyi.Core.Settings;

namespace Downkyi.Core.Aria2cNet;

/// <summary>
/// Manages the aria2c process lifecycle.
/// After calling Start(), callers use the Aria2cNet.Client.AriaClient static class directly,
/// configured via AriaManager.Host, Port, and Token properties.
/// </summary>
public static class AriaManager
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
    private static Process? _ariaProcess;
    private static readonly object _lock = new();

    public static string Host => SettingsManager.Instance.GetAriaHost();
    public static int Port => SettingsManager.Instance.GetAriaListenPort();
    public static string Token => SettingsManager.Instance.GetAriaToken();

    /// <summary>Starts the aria2c process. Safe to call multiple times — no-ops if already running.</summary>
    public static void Start()
    {
        lock (_lock)
        {
            if (_ariaProcess != null && !_ariaProcess.HasExited) return;

            string aria2c = GetAria2cPath();
            if (!File.Exists(aria2c))
            {
                Log.Warn("aria2c binary not found at {Path}, skipping Aria2c start", aria2c);
                return;
            }

            string ariaDir = Storage.StorageManager.GetAriaDir();
            string sessionFile = Path.Combine(ariaDir, "aria2.session");
            string logFile = Path.Combine(ariaDir, "aria2.log");

            string split = SettingsManager.Instance.GetAriaSplit().ToString();
            string maxOverall = SettingsManager.Instance.GetAriaMaxOverallDownloadLimit().ToString();
            string maxDl = SettingsManager.Instance.GetAriaMaxDownloadLimit().ToString();

            string args = string.Join(" ",
                "--enable-rpc",
                $"--rpc-listen-port={Port}",
                $"--rpc-secret={Token}",
                "--rpc-allow-origin-all=true",
                "--continue=true",
                "--max-concurrent-downloads=5",
                "--max-connection-per-server=16",
                $"--split={split}",
                "--min-split-size=1M",
                $"--max-overall-download-limit={maxOverall}",
                $"--max-download-limit={maxDl}",
                $"--save-session=\"{sessionFile}\"",
                $"--input-file=\"{sessionFile}\"",
                $"--log=\"{logFile}\"",
                "--log-level=warn",
                "--quiet=true"
            );

            _ariaProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = aria2c,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            try
            {
                _ariaProcess.Start();
                Log.Info("aria2c started with PID {Pid}", _ariaProcess.Id);
                Task.Delay(500).Wait();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to start aria2c");
                _ariaProcess = null;
            }
        }
    }

    /// <summary>Kills the aria2c process.</summary>
    public static void Stop()
    {
        lock (_lock)
        {
            try
            {
                if (_ariaProcess != null && !_ariaProcess.HasExited)
                {
                    _ariaProcess.Kill();
                    _ariaProcess.WaitForExit(3000);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "AriaManager.Stop failed");
            }
            finally
            {
                _ariaProcess = null;
            }
        }
    }

    public static bool IsRunning()
        => _ariaProcess != null && !_ariaProcess.HasExited;

    private static string GetAria2cPath()
        => Path.Combine(Environment.CurrentDirectory, OperatingSystem.IsWindows() ? "aria2c.exe" : "aria2c");
}
