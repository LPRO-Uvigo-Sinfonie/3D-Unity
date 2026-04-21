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
        Tempo = 30
    }

    public MidiFilePlayer midiPlayer;

    [Header("Configuración de Orquesta")]
    public string tagMusicos = "Musico";
    private List<Animator> animadoresValidos = new List<Animator>();
    public bool calderonActive;
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

        // Carga de configuración de canales
        foreach (var c in midiPlayer.MPTK_Channels)
        {
            var presetNum = c.PresetNum;
            var bankNum = c.BankNum;
            var presetForced = c.ForcedPreset;
            var sPreset = presetForced == -1 ? $"{presetNum} / {bankNum}" : $"F{presetForced} / {bankNum}";
            Debug.LogFormat(sPreset);
        }

        textIndicanciones.text = "";
        calderonActive = false;

        tcpListener = new TcpListener(IPAddress.Any, 5005);
        tcpListener.Start();

        udpClient = new UdpClient(5005);

        running = true;

        // Lanzamiento de workers asíncronos (Mejora de rendimiento)
        _ = TcpWorker();
        _ = UDPWorker();

        // Buscar los musicos activos que tienen animacion (Lógica VR)
        ObtenerMusicos();
    }

    public void OnDestroy()
    {
        running = false;
        tcpListener?.Stop();
        udpClient?.Close();
    }

    private void ObtenerMusicos()
    {
        GameObject[] musicos = GameObject.FindGameObjectsWithTag(tagMusicos);
        animadoresValidos.Clear();
        foreach (GameObject musico in musicos)
        {
            Animator anim = musico.GetComponent<Animator>();
            if (anim != null) animadoresValidos.Add(anim);
        }
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
            catch (Exception e) { Debug.LogError(e); }
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
            catch (Exception e) { Debug.LogError(e); }
        }
    }

    public void Update()
    {
        midiPlayer.MPTK_KeepNoteOff = calderonActive;
    }

    private async Task SetIndication(string text, int delayClear = 750)
    {
        try
        {
            textIndicacionesCancelationToken?.Cancel();
            textIndicacionesCancelationToken = new CancellationTokenSource();
            textIndicanciones.text = text;
            await Task.Delay(delayClear, textIndicacionesCancelationToken.Token);
            textIndicanciones.text = "";
        }
        catch (OperationCanceledException) { /* Ignorar al cancelar */ }
        catch (Exception e) { Debug.LogWarning(e); }
    }

    private async Task HandleGesture(byte[] message)
    {
        if (message == null || message.Length == 0) return;
        if (!Enum.IsDefined(typeof(MessageType), message[0])) return;

        var messageType = (MessageType)message[0];

        // 1. Estado de Preparación
        if (messageType == MessageType.Ready)
        {
            _ = SetIndication("Director listo");
            midiPlayer.MPTK_Stop();
            midiPlayer.MPTK_TickCurrent = 0;
            calderonActive = false;

            foreach (Animator anim in animadoresValidos)
            {
                if (anim != null)
                {
                    anim.SetTrigger("doReady");
                    anim.SetBool("isPlaying", false);
                }
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
                if (anim != null) anim.SetBool("isPlaying", true);
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
                if (anim != null) anim.SetBool("isPlaying", false);
            }
            return;
        }
    }
}
