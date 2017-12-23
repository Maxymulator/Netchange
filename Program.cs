﻿using System;
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
                //Console.WriteLine("New listener thread started for port: " + kvp.Key);
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
                int dest = int.Parse(input.Split()[1]);
                if (Nbu.ContainsKey(dest))
                    SendMessage(input.Remove(0, input.IndexOf(' ') + 1));
                else
                    Console.WriteLine("Poort " + dest + " is niet bekend");
            }

            //Make connection
            else if (input.StartsWith("C"))
            {
                int dest = int.Parse(input.Split()[1]);
                //MAKE CONNECTION
                Console.WriteLine("Verbonden: " + dest);
            }

            //Break connection
            else if (input.StartsWith("D"))
            {
                int dest = int.Parse(input.Split()[1]);
                if (Nbu.ContainsKey(dest))
                {
                    //BREAK CONNECTION
                    Console.WriteLine("Verbroken: " + dest);
                }
                else
                    Console.WriteLine("Poort " + dest + " is niet bekend");
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
                Thread.Sleep(1000);
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
                // X means we have a broadcast message that needs to be sent to all reachable nodes
                // format: "[destinationNr]" + " " + "X" + " " + "Create"/"Update"/... + " " + "[portNr of sender]" + " " + "[portNr of initiator of broadcast]" + " " + "[cycleNr]" + " " + "ID"
                if (s.Split()[1][0] == 'X')
                {
                    string[] mes = s.Split();

                    if (mes[2] == "Create") // A process wants us to know that he has been created
                    {

                    }
                }


                //Remove portnumber and print message
                Console.WriteLine(s.Remove(0, s.IndexOf(' ') + 1));
            }
            //Send message to correct destination
            else
            {
                int dest = int.Parse(s.Split()[0]);
                //STUUR BERICHT DOOR VIA ROUTING TABLE
                Console.WriteLine("Bericht voor " + dest + " doorgestuurd naar " + Nbu[dest]);
            }
        }

        /// <summary>
        /// Send a message to a certain port
        /// </summary>
        /// <param name="dest">The destination port</param>
        /// <param name="message">The message to be sent</param>
        static void SendMessage(int dest, string message)
        {
            neighbours[dest].Write.WriteLine(dest + " " + message);
        }

        /// <summary>
        /// Send a message to a certain port
        /// </summary>
        /// <param name="destMessage">the destination and message in a single string, divided by a space</param>
        static void SendMessage(string destMessage)
        {
            int dest = int.Parse(destMessage.Split()[0]);
            neighbours[dest].Write.WriteLine(destMessage);
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