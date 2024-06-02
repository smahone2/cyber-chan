using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberChan.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<string> Split(this string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        public static IEnumerable<string> SplitBy(this string str, int chunkLength)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentException();
            if (chunkLength < 1) throw new ArgumentException();

            for (int i = 0; i < str.Length; i += chunkLength)
            {
                if (chunkLength + i > str.Length)
                    chunkLength = str.Length - i;

                yield return str.Substring(i, chunkLength);
            }
        }
    }
}
