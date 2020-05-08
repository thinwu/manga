using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Slave
{
    public abstract class Web
    {
        /*
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
        */
        private static WebClient webClient = null;
        private static dynamic _ie = null;
#if DEBUG
        public static bool Visible = true;
#else
        public static bool Visible = false;
#endif
        public static dynamic IE
        {
            get
            {
                if(_ie == null)
                {
                    _ie = new SHDocVw.InternetExplorerClass();
                    _ie.Visible = Visible;
                }
                return _ie;
            }
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
        public static void WaitForReady(int generalSleep = 200)
        {
            while (Convert.ToInt16(IE.ReadyState) != Convert.ToInt16(SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE))
            {
                IEWait();
            }
            foreach (var a in IE.Document.IHTMLDocument2_all)
            {
                //to prevent lazy load
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
            return Web.GetProperFolderName(uri.AbsolutePath);
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
            return Path.Combine(workingDir, String.Format("{1}.MangaSpider.{0}.dat", MangaName, Web.TrimURL(URL)));
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
        public static void IENavigate2(object URL)
        {
            object nullObj = String.Empty;
            Web.IE.Navigate2(ref URL, ref nullObj, ref nullObj, ref nullObj, ref nullObj);
            Web.WaitForReady();
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
