using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class Settings
    {
        public static int Entry;
        public static string Ipl, Ide, Map;
        public static void UpdateSettings()
        {
            Settings.Map = "Map" + Settings.Entry.ToString();
            Settings.Ipl = "Ipl" + Settings.Entry.ToString();
            Settings.Ide = "Ide" + Settings.Entry.ToString();
        }
        static public string[] GetFromIni(string entry)
        {
            string raw = Archivos.GetRaw2(Archivos.ini);
            string[] line = Regex.Split(raw, "\r\n");
            int stat = 0, start_index = 0, end_index = 0;
            string[] result = new string[0];
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i].StartsWith(";") || line[i].StartsWith("#") || line[i].StartsWith("//")) continue;
                switch (stat)
                {
                    case 0:
                        if (line[i].StartsWith("[") && line[i].EndsWith("]"))
                        {
                            string dummy = line[i].Substring(1, line[i].Length - 2);
                            if (dummy.Equals(entry))
                            {
                                stat = 1;
                                start_index = i + 1;
                            }
                        }
                        break;
                    case 1:
                        if (line[i].StartsWith("[") || i == line.Length - 1)
                        {
                            stat = 2;
                            end_index = --i;
                        }
                        break;
                    case 2:
                        result = new string[end_index - start_index + 1];
                        for (int j = 0; j < (end_index - start_index + 1); j++)
                        {
                            if (
                                !line[j + start_index].StartsWith(";") && !line[j + start_index].StartsWith("#") &&
                                !line[j + start_index].StartsWith("//") && !line[j + start_index].Equals("")
                                )
                                result[j] = line[j + start_index];
                        }
                        stat = 3;
                        break;
                }
            }
            return result;
        }
        static public bool StoreInIni(string entry, string[] data)
        {
            string raw = Archivos.GetRaw2(Archivos.ini);
            string[] line = Regex.Split(raw, "\r\n");
            List<string> list = new List<string>();
            int stat = 0;
            for (int i = 0; i < line.Length; i++)
            {
                switch (stat)
                {
                    case 0:
                        list.Add(line[i]);
                        if (line[i].StartsWith("[") && line[i].EndsWith("]"))
                        {
                            string dummy = line[i].Substring(1, line[i].Length - 2);
                            if (dummy.Equals(entry)) stat = 1;
                        }
                        break;
                    case 1:
                        for (int j = 0; j < data.Length; j++)
                            list.Add(data[j]);
                        stat = 2;
                        break;
                    case 2:
                        if (line[i].StartsWith("[") && line[i].EndsWith("]"))
                        {
                            stat = 3;
                            list.Add(line[i]);
                        }
                        break;
                    case 3:
                        list.Add(line[i]);
                        break;
                }
            }
            if (stat == 0) return false;
            else
            {
                raw = string.Join("\r\n", list);
                Archivos.StoreRaw(Archivos.ini, raw);
                return true;
            }
        }
    }
}
