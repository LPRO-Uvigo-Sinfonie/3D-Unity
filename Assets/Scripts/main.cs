using UnityEngine;
using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

public class main : MonoBehaviour
{
    private Zeromq _zeromq;

    private void Start()
    {
        _zeromq = new Zeromq();
        _zeromq.Start();
    }

    private void OnDestroy()
    {
        _zeromq.Stop();
    }
}
