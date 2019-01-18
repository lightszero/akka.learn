using Akka.Actor;
using Akka.Configuration;
using Akka.Remote;
using System;
using System.Collections.Generic;
using System.Net;

namespace testakka
{

    class Program
    {
        static void AddAkkaRemoteHost(ActorSystem system, string[] morehost = null)
        {
            var ext = system as ExtendedActorSystem;
            IRemoteActorRefProvider provider = ext.Provider as IRemoteActorRefProvider;
            var addrs = provider.Transport.Addresses;
            var addrslist = addrs.GetEnumerator();
            addrslist.MoveNext();
            Address first = addrslist.Current;
            if (first.Host != "localhost")
                addrs.Add(new Address(first.Protocol, first.System, "localhost", first.Port));
            if (first.Host != "127.0.0.1")
                addrs.Add(new Address(first.Protocol, first.System, "127.0.0.1", first.Port));

            if (morehost != null)
            {
                foreach (var host in morehost)
                {
                    if (first.Host != host)
                        addrs.Add(new Address(first.Protocol, first.System, host, first.Port));

                }
            }
            foreach(var add in addrs)
            {
                Console.WriteLine("host at:" + add.ToString());
            }
        }

        static void Main(string[] args)
        {
            #region
            Console.WriteLine("I am akka server.");
            var config = Akka.Configuration.ConfigurationFactory.ParseString(@"
            akka {  
                actor{
                    provider = remote

                    serializers {
                        json = ""Akka.Serialization.NewtonSoftJsonSerializer""
                        bytes = ""Akka.Serialization.ByteArraySerializer""
                        myown = ""shareinfo.MySerializer, shareinfo""
                     }

                    serialization-bindings {
                        ""System.Byte[]"" = myown
                    }
                 }
                remote {
                    dot-netty.tcp {
                        port = 8090
                        hostname = 0.0.0.0
                    }
                }
            }");
            #endregion
            using (var system = ActorSystem.Create("server", config))
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                List<string> myip = new List<string>();
                foreach(var ip in IpEntry.AddressList)
                {
                    if(ip.AddressFamily== System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        myip.Add(ip.ToString());
                    }
                }
                AddAkkaRemoteHost(system,myip.ToArray());

                system.ActorOf(
                    Props.Create(
                        () => new ChatServerActor()
                        )
                    , "Echo"
                    );

                Console.ReadKey();
            }
        }
    }
    class ChatServerActor : ReceiveActor
    {
        ulong recvlen = 0;
        ulong recvcount = 0;
        DateTime begin;

        public ChatServerActor()
        {
            Receive<shareinfo.Hello>((msg) =>
            {
                Console.WriteLine("hihihi" + msg.Message);
            });
            Receive<byte[]>(data =>
            {
                recvlen += (uint)data.Length;
                if (recvcount == 0)
                {
                    begin = DateTime.Now;
                }
                recvcount++;
                if (recvcount % 5000 == 0)
                {
                    var end = DateTime.Now;
                    Console.WriteLine("recv bytes:" + recvlen + " span=" + (end - begin));
                }
            });
            //Receive<ChatMessages.ConnectRequest>(message =>
            //{
            //    Console.WriteLine("info in");

            //    //_clients.Add(Sender);
            //    //Sender.Tell(new ConnectResponse
            //    //{
            //    //    Message = "Hello and welcome to Akka.NET chat example",
            //    //}, Self);
            //});
        }
    }
}
