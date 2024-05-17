using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;

namespace LogitechBatteryIndicator.helpers
{
    public sealed class AssemblyLoader : IDisposable
    {
        private static readonly IList<string> _logiNethidppioDependentAssemblies = new ReadOnlyCollection<string>(
        [
            "api-ms-win-core-console-l1-1-0.dll",
            "api-ms-win-core-console-l1-2-0.dll",
            "api-ms-win-core-datetime-l1-1-0.dll",
            "api-ms-win-core-debug-l1-1-0.dll",
            "api-ms-win-core-errorhandling-l1-1-0.dll",
            "api-ms-win-core-file-l1-1-0.dll",
            "api-ms-win-core-file-l1-2-0.dll",
            "api-ms-win-core-file-l2-1-0.dll",
            "api-ms-win-core-handle-l1-1-0.dll",
            "api-ms-win-core-heap-l1-1-0.dll",
            "api-ms-win-core-interlocked-l1-1-0.dll",
            "api-ms-win-core-libraryloader-l1-1-0.dll",
            "api-ms-win-core-localization-l1-2-0.dll",
            "api-ms-win-core-memory-l1-1-0.dll",
            "api-ms-win-core-namedpipe-l1-1-0.dll",
            "api-ms-win-core-processenvironment-l1-1-0.dll",
            "api-ms-win-core-processthreads-l1-1-0.dll",
            "api-ms-win-core-processthreads-l1-1-1.dll",
            "api-ms-win-core-profile-l1-1-0.dll",
            "api-ms-win-core-rtlsupport-l1-1-0.dll",
            "api-ms-win-core-string-l1-1-0.dll",
            "api-ms-win-core-synch-l1-1-0.dll",
            "api-ms-win-core-synch-l1-2-0.dll",
            "api-ms-win-core-sysinfo-l1-1-0.dll",
            "api-ms-win-core-timezone-l1-1-0.dll",
            "api-ms-win-core-util-l1-1-0.dll",
            "API-MS-Win-core-xstate-l2-1-0.dll",
            "api-ms-win-crt-conio-l1-1-0.dll",
            "api-ms-win-crt-convert-l1-1-0.dll",
            "api-ms-win-crt-environment-l1-1-0.dll",
            "api-ms-win-crt-filesystem-l1-1-0.dll",
            "api-ms-win-crt-heap-l1-1-0.dll",
            "api-ms-win-crt-locale-l1-1-0.dll",
            "api-ms-win-crt-math-l1-1-0.dll",
            "api-ms-win-crt-multibyte-l1-1-0.dll",
            "api-ms-win-crt-private-l1-1-0.dll",
            "api-ms-win-crt-process-l1-1-0.dll",
            "api-ms-win-crt-runtime-l1-1-0.dll",
            "api-ms-win-crt-stdio-l1-1-0.dll",
            "api-ms-win-crt-string-l1-1-0.dll",
            "api-ms-win-crt-time-l1-1-0.dll",
            "api-ms-win-crt-utility-l1-1-0.dll",
            "concrt140.dll",
            "msvcp140.dll",
            "msvcp140_1.dll",
            "msvcp140_2.dll",
            "msvcp140_codecvt_ids.dll",
            "ucrtbase.dll",
            "vcruntime140.dll"
        ]);
        private readonly Dictionary<string, Assembly> _assemblies = [];
        private readonly string assemblyCacheDir;

        public AssemblyLoader(string assemblyCacheDir)
        {
            this.assemblyCacheDir = assemblyCacheDir;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            GC.SuppressFinalize(this);
        }

        private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            if (_assemblies.TryGetValue(args.Name, out Assembly? value))
            {
                return value;
            }
            var name1 = string.Empty;
            var name2 = new AssemblyName(args.Name).Name;
            if (name2 == GetType().Assembly.GetName().Name + ".resources")
            {
                var cultureName = new AssemblyName(args.Name).CultureName;
                if (!string.IsNullOrEmpty(cultureName))
                    name1 = string.Format("{0}.embeddeddlls.Resources_{1}.dll", Assembly.GetExecutingAssembly().GetName().Name, cultureName);
            }
            if (string.IsNullOrEmpty(name1))
            {
                name1 = string.Format("{0}.embeddeddlls.{1}.dll", Assembly.GetExecutingAssembly().GetName().Name, name2);
            }
            if (name2 == "logi_nethidppio")
            {
                ExtractResourceAssemblies(_logiNethidppioDependentAssemblies);
            }
            using var manifestResourceStream = GetType().Assembly.GetManifestResourceStream(name1);
            if (manifestResourceStream == null)
            {
                return null;
            }
            byte[] numArray = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(numArray, 0, numArray.Length);
            Assembly? assembly = null;
            try
            {
                assembly = Assembly.Load(numArray);
            }
            catch
            {
                if (!Directory.Exists(assemblyCacheDir))
                {
                    Directory.CreateDirectory(assemblyCacheDir);
                }
                string str1 = Path.Combine(assemblyCacheDir, new AssemblyName(args.Name).Name + ".dll");
                bool flag = true;
                string str2 = BitConverter.ToString(SHA1.HashData(numArray)).Replace("-", string.Empty);
                if (File.Exists(str1))
                {
                    byte[] buffer = File.ReadAllBytes(str1);
                    string str3 = BitConverter.ToString(SHA1.HashData(buffer)).Replace("-", string.Empty);
                    if (str2 == str3)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    File.WriteAllBytes(str1, numArray);
                }
                assembly = Assembly.LoadFrom(str1);
            }
            _assemblies[args.Name] = assembly;
            return assembly;
        }

        private void ExtractResourceAssemblies(IList<string> resources)
        {
            if (!Directory.Exists(assemblyCacheDir))
            {
                Directory.CreateDirectory(assemblyCacheDir);
            }
            foreach (string resource in (IEnumerable<string>)resources)
            {
                string name = string.Format("{0}.embeddeddlls.{1}", Assembly.GetExecutingAssembly().GetName().Name, resource);
                using var manifestResourceStream = GetType().Assembly.GetManifestResourceStream(name);
                if (manifestResourceStream == null)
                {
                    Console.WriteLine("Failed to find resource assembly with name {0}", name);
                }
                else
                {
                    byte[] numArray = new byte[manifestResourceStream.Length];
                    manifestResourceStream.Read(numArray, 0, numArray.Length);
                    string path = Path.Combine(assemblyCacheDir, resource);
                    bool flag = true;
                    string str1 = BitConverter.ToString(SHA1.HashData(numArray)).Replace("-", string.Empty);
                    if (File.Exists(path))
                    {
                        byte[] buffer = File.ReadAllBytes(path);
                        string str2 = BitConverter.ToString(SHA1.HashData(buffer)).Replace("-", string.Empty);
                        if (str1 == str2)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        File.WriteAllBytes(path, numArray);
                    }
                }
            }
        }
    }
}

