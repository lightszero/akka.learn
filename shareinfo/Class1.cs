using Akka.Actor;
using Akka.Serialization;
using System;

namespace shareinfo
{
    public class MySerializer : Serializer
    {
        public MySerializer(ExtendedActorSystem system) : base(system)
        {
        }

        /// <summary>
        /// This is whether <see cref="FromBinary"/> requires a <see cref="Type"/> or not
        /// </summary>
        public override bool IncludeManifest { get; } = false;

        /// <summary>
        /// Completely unique value to identify this implementation of the
        /// <see cref="Serializer"/> used to optimize network traffic
        /// 0 - 40 is reserved by Akka.NET itself
        /// </summary>
        public override int Identifier => 1234567;

        /// <summary>
        /// Serializes the given object to an Array of Bytes
        /// </summary>
        public override byte[] ToBinary(object obj)
        {
            // Put the real code that serializes the object here
            return obj as byte[];
        }

        /// <summary>
        /// Deserializes the given array,  using the type hint (if any, see <see cref="IncludeManifest"/> above)
        /// </summary>
        public override object FromBinary(byte[] bytes, Type type)
        {
            // Put the real code that deserializes here
            return bytes;
        }
    }
    public class EchoActor : ReceiveActor
    {
        public EchoActor()
        {
            Receive<Hello>(hello =>
            {
                Console.WriteLine("--hello[{0}]: {1}", Sender, hello.Message);
                Sender.Tell(hello);
            });
        }
    }

    public class Hello
    {
        public Hello(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
