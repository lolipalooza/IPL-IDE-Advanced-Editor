using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class Log
    {
        private static string Raw;
        private static string File;
        private static List<string> Coord;
        public static void ReportFile(string file)
        {
            Log.File = file;
        }
        public static void ClearFile()
        {
            Log.File = String.Empty;
        }
        public static void InitCoordinates()
        {
            Log.Coord = new List<string>();
        }
        public static void LogCoordinates(string x, string y, string z)
        {
            Coord.Add(x);
            Coord.Add(y);
            Coord.Add(z);
        }
        public static void WriteCoordErrorLine(string ipl_item, string x, string y, string z)
        {
            string x1 = Log.Coord[0],
                y1 = Log.Coord[1],
                z1 = Log.Coord[2],
                x2 = Log.Coord[3],
                y2 = Log.Coord[4],
                z2 = Log.Coord[5];
            decimal dx1 = Decimal.Parse(Log.Coord[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                dy1 = Decimal.Parse(Log.Coord[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                dz1 = Decimal.Parse(Log.Coord[2], NumberStyles.Any, CultureInfo.InvariantCulture),
                dx2 = Decimal.Parse(Log.Coord[3], NumberStyles.Any, CultureInfo.InvariantCulture),
                dy2 = Decimal.Parse(Log.Coord[4], NumberStyles.Any, CultureInfo.InvariantCulture),
                dz2 = Decimal.Parse(Log.Coord[5], NumberStyles.Any, CultureInfo.InvariantCulture);
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
                    if (Log.File != String.Empty)
                    {
                        Log.Raw += String.Format("File: {0}\r\n", Log.File);
                        Log.ClearFile();
                    }
                    Log.Raw += String.Format(
                        "Warning! Decimal difference found in: \"{0}\", " 
                        + "coord change: x {1} -> {2}, y {3} -> {4}, z {5} -> {6}\r\n",
                        ipl_item, x1, x2, y1, y2, z1, z2);
                }
            }
        }
        public static void EndLogging(string logfile)
        {
            if (Log.Raw != String.Empty)
                Archivos.StoreRaw("coordinate_change.log", Log.Raw);
        }
    }
}
