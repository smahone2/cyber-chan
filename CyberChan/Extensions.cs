using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberChan
{
    public static class Extensions
    {
        public static IEnumerable<string> Split(this string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
    }
}
