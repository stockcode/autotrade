using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace autotrade.util
{
    public class SettingUtils
    {
        public static Dictionary<string, string> ToDictionary(StringCollection sc)
        {
            if (sc.Count%2 != 0) throw new InvalidDataException("Broken dictionary");

            var dic = new Dictionary<string, string>();
            for (int i = 0; i < sc.Count; i += 2)
            {
                dic.Add(sc[i], sc[i + 1]);
            }
            return dic;
        }

        public static StringCollection ToStringCollection(Dictionary<string, string> dic)
        {
            var sc = new StringCollection();
            foreach (var d in dic)
            {
                sc.Add(d.Key);
                sc.Add(d.Value);
            }
            return sc;
        }
    }
}