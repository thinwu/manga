using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Configuration;

namespace Slave
{
    public abstract class Utility
    {
        public static dynamic _settings = null;
        public static dynamic Configs
        {
            get
            {
                try
                {
                    if(_settings == null)
                    {
                        _settings = ConfigurationManager.AppSettings;
                        return _settings;
                    }
                    return _settings;
                    
                }
                catch (Exception e)
                {
                    Console.Out.Write("An error occurred while reading the configuration file.", e);
                }
                return null;
            }
        }
        public static void SaveProcess(object StartFrom, string savedataFullName)
        {
            using (Stream ms = File.OpenWrite(savedataFullName))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, StartFrom);
            }
        }
        public static void ConsoleOut(string log, bool trace = false)
        {
            if (trace)
            {
                Trace.WriteLine(log);
            }
            else
            {
                Console.Out.WriteLine(log);
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
        public static void Log(string logMessage, TextWriter w = null )
        {
            if (w != null)
            {
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine($"  :{logMessage}");
                w.WriteLine("-------------------------------");
            }
            ConsoleOut(logMessage);
        }
    }
}
