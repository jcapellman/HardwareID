using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HardwareID
{
    public class DeviceIdentification
    {
        private static string ExecProcess(string processName, string? command = null)
        {
            var procStartInfo = new ProcessStartInfo(processName, command ?? string.Empty)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = new Process() { StartInfo = procStartInfo };
            proc.Start();

            return proc.StandardOutput.ReadToEnd();
        }

        private static string GetLinuxID() => ExecProcess("/bin/bash", "-c \"cat /etc/machine-id\"").Trim().ToUpper();

        private static string GetWindowsID()
        {
            var output = ExecProcess("cmd", "/c " + "wmic csproduct get UUID");

            return output.Replace("UUID", string.Empty).Trim().ToUpper();
        }

        // This is a path forward, but system_profiler is slow and since this is being called on registration this would cause a bad UX as is
        private static string GetMacOSID() {
            var terminalPath = "/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            
            // Fallback for older versions of macOS
            if (!System.IO.File.Exists(terminalPath)) {
                terminalPath = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
            }
            
            var output = ExecProcess(terminalPath, "system_profiler | grep "\Hardware UUID:\"");
            
            return output.Replace("Hardware UUID:", string.Empty).Trim().ToUpper();
        }
        
        public static string UniqueID
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return GetWindowsID();
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return GetLinuxID();
                }

                throw new PlatformNotSupportedException($"Unsupported Platform ({RuntimeInformation.OSDescription})");
            }
        }
    }
}
