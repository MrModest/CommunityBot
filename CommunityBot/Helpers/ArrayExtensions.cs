using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityBot.Helpers
{
    public static class ArrayExtensions
    {
        public static bool InWithIgnoreCase(this string value, params string[] constrainList)
        {
            return constrainList.Contains(value, StringComparer.InvariantCultureIgnoreCase);
        }
        
        public static bool In<T>(this T value, params T[] constrainList)
        {
            return constrainList.Contains(value);
        }
        
        public static bool NotIn<T>(this T value, params T[] constrainList)
        {
            return !constrainList.Contains(value);
        }

        public static T[] EmptyIfNull<T>(this T[]? array)
        {
            return array ?? Array.Empty<T>();
        }
    }
}