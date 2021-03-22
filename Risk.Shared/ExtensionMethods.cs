using System;
using System.Collections.Generic;
using System.Threading;

namespace Risk.Shared
{
    public static class Extensions
    {
        [ThreadStatic]
        private static Random localRng;

        public static Random ThisThreadRandom => localRng ??= new Random(unchecked((Environment.TickCount * 31) + Thread.CurrentThread.ManagedThreadId));

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThisThreadRandom.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}