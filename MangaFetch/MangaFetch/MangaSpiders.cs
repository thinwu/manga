using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Slave;

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
            Console.Title = subFolder;
            Web.IENavigate2(IE, URL);
            string mangaFolder = Path.Combine(WorkingDir, subFolder);
            int serverIndex = 0;
            int totalPages = 0;
            foreach (string s in tabServer)
            {
                try
                {
                    IE.Document.getElementById(s).click();
                    if (Visable)
                    {
                        Console.Out.WriteLine(s);
                    }
                    Web.WaitForReady(IE);
                    totalPages = IE.Document.getElementsByClassName("selectTT")[0].getElementsByTagName("option").Length;
                    if (Visable)
                    {
                        Console.Out.WriteLine(totalPages);
                    }
                    break;
                }
                catch
                {
                    continue;
                }
            }
            
            string currentTitle = IE.Document.IHTMLDocument2_nameProp;
            using (StreamWriter w = File.AppendText(logFile))
            {
                Utility.Log($"{currentTitle} has {totalPages} pages, {URL}", w);
            }
            string src = "";
            string result = "";
            Web.CalVolResult(currentTitle, ref vol, ref result);
            bool switchServer = false;
            string folder = "";
            //true -and 
            string fileName = "0";
            folder = String.Format(@"{0}\{1}", mangaFolder, Web.GetProperFolderName(currentTitle));
            Directory.CreateDirectory(folder);
            bool skip = false;
            for (int index = 1; index <= totalPages; index++)
            {
                string page = Web.GetCalPage(index);
                serverIndex = 0;
                fileName = page;
                fileName = String.Format(folder + @"\{0}.jpg", result.ToString() + "_" + page);
                while (true)
                {
                    Web.WaitForReady(IE);
                    var element = IE.Document.getElementById("dracga");
                    src = element.src;
                    string logLine = $"saved {src} as {fileName}";
                    while (src.Contains("gif") || switchServer)
                    {
                        serverIndex++;
                        if (serverIndex < tabServer.Length)
                        {
                            IE.Document.getElementById(tabServer[serverIndex]).click();
                            Web.WaitForReady(IE);
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
                            Web.WebClientDownloadFile(src, fileName);
                            switchServer = false;
                            using (StreamWriter w = File.AppendText(logFile))
                            {
                                Utility.Log(logLine, w);
                            }
                        }
                        else
                        {
                            logLine = $"{currentTitle}skipped: {src} as {fileName}, address {URL}";
                            using (StreamWriter w = File.AppendText(logFile + ".skppedList"))
                            {
                                Utility.Log(logLine, w);
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
            var IE = Web.IEObj;
            IE.Visible = Visable;
            Web.IENavigate2(IE, URL);
            string subFolder = IE.Document.IHTMLDocument2_nameProp.ToString();
            subFolder = Web.GetProperFolderName(subFolder.Split(' ')[0]);
            string savedataName = Web.GetSaveDataFullPath(WorkingDir, subFolder,URL);
            savedata = Web.GetNewSaveData(savedata, (Dictionary<string, object>)Utility.ReadProcess(savedataName));
            var TitleLi = IE.Document.getElementById("coclist1").getElementsByTagName("li");
            int StartPage = int.Parse(savedata["StartPage"].ToString());
            int fatchSize = TitleLi.Length - StartPage + 1;
            fatchSize = fatchSize < 0 ? 0 : fatchSize;
            string[] VolURLs = new string[fatchSize];
            int j = 0;
            Utility.Log("Collecting Volumns...");
            for (int i = fatchSize - 1; i >= 0; i--)
            {
                VolURLs[j] = TitleLi[i].getElementsByTagName("a")[0].IHTMLAnchorElement_href;
                j++;
            }
            float vol = 0.0f;
            for (int i = 0; i < VolURLs.Length; i++)
            {
                Utility.Log($"Starting from Volumn: {VolURLs[i]}");
                XXMHSubVolumn(IE, VolURLs[i],subFolder, ref vol);
                savedata["StartPage"] = StartPage + i + 1;
                Utility.SaveProcess(savedata, savedataName);
            }
            Web.WebClient.Dispose();
            IE.Quit();
            Web.CloseIE(subFolder);
        }

        public static void KUKUKKK(string URL, Dictionary<string, object> savedata)
        {
            var IE = Web.IEObj;
            IE.Visible = Visable;
            Web.IENavigate2(IE, URL);
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
            string subFolder = Web.GetProperFolderName(TitleLi[0].getElementsByTagName("a")[0].innerText.Split(' ')[0]);
            string savedataName = Web.GetSaveDataFullPath(WorkingDir, subFolder, URL);
            savedata = Web.GetNewSaveData(savedata, (Dictionary < string, object > )Utility.ReadProcess(savedataName));
            int StartPage = int.Parse(savedata["StartPage"].ToString());
            int fatchSize = TitleLi.Length - StartPage + 1;
            fatchSize = fatchSize < 0 ? 0 : fatchSize;
            string[] VolURLs = new string[fatchSize];
            Utility.Log("Collecting Volumns...");
            for (int i = 0; i < fatchSize; i++)
            {
                VolURLs[i] = TitleLi[StartPage + i - 1].getElementsByTagName("a")[0].pathname;
            }
            float vol = 0.0f;
            for (int i = 0; i < VolURLs.Length; i++)
            {
                Utility.Log($"Starting from Volumn: {Hosts[0]}{VolURLs[i]}");
                KUKUSubVolumn(IE, $"{Hosts[0]}{VolURLs[i]}",subFolder, ref vol, Hosts);
                savedata["StartPage"] = StartPage + i + 1 ;
                Utility.SaveProcess(savedata, savedataName);
            }
            Web.WebClient.Dispose();
            IE.Quit();
            Web.CloseIE(subFolder);
        }
        private static void KUKUSubVolumn(dynamic IE, string URL, string subFolder, ref float vol, List<String> Hosts)
        {
            string mangaFolder = Path.Combine(WorkingDir, subFolder);
            Console.Title = subFolder;
            string logFile = Path.Combine(WorkingDir, $"MangaSpider.{subFolder}.log");
            Web.IENavigate2(IE, URL);
            string currentTitle = IE.Document.IHTMLDocument2_nameProp;
            string result = "";
            Web.CalVolResult(currentTitle, ref vol, ref result);
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
                folder = String.Format(@"{0}\{1}", mangaFolder, Web.GetProperFolderName(currentTitle));
                string page = Web.GetCalPage(index);
                fileName = String.Format(folder + @"\{0}.jpg", result.ToString() + "_" + page.ToString());
                Directory.CreateDirectory(folder);
                bool skip = false;
                string logLine = $"saved {src} as {fileName}";
                foreach (string h in Hosts)
                {
                    try
                    {
                        Web.WebClientDownloadFile(src, fileName);
                        using (StreamWriter w = File.AppendText(logFile))
                        {
                            Utility.Log(logLine, w);
                        }
                        skip = false;
                        break;
                    }
                    catch
                    {
                        Web.IENavigate2(IE, $"{h}{currentPathName}");
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
                        Utility.Log(logLine, w);
                    }
                }
                Web.IENavigate2(IE, $"{Hosts[0]}{nextPage}");
                index++;
            }
        }
    }
}
