using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Slave
{
    public abstract class Utility
    {
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
        public static void Log(string logMessage, TextWriter w = null)
        {
            if (w != null)
            {
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine($"  :{logMessage}");
                w.WriteLine("-------------------------------");
            }
            Console.Out.WriteLine(logMessage);
        }
    }
}
