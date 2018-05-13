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
            "; IPL/IDE Advanced Editor" + "\r\n" +
            "; Archivo de configuración" + "\r\n" +
            "; En este archivo están almacenadas las entradas por defecto" + "\r\n" +
            "; Aqui puedes agregar más entradas si lo deseas, pero cuidado de respetar las reglas de sintaxis" + "\r\n" +
            "; o podrían ocurrir resultados inesperados. Si se desea restaurar settings.ini a su versión original" + "\r\n" +
            "; entonces borrarlo y ejecutar nuevamente el IDE-IPL Advanced Editor.exe" + "\r\n" +
            "[DefaultEntryPath]" + "\r\n" +
            "input" + "\r\n" +
            "[DefaultEntry]" + "\r\n" +
            "1" + "\r\n" +
            "[Map1]" + "\r\n" +
            "GTA: Liberty City Stories Map" + "\r\n" +
            "[Ide1]" + "\r\n" +
            "commer\\commer.ide" + "\r\n" +
            "indust\\indust.ide" + "\r\n" +
            "suburb\\suburb.ide" + "\r\n" +
            "underg\\underg.ide" + "\r\n" +
            "[Ipl1]" + "\r\n" +
            "commer\\comNbtm.ipl" + "\r\n" +
            "commer\\comNtop.ipl" + "\r\n" +
            "commer\\comSE.ipl" + "\r\n" +
            "commer\\comSW.ipl" + "\r\n" +
            "indust\\industNE.ipl" + "\r\n" +
            "indust\\industNW.ipl" + "\r\n" +
            "indust\\industSE.ipl" + "\r\n" +
            "indust\\industSW.ipl" + "\r\n" +
            "indust\\props.ipl" + "\r\n" +
            "suburb\\landne.ipl" + "\r\n" +
            "suburb\\landsw.ipl" + "\r\n" +
            "underg\\underg.ipl" + "\r\n" +
            "underg\\overview.ipl" + "\r\n" +
            "[Map2]" + "\r\n" +
            "GTA: Vice City Stories Map" + "\r\n" +
            "[Ide2]" + "\r\n" +
            "files\\beach.ide" + "\r\n" +
            "files\\mainla.ide" + "\r\n" +
            "files\\mall.ide" + "\r\n" +
            "[Ipl2]" + "\r\n" +
            "files\\airport.ipl" + "\r\n" +
            "files\\airportN.ipl" + "\r\n" +
            "files\\bridge.ipl" + "\r\n" +
            "files\\cisland.ipl" + "\r\n" +
            "files\\docks.ipl" + "\r\n" +
            "files\\downtown.ipl" + "\r\n" +
            "files\\downtows.ipl" + "\r\n" +
            "files\\golf.ipl" + "\r\n" +
            "files\\haiti.ipl" + "\r\n" +
            "files\\haitin.ipl" + "\r\n" +
            "files\\islandsf.ipl" + "\r\n" +
            "files\\littleha.ipl" + "\r\n" +
            "files\\mall.ipl" + "\r\n" +
            "files\\nbeach.ipl" + "\r\n" +
            "files\\nbeachbt.ipl" + "\r\n" +
            "files\\nbeachw.ipl" + "\r\n" +
            "files\\oceandN.ipl" + "\r\n" +
            "files\\oceandrv.ipl" + "\r\n" +
            "files\\other-interiors.ipl" + "\r\n" +
            "files\\starisl.ipl" + "\r\n" +
            "files\\washintn.ipl" + "\r\n" +
            "files\\washints.ipl" + "\r\n" +
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
            string raw = Archivos.GetRaw(Settings.ini);
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
                        else result.Add(line);
                        break;
                }
            }
            return result.ToArray();
        }
        static public bool StoreInIni(string entry, string[] data)
        {
            string raw = Archivos.GetRaw(Settings.ini);
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
                Archivos.StoreRaw(Settings.ini, raw);
                return true;
            }
        }
    }
}
