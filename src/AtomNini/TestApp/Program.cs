using AtomNini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IniFile ini = IniFileManager.GetIniFile("./test.ini");
           
            while(true)
            {
                string ip = ini.GetValue("PLC", "IP");
                Console.WriteLine(ip);
                
            }
        }
    }
}
