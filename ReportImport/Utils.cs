using System;
using System.Globalization;
using System.IO;

namespace ReportImport
{
    static class Utils
    {
        static string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmm")}-log.txt";

        public static void Log(string msg)
        {
            File.AppendAllText(fileName, $"{DateTime.Now.ToString()} : {msg}{Environment.NewLine}");
            Console.WriteLine(msg);
        }

        public static int ToInt(string s)
        {
            int i;
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
                return i;
            throw new ArgumentException($"ToInt failed {s}");
        }

        public static double ToDouble(string s)
        {
            double d;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return d;
            throw new ArgumentException($"ToDouble failed {s}");
        }
    }
}
