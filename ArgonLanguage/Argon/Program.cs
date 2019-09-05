using System;
using System.IO;
using ArgonRunnable;

namespace Argon
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = "";
            using (StreamReader f = new StreamReader(@"D:\Projects\Argon\ArgonLanguage\hello.ar"))
                text = f.ReadToEnd();

            var list = ArgonRunnable.Argon.GetTokenList(text).ToArray();

            foreach (var x in list)
                Console.WriteLine($"{x.lineNumber:0000}  [{x.tokenType}]\t\t{x.tokenValue}");

            var s = ArgonRunnable.Argon.GetAST(list);
            ArgonRunnable.Argon.SanitizeAST(s);

            Console.WriteLine("\n\n\n");
            ArgonRunnable.Argon.PrintAST(s);

            Console.WriteLine("\n\n\n");
            ArgonRunnable.Argon.GenerateIR(s);
        }
    }
}
