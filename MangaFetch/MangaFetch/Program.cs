﻿using System;
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
        
        static void Main(string[] args)
        {

            
            Console.WriteLine("The starter Volumn on www.177mh.net or comic.kukukkk.com");
            string URL = Console.ReadLine(); // "https://www.177mh.net/201301/239684.html";//"https://www.177mh.net/201902/409391.html";
            Console.WriteLine("Starts From:");
            string startPage = Console.ReadLine();
            if (URL.ToString().Contains("www.177mh.net"))
            {
                if (startPage.Equals(String.Empty))
                {
                    MangaSpiders.XXMHV2(URL);
                }
                else
                {
                    MangaSpiders.XXMHV2(URL,int.Parse(startPage));
                }
                
            }
            else if (URL.ToString().Contains("comic.kukukkk.com"))
            {

            }
        }
        
    }
}
