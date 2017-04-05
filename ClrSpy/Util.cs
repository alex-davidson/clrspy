using System.Collections.Generic;

namespace ClrSpy
{
    public static class Util
    {
        public static void Swap<T>(ref T start, ref T stop)
        {
            var tmp = start;
            start = stop;
            stop = tmp;
        }

        public static long InKilobytes(this long bytes)
        {
            return (bytes + 1023) / 1024;
        }

        public static T RemoveLast<T>(this IList<T> list)
        {
            var lastIndex = list.Count - 1;
            var item = list[lastIndex];
            list.RemoveAt(lastIndex);
            return item;
        }

        /// <summary>
        /// Cast via 'object' to escape compiler type checking, but do so 'safely': this method requires explicit
        /// specification of both input and output types to try to defeat accidental 'clever' fixups by refactoring tools.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static TOutput ForceCast<TInput, TOutput>(TInput input)
        {
            return (TOutput)(object)input;
        }
    }
}
