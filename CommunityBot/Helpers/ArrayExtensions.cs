﻿using System.Collections.Generic;
using System.Linq;

namespace CommunityBot.Helpers
{
    public static class ArrayExtensions
    {
        public static bool In<T>(this T value, params T[] constrainList)
        {
            return constrainList.Contains(value);
        }
        
        public static bool NotIn<T>(this T value, params T[] constrainList)
        {
            return !constrainList.Contains(value);
        }
    }
}