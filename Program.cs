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
        /// This instance's randomizer
        /// </summary>
        static Random r = new Random();

        /// <summary>
        /// When we broadcast an update of creation, we only want to handle that broadcast once, so we use serial numbers to keep uniqueness
        /// </summary>
        static List<int> knownIDs = new List<int>();

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
            BroadcastOwn();
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
            StringBuilder sb = new StringBuilder();
            foreach(KeyValuePair<int, int> kvp in Nbu)
            {
                sb.Append(kvp.Key);
                sb.Append(space);
                sb.Append(Du[kvp.Key]);
                sb.Append(space);
                sb.Append(kvp.Value);
                sb.Append("\n");
            }
            Console.WriteLine(sb);
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
                Console.WriteLine(s);
                // X means we have a broadcast message that needs to be sent to all reachable nodes
                // format: 0"[destinationNr]" + " " + 1"X" + " " + 2"Create"/"Update"/... + " " + 3"[portNr of sender]" + " " + 4"[portNr of initiator of broadcast]" + " " + 5"[cycleNr]" + " " + 6"[ID]" + " " + 7"[hops]"
                if (s.Split()[1][0] == 'X')
                {
                    string[] mes = s.Split();
                    if (int.Parse(mes[5]) > 20)
                        return;

                    if (knownIDs.Contains(int.Parse(mes[6]))) // If we have seen this broadcast before or the message is older than the amount of nodes in our system, we stop it here.
                        return;
                    else
                        knownIDs.Add(int.Parse(mes[6]));

                    if (mes[2] == "Create") // A process wants us to know that he has been created
                    {
                        if (!Nbu.ContainsKey(int.Parse(mes[5])))
                        {
                            Du.Add(int.Parse(mes[4]), int.Parse(mes[7])+1);
                            Nbu.Add(int.Parse(mes[4]), int.Parse(mes[3]));
                        }
                        else if (Du[int.Parse(mes[4])] > int.Parse(mes[7]+1))
                        {
                            Du[int.Parse(mes[4])] = int.Parse(mes[7] + 1);
                            Nbu[int.Parse(mes[4])] = int.Parse(mes[3]);
                        }
                        Broadcast(mes);
                        BroadcastUpdate();
                        
                        // HOPS AND CYLCE + 1
                        // FORWARD INC BROADCAST
                        // BROADCAST OUR OWN CREATION WITH [ID] TO ALL
                    }
                }
                else
                {
                    //Remove portnumber and print message
                    Console.WriteLine(s.Remove(0, s.IndexOf(' ') + 1));
                }
            }
            //Send message to correct destination
            else
            {
                int dest = int.Parse(s.Split()[0]);
                SendMessage(s);
                Console.WriteLine("Bericht voor " + dest + " doorgestuurd naar " + Nbu[dest]);
            }
        }

        /// <summary>
        /// Initiate a broadcast sequence
        /// </summary>
        static void BroadcastOwn()
        {            
            foreach (KeyValuePair<int, Connection> thisn in neighbours)
            {
                StringBuilder message = new StringBuilder();
                message.Append(thisn.Key);
                message.Append(space).Append('X');
                message.Append(space).Append("Create");
                message.Append(space).Append(myPort);
                message.Append(space).Append(myPort);
                message.Append(space).Append(0);
                message.Append(space).Append(r.Next(100000));
                message.Append(space).Append(0);
                SendMessage(message.ToString());
            }
        }

        /// <summary>
        /// Relay a broadcast sequence
        /// </summary>
        /// <param name="mes">message to relay</param>
        static void Broadcast(string[] mes)
        {
            // format: 0"[destinationNr]" + " " + 1"X" + " " + 2"Create"/"Update"/... + " " + 3"[portNr of sender]" + " " + 4"[portNr of initiator of broadcast]" + " " + 5"[cycleNr]" + " " + 6"[ID]" + " " + 7"[hops]"
            foreach (KeyValuePair<int, Connection> thisn in neighbours)
            {
                StringBuilder message = new StringBuilder();
                message.Append(thisn.Key);
                message.Append(space).Append(mes[1]);
                message.Append(space).Append(mes[2]);
                message.Append(space).Append(myPort);
                message.Append(space).Append(mes[4]);
                message.Append(space).Append((int.Parse(mes[5]) + 1));
                message.Append(space).Append(mes[6]);
                message.Append(space).Append((int.Parse(mes[7]) + 1));
                SendMessage(message.ToString());
            }
        }

        /// <summary>
        /// Update the rest of the network
        /// </summary>
        static void BroadcastUpdate()
        {
            foreach (KeyValuePair<int, Connection> thisN in neighbours)
            {
                foreach(KeyValuePair<int, int> thisR in Nbu)
                {
                    if (thisR.Key != thisN.Key)
                    {
                        StringBuilder message = new StringBuilder();
                        message.Append(thisN.Key);
                        message.Append(space).Append('X');
                        message.Append(space).Append("Create");
                        message.Append(space).Append(myPort);
                        message.Append(space).Append(thisR.Key);
                        message.Append(space).Append(0);
                        message.Append(space).Append(r.Next(100000));
                        message.Append(space).Append(Du[thisR.Key]);
                        SendMessage(message.ToString());
                    }
                }
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
            int dest = int.Parse(destMessage.Split(' ')[0]);
            try
            {
                neighbours[dest].Write.WriteLine(destMessage);
            }
            catch (Exception e)
            {
                try
                {
                    neighbours[Nbu[dest]].Write.WriteLine(destMessage);
                }
                catch (Exception e2)
                {
                    Console.WriteLine("Error in SendMessage: " + e.Message + " and: " + e2);
                }
            }
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