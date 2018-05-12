using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class Archivos
    {
        public static string ini = "settings.ini";

        public static string ini_raw = "" +
            "; IPL/IDE Advanced Editor" + "\r\n" +
            "; Archivo de configuración" + "\r\n" +
            "; En este archivo están almacenadas las entradas por defecto" + "\r\n" +
            "; Aqui puedes agregar más entradas si lo deseas, pero cuidado de respetar las reglas de sintaxis" + "\r\n" +
            "; o podrían ocurrir resultados inesperados. Si se desea restaurar settings.ini a su versión original" + "\r\n" +
            "; entonces borrarlo y ejecutar nuevamente el IDE-IPL Advanced Editor.exe" + "\r\n" +
            "[DefaultEntryPath]" + "\r\n" +
            "C:" + "\r\n" +
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

        /*public static string[] ide = { "commer.ide", "indust.ide", "suburb.ide", "underg.ide" };
        public static string[] ipl = { "comNbtm.ipl", "comNtop.ipl", "comSE.ipl", "comSW.ipl", 
                                  "industNE.ipl", "industNW.ipl", "industSE.ipl", "industSW.ipl", 
                                  "props.ipl", "landne.ipl", "landsw.ipl", 
                                  "underg.ipl", "overview.ipl" };*/
        /*public static string[] ide = { "beach.ide", "mainla.ide", "mall.ide" };
        public static string[] ipl = { "airport.ipl", "airportN.ipl", "bridge.ipl", "cisland.ipl", 
                                  "docks.ipl", "downtown.ipl", "downtows.ipl", "golf.ipl", 
                                  "haiti.ipl", "haitin.ipl", "islandsf.ipl", 
                                  "littleha.ipl", "mall.ipl" , "nbeach.ipl" , "nbeachbt.ipl" , "nbeachw.ipl",
                                  "oceandN.ipl", "oceandrv.ipl" , "other-interiors.ipl", "starisl.ipl",
                                  "washintn.ipl", "washints.ipl" };*/
        //public static string[] ide = { "colmap.ide" }, ipl = { "colmap.ipl" };
        public static bool CheckFiles(string path)
        {
            string[] ide = Settings.GetFromIni(Settings.Ide),
                ipl = Settings.GetFromIni(Settings.Ipl);
            for (int i = 0; i < ide.Length; i++)
            {
                if (!File.Exists(Path.Combine(path, ide[i])))
                    return false;
            }
            for (int i = 0; i < ipl.Length; i++)
            {
                if (!File.Exists(Path.Combine(path, ipl[i])))
                    return false;
            }
            return true;
        }

        public static void CreateIni()
        {
            if (!File.Exists(ini))
                Archivos.StoreRaw(ini, ini_raw);
        }

        public static int getStartID(string ide_raw)
        {
            string[] line = Regex.Split(ide_raw, "\r\n");
            for (int i = 0; i < line.Length; i++ )
            {
                string[] dummy = line[i].Split(',');
                if (!line[i].StartsWith("#") && (dummy.Length >= 5))
                    return Int32.Parse(dummy[0]);
            }
            return -1;
        }

        public static int getFinalID(string ide_raw)
        {
            int stat = 0, Id = 0;
            string[] line = Regex.Split(ide_raw, "\r\n");
            for (int i = 0; i < line.Length; i++)
            {
                switch (stat)
                {
                    case 0:
                        if (line[i].Equals("objs")) stat = 1;
                        break;
                    case 1:
                        if (line[i].Equals("end")) stat = 2;
                        else
                        {
                            string[] dummy = line[i].Split(',');
                            Id = Int32.Parse(dummy[0]);
                        }
                        break;
                }
            }
            return Id;
        }
        //public static int GetTotal()
        static public string[] GetRaw(string path, string[] source)
        {
            string[] raw = new string[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                using (FileStream fs = new FileStream(Path.Combine(path, source[i]), FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader r = new StreamReader(fs))
                    {
                        raw[i] = r.ReadToEnd();
                    }
                }
            }
            return raw;
        }
        static public string GetRaw2(string fullpath)
        {
            string raw;
            using (FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader r = new StreamReader(fs))
                {
                    raw = r.ReadToEnd();
                }
            }
            return raw;
        }
        static public void StoreRaw(string fullpath, string raw)
        {
            using (FileStream fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(raw);
                }
            }
        }
        static public void CreateDirectoryOf(string file)
        {
            string[] path = file.Split('\\');   //Regex.Split(file, "\\");
            if (path.Length > 1)
            {
                string combined_path = "";
                for (int i = 0; i < (path.Length - 1); i++)
                {
                    if (i != 0) combined_path += "\\";
                    combined_path += path[i];
                }
                if (!Directory.Exists(combined_path))
                    Directory.CreateDirectory(combined_path);
            }
        }
        /*static public void LOD_names(string path)
        {
            string[] ipl_raw = Archivos.GetRaw(path, Archivos.ipl),
                ide_raw = Archivos.GetRaw(path, Archivos.ide);
            string errorLog = "ERROR LOG:\r\n", log = "LOG:\r\n";

            for (int ide_index = 0; ide_index < ide.Length; ide_index++)
            {
                int percentageComplete, stat = 0, replaced = 0;
                string[] line = Regex.Split(ide_raw[ide_index], "\r\n");
                string normExpr, LODexpr;

                for (int i = 0; i < line.Length; i++)
                {
                    Console.Clear();
                    percentageComplete = (int)((float)100 * i / ((float)(line.Length)));
                    Console.Write(percentageComplete.ToString() + "% completado - procesando: " + Archivos.ide[ide_index]);
                    switch (stat)
                    {
                        case 0:
                            if (line[i].Equals("objs")) stat = 1;
                            break;
                        case 1:
                            if (line[i].Equals("end")) stat = 2;
                            else
                            {
                                string[] dummy = line[i].Split(',');
                                normExpr = dummy[0] + ", " + Archivos.ide[ide_index].Substring(0, 3) + dummy[1].Substring(4);
                                LODexpr = dummy[0] + ", LOD" + dummy[1].Substring(4);
                                if (Int32.Parse(dummy[4]) == 3000)
                                {
                                    for (int j = 0; j < ipl_raw.Length; j++)
                                        if (ipl_raw[j].Contains(normExpr))
                                        {
                                            replaced++;
                                            log += "Reemplazando \"" + normExpr + "\" por \"" + LODexpr + "\" en archivo " + Archivos.ipl[j] + "\r\n";
                                            ipl_raw[j] = ipl_raw[j].Replace(normExpr, LODexpr);
                                        }
                                }
                                else if (Int32.Parse(dummy[4]) == 299)
                                {
                                    for (int j = 0; j < ipl_raw.Length; j++)
                                    {
                                        if (ipl_raw[j].Contains(LODexpr))
                                        {
                                            ipl_raw[j] = ipl_raw[j].Replace(LODexpr, normExpr);
                                            errorLog = "-------------------------------------------\r\n" +
                                                "ERROR: han sido encontrados nombres con LOD donde no debían haber.\r\n" +
                                                "A ver si para la próxima te fijas más en lo que haces marikon pendejete.\r\n" +
                                                "Pero por suerte me tienes a mí, tu app autimatizada que acaba de arreglar tus cagadas!\r\n";
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

                if (!Directory.Exists("output"))
                    Directory.CreateDirectory("output");
                for (int i = 0; i < ipl_raw.Length; i++)
                {
                    //percentageComplete = (int)(100 * (float)Id / (float)(offset + interval));
                    Archivos.StoreRaw(Path.Combine("output", Archivos.ipl[i]), ipl_raw[i]);
                    Console.Clear();
                    Console.Write("100% completado - construyendo archivos: " + Archivos.ipl[i]);
                }
                Console.Clear();
                Console.Write("100% completado - el proceso ha finalizado\n" +
                    "Presione ENTER para terminar jeje y komase mi mierdha invesil ejhejehejjeej\n");

                if (!Directory.Exists("output\\log"))
                    Directory.CreateDirectory("output\\log");
                string log_raw = log + "\r\n\r\n------------\r\n\r\n" + errorLog;
                Archivos.StoreRaw(Path.Combine("output\\log", "log.txt"), log_raw);
            }
        }*/
        static public string FixIde(string ide_raw)
        {
            int stat = 0;
            string[] line = Regex.Split(ide_raw, "\r\n");
            for (int i = 0; i < line.Length; i++)
            {
                switch (stat)
                {
                    case 0:
                        if (line[i].Equals("objs")) stat = 1;
                        break;
                    case 1:
                        if (line[i].Equals("end")) stat = 2;
                        else if (!line[i].StartsWith("#"))
                        {
                            string[] dummy = line[i].Split(',');
                            if (dummy.Length == 5)
                            {
                                string[] fix = new string[6];
                                for (int j = 0; j < 6; j++)
                                {
                                    if (j >= 0 && j <= 2) fix[j] = dummy[j];
                                    else if (j == 3) fix[j] = " 1";
                                    else if (j == 4 || j == 5) fix[j] = dummy[j - 1];
                                }
                                line[i] = String.Join(",", fix);
                            }
                        }
                        break;
                    case 2:
                        if (line[i].Equals("tobj")) stat = 3;
                        break;
                    case 3:
                        if (line[i].Equals("end")) stat = 4;
                        else if (!line[i].StartsWith("#"))
                        {
                            string[] dummy = line[i].Split(',');
                            if (dummy.Length == 7)
                            {
                                string[] fix = new string[8];
                                for (int j = 0; j < 8; j++)
                                {
                                    if (j >= 0 && j <= 2) fix[j] = dummy[j];
                                    else if (j == 3) fix[j] = " 1";
                                    else if (j >= 4 && j <= 7) fix[j] = dummy[j - 1];
                                }
                                line[i] = String.Join(",", fix);
                            }
                        }
                        break;
                }
            }
            ide_raw = String.Join("\r\n", line);
            return ide_raw;
        }
    }
}
