using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Management.Deployment;

namespace Flarial.Launcher
{

    public static partial class Minecraft
    {

        public static Process Process;
        public const string FamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        public static PackageManager PackageManager { get; private set; }
        public static Windows.ApplicationModel.Package Package { get; private set; }
        public static Windows.ApplicationModel.PackageId PackageId => Package.Id ?? throw new NullReferenceException();
        public static Windows.System.ProcessorArchitecture PackageArchitecture => PackageId.Architecture;
        public static Windows.ApplicationModel.PackageVersion PackageVersion => PackageId.Version;
        public static Windows.Storage.ApplicationData ApplicationData { get; private set; }

        public static void Init()
        {

            InitManagers();
            FindPackage();



            var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (mcIndex.Length > 0)
            {
                Process = mcIndex[0];

            }

        }

        public static bool isInstalled()
        {
            PackageManager = new PackageManager();
            var Packages = PackageManager.FindPackages(FamilyName);
            return Packages.Count() != 0;
        }

        public static void InitManagers()
        {
            PackageManager = new PackageManager();
            var Packages = PackageManager.FindPackages(FamilyName);
            
            if (Packages.Count() == 0)
            {

                MainWindow.CreateMessageBox("Minecraft needs to be installed");
                //Environment.Exit(0);
            }
            else
            {
                ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);
            }

            //  ApplicationData = Windows.Management.Core.ApplicationDataManager.CreateForPackageFamily(FamilyName);


        }

        public static void FindPackage()
        {

            if (PackageManager is null) throw new NullReferenceException();
            var Packages = PackageManager.FindPackages(FamilyName);

            if (Packages.Count() != 0)
            {
                Package = Packages.First();
            }
        }
        public static double RoundToSignificantDigits(this double d, int digits)
        {
            if (d == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }
        public static Version GetVersion()
        {
            var v = PackageVersion;

            // Get the build number as a string
            string buildString = v.Build.ToString();

            // Take the first two digits of the build number
            string truncatedBuild = buildString.Substring(0, Math.Min(2, buildString.Length));

            // Construct the version string with Major, Minor, and truncated Build
            string versionString = $"{v.Major}.{v.Minor}.{truncatedBuild}";

            // Parse the version string to a Version object
            Version parsedVersion;
            if (!Version.TryParse(versionString, out parsedVersion))
            {
                // Handle the case where parsing fails
                throw new InvalidOperationException($"Failed to parse version string: {versionString}");
            }

            return parsedVersion;
        }
        
        public static async Task WaitForModules()
        {
            if(Process is { HasExited: true }) Process = null;
            
            while (Process == null)
            {
                var mcIndex = Process.GetProcessesByName("Minecraft.Windows");
                if (mcIndex.Length > 0)
                {
                    
                    Process = mcIndex[0];

                }
                
                await Task.Delay(100);
            }

            Trace.WriteLine("Waiting for Minecraft to load");
            while (true)
            {
                Process.Refresh();
                if (!Process.HasExited)
                {
                    if (Process.Modules.Count > 160)
                    {
                        await Task.Delay(1500);
                        break;
                    }
                }
            }
            Trace.WriteLine("Minecraft finished loading");
        }

    }
}