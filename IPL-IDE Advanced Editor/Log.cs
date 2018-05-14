using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class LogCoord
    {
        private static string Raw;
        private static string File;
        private static List<string> Coord;
        public static void ReportFile(string file)
        {
            LogCoord.File = file;
        }
        public static void ClearFile()
        {
            LogCoord.File = String.Empty;
        }
        public static void InitCoordinates()
        {
            LogCoord.Coord = new List<string>();
        }
        public static void LogCoordinates(string x, string y, string z)
        {
            Coord.Add(x);
            Coord.Add(y);
            Coord.Add(z);
        }
        public static void WriteCoordErrorLine(string ipl_item, string x, string y, string z)
        {
            string x1 = LogCoord.Coord[0],
                y1 = LogCoord.Coord[1],
                z1 = LogCoord.Coord[2],
                x2 = LogCoord.Coord[3],
                y2 = LogCoord.Coord[4],
                z2 = LogCoord.Coord[5];
            decimal dx1 = Decimal.Parse(LogCoord.Coord[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                dy1 = Decimal.Parse(LogCoord.Coord[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                dz1 = Decimal.Parse(LogCoord.Coord[2], NumberStyles.Any, CultureInfo.InvariantCulture),
                dx2 = Decimal.Parse(LogCoord.Coord[3], NumberStyles.Any, CultureInfo.InvariantCulture),
                dy2 = Decimal.Parse(LogCoord.Coord[4], NumberStyles.Any, CultureInfo.InvariantCulture),
                dz2 = Decimal.Parse(LogCoord.Coord[5], NumberStyles.Any, CultureInfo.InvariantCulture);
            decimal x_offset = Decimal.Parse(x, NumberStyles.Any, CultureInfo.InvariantCulture);
            decimal y_offset = Decimal.Parse(y, NumberStyles.Any, CultureInfo.InvariantCulture);
            decimal z_offset = Decimal.Parse(z, NumberStyles.Any, CultureInfo.InvariantCulture);
            if (x1.Split('.').Length > 1
                && x2.Split('.').Length > 1
                && x1.Split('.')[1] != x2.Split('.')[1]
                || y1.Split('.').Length > 1
                && y2.Split('.').Length > 1
                && y1.Split('.')[1] != y2.Split('.')[1]
                || z1.Split('.').Length > 1
                && z2.Split('.').Length > 1
                && z1.Split('.')[1] != z2.Split('.')[1])
            {
                if (dx2 != (dx1 + x_offset)
                    || dy2 != (dy1 + y_offset)
                    || dz2 != (dz1 + z_offset))
                {
                    if (LogCoord.File != String.Empty)
                    {
                        LogCoord.Raw += String.Format("File: {0}\r\n", LogCoord.File);
                        LogCoord.ClearFile();
                    }
                    LogCoord.Raw += String.Format(
                        "Warning! Decimal difference found in: \"{0}\", " 
                        + "coord change: x {1} -> {2}, y {3} -> {4}, z {5} -> {6}\r\n",
                        ipl_item, x1, x2, y1, y2, z1, z2);
                }
            }
        }
        public static void EndLogging(string logfile)
        {
            if (LogCoord.Raw != String.Empty)
                Archivos.StoreRaw("coordinate_change.log", LogCoord.Raw);
        }
    }

    class LogIds
    {
        private static string Raw;

        // This is just to Watch ids on breakpoints
        private static List<string> Ids;

        public static void Log(string line)
        {
            LogIds.Raw += String.Format("{0}\r\n", line);
        }

        public static void Log(Dictionary<string, List<string>> Ids)
        {
            LogIds.Ids = new List<string>();
            List<int> raw_lines_ids = new List<int>();
            Dictionary<int, string> raw_lines = new Dictionary<int, string>();
            foreach (KeyValuePair<string, List<string>> item in Ids)
            {
                raw_lines_ids.Add(Int32.Parse(item.Value.First()));
                raw_lines[raw_lines_ids[raw_lines_ids.Count - 1]]
                    = String.Format("File {0} - first Id: {1}, last id: {2}\r\n",
                    item.Key, item.Value.First(), item.Value.Last());
            }
            raw_lines_ids.Sort();
            foreach (int id in raw_lines_ids)
            {
                LogIds.Raw += raw_lines[id];
                LogIds.Ids.Add(raw_lines[id]);
            }
        }

        public static void EndLogging(string logfile)
        {
            if (LogIds.Raw != String.Empty)
                Archivos.StoreRaw(logfile, LogIds.Raw);
        }
    }
}
