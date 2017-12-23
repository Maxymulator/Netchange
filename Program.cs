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
        /// <summary>
        /// This instance's port number
        /// </summary>
        static public int myPort;

        /// <summary>
        /// This instance's neighbours
        /// </summary>
        static public Dictionary<int, Connection> neighbours = new Dictionary<int, Connection>();

        /// <summary>
        /// This instance's threads
        /// </summary>
        static Dictionary<string, Thread> threads = new Dictionary<string, Thread>();

        /// <summary>
        /// A list with for every neighbor a thread that listens to that neighbor
        /// </summary>
        static Dictionary<int, Thread> listenerThreads = new Dictionary<int, Thread>();

        /// <summary>
        /// static char to make stringbuilding easier
        /// </summary>
        static char space = ' ';

        /// <summary>
        /// Table that contains all nodes and the direct neightbor we need to send a message through to that node.
        /// <para/>First int: Destination node, Second int: Direct neighbor to get there
        /// </summary>
        static Dictionary<int, int> Nbu = new Dictionary<int, int>();

        /// <summary>
        /// Table that contains all nodes and the estimated amount of hops needed to get a message to that node.
        /// <para/>First int: Destination node, Second int: Estimated distance
        /// </summary>
        static Dictionary<int, int> Du = new Dictionary<int, int>();

        static void Main(string[] args)
        {
            Initiate(args);
            HandleThreads();
        }

        /// <summary>
        /// Initiates this instance of the program
        /// </summary>
        /// <param name="args">The arguments given to the program at launch</param>
        static void Initiate(string[] args)
        {
            //Set own gate
            myPort = int.Parse(args[0]);
            new Server(myPort);

            //Set Console title
            Console.Title = "NetChange " + myPort;

            //Create dictionary of neighbours
            for (int i = 1; i < args.Length; i++)
            {
                int port = int.Parse(args[i]);
                AddNeigbour(port);
                listenerThreads.Add(port, new Thread(WaitForMessage));
            }

            CreateRoutingTable();
        }

        /// <summary>
        /// Creates this instance's routing table
        /// </summary>
        static void CreateRoutingTable()
        {
            Nbu.Add(myPort, myPort);
            Du.Add(myPort, 0);

            foreach (KeyValuePair<int, Connection> kp in neighbours)
            {
                Nbu.Add(kp.Key, kp.Key);
                Du.Add(kp.Key, 1);
            }
        }

        /// <summary>
        /// Handles the threads used by this instance
        /// </summary>
        static void HandleThreads()
        {
            //Create threads
            threads.Add("Input", new Thread(HandleInput));
            threads.Add("Connection", new Thread(HandleNewConnection));

            //Start threads
            threads["Input"].Start();
            threads["Connection"].Start();

            foreach (KeyValuePair<int, Thread> kvp in listenerThreads)
            {
                Console.WriteLine("New listener thread started for port: " + kvp.Key);
                listenerThreads[kvp.Key].Start(kvp.Key);
            }
        }

        /// <summary>
        /// This function handles the input in the console
        /// </summary>
        static void HandleInput()
        {
            //Wait for input
            string input = Console.ReadLine();

            //Print routing table
            if (input.StartsWith("R"))
            {
                PrintRoutingTable();
            }

            //Send message
            else if (input.StartsWith("B"))
            {
                string[] message = input.Split();
                SendMessage(int.Parse(message[1]), message[2]);
            }

            //Make connection
            else if (input.StartsWith("C"))
            {

            }

            //Break connection
            else if (input.StartsWith("D"))
            {

            }
            HandleInput();
        }

        /// <summary>
        /// Prints this instance's routing table
        /// </summary>
        static void PrintRoutingTable()
        {
            //TEMP TOTDAT WE EEN ROUTING TABLE HEBBEN
            //HET KAN NAMELIJK VEEL MAKKELIJKER WANNEER WE EENMAAL EEN ROUTING TABLE HEBBEN
            StringBuilder sb = new StringBuilder();
            sb.Append(myPort).Append(space).Append(0).Append(space).Append("local").Append("\n");
            foreach (KeyValuePair<int, Connection> i in neighbours)
            {
                sb.Append(i.Key);
                sb.Append(space);
                sb.Append(CalcDistance(i.Key));
                sb.Append(space);
                sb.Append("temp");
                sb.Append("\n");
            }
            Console.WriteLine(sb);
        }

        //TEMP TOTDAT WE EEN ROUTING TABLE HEBBEN
        //UITEINDELIJK VIA LOOKUP IN "ndis" (zie NetchangeBoek.pdf)
        static int CalcDistance(int destination)
        {
            if (destination == myPort)
                return 0;
            else if (neighbours.ContainsKey(destination))
                return 1;
            else
                return 2;
        }

        /// <summary>
        /// Handles any new incoming connections
        /// </summary>
        static void HandleNewConnection()
        {

        }

        /// <summary>
        /// Waits for incomming messages before it handles them
        /// </summary>
        static void WaitForMessage(object port)
        {
            int p = (int)port;
            try
            {
                while (true)
                    HandleMessage(neighbours[p].Read.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in " + p + " :" + e.Message);
                Console.WriteLine("Sleeping for 2 seconds then trying again");
                Thread.Sleep(2000);
                WaitForMessage(port);
            }
        } 

        /// <summary>
        /// Handles a message
        /// </summary>
        /// <param name="s">message to be handled</param>
        static void HandleMessage(string s)
        {
            //Check if message is meant for this port
            if (s.StartsWith(myPort.ToString()))
            {
                //Remove portnumber and print message
                Console.WriteLine(s.Remove(0, s.IndexOf(' ') + 1));
            }
            //Send message to correct destination
            else
            {
                //STUUR BERICHT DOOR VIA ROUTING TABLE
            }
        }

        static void SendMessage(int dest, string message)
        {
            neighbours[dest].Write.WriteLine(dest + " " + message + " " + myPort);
        }

        /// <summary>
        /// Adds a neighbour to this instance's dictionary
        /// </summary>
        /// <param name="port">Port number of the neighbour</param>
        static void AddNeigbour(int port)
        {
            //Check if neighbour already exists
            if (neighbours.ContainsKey(port))
                Console.WriteLine("Hier is al verbinding naar!");
            //Only create connection with bigger port numbers to avoid double connections
            else if (port >= myPort)
                neighbours.Add(port, new Connection(port));
        }
    }
}

//Code uit voorbeeld
/* namespace MultiClientServer
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
}*/