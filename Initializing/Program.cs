using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransportClasses;

namespace Initializing
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.TryInitialize();
            Console.WriteLine("Loaded...");
            Console.ReadKey(true);
        }
    }
}
