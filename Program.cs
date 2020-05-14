using System;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using System.Linq;

namespace FTPow
{
    class Program
    {
        // Entrance
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            else
            {
                // Encoding Registration
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Get Binary Path
                string binaryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                
                // Parse User Configuration
                UConfig uconfig = LoadConfig(Path.Combine(binaryPath, "config.json"));

                // Get File URL
                string URL = args[0];

                // Parse Username and Password
                Match m = Regex.Match(URL, @"^ftp://(?<fore>(?<username>.*?)?(?:\:(?<password>.*?)?)?@)?(?<back>(?<domain>.+?)(?:/|$).*$)");

                if (m.Success)
                {
                    string username = m.Groups["username"].Value;
                    string domain = m.Groups["domain"].Value;
                    string back = m.Groups["back"].Value;

                    foreach (ServerItem server in uconfig.servers)
                    {
                        // Reconstruct URL
                        if(Uri.UnescapeDataString(username) == server.username && Uri.UnescapeDataString(domain) == server.address)
                        {
                            if (server.username == "")
                            {
                                URL = string.Format(@"ftp://{0}", back);
                            }
                            else if (server.password == "")
                            {
                                URL = string.Format(@"ftp://{0}@{1}", username, back);
                            }
                            else
                            {
                                URL = string.Format(@"ftp://{0}:{1}@{2}", username, Uri.EscapeDataString(server.password), back);
                            }
                            break;
                        }
                    }
                    
                    // Get File Name Extension
                    string ext = Regex.Replace(Path.GetExtension(URL), @"^\.([^\?]+)", "$1");
                    
                    // Filter and Execute
                    foreach (AppItem item in uconfig.apps)
                    {
                        if (item.extList.Any(s => s.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Decode URL
                            URL = DecodeURL(item, URL);

                            // Append Query String
                            URL += "?" + item.queryString;

                            // Execute
                            ExecuteApplication(item.programPath, string.Join(URL, item.command));

                            // Return after Execution
                            return;
                        }
                    }
                }

                // Fallback Execution
                URL = DecodeURL(uconfig.fallback, URL);
                URL = Uri.EscapeUriString(URL);
                ExecuteApplication(uconfig.fallback.programPath, string.Join(URL, uconfig.fallback.command));
                return;
            }
        }

        // Decode URL
        static string DecodeURL(AppItem item, string URL)
        {
            // Get Encoding Type
            Encoding e = EncodingType[item.decode.ToUpper()];

            // "+" Decode
            if (!item.decodePlus)
            {
                string oldPathAndQuery = new Uri(URL).PathAndQuery;
                string newPathAndQuery = oldPathAndQuery.Replace("+", "%2B");
                URL = URL.Replace(oldPathAndQuery, newPathAndQuery);
            }

            // Decode
            if (e != null)
            {
                URL = HttpUtility.UrlDecode(URL, e);
            }
            return URL;
        }

        // Load Json Configuration
        static UConfig LoadConfig(string fileName)
        {
            using (StreamReader r = new StreamReader(fileName))
            {
                string json = r.ReadToEnd();
                UConfig uconfig = JsonConvert.DeserializeObject<UConfig>(json);
                return uconfig;
            }
        }
        // Execute Application
        static void ExecuteApplication(string path, string command)
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = path;
                pProcess.StartInfo.Arguments = command;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true;
                pProcess.Start();
                pProcess.WaitForExit();
            }
        }
        // User Configuration Class Definition
        public class UConfig
        {
            public List<ServerItem> servers;
            public List<AppItem> apps;
            public AppItem fallback;
        }
        // FTP Server Item Class Definition
        public class ServerItem
        {
            public string address;
            public string username;
            public string password;
        }
        // Application Item Class Definition
        public class AppItem
        {
            public string programPath;
            public List<string> command;
            public List<string> extList;
            public string queryString;
            public bool decodePlus;
            public string decode;
        }

        public static Dictionary<string, Encoding> EncodingType = new Dictionary<string, Encoding>
        {
            {"", null },
            {"NONE", null },
            {"GB2312", Encoding.GetEncoding("GB2312") },
            {"UTF8", Encoding.UTF8 },
            {"UTF32", Encoding.UTF32 },
            {"ASCII", Encoding.ASCII },
            {"AUTO", Encoding.Default },
            {"DEFAULT", Encoding.Default }
        };
    }
}
