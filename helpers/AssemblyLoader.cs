using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LogitechBatteryIndicator.helpers
{
    public sealed class AssemblyLoader : IDisposable
    {
        private static readonly IList<string> _logiNethidppioDependentAssemblies = (IList<string>)new ReadOnlyCollection<string>((IList<string>)
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
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly string assemblyCacheDir;

        public AssemblyLoader(string assemblyCacheDir)
        {
            this.assemblyCacheDir = assemblyCacheDir;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
            GC.SuppressFinalize((object)this);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (this._assemblies.ContainsKey(args.Name))
                return this._assemblies[args.Name];
            string name1 = string.Empty;
            string name2 = new AssemblyName(args.Name).Name;
            if (name2 == this.GetType().Assembly.GetName().Name + ".resources")
            {
                string cultureName = new AssemblyName(args.Name).CultureName;
                if (!string.IsNullOrEmpty(cultureName))
                    name1 = string.Format("{0}.embeddeddlls.Resources_{1}.dll", (object)Assembly.GetExecutingAssembly().GetName().Name, (object)cultureName);
            }
            if (string.IsNullOrEmpty(name1))
                name1 = string.Format("{0}.embeddeddlls.{1}.dll", (object)Assembly.GetExecutingAssembly().GetName().Name, (object)name2);
            if (name2 == "logi_nethidppio")
                this.extractResourceAssemblies(AssemblyLoader._logiNethidppioDependentAssemblies);
            using (Stream manifestResourceStream = this.GetType().Assembly.GetManifestResourceStream(name1))
            {
                if (manifestResourceStream == null)
                    return (Assembly)null;
                byte[] numArray = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(numArray, 0, numArray.Length);
                Assembly assembly = (Assembly)null;
                try
                {
                    assembly = Assembly.Load(numArray);
                }
                catch
                {
                    if (!Directory.Exists(this.assemblyCacheDir))
                        Directory.CreateDirectory(this.assemblyCacheDir);
                    string str1 = Path.Combine(this.assemblyCacheDir, new AssemblyName(args.Name).Name + ".dll");
                    bool flag = true;
                    using (SHA1CryptoServiceProvider cryptoServiceProvider = new SHA1CryptoServiceProvider())
                    {
                        string str2 = BitConverter.ToString(cryptoServiceProvider.ComputeHash(numArray)).Replace("-", string.Empty);
                        if (File.Exists(str1))
                        {
                            byte[] buffer = File.ReadAllBytes(str1);
                            string str3 = BitConverter.ToString(cryptoServiceProvider.ComputeHash(buffer)).Replace("-", string.Empty);
                            if (str2 == str3)
                                flag = false;
                        }
                        if (flag)
                            File.WriteAllBytes(str1, numArray);
                        assembly = Assembly.LoadFrom(str1);
                    }
                }
                this._assemblies[args.Name] = assembly;
                return assembly;
            }
        }

        private void extractResourceAssemblies(IList<string> resources)
        {
            if (!Directory.Exists(this.assemblyCacheDir))
                Directory.CreateDirectory(this.assemblyCacheDir);
            foreach (string resource in (IEnumerable<string>)resources)
            {
                string name = string.Format("{0}.embeddeddlls.{1}", (object)Assembly.GetExecutingAssembly().GetName().Name, (object)resource);
                using (Stream manifestResourceStream = this.GetType().Assembly.GetManifestResourceStream(name))
                {
                    if (manifestResourceStream == null)
                    {
                        Console.WriteLine("Failed to find resource assembly with name {0}", (object)name);
                    }
                    else
                    {
                        byte[] numArray = new byte[manifestResourceStream.Length];
                        manifestResourceStream.Read(numArray, 0, numArray.Length);
                        string path = Path.Combine(this.assemblyCacheDir, resource);
                        bool flag = true;
                        using (SHA1CryptoServiceProvider cryptoServiceProvider = new SHA1CryptoServiceProvider())
                        {
                            string str1 = BitConverter.ToString(cryptoServiceProvider.ComputeHash(numArray)).Replace("-", string.Empty);
                            if (File.Exists(path))
                            {
                                byte[] buffer = File.ReadAllBytes(path);
                                string str2 = BitConverter.ToString(cryptoServiceProvider.ComputeHash(buffer)).Replace("-", string.Empty);
                                if (str1 == str2)
                                    flag = false;
                            }
                            if (flag)
                                File.WriteAllBytes(path, numArray);
                        }
                    }
                }
            }
        }
    }
}

