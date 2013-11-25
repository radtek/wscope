using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeAuto
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("");
                Console.WriteLine("usage: PassGen.exe myplainpassword");
                return;
            }
            Console.WriteLine(MyAesMan.EncPass(args[0]));
        }
    }
}
