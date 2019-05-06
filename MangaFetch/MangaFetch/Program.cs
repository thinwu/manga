using System;
using System.Collections.Generic;

namespace MangaFetch
{
    class Program
    {
        static void Main(string[] args)
        {
            string URL = null;
            Dictionary<string, object> savedata = new Dictionary<string, object>();
            if (args.Length == 1 && args[0].Contains(".dat"))
            {
                savedata = (Dictionary < string, object> )Utilities.ReadProcess(args[0]);
                URL = savedata["URL"].ToString();
            }
            if (URL == null)
            {
                Utilities.Log("The starter Volumn on www.177mh.net or comic.kukukkk.com，or .dat file path");
                string path = Console.ReadLine();
                if (path.Contains(".dat"))
                {
                    savedata = (Dictionary<string, object>)Utilities.ReadProcess(path);
                    URL = savedata["URL"].ToString();
                }else
                {
                    URL = path;
                }
                

            }
            Utilities.Log(String.Format("Processing The Volumns on {0}", URL));
            Utilities.Log("Set a Start volumn within the next 5 seconds.");
            if (!savedata.ContainsKey("StartPage"))
            {
                savedata["StartPage"] = 1;
                Utilities.Log("By default is the value in savedata or 1 if it doesn't exist.");
            }
            else
            {
                Utilities.Log(String.Format("By default is using the value {0} in savedata", savedata["StartPage"]));
            }
            string startPage;
            bool success = Reader.TryReadLine(out startPage, 5000);
            if (!success)
            {
                Utilities.Log("Waited too long, using the default value to proceed...");
            }
            else
            {
                if (!startPage.Equals(String.Empty))
                {
                    savedata["StartPage"] = int.Parse(startPage);
                }
            }
            Utilities.Log("Starting MangaSpider...");
            if (URL.ToString().Contains("177mh.net"))
            {
                MangaSpiders.XXMHV2(URL, savedata);
            }
            else if (URL.ToString().Contains("kukukkk.com"))
            {
                MangaSpiders.KUKUKKK(URL, savedata);
            }
        }
    }
}
