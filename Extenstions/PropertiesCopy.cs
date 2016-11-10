using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace ASTV.Extenstions {
    public static class PropertyCopy {
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            CopyPropertiesTo(source, dest, null);
        }
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest, IList<string> exclude)
        {

            var sourceProps = typeof (T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            if (exclude == null) {
                exclude = new List<string>();                
            }

            foreach (var sourceProp in sourceProps)
            {
                if ( exclude.Contains(sourceProp.Name)) { continue; }
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    p.SetValue(dest, sourceProp.GetValue(source, null), null);
                }

            }

        }
    }
}    