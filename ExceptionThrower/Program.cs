using System;

namespace ExceptionThrower
{
    class Program
    {
        static void Main(string[] args)
        {
            var message = args.Length > 0 ? args[0] : "This is a test";
            Console.Read();
            throw new Exception(message);
        }
    }
}
