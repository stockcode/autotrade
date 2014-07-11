using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
