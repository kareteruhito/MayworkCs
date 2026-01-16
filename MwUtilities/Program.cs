using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MwUtilities;
class Program
{
    static void Func(
        [System.Runtime.CompilerServices.CallerFilePath] string file = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [System.Runtime.CompilerServices.CallerMemberName] string member = "")
    {
        System.Diagnostics.Debug.WriteLine($"{file}({line}) {member}");
    }
    static void Main(string[] args)
    {
        Func();
        System.Console.WriteLine("Hello, World!");
    }
}