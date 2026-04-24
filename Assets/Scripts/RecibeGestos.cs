using System.Net;
using System.Net.Sockets;
using MidiPlayerTK;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;

public class RecibeGestos : MonoBehaviour
{
    enum MessageType : byte
    {
        Ready = 0,
        Start = 1,
        Stop = 2,
        Calderon = 10,
        OffCalderon = 11,
        VolumeUp = 20,
        VolumeDown = 21,
        Volume = 22,
        Tempo = 30
    }

    public MidiFilePlayer midiPlayer;

    [Header("Configuración de Orquesta")]
    public string tagMusicos = "Musico";
    private List<Animator> animadoresValidos = new List<Animator>();
    private bool running = false;

    private TcpListener tcpListener;
    private UdpClient udpClient;

    public TMP_Text textIndicanciones;
    [CanBeNull] private CancellationTokenSource textIndicacionesCancelationToken;

    public void Start()
    {
        Debug.Log("Start");
        if (midiPlayer == null)
            midiPlayer = FindFirstObjectByType<MidiFilePlayer>();

        midiPlayer.MPTK_KeepNoteOff = false;


        ObtenerMusicos();

        textIndicanciones.text = "";
        tcpListener = new TcpListener(IPAddress.Any, 8090);
        tcpListener.Start();

        udpClient = new UdpClient(8090);

        running = true;

        // Workers asincronos
        _ = TcpWorker();
        _ = UDPWorker();
    }

    public void OnDestroy()
    {
        running = false;
        //tcpListener?.Stop();
        //udpClient?.Close();
    }

    private void ObtenerMusicos()
    {
        GameObject[] musicos = GameObject.FindGameObjectsWithTag(tagMusicos);
        animadoresValidos.Clear();
        foreach (GameObject musico in musicos)
        {
            Animator anim = musico.GetComponent<Animator>();
            animadoresValidos?.Add(anim);
        }
        Debug.Log(animadoresValidos.Count + " músicos listos");
    }

    private async Task TcpWorker()
    {
        while (running)
        {
            try
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                var buffer = new byte[client.ReceiveBufferSize];
                var stream = client.GetStream();

                while (client.Connected)
                {
                    var length = await stream.ReadAsync(buffer);
                    if (length == 0) break;
                    var message = new byte[length];
                    Array.Copy(buffer, message, length);
                    _ = HandleGesture(message);
                }
                client.Close();
            }
            catch (Exception e) { 
                Debug.LogError(e); 
            }
        }
    }

    private async Task UDPWorker()
    {
        while (running)
        {
            try
            {
                var r = await udpClient.ReceiveAsync();
                _ = HandleGesture(r.Buffer);
            }
            catch (Exception e) { 
                Debug.LogError(e); 
            }
        }
    }

    private async Task SetIndication(string text, int delayClear = 750)
    {
        try
        {
            textIndicacionesCancelationToken ??= new CancellationTokenSource();
            textIndicanciones.text = text;
            await Task.Delay(delayClear, textIndicacionesCancelationToken.Token);
            textIndicacionesCancelationToken.Token.ThrowIfCancellationRequested();
            textIndicanciones.text = "";
        }
        catch
        {
            // Ignore
        }
    }

    #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task HandleGesture(byte[] message)
    {
        if (message == null || message.Length == 0) return;
        if (!Enum.IsDefined(typeof(MessageType), message[0])) return;

        var messageType = (MessageType)message[0];

        Debug.Log(messageType);
        // // 1. Estado de Preparación
        if (messageType == MessageType.Ready)
        {
            _ = SetIndication("Director listo");
            midiPlayer.MPTK_Stop();
            midiPlayer.MPTK_TickCurrent = 0;

            foreach (Animator anim in animadoresValidos)
            {
                if (anim == null) continue;

                anim.SetTrigger("doReady");
                anim.SetBool("isPlaying", false);
            }

            return;
        }

        // 2. Inicio de la música
        if (messageType == MessageType.Start)
        {
            _ = SetIndication("Comienzo");

            if (!midiPlayer.MPTK_IsPlaying)
                midiPlayer.MPTK_Play();
            else
                midiPlayer.MPTK_UnPause();

            foreach (Animator anim in animadoresValidos)
            {
                if (anim == null) continue;
                anim.SetBool("isPlaying", true);
            }
            return;
        }

        // 3. Finalización (Stop)
        if (messageType == MessageType.Stop)
        {
            _ = SetIndication("Finalización");
            midiPlayer.MPTK_Stop();

            if (midiPlayer.MPTK_Volume < 0.2f) midiPlayer.MPTK_Volume = 0.5f;

            foreach (Animator anim in animadoresValidos)
            {
                if(anim == null) continue;

                anim.SetTrigger("doStop");
                anim.SetBool("isPlaying", false);
            }
            return;
        }

        // Volumen 
        if (messageType == MessageType.Volume)
        {
            _ = SetIndication("Cambio de Volumen", 500);         
            var powerNormalized = message[1];
            var power = powerNormalized / 100f;

            midiPlayer.MPTK_Volume = power;

            return;
        }
    }
}
