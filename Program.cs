using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netchange
{
    class Program
    {
        static public int myGate;

        static public Dictionary<int, Connection> neighbours = new Dictionary<int, Connection>();

        static void Main(string[] args)
        {
            Initiate(args);
            Console.ReadLine();
        }

        static void Initiate(string[] args)
        {
            myGate = int.Parse(args[0]);
            Console.Title = "NetChange " + myGate;
            foreach (string s in args.Skip(1))
            {
                AddNeigbour(int.Parse(s));
            }
        }

        static void AddNeigbour(int gate)
        {
            neighbours.Add(gate, new Connection(gate));
        }
    }
}


//Code uit voorbeeld
/*
namespace MultiClientServer
{
    class Program
    {
        static public int MijnPoort;

        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();

        static void Main(string[] args)
        {
            Console.Write("Op welke poort ben ik server? ");
            MijnPoort = int.Parse(Console.ReadLine());
            new Server(MijnPoort);

            Console.WriteLine("Typ [verbind poortnummer] om verbinding te maken, bijvoorbeeld: verbind 1100");
            Console.WriteLine("Typ [poortnummer bericht] om een bericht te sturen, bijvoorbeeld: 1100 hoi hoi");

            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("verbind"))
                {
                    int poort = int.Parse(input.Split()[1]);
                    if (Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
                else
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 2);
                    int poort = int.Parse(delen[0]);
                    if (!Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                        Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[1]);
                }
            }
        }
    }
}
*/


