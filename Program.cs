using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Netchange
{
    class Program
    {
        static public int myGate;

        static public Dictionary<int, Connection> neighbours = new Dictionary<int, Connection>();

        static Dictionary<string, Thread> threads = new Dictionary<string, Thread>();

        static void Main(string[] args)
        {
            Initiate(args);
            HandleInput();
        }

        static void Initiate(string[] args)
        {
            //Set own gate
            myGate = int.Parse(args[0]);
            new Server(myGate);

            //Set Console title
            Console.Title = "NetChange " + myGate;

            //Create dictionary of neighbours
            foreach (string s in args.Skip(1))
            {
                AddNeigbour(int.Parse(s));
            }
            /*
            //Create threads
            threads.Add("Input", new Thread(HandleInput));
            threads.Add("Connection", new Thread(HandleNewConnection));
            threads.Add("Message", new Thread(HandleMessage));

            //Start threads
            threads["Input"].Start();
            threads["Connection"].Start();
            threads["Message"].Start();
            */
        }

        static void HandleInput()
        {
            string input = Console.ReadLine();
            if (input.StartsWith("R"))
            {
                foreach (KeyValuePair<int, Connection> i in neighbours)
                {
                   Console.WriteLine("succes");
                }
                Console.WriteLine("Fail");
            }
            else if (input.StartsWith("B"))
            {

            }
            else if (input.StartsWith("C"))
            {

            }
            else if (input.StartsWith("D"))
            {

            }
            HandleInput();
        }

        static void HandleNewConnection()
        {

        }

        static void HandleMessage()
        {

        }

        static void AddNeigbour(int port)
        {
            if (neighbours.ContainsKey(port))
                Console.WriteLine("Hier is al verbinding naar!");
            else
                neighbours.Add(port, new Connection(port));
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


