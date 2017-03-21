using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClrSpy.UnitTests.AsyncTaskTarget
{
    public class Program
    {
        private static readonly TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
        private static readonly TaskCompletionSource<string> tcs2 = new TaskCompletionSource<string>();

        static int Main(string[] args)
        {
            var task = Task.Run(() => GetTestTask(args.FirstOrDefault()));
            Console.Out.WriteLine();
            Console.In.ReadLine();
            tcs.SetResult(0);
            tcs2.SetResult("2");
            task.Wait();
            return 0;
        }

        private static Task GetTestTask(string test)
        {
            switch (test)
            {
                case "WhenAll": return WhenAllTest();
                case "ContinueWithDelegate": return ContinueWithDelegateTest();
                default: return SimpleTest();
            }
        }

        private static async Task SimpleTest()
        {
            await tcs.Task;
        }

        private static async Task WhenAllTest()
        {
            await Task.WhenAll(tcs.Task, tcs2.Task);
        }

        private static async Task ContinueWithDelegateTest()
        {
            await tcs.Task.ContinueWith(t =>
                { GC.KeepAlive(t); });
            // Braces positioned to yield the same line numbers in Debug and Release builds.
        }
    }
}
