using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace autotrade.util
{
    class ObjectUtils
    {
        public static void Copy(object dest, object src)
        {
            Type type1 = src.GetType();
            Type type2 = dest.GetType();
            foreach (var mi in type1.GetProperties())
            {
                var des = type2.GetField(mi.Name);
                if (des != null)
                {

                    mi.SetValue(src, des.GetValue(dest));
                }
            }
        }

        public static T DeepCopy<T>(T obj)
        {
            BinaryFormatter s = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                s.Serialize(ms, obj);
                ms.Position = 0;
                T t = (T)s.Deserialize(ms);

                return t;
            }
        }
    }
}
