using System;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace MangaFetch
{
    class Program
    {
        static void Main(string[] args)
        {
            WebRequest.DefaultWebProxy = null;
            string URL = null;
            Dictionary<string, object> savedata = null;
            if (args.Length == 1 && args[0].Contains(".dat"))
            {
                savedata = (Dictionary < string, object> )Utilities.ReadProcess(args[0]);
                URL = savedata["URL"].ToString();
            }
            if (URL == null)
            {
                Console.WriteLine("The starter Volumn on www.177mh.net or comic.kukukkk.com");
                URL = Console.ReadLine(); // "https://www.177mh.net/201301/239684.html";//"https://www.177mh.net/201902/409391.html";
            }
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
