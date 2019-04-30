using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace MangaFetch
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }
        public static void MainWindowPressEnter(Process[] processes)
        {
            processes = Process.GetProcessesByName("iexplore");

            foreach (Process proc in processes)
            {
                SetForegroundWindow(proc.MainWindowHandle);
                SendKeys.SendWait("{ENTER}");


            }
        }
        static void Main(string[] args)
        {
            Process[] processes = Process.GetProcessesByName("iexplore");

            foreach (Process proc in processes)
            {
                proc.Close();
            }
            var IE = new SHDocVw.InternetExplorer();
            Console.WriteLine("The starter Volumn on www.177mh.net");
            object URL = Console.ReadLine(); // "https://www.177mh.net/201301/239684.html";//"https://www.177mh.net/201902/409391.html";
            Console.WriteLine("Folder to place the Volumns, default is 'Manga'");
            string subFolder = Console.ReadLine();
            subFolder = subFolder == String.Empty ? "Manga" : subFolder;
            IE.Visible = true;
            IE.Navigate2(ref URL);
            int generalSleep = 200;
            string pwd = Directory.GetCurrentDirectory();
            string mangaFolder = Path.Combine(pwd, subFolder);
            string logFile = Path.Combine(pwd, "MangaSpider.log");
            string[] tabServer = { "tab_srv1", "tab_srv2", "tab_srv3", "tab_srv4", "tab_srv5"};
            
            while (IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
            {
                Thread.Sleep(generalSleep);
            }
            int serverIndex = 0;
            IE.Document.getElementById(tabServer[serverIndex]).click();
            while (IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
            {
                Thread.Sleep(generalSleep);
            }
            string currentTitle = "";
            int index = 1;
            int retry = 5;
            string src = "";
            WebClient webClient = new WebClient();
            string olderURL = "";
            float vol = 0.0f;

            bool switchServer = false;
            string folder = "";
            //true -and 
            while (retry>0)
            {
                string fileName="0";
                bool taskResult = false;
                Thread.Sleep(2);
                if (switchServer)
                {
                    if (serverIndex >= 5)
                    {
                        using (StreamWriter w = File.AppendText(logFile))
                        {
                            Log("running out of switching servers", w);
                            using (StreamWriter imgBroken = File.AppendText(fileName + "." + "broken"))
                            {
                                Log("running out of switching servers", w);
                            }   
                        }
                        taskResult = Task.Run(() => { IE.Document.getElementById("dracga").click(); }).Wait(3000);
                        if (!taskResult)
                        {
                            MainWindowPressEnter(processes);
                        }
                    }
                    else
                    {
                        IE.Document.getElementById(tabServer[serverIndex]).click();
                        while (IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                        {
                            Thread.Sleep(generalSleep);
                        }
                        serverIndex++;
                    }
                    
                }
                Thread.Sleep(generalSleep);
                if (!currentTitle.Equals(IE.Document.IHTMLDocument2_nameProp)){
                    currentTitle = IE.Document.IHTMLDocument2_nameProp;
                    serverIndex = 0;
                    try
                    {
                        float newVol = float.Parse(Regex.Match(currentTitle, @"\d+").Value);
                        vol = vol == newVol ? (vol += 0.1f) : (vol = newVol);
                    }
                    catch
                    {
                        vol += 0.1f;
                    }
                    index = 1;
                }
                
                string result = (vol / 1000).ToString().Replace(".", "");
                string page = ((index / 100.00).ToString()).Replace(".", "");
                if (page.Length == 2){
                    page = page + "0";
                 }
                if (result.Length == 4){
                    result = result + "0";
                }
                if (result.Length == 3){
                    result = result + "00";
                }
                if (result.Length == 2){
                    result = result + "000";
                }
                folder = String.Format(@"{0}\{1}", mangaFolder, currentTitle + "_" + result.ToString()).Replace('?', '!');
                System.IO.Directory.CreateDirectory(folder);
                var element = IE.Document.getElementById("dracga");
                fileName = page;
                fileName = String.Format(folder+@"\{0}.jpg", result.ToString()+"_"+page.ToString());
                if ( !src.Equals( element.src) || switchServer ){
                    src = element.src;
                    bool skip = false;
                    if (src.Contains("gif"))
                    {
                        skip = true;
                    }
                    while (true)
                    {
                        try
                        {
                            
                            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");
                            webClient.Headers.Add("X-UA-Compatible", "IE=11");
                            string logLine = src + " as " + fileName;
                            if (!skip)
                            {
                                webClient.DownloadFile(src, fileName);
                                Console.Out.WriteLine(logLine);
                                using (StreamWriter w = File.AppendText(logFile))
                                {
                                    Log(logLine, w);
                                }
                            }
                            else
                            {
                                logLine = "skipped: " + src + " as " + fileName;
                                Console.Out.WriteLine("skipped: " + src + " as " + fileName);
                                using (StreamWriter w = File.AppendText(logFile+".skppedList"))
                                {
                                    Log(logLine, w);
                                }
                            }
                            
                            switchServer = false;
                            serverIndex = 0;
                            if (olderURL == IE.LocationURL)
                            {
                                retry--;
                            }
                            else
                            {
                                retry = 5;//reset
                                olderURL = IE.LocationURL;
                            }
                            olderURL = IE.LocationURL;
                            taskResult = Task.Run(() => { element.click(); }).Wait(3000);
                            while (IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                            {
                                Thread.Sleep(2);
                            }
                            if (!taskResult)
                            {
                                while (olderURL == IE.LocationURL && retry > 0)
                                {
                                    MainWindowPressEnter(processes);
                                    while (IE.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                                    {
                                        Thread.Sleep(generalSleep);
                                    }
                                    Thread.Sleep(generalSleep);
                                    retry--;
                                    index = 1;

                                }
                            }
                            else
                            {
                                index = index + 1;
                            }
                            break;

                        }
                        catch
                        {
                            switchServer = true;
                            break;
                        }
                    }
                    
                }
            }
        }
    }
}
