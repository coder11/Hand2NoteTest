using System.Collections;
using System.Linq;

namespace Hand2Note.ProgressView.Util
{
    public static class CollectionExtensions
    {
        public static bool In<T>(this T value, params T[] values)
        {
            return values.Contains(value);
        }
    }
}