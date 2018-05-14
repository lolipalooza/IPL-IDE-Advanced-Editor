using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class Settings
    {
        public static string ini = "settings.ini";

        public static string default_raw = "" +
            "; IPL/IDE Advanced Editor - Settings File" + "\r\n" +
            "; =======================================" + "\r\n" +
            "; Add as many Maps as you want with names \"Map\"+i." + "\r\n" +
            "; If you need to restore default settings, just delete this file and run once again 'IPL/IDE Advanced Editor.exe'" + "\r\n" +
            "" + "\r\n" +
            "[General]" + "\r\n" +
            "DefaultSelected = 1" + "\r\n" +
            "" + "\r\n" +
            "[Map1]" + "\r\n" +
            "name = GTA III" + "\r\n" +
            "path = input" + "\r\n" +
            "" + "\r\n" +
            "";

        public static Dictionary<string, Dictionary<string, string>> Data;

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
            string raw = Editor.GetRaw(Settings.ini);
            string[] lines = Regex.Split(raw, "\r\n");
            int stat = 0;
            List<string> result = new List<string>();
            foreach(string line in lines)
            {
                if (line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("//")) continue;
                switch (stat)
                {
                    case 0:
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            string dummy = line.Substring(1, line.Length - 2);
                            if (dummy.Equals(entry))
                                stat = 1;
                        }
                        break;
                    case 1:
                        if (line.StartsWith("["))
                            stat = 2;
                        else if (line != String.Empty) result.Add(line);
                        break;
                }
            }
            return result.ToArray();
        }
        static public bool StoreInIni(string entry, string[] data)
        {
            string raw = Editor.GetRaw(Settings.ini);
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
                Editor.StoreRaw(Settings.ini, raw);
                return true;
            }
        }

        public static void Initialize()
        {
            Settings.Data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            string raw = Editor.GetRaw(Settings.ini);
            string[] lines = Regex.Split(raw, "\r\n");
            string section = "", key = "", value = "";
            foreach (string line in lines)
            {
                if (line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("//"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2);
                    Settings.Data.Add(section, new Dictionary<string, string>());
                }
                else if (line.Split('=').Length > 1)
                {
                    key = line.Split('=')[0].Trim();
                    value = line.Split('=')[1].Trim();

                    if (section != String.Empty)
                        Settings.Data[section].Add(key, value);
                }
            }
        }

        public static void Save()
        {
            List<string> save = new List<string>();
            string raw = Editor.GetRaw(Settings.ini);
            string[] lines = Regex.Split(raw, "\r\n");
            string section = "", key = "";
            foreach (string line in lines)
            {
                if (line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("//"))
                {
                    save.Add(line);
                }
                else if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2);
                    save.Add(line);
                }
                else if (line.Split('=').Length > 1)
                {
                    key = line.Split('=')[0].Trim();

                    if (section != String.Empty && key != String.Empty)
                        save.Add(String.Format("{0} = {1}", key, Settings.Data[section][key]));
                }
                else
                    save.Add(line);
            }
            Editor.StoreRaw(Settings.ini, String.Join("\r\n", save));
        }

        public static int GetDefaultSelected()
        {
            int defaultSelected;
            try
            {
                defaultSelected = Int32.Parse(Settings.Data["General"]["DefaultSelected"]) - 1;
            }
            catch
            {
                defaultSelected = 0;
            }
            return defaultSelected;
        }

        public static List<string> GetAllFilesFrom(string path, string filetype)
        {
            List<string> files = new List<string>();
            string[] subfiles = System.IO.Directory.GetFiles(path, filetype, SearchOption.AllDirectories);
            foreach (string file in subfiles)
                files.Add(file);
            return files;
        }
    }
}
