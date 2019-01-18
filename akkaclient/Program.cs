using Akka.Actor;
using System;

namespace akkaclient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("I am client!");
            var config = Akka.Configuration.ConfigurationFactory.ParseString(@"
akka {  
    actor {
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
		    port = 0
		    hostname = localhost
        }
    }

");

            using (var system = ActorSystem.Create("client", config))
            {
                string path1 = "akka.tcp://server@localhost:8090/user/Echo";
                string path2 = "akka.tcp://server@127.0.0.1:8090/user/Echo";

                var prop = Props.Create(() => new ClientActor(path1));
                var client = system.ActorOf(prop);
                while (true)
                {

                    Console.WriteLine("enter to continue.");
                    Console.ReadLine();

                    client.Tell(new Begin());

                }
            }
        }
    }


    public class Begin
    {
        public string Username { get; set; }
    }
    class ClientActor : ReceiveActor,ILogReceive
    {
        private ActorSelection _server;

        public ClientActor(string path)
        {
            _server = Context.ActorSelection(path);
            Receive<Begin>((msg) =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    _server.Tell(new byte[8096]);
                }
                //_server.Tell(new shareinfo.Hello("hello there."));
            });
        }
    }



}
