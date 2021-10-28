using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace k2vr_installer_gui.Tools
{
    static class Utils
    {
        public static bool IsDirectoryEmpty(string path)
        {
            return (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length) == 0;
        }

        public static bool EnsureSteamVrClosed()
        {
            Logger.Log("Checking if SteamVR is open...", false);

            if (Process.GetProcesses().FirstOrDefault((Process proc) => proc.ProcessName == "vrserver" || proc.ProcessName == "vrserver") != null)
            {
                //Ask if SteamVR should be closed
                if (MessageBox.Show(
                    "SteamVR needs to be closed to continue the installation." + Environment.NewLine + "Do you want Setup to close SteamVR?",
                    "Close SteamVR?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                    ) != MessageBoxResult.Yes)
                {
                    //Only exit if SteamVR is still running
                    if (Process.GetProcesses().FirstOrDefault((Process proc) => proc.ProcessName == "vrserver" || proc.ProcessName == "vrserver") != null)
                    {
                        return false;
                    }
                }
            }

            //Close VrMonitor
            foreach (Process process in Process.GetProcesses().Where((proc) => proc.ProcessName == "vrmonitor"))
            {
                Logger.Log("Closing vrmonitor (PID: " + process.Id + ")...", false);

                process.CloseMainWindow();
                Thread.Sleep(5000);
                if (!process.HasExited)
                {
                    Logger.Log("Force closing...", false);
                    /* When SteamVR is open with no headset detected,
                        CloseMainWindow will only close the "headset not found" popup
                        so we kill it, if it's still open */
                    process.Kill();
                    Thread.Sleep(3000);
                }
            }

            //Close VrServer
            /* Apparently, SteamVR server can run without the monitor,
               so we close that, if it's open as well (monitor will complain if you close server first) */
            foreach (Process process in Process.GetProcesses().Where((proc) => proc.ProcessName == "vrserver"))
            {
                Logger.Log("Closing vrserver (PID: " + process.Id + ")...", false);

                // CloseMainWindow won't work here because it doesn't have a window
                process.Kill(); 
                Thread.Sleep(5000);
                if (!process.HasExited)
                {
                    MessageBox.Show(Properties.Resources.install_steamvr_close_failed);
                    return false;
                }
            }
            Logger.Log("Done!");
            return true;
        }
    }
}
