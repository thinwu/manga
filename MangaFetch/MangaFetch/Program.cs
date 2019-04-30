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
            if (args.Length == 1 && args[0].Contains(".dat"))
            {
                Dictionary<string, object> StartFrom = (Dictionary < string, object> )Utilities.ReadProcess(args[0]);
                URL = StartFrom["URL"].ToString();
            }
            if (URL == null)
            {
                Console.WriteLine("The starter Volumn on www.177mh.net or comic.kukukkk.com");
                URL = Console.ReadLine(); // "https://www.177mh.net/201301/239684.html";//"https://www.177mh.net/201902/409391.html";
            }
            if (URL.ToString().Contains("www.177mh.net"))
            {
                MangaSpiders.XXMHV2(URL);
            }
            else if (URL.ToString().Contains("comic.kukukkk.com"))
            {

            }
        }
    }
}
