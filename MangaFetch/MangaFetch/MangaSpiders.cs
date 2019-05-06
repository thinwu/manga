using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace MangaFetch
{

    class Reader
    {
        private static Thread inputThread;
        private static AutoResetEvent getInput, gotInput;
        private static string input;

        static Reader()
        {
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
            inputThread = new Thread(reader);
            inputThread.IsBackground = true;
            inputThread.Start();
        }

        private static void reader()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadLine();
                gotInput.Set();
            }
        }
        public static bool TryReadLine(out string line, int timeOutMillisecs = Timeout.Infinite)
        {
            getInput.Set();
            bool success = gotInput.WaitOne(timeOutMillisecs);
            if (success)
                line = input;
            else
                line = null;
            return success;
        }
    }
    abstract class Utilities
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
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
        public static void MainWindowPressEnter()
        {
            Process[] processes = Process.GetProcessesByName("iexplore");

            foreach (Process proc in processes)
            {
                SetForegroundWindow(proc.MainWindowHandle);
                SendKeys.SendWait("{ENTER}");


            }
        }
        public static void WaitForReady(dynamic IE, int generalSleep = 200)
        {
            while (IE.ReadyState != Convert.ToInt16(SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE))
            {
                Thread.Sleep(generalSleep);
            }
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
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");
                    webClient.Headers.Add("X-UA-Compatible", "IE=11");
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
    abstract class MangaSpiders
    {
#if DEBUG
        public static bool Visable = true;
#else
        public static bool Visable = false;
#endif

        static string WorkingDir = Directory.GetCurrentDirectory();
        private static void XXMHSubVolumn(dynamic IE, string URL, string subFolder, ref float vol)
        {
            string logFile = Path.Combine(WorkingDir, String.Format("MangaSpider.{0}.log", subFolder));
            string[] tabServer = { "tab_srv1", "tab_srv2", "tab_srv3", "tab_srv4", "tab_srv5" };

            Utilities.IENavigate2(IE, URL);
            string mangaFolder = Path.Combine(WorkingDir, subFolder);
            int serverIndex = 0;
            int totalPages = 0;
            foreach (string s in tabServer)
            {
                try
                {
                    IE.Document.getElementById(s).click();
                    Utilities.WaitForReady(IE);
                    totalPages = IE.Document.getElementsByClassName("selectTT")[0].getElementsByTagName("option").Length;
                    break;
                }
                catch
                {
                    continue;
                }
            }
            using (StreamWriter w = File.AppendText(logFile))
            {
                Utilities.Log($"{subFolder} has {totalPages} pages", w);
            }
            string currentTitle = IE.Document.IHTMLDocument2_nameProp;
            string src = "";
            string result = "";
            Utilities.CalVolResult(currentTitle, ref vol, ref result);
            bool switchServer = false;
            string folder = "";
            //true -and 
            string fileName = "0";
            folder = String.Format(@"{0}\{1}", mangaFolder, Utilities.GetProperFolderName(currentTitle));
            Directory.CreateDirectory(folder);
            bool skip = false;
            for (int index = 1; index <= totalPages; index++)
            {
                string page = Utilities.GetCalPage(index);
                serverIndex = 0;
                fileName = page;
                fileName = String.Format(folder + @"\{0}.jpg", result.ToString() + "_" + page);
                while (true)
                {
                    Utilities.WaitForReady(IE);
                    var element = IE.Document.getElementById("dracga");
                    src = element.src;
                    string logLine = $"saved {src} as {fileName}";
                    while (src.Contains("gif") || switchServer)
                    {
                        serverIndex++;
                        if (serverIndex < tabServer.Length)
                        {
                            IE.Document.getElementById(tabServer[serverIndex]).click();
                            Utilities.WaitForReady(IE);
                            element = IE.Document.getElementById("dracga");
                            src = element.src;
                            switchServer = false;
                        }
                        else
                        {
                            skip = true;
                            break;
                        }
                    }
                    try
                    {
                        if (!skip)
                        {
                            Utilities.WebClientDownloadFile(src, fileName);
                            switchServer = false;
                            using (StreamWriter w = File.AppendText(logFile))
                            {
                                Utilities.Log(logLine, w);
                            }
                        }
                        else
                        {
                            logLine = $"skipped: {src} as {fileName}";
                            using (StreamWriter w = File.AppendText(logFile + ".skppedList"))
                            {
                                Utilities.Log(logLine, w);
                            }
                        }
                        if (index < totalPages)
                        {
                            element.click();
                        }
                        serverIndex = 0;
                        break;
                    }
                    catch
                    {
                        switchServer = true;
                        continue;
                    }
                }
            }
        }
        public static void XXMHV2(string URL, Dictionary<string, object> savedata)
        {
            var IE = new SHDocVw.InternetExplorer();
            IE.Visible = Visable;
            Utilities.IENavigate2(IE, URL);
            string subFolder = IE.Document.IHTMLDocument2_nameProp.ToString();
            subFolder = Utilities.GetProperFolderName(subFolder.Split(' ')[0]);
            string savedataName = Utilities.GetSaveDataFullPath(WorkingDir, subFolder,URL);
            savedata = Utilities.GetNewSaveData(savedata, (Dictionary<string, object>)Utilities.ReadProcess(savedataName));
            var TitleLi = IE.Document.getElementById("coclist1").getElementsByTagName("li");
            int StartPage = int.Parse(savedata["StartPage"].ToString());
            int fatchSize = TitleLi.Length - StartPage + 1;
            fatchSize = fatchSize < 0 ? 0 : fatchSize;
            string[] VolURLs = new string[fatchSize];
            int j = 0;
            Utilities.Log("Collecting Volumns...");
            for (int i = fatchSize - 1; i >= 0; i--)
            {
                VolURLs[j] = TitleLi[i].getElementsByTagName("a")[0].IHTMLAnchorElement_href;
                j++;
            }
            float vol = 0.0f;
            for (int i = 0; i < VolURLs.Length; i++)
            {
                Utilities.Log($"Starting from Volumn: {VolURLs[i]}");
                XXMHSubVolumn(IE, VolURLs[i],subFolder, ref vol);
                savedata["StartPage"] = StartPage + i + 1;
                Utilities.SaveProcess(savedata, savedataName);
            }
            Utilities.WebClient.Dispose();
            IE.Quit();
            Utilities.CloseIE(subFolder);
        }

        public static void KUKUKKK(string URL, Dictionary<string, object> savedata)
        {
            var IE = new SHDocVw.InternetExplorer();
            IE.Visible = Visable;
            Utilities.IENavigate2(IE, URL);
            var TitleLi = IE.Document.getElementById("comiclistn").getElementsByTagName("dd");
            List<String> Hosts = new List<String>();
            foreach (var href in TitleLi[0].getElementsByTagName("a"))
            {
                string h = href.host;
                if (!Hosts.Contains(h))
                {
                    Hosts.Add(h);
                }
            }
            string host = Hosts[0];
            string subFolder = Utilities.GetProperFolderName(TitleLi[0].getElementsByTagName("a")[0].innerText.Split(' ')[0]);
            string savedataName = Utilities.GetSaveDataFullPath(WorkingDir, subFolder, URL);
            savedata = Utilities.GetNewSaveData(savedata, (Dictionary < string, object > )Utilities.ReadProcess(savedataName));
            int StartPage = int.Parse(savedata["StartPage"].ToString());
            int fatchSize = TitleLi.Length - StartPage + 1;
            fatchSize = fatchSize < 0 ? 0 : fatchSize;
            string[] VolURLs = new string[fatchSize];
            Utilities.Log("Collecting Volumns...");
            for (int i = 0; i < fatchSize; i++)
            {
                VolURLs[i] = TitleLi[StartPage + i - 1].getElementsByTagName("a")[0].pathname;
            }
            float vol = 0.0f;
            for (int i = 0; i < VolURLs.Length; i++)
            {
                Utilities.Log($"Starting from Volumn: {Hosts[0]}{VolURLs[i]}");
                KUKUSubVolumn(IE, $"{Hosts[0]}{VolURLs[i]}",subFolder, ref vol, Hosts);
                savedata["StartPage"] = StartPage + i + 1 ;
                Utilities.SaveProcess(savedata, savedataName);
            }
            Utilities.WebClient.Dispose();
            IE.Quit();
            Utilities.CloseIE(subFolder);
        }
        private static void KUKUSubVolumn(dynamic IE, string URL, string subFolder, ref float vol, List<String> Hosts)
        {
            string mangaFolder = Path.Combine(WorkingDir, subFolder);
            string logFile = Path.Combine(WorkingDir, $"MangaSpider.{subFolder}.log");
            Utilities.IENavigate2(IE, URL);
            string currentTitle = IE.Document.IHTMLDocument2_nameProp;
            string result = "";
            Utilities.CalVolResult(currentTitle, ref vol, ref result);
            int index = 1;
            string nextPage = "";
            while (!nextPage.Contains("exit"))
            {
                var table = IE.Document.getElementsByTagName("table")[1];
                var imgCell = table.getElementsByTagName("tbody")[0].getElementsByTagName("tr")[0].getElementsByTagName("td")[0];
                nextPage = imgCell.getElementsByTagName("a")[0].pathname;
                string currentPage = IE.LocationURL;
                string currentPathName = "";
                string src = imgCell.getElementsByTagName("a")[0].getElementsByTagName("img")[0].src;
                foreach (string h in Hosts)
                {
                    if (currentPage.Contains(h))
                    {
                        currentPathName = currentPage.Replace(h, "");
                        break;
                    }

                }
                string folder = "";
                string fileName = "0";
                folder = String.Format(@"{0}\{1}", mangaFolder, Utilities.GetProperFolderName(currentTitle));
                string page = Utilities.GetCalPage(index);
                fileName = String.Format(folder + @"\{0}.jpg", result.ToString() + "_" + page.ToString());
                Directory.CreateDirectory(folder);
                bool skip = false;
                string logLine = $"saved {src} as {fileName}";
                foreach (string h in Hosts)
                {
                    try
                    {
                        Utilities.WebClientDownloadFile(src, fileName);
                        using (StreamWriter w = File.AppendText(logFile))
                        {
                            Utilities.Log(logLine, w);
                        }
                        skip = false;
                        break;
                    }
                    catch
                    {
                        Utilities.IENavigate2(IE, $"{h}{currentPathName}");
                        table = IE.Document.getElementsByTagName("table")[1];
                        imgCell = table.getElementsByTagName("tbody")[0].getElementsByTagName("tr")[0].getElementsByTagName("td")[0];
                        src = imgCell.getElementsByTagName("a")[0].getElementsByTagName("img")[0].src;
                        skip = true;
                        continue;
                    }
                }
                if (skip)
                {
                    logLine = $"skipped: {src} as {fileName}";
                    using (StreamWriter w = File.AppendText(logFile + ".skppedList"))
                    {
                        Utilities.Log(logLine, w);
                    }
                }
                Utilities.IENavigate2(IE, $"{Hosts[0]}{nextPage}");
                index++;
            }
        }
    }
}
