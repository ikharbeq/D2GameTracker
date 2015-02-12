using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading;

namespace Project1
{
    class D2GameTracker
    {        
        public static void Main()
        {
            IPGlobalProperties ipGlobal = IPGlobalProperties.GetIPGlobalProperties();
            int counter = 0;
            string endpoint="";
            string state = "";
            string port = "";
            LinkedList<coninfo> coninformation = new LinkedList<coninfo>();
            while (true)
            {                
                Console.Clear();
                ipGlobal = IPGlobalProperties.GetIPGlobalProperties();
                LinkedList<TcpConnectionInformation> currentlistcompare = new LinkedList<TcpConnectionInformation>();
                Array.ForEach<TcpConnectionInformation>(ipGlobal.GetActiveTcpConnections(),delegate(TcpConnectionInformation i){             //current cons, find all games add new ones to list
                    endpoint = i.RemoteEndPoint.ToString();
                    state = i.State.ToString();
                    port = i.LocalEndPoint.ToString().Substring(i.LocalEndPoint.ToString().IndexOf(";")+1);
                    Boolean connectionexists = false;
                    if(endpoint.EndsWith(":4000")&&state.Equals("Established"))//4000 is the port number for game connection in d2
                    {
                        for (int j = 0; j < coninformation.Count; j++)
                        {
                            if (coninformation.ElementAt(j).open && coninformation.ElementAt(j).port.Equals(port))
                            {
                                connectionexists = true;
                                coninformation.ElementAt(j).connected = true;
                                break;
                            }
                        }
                        if (!connectionexists)
                        {
                            coninformation.AddLast(new coninfo(DateTime.Now, i));
                            counter++;
                        }
                    }
                });
                Console.WriteLine("games last hour:" + counter);
                for (int j = 0; j < coninformation.Count; j++) //print out active connections
                {

                    if (coninformation.ElementAt(j).open)
                    {
                        if (coninformation.ElementAt(j).connected)
                        {
                            Console.WriteLine("current: server: {0} duration: {1}", coninformation.ElementAt(j).server, DateTime.Now.Subtract(coninformation.ElementAt(j).start));

                        }
                        else
                        {
                            coninformation.ElementAt(j).close();
                        }
                    }
                    else
                    {
                        if (coninformation.ElementAt(j).end.AddHours(1).CompareTo(DateTime.Now) < 0) //remove connections that ended more than 1 hour ago from list
                        {
                            coninformation.Remove(coninformation.ElementAt(j));
                            counter--;
                        }
                    }
                    
                }
                for (int j = coninformation.Count; j > 0; j--) //print out list of closed connections
                {
                    if (!coninformation.ElementAt(j-1).open)
                    {
                        Console.WriteLine("game: {0} server: {1} duration: {2}",j, coninformation.ElementAt(j-1).server, coninformation.ElementAt(j-1).duration());
                    }
                    coninformation.ElementAt(j-1).connected = false;
                }            
                Thread.Sleep(1000);
            }
            
        }
    }
    class coninfo 
    {
        public DateTime start;
        public DateTime end;
        public String server;
        public String port;
        public Boolean open =true;
        public Boolean connected = true;
        public coninfo(DateTime s, TcpConnectionInformation t)
        {
            start = s;
            String endpoint = t.RemoteEndPoint.ToString();
            if (endpoint.Substring(endpoint.Length - 8, 1).Equals("."))
            {
                server=(endpoint.Substring(endpoint.Length - 7, 2));
            }
            else
            {
                server=(endpoint.Substring(endpoint.Length - 8, 3));
            }
            endpoint = t.LocalEndPoint.ToString();
            port = endpoint.Substring(endpoint.IndexOf(";")+1);
            open = true;
            connected = true;
        }
        public String duration() 
        {
            return end.Subtract(start).ToString();
        }
        public void close() 
        {
            end = DateTime.Now;
            open = false;
        }
    }
}