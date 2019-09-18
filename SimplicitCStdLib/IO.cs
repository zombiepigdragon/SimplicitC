using System;
using System.Collections.Generic;
using System.Text;

namespace SimplicitCStdLib
{
    public static partial class Globals
    {
        public static void print(object text)
        {
            Console.WriteLine(text.ToString());
        }
    }
}
