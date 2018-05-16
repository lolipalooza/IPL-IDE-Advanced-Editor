using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPL_IDE_Advanced_Editor
{
    class Raw
    {
        static public List<string> Get(List<string> source)
        {
            List<string> raw = new List<string>();
            for (var i = 0; i < source.Count; i++)
            {
                using (FileStream fs = new FileStream(source[i], FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader r = new StreamReader(fs))
                    {
                        raw.Add(r.ReadToEnd());
                    }
                }
            }
            return raw;
        }
        static public string Get(string fullpath)
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
        static public void Store(string fullpath, string raw)
        {
            using (FileStream fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter w = new StreamWriter(fs))
                {
                    w.Write(raw);
                }
            }
        }
    }
}
