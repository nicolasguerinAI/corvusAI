using System;
using System.IO;
using Newtonsoft.Json;

namespace Corvus
{
    class Program
    {

        static void Usage()
        {
            Console.WriteLine("Corvus: Usage: dotnet run [--train] match_file.json");
        }

        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--train") {

                if (!File.Exists(args[1]))
                {
                    Console.WriteLine("Corvus: File \"" + args[1] + "\" does not exist");
                    return;
                }

                try {
                    var manager = new Manager(args[1]);
                    manager.InitNeuralNetworks();
                    manager.Train();
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            } else if (args.Length == 2) {
                try {
                    var manager = new Manager();
                    manager.PredictMatch(args[0], args[1]);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            } else
                Usage();
        }
    }
}
