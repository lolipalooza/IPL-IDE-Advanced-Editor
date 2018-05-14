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
        public static string ini = "settings.ini";

        public static string default_raw = "" +
            "; IPL/IDE Advanced Editor - Settings File" + "\r\n" +
            "; =======================================" + "\r\n" +
            "; Add as many Maps as you want with names \"Map\"+i, \"Ide\"+i, \"Ipl\"+i." + "\r\n" +
            "; You need to register here relative or absoulte addresses of each Ide/Ipl file you want to process." + "\r\n" +
            "; If you need to restore default settings, just delete this file and run once again 'IPL/IDE Advanced Editor.exe'" + "\r\n" +
            "" + "\r\n" +
            "[DefaultEntryPath]" + "\r\n" +
            "input" + "\r\n" +
            "" + "\r\n" +
            "[DefaultEntry]" + "\r\n" +
            "1" + "\r\n" +
            "" + "\r\n" +
            "[Map1]" + "\r\n" +
            "GTA: Liberty City Stories Map" + "\r\n" +
            "" + "\r\n" +
            "[Ide1]" + "\r\n" +
            "lcs\\commer\\commer.ide" + "\r\n" +
            "lcs\\indust\\indust.ide" + "\r\n" +
            "lcs\\suburb\\suburb.ide" + "\r\n" +
            "lcs\\underg\\underg.ide" + "\r\n" +
            "" + "\r\n" +
            "[Ipl1]" + "\r\n" +
            "lcs\\commer\\comNbtm.ipl" + "\r\n" +
            "lcs\\commer\\comNtop.ipl" + "\r\n" +
            "lcs\\commer\\comSE.ipl" + "\r\n" +
            "lcs\\commer\\comSW.ipl" + "\r\n" +
            "lcs\\indust\\industNE.ipl" + "\r\n" +
            "lcs\\indust\\industNW.ipl" + "\r\n" +
            "lcs\\indust\\industSE.ipl" + "\r\n" +
            "lcs\\indust\\industSW.ipl" + "\r\n" +
            "lcs\\indust\\props.ipl" + "\r\n" +
            "lcs\\suburb\\landne.ipl" + "\r\n" +
            "lcs\\suburb\\landsw.ipl" + "\r\n" +
            "lcs\\underg\\underg.ipl" + "\r\n" +
            "lcs\\underg\\overview.ipl" + "\r\n" +
            "" + "\r\n" +
            "[Map2]" + "\r\n" +
            "GTA: Vice City Stories Map" + "\r\n" +
            "" + "\r\n" +
            "[Ide2]" + "\r\n" +
            "vcs\\beach.ide" + "\r\n" +
            "vcs\\mainla.ide" + "\r\n" +
            "vcs\\mall.ide" + "\r\n" +
            "" + "\r\n" +
            "[Ipl2]" + "\r\n" +
            "vcs\\airport.ipl" + "\r\n" +
            "vcs\\airportN.ipl" + "\r\n" +
            "vcs\\bridge.ipl" + "\r\n" +
            "vcs\\cisland.ipl" + "\r\n" +
            "vcs\\docks.ipl" + "\r\n" +
            "vcs\\downtown.ipl" + "\r\n" +
            "vcs\\downtows.ipl" + "\r\n" +
            "vcs\\golf.ipl" + "\r\n" +
            "vcs\\haiti.ipl" + "\r\n" +
            "vcs\\haitin.ipl" + "\r\n" +
            "vcs\\islandsf.ipl" + "\r\n" +
            "vcs\\littleha.ipl" + "\r\n" +
            "vcs\\mall.ipl" + "\r\n" +
            "vcs\\nbeach.ipl" + "\r\n" +
            "vcs\\nbeachbt.ipl" + "\r\n" +
            "vcs\\nbeachw.ipl" + "\r\n" +
            "vcs\\oceandN.ipl" + "\r\n" +
            "vcs\\oceandrv.ipl" + "\r\n" +
            "vcs\\other-interiors.ipl" + "\r\n" +
            "vcs\\starisl.ipl" + "\r\n" +
            "vcs\\washintn.ipl" + "\r\n" +
            "vcs\\washints.ipl" + "\r\n" +
            "" + "\r\n" +
            "[Map3]" + "\r\n" +
            "LCS Colmap" + "\r\n" +
            "" + "\r\n" +
            "[Ide3]" + "\r\n" +
            "lcscolmap\\colmap.ide" + "\r\n" +
            "" + "\r\n" +
            "[Ipl3]" + "\r\n" +
            "lcscolmap\\colmap.ipl" + "\r\n" +
            "" + "\r\n" +
            "[Map4]" + "\r\n" +
            "VCS Colmap" + "\r\n" +
            "" + "\r\n" +
            "[Ide4]" + "\r\n" +
            "vcscolmap\\colmap.ide" + "\r\n" +
            "" + "\r\n" +
            "[Ipl4]" + "\r\n" +
            "vcscolmap\\colmap.ipl" + "\r\n" +
            "";

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
    }
}
