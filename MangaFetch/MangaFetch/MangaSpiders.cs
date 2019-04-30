using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace MangaFetch
{
    abstract class Utilities
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
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
        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
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
    }
    abstract class MangaSpiders
    {
        static bool visable = false;
        public static void XXMHSubVolumn(dynamic IE, string subFolder, ref float vol, WebClient webClient)
        {
            string pwd = Directory.GetCurrentDirectory();

            string logFile = Path.Combine(pwd, String.Format("MangaSpider.{0}.log", subFolder));
            string[] tabServer = { "tab_srv1", "tab_srv2", "tab_srv3", "tab_srv4", "tab_srv5" };

            Utilities.WaitForReady(IE);
            string mangaFolder = Path.Combine(pwd, subFolder);
            int serverIndex = 0;
            IE.Document.getElementById(tabServer[serverIndex]).click();
            Utilities.WaitForReady(IE);
            int totalPages = IE.Document.getElementsByClassName("selectTT")[0].getElementsByTagName("option").Length;
            string currentTitle = IE.Document.IHTMLDocument2_nameProp;
            string src = "";
            try
            {
                float newVol = float.Parse(Regex.Match(currentTitle, @"\d+").Value);
                vol = vol == newVol ? (vol += 0.1f) : (vol = newVol);
            }
            catch
            {
                vol += 0.1f;
            }
            string result = (vol / 1000).ToString().Replace(".", "");
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
            bool switchServer = false;
            string folder = "";
            //true -and 
            string fileName = "0";
            folder = String.Format(@"{0}\{1}", mangaFolder, currentTitle).Replace('?', '!');
            Directory.CreateDirectory(folder);
            bool skip = false;
            for (int index = 1; index<= totalPages; index++)
            {
                string page = ((index / 100.00).ToString()).Replace(".", "");
                if (page.Length == 2)
                {
                    page = page + "0";
                }
                serverIndex = 0;
                fileName = page;
                fileName = String.Format(folder + @"\{0}.jpg", result.ToString() + "_" + page.ToString());
                while (true)
                {
                    Utilities.WaitForReady(IE);
                    var element = IE.Document.getElementById("dracga");
                    src = element.src;
                    string logLine = src + " as " + fileName;
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
                            webClient.DownloadFile(src, fileName);
                            switchServer = false;
                            Console.Out.WriteLine(logLine);
                            using (StreamWriter w = File.AppendText(logFile))
                            {
                                Utilities.Log(logLine, w);
                            }
                        }
                        else
                        {
                            logLine = "skipped: " + src + " as " + fileName;
                            Console.Out.WriteLine("skipped: " + src + " as " + fileName);
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
        public static void XXMHV2(string URL)
        {
            var IE = new SHDocVw.InternetExplorer();
            IE.Visible = visable;
            IE.Navigate2(URL);
            string pwd = Directory.GetCurrentDirectory();
            Utilities.WaitForReady(IE);
            string subFolder = IE.Document.IHTMLDocument2_nameProp.ToString();
            subFolder = subFolder.Split(' ')[0];
            string savedataName = Path.Combine(pwd, String.Format("MangaSpider.{0}.dat", subFolder));
            object StartFrom = Utilities.ReadProcess(savedataName);
            if (StartFrom == null)
            {
                Dictionary<string, object> newManga = new Dictionary<string, object>();
                newManga["StartPage"] = 1;
                StartFrom = (object)newManga;
            }
            var TitleLi = IE.Document.getElementById("coclist1").getElementsByTagName("li");
            ((Dictionary<string, object>)StartFrom)["URL"] = URL;
            int StartPage = int.Parse(((Dictionary<string, object>)StartFrom)["StartPage"].ToString());
            int fatchSize = TitleLi.Length - StartPage + 1;
            string[] VolURLs = new string[fatchSize];
            int j = 0;
            for (int i = fatchSize-1; i >= 0; i--)
            {
                VolURLs[j] = TitleLi[i].getElementsByTagName("a")[0].IHTMLAnchorElement_href;
                j++;
            }
            float vol = 0.0f;
            WebClient webClient = new WebClient();
            webClient.Proxy = null;
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");
            webClient.Headers.Add("X-UA-Compatible", "IE=11");
            Utilities.CloseIE(subFolder);
            for (int i = 0; i< VolURLs.Length; i++)
            {
                IE.Navigate2(VolURLs[i]);
                ((Dictionary<string, object>)StartFrom)["StartPage"] = StartPage + i;
                Utilities.SaveProcess(StartFrom, savedataName);
                XXMHSubVolumn(IE, subFolder, ref vol, webClient);
            }
            webClient.Dispose();
            IE.Quit();
            Utilities.CloseIE(subFolder);
        }
        public static void KUKUKKK(dynamic IE, string subFolder)
        {

        }
    }
}
