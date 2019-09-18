using System;

namespace SimplicitC
{
    class Program
    {
        static void Main()
        {
            var backend = new Backends.CSharpBackend.Backend();
            string source =
@"
int i = 0;
while (i < 1) {
    print(i);
}
";
            Console.WriteLine(source);
            bool result = backend.Compile(
                source
                );
            if (!result)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach(Error error in backend.Errors)
                {
                    Console.WriteLine(error);
                }
                Console.ResetColor();
                Console.WriteLine("Compilation failed, Press Enter to continue");
                Console.ReadLine();
                Environment.Exit(1);
            }
            Environment.ExitCode = backend.Execute();
            Console.WriteLine("Execution is complete, Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
