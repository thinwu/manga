using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MangaFetch
{
    abstract class Utilities
    {
        /*
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
        */
        private static WebClient webClient = null;
        public static void SaveProcess(object StartFrom, string savedataFullName)
        {
            using (Stream ms = File.OpenWrite(savedataFullName))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, StartFrom);
            }
        }
        public static object ReadProcess(string savedataFullName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            object obj = null;
            if (File.Exists(savedataFullName))
            {
                using (FileStream fs = File.Open(savedataFullName, FileMode.Open))
                {
                    obj = formatter.Deserialize(fs);
                }
            }
            return obj;

        }
        public static void Log(string logMessage, TextWriter w=null)
        {
            if(w != null)
            {
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine($"  :{logMessage}");
                w.WriteLine("-------------------------------");
            }
            Console.Out.WriteLine(logMessage);
        }
        public static void CloseIE(string titleLike)
        {
            Process[] processes = Process.GetProcessesByName("iexplore");

            foreach (Process proc in processes)
            {
                string title = proc.MainWindowTitle;
                if (titleLike.Equals(String.Empty))
                {
                    proc.CloseMainWindow();
                }
                else if (title.Contains(titleLike))
                {
                    proc.CloseMainWindow();
                    proc.Dispose();
                }

            }
        }
        /*
        public static void MainWindowPressEnter()
        {
            Process[] processes = Process.GetProcessesByName("iexplore");

            foreach (Process proc in processes)
            {
                SetForegroundWindow(proc.MainWindowHandle);
                SendKeys.SendWait("{ENTER}");


            }
        }*/
        public static void WaitForReady(dynamic IE, int generalSleep = 200)
        {
            while (IE.ReadyState != Convert.ToInt16(SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE))
            {
                IEWait();
            }
        }
        public static void IEWait(int generalSleep = 200)
        {
            Thread.Sleep(generalSleep);
        }
        public static void CalVolResult(string currentTitle, ref float vol, ref string result)
        {
            try
            {
                float newVol = float.Parse(Regex.Match(currentTitle, @"\d+").Value);
                vol = vol == newVol ? (vol += 0.1f) : (vol = newVol);
            }
            catch
            {
                vol += 0.1f;
            }
            result = (vol / 1000).ToString().Replace(".", "");
            if (result.Length == 4)
            {
                result = result + "0";
            }
            if (result.Length == 3)
            {
                result = result + "00";
            }
            if (result.Length == 2)
            {
                result = result + "000";
            }
        }
        public static string GetCalPage(int index)
        {
            string page = ((index / 100.00).ToString()).Replace(".", "");
            if (page.Length == 2)
            {
                page = page + "0";
            }
            return page;
        }
        public static string GetProperFolderName(string folderName)
        {
            return folderName.Replace(':', '：')
                .Replace('?', '？')
                .Replace('*', '×')
                .Replace('<', '《')
                .Replace('>', '》')
                .Replace('|', '-')
                .Replace('"', '“')
                .Replace("\\", "")
                .Replace("/", "");
        }
        public static string TrimURL(string URL)
        {
            Uri uri = new Uri(URL);
            return Utilities.GetProperFolderName(uri.AbsolutePath);
        }
        public static WebClient WebClient
        {
            get
            {
                if (webClient == null)
                {
                    webClient = new WebClient();
                    webClient.Proxy = null;
                }
                return webClient;
            }
        }
        public static string GetSaveDataFullPath(string workingDir, string MangaName, string URL)
        {
            return Path.Combine(workingDir, String.Format("{1}.MangaSpider.{0}.dat", MangaName, Utilities.TrimURL(URL)));
        }
        public static Dictionary<string, object> GetNewSaveData(Dictionary<string, object> memData, Dictionary<string, object> diskData)
        {
            if (diskData != null)
            {
                if (int.Parse(memData["StartPage"].ToString()) == 1 && (diskData.ContainsKey("StartPage") && int.Parse(diskData["StartPage"].ToString()) != 1))
                {
                    memData["StartPage"] = diskData["StartPage"];
                }
            }
            
            return memData;
        }
        public static void IENavigate2(dynamic IE, string URL)
        {
            IE.Navigate2(URL);
            Utilities.WaitForReady(IE);
        }
        public static void WebClientDownloadFile(string src, string fileFullPath)
        {
            string tempName = $"{fileFullPath}.temp";
            WebClient.DownloadFile(src, tempName);
            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
            }
            File.Move(tempName, fileFullPath);
        }
    }
}
