using AsyncIO;
using System.Threading;
using System;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

/// <summary>
///     Example of requester who only sends Hello. Very nice guy.
///     You can copy this class and modify Run() to suits your needs.
///     To use this class, you just instantiate, call Start() when you want to start and Stop() when you want to stop.
/// </summary>
public class Zeromq : RunAbleThread
{
    /// <summary>
    ///     Request Hello message to server and receive message back. Do it 10 times.
    ///     Stop requesting when Running=false.
    /// </summary>
    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (var responder = new ResponseSocket())
        {
            responder.Bind("tcp://*:5555");

            for (int i = 0; i < 10 && Running; i++)
            {
                string str = responder.ReceiveFrameString();
                Console.WriteLine("Received Hello");
                Thread.Sleep(1000);  //  Do some 'work'
                responder.SendFrame("World");
            }
        }

        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }
}
