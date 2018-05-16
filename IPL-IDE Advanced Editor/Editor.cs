using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class Editor
    {
        private static byte version = 2, revision = 0, patch = 0;

        public static string fullname = String.Format(
            "IDE/IPL Advanced Editor v{0}.{1}.{2}",
            Editor.version, Editor.revision, Editor.patch);

        public static decimal xOff, yOff, zOff;
        public static uint offset;
        public static bool PatchIDEs, IgnoreLODs;
        public static int Progress, PercentageCompleted, Interval;

        public static Dictionary<string, List<string>> Ids;

        public static Dictionary<string, List<string>> GetAllIds(List<string> ide, List<string> ide_raw)
        {
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            foreach (string file in ide)
                dict[file] = new List<string>();
            for (int i = 0; i < ide.Count; i++)
            {
                int stat = 0;
                foreach (string line in Regex.Split(ide_raw[i], "\r\n"))
                {
                    switch (stat)
                    {
                        case 0:
                            if (line.Equals("objs") || line.Equals("tobj")) stat = 1;
                            break;
                        case 1:
                            if (line.Equals("end")) stat = 0;
                            else
                            {
                                string[] dummy = line.Split(',');
                                dict[ide[i]].Add(dummy[0]);
                            }
                            break;
                    }
                }
                dict[ide[i]].Sort();
            }
            return dict;
        }
        public static int GetStartID(string ide_raw)
        {
            foreach (string line in Regex.Split(ide_raw, "\r\n"))
            {
                string[] dummy = line.Split(',');
                if (!line.StartsWith("#") && (dummy.Length >= 5))
                    return Int32.Parse(dummy[0]);
            }
            return -1;
        }

        public static int GetFinalID(string ide_raw)
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

        public static int GetStartID(Dictionary<string, List<string>> ids)
        {
            int firstId = Int32.MaxValue;
            foreach (KeyValuePair<string, List<string>> elem in ids)
                foreach (string id in elem.Value)
                    firstId = (Convert.ToInt32(id) < firstId) ? Convert.ToInt32(id) : firstId;
            return firstId;
        }

        public static int GetFinalID(Dictionary<string, List<string>> ids)
        {
            int finalId = 0;
            foreach (KeyValuePair<string, List<string>> elem in ids)
                foreach (string id in elem.Value)
                    finalId = (Convert.ToInt32(id) > finalId) ? Convert.ToInt32(id) : finalId;
            return finalId;
        }

        public static void UpdateProgress()
        {
            Editor.PercentageCompleted = (int)(100 * (float)Editor.Progress / (float)(Editor.offset + Editor.Interval));
            Editor.PercentageCompleted = (Editor.PercentageCompleted > 100) ? 100 : Editor.PercentageCompleted;
        }

        static public void CreateDirectoryOf(string file)
        {
            List<string> path = file.Split('\\').ToList();   //Regex.Split(file, "\\");
            if (path.Count > 1)
            {
                path.RemoveAt(path.Count - 1);
                if (Path.GetPathRoot(file) != String.Empty)
                    path[0] = Path.GetPathRoot(file);
                if (!Directory.Exists(Path.Combine(path.ToArray())))
                    Directory.CreateDirectory(Path.Combine(path.ToArray()));
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

        public static List<string> PatchAllIpl(List<string> ipl, List<string> ipl_raw, BackgroundWorker bgWorker)
        {
            for (int i = 0; i < ipl_raw.Count; i++)
            {
                string[] line = Regex.Split(ipl_raw[i], "\r\n");
                int stat = 0;
                for (int j = 0; j < line.Length; j++)
                {
                    switch (stat)
                    {
                        case 0:
                            if (line[j].Equals("inst")) stat = 1;
                            break;
                        case 1:
                            if (line[j].Equals("end")) stat = 2;
                            else
                            {
                                string[] dummy = Regex.Split(line[j], ", ");

                                string id = "", modelName = "", interior = "0",
                                    posX = "", posY = "", posZ = "",
                                    scaleX = "1", scaleY = "1", scaleZ = "1",
                                    rotX = "", rotY = "", rotZ = "", rotW = "", lod = "-1";

                                // Input format
                                if (dummy.Length == 12) // GTA III format Ipl
                                {
                                    id = dummy[0]; modelName = dummy[1]; posX = dummy[2];
                                    posY = dummy[3]; posZ = dummy[4]; scaleX = dummy[5];
                                    scaleY = dummy[6]; scaleZ = dummy[7]; rotX = dummy[8];
                                    rotY = dummy[9]; rotZ = dummy[10]; rotW = dummy[11];
                                }
                                else if (dummy.Length == 13) // GTA VC format Ipl
                                {
                                    id = dummy[0]; modelName = dummy[1]; interior = dummy[2];
                                    posX = dummy[3]; posY = dummy[4]; posZ = dummy[5]; scaleX = dummy[6];
                                    scaleY = dummy[7]; scaleZ = dummy[8]; rotX = dummy[9];
                                    rotY = dummy[10]; rotZ = dummy[11]; rotW = dummy[12];
                                }
                                else if (dummy.Length == 11) // GTA SA format Ipl
                                {
                                    id = dummy[0]; modelName = dummy[1]; interior = dummy[2];
                                    posX = dummy[3]; posY = dummy[4]; posZ = dummy[5]; rotX = dummy[6];
                                    rotY = dummy[7]; rotZ = dummy[8]; rotW = dummy[9]; lod = dummy[10];
                                }

                                // Output format - TODO
                                if (false) // GTA III format
                                {
                                    line[j] = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
                                        id, modelName, posX, posY, posZ, scaleX, scaleY, scaleZ, rotX, rotY, rotZ, rotW);
                                }
                                else if (false) // Vice City format
                                {
                                    line[j] = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}",
                                        id, modelName, interior, posX, posY, posZ, scaleX, scaleY, scaleZ, rotX, rotY, rotZ, rotW);
                                }
                                else if (true) // San Andreas format
                                {
                                    if (!Editor.IgnoreLODs)
                                        lod = Editor.GetLodInt(modelName, ipl_raw[i], lod).ToString();

                                    line[j] = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}",
                                        id, modelName, interior, posX, posY, posZ, rotX, rotY, rotZ, rotW, lod);
                                }
                                int total = (int)(100 * (float)j / (float)line.Length);
                                bgWorker.ReportProgress(Editor.PercentageCompleted,
                                    String.Format("{0} %\nPatching IPL files ({1}/{2}):\n{3} {4}%",
                                    Editor.PercentageCompleted.ToString(), i + 1, ipl.Count,
                                    ipl[i].Split('\\').ToList().Last(), total.ToString()));
                            }
                            break;
                    }
                }
                ipl_raw[i] = String.Join("\r\n", line);
            }
            return ipl_raw;
        }

        private static int GetLodInt(string modelName, string raw, string prevLod)
        {
            if (!modelName.StartsWith("lod", StringComparison.OrdinalIgnoreCase))
            {
                string[] line = Regex.Split(raw, "\r\n");
                int current_line = 0;
                int stat = 0;
                for (int j = 0; j < line.Length; j++)
                {
                    switch (stat)
                    {
                        case 0:
                            if (line[j].Equals("inst"))
                            {
                                stat = 1;
                                current_line = 0;
                            }
                            break;
                        case 1:
                            if (line[j].Equals("end")) stat = 2;
                            else
                            {
                                string[] dummy = Regex.Split(line[j], ", ");
                                string lodName = "LOD" + modelName.Substring(3);
                                if (dummy[1].Equals(lodName))
                                    return current_line;
                                current_line++;
                            }
                            break;
                    }
                }
                return Convert.ToInt32(prevLod);
            }
            else
                return Convert.ToInt32(prevLod);
        }

        public static List<string> CreateOutputPaths(List<string> inputPaths, string inputPathBase, string outputPathBase)
        {
            List<string> outputPaths = new List<string>();
            foreach (string path in inputPaths)
                outputPaths.Add(path.Replace(inputPathBase, outputPathBase));
            return outputPaths;
        }

        public static void BatchIdsReConversion(List<string> ide, List<string> ide_raw, List<string> ipl_raw, int startID, BackgroundWorker bgWorker)
        {
            for (int i = 0; i < ide_raw.Count; i++)
            {
                string[] line = Regex.Split(ide_raw[i], "\r\n");    // ide_raw[i].Split(new [] { '\r', '\n' });
                int stat = 0;
                for (int j = 0; j < line.Length; j++)
                {
                    switch (stat)
                    {
                        case 0:
                            if (line[j].Equals("objs") || line[j].Equals("tobj")) stat = 1;
                            break;
                        case 1:
                            if (line[j].Equals("end")) stat = 0;
                            else
                            {
                                string oldExpr, newExpr;
                                string[] dummy = line[j].Split(',');
                                int Id;
                                /*if (dummy[0].StartsWith("#"))
                                    dummy[0] = dummy[0].Substring(dummy[0].IndexOf("#"));*/
                                try
                                {
                                    Id = Int32.Parse(dummy[0]);
                                }
                                catch
                                {
                                    throw new Exception(
                                        String.Format("Error: invalid Id value: '{0}' on file '{1}', line {2}",
                                        dummy[0], ide[i], j + 1));
                                }
                                oldExpr = dummy[0] + "," + dummy[1];
                                line[j] = (Id + Editor.offset - startID).ToString() + line[j].Substring(line[j].IndexOf(','));
                                dummy = line[j].Split(',');
                                newExpr = dummy[0] + "," + dummy[1];
                                for (int i2 = 0; i2 < ipl_raw.Count; i2++)
                                    ipl_raw[i2] = ipl_raw[i2].Replace(oldExpr, newExpr);
                                Editor.Progress++;
                                Editor.UpdateProgress();
                                bgWorker.ReportProgress(Editor.PercentageCompleted,
                                    String.Format("{0} %\nProcessing: {1}",
                                    Editor.PercentageCompleted.ToString(), ide[i].Split('\\').ToList().Last()));
                            }
                            break;
                    }
                }
                ide_raw[i] = String.Join("\r\n", line);
            }
        }

        public static void FixIplCoordinates(List<string> ipl, List<string> ipl_raw, BackgroundWorker bgWorker)
        {
            for (int i = 0; i < ipl_raw.Count; i++)
            {
                LogCoord.ReportFile(ipl[i]);
                string[] line = Regex.Split(ipl_raw[i], "\r\n");
                int stat = 0;
                for (int j = 0; j < line.Length; j++)
                {
                    switch (stat)
                    {
                        case 0:
                            if (line[j].Equals("inst")) stat = 1;
                            break;
                        case 1:
                            if (line[j].Equals("end")) stat = 2;
                            else
                            {
                                string[] dummy = line[j].Split(',');
                                LogCoord.InitCoordinates();
                                LogCoord.LogCoordinates(dummy[3], dummy[4], dummy[5]);
                                if (dummy.Length > 1)
                                {
                                    decimal posx, posy, posz;
                                    posx = Decimal.Parse(dummy[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                                    posy = Decimal.Parse(dummy[4], NumberStyles.Any, CultureInfo.InvariantCulture);
                                    posz = Decimal.Parse(dummy[5], NumberStyles.Any, CultureInfo.InvariantCulture);
                                    posx += Editor.xOff;
                                    posy += Editor.yOff;
                                    posz += Editor.zOff;
                                    dummy[3] = " " + posx.ToString().Replace(",", ".");
                                    dummy[4] = " " + posy.ToString().Replace(",", ".");
                                    dummy[5] = " " + posz.ToString().Replace(",", ".");
                                    line[j] = String.Join(",", dummy);
                                }
                                LogCoord.LogCoordinates(dummy[3], dummy[4], dummy[5]);
                                LogCoord.WriteCoordErrorLine(dummy[0] + ", " + dummy[1], Editor.xOff, Editor.yOff, Editor.zOff);
                            }
                            break;
                    }
                }
                Editor.UpdateProgress();
                bgWorker.ReportProgress(Editor.PercentageCompleted,
                    String.Format("{0} %\nEditing coordinates of: {1}",
                    Editor.PercentageCompleted.ToString(), ipl[i]));
                ipl_raw[i] = String.Join("\r\n", line);
            }
            LogCoord.EndLogging("coordinate_change.log");
        }
    }
}
