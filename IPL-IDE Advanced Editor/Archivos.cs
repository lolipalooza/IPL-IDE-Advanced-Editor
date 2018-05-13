using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class Editor
    {
        public static byte version = 1, revision = 0, patch = 0;

        public static string fullname = String.Format(
            "IDE/IPL Advanced Editor v{0}.{1}.{2}",
            Editor.version, Editor.revision, Editor.patch);

        public static bool CheckFiles(string path)
        {
            foreach (string ide in Settings.GetFromIni(Settings.Ide))
            {
                if (!File.Exists(Path.Combine(path, ide)))
                    return false;
            }
            foreach (string ipl in Settings.GetFromIni(Settings.Ipl))
            {
                if (!File.Exists(Path.Combine(path, ipl)))
                    return false;
            }
            return true;
        }

        public static int getStartID(string ide_raw)
        {
            foreach (string line in Regex.Split(ide_raw, "\r\n"))
            {
                string[] dummy = line.Split(',');
                if (!line.StartsWith("#") && (dummy.Length >= 5))
                    return Int32.Parse(dummy[0]);
            }
            return -1;
        }

        public static int getFinalID(string ide_raw)
        {
            int stat = 0, Id = 0;
            foreach (string line in Regex.Split(ide_raw, "\r\n"))
            {
                switch (stat)
                {
                    case 0:
                        if (line.Equals("objs")) stat = 1;
                        break;
                    case 1:
                        if (line.Equals("end")) stat = 2;
                        else
                        {
                            string[] dummy = line.Split(',');
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
        static public string GetRaw(string fullpath)
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
                path[path.Length - 1] = "";
                if (!Directory.Exists(Path.Combine(path)))
                    Directory.CreateDirectory(Path.Combine(path));
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
