using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Program
    {
        public static void Main(string[] args)
        {
            Configurations configurations = ReadConfig();
            Console.WriteLine(configurations.ToString());
            Console.ReadLine();
        }

        public static Configurations ReadConfig()
        {
            FileParser parser = new FileParser();
            return parser.parse("../../../config.txt");
        }

        public static void initializeProcesses()
        {

        }

        public static void createMenu()
        {

        }

    }
}
