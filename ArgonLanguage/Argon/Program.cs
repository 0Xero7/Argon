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
            using (StreamReader f = new StreamReader(@"C:\Users\smpsm\source\repos\Argon\Argon\hello.ar"))
                text = f.ReadToEnd();

            var y = ArgonRunnable.Argon.GetTokenList("y = 3;");
            ArgonRunnable.Argon.GetAST(y.ToArray());

            var list = ArgonRunnable.Argon.GetTokenList(text);

            foreach (var x in list)
                Console.WriteLine($"{x.lineNumber:0000}  [{x.tokenType}]\t\t{x.tokenValue}");
        }
    }
}
