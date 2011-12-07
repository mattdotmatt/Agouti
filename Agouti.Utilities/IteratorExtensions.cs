using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agouti.Utilities.Extensions
{
    public static class IteratorExtensions
    {
        /// <summary>
        /// A ruby-style iterator on an integer literal
        /// </summary>
        /// <param name="literal">The integer</param>
        /// <param name="action">An action to be performed, returning a zero-indexed counter</param>
        public static void Times(this int literal, Action<int> action)
        {
            for (var ctr = 0; ctr < literal; ctr++)
            {
                action(ctr);
            }
        }

        public static string CommaSeparated<T>(this IEnumerable<T> list)
        {
            return string.Join(", ", list.Select(o => o.ToString()).ToArray());
        }

    }
}