using System.Net;
using System.Net.Sockets;
using MidiPlayerTK;
using UnityEngine;
using System;
using System.Threading;
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
        
        textIndicanciones.text = "";
        
        calderonActive = false; // Corregido: ya no sombrea la variable global

        tcpListener = new TcpListener(IPAddress.Any, 5005);
        tcpListener.Start();

        udpClient = new UdpClient(5005);
        
        running = true;
        
        _ = TcpWorker();
        _ = UDPWorker();
    }

    public void OnDestroy()
    {
        running = false;
    }

    private async Task TcpWorker()
    {
        while (running)
        {
            try
            {
                var client = await tcpListener.AcceptTcpClientAsync();                        // waits for 'client' to 'connect'
                
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
            } catch (Exception e) { 
                Debug.LogError(e);
            }
        }
    }

    public void Update()
    {
        // 1. Lógica del Calderón (KeepNoteOff evita que las notas se detengan)
        // Si calderonActive es true, KeepNoteOff debe ser true.
        midiPlayer.MPTK_KeepNoteOff = calderonActive;
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

        // Comprobación de que el messageType es válido
        if (!Enum.IsDefined(typeof(MessageType), message[0])) return;
        
        var messageType = (MessageType) message[0];
        // Debug.Log("Gesto recibido: " + messageType);
        
        // 1. Estado de Preparación
        if (messageType == MessageType.Ready)
        {
            _ = SetIndication("Director listo");
            // Si el MIDI estaba pausado, lo prepara
            //midiPlayer.MPTK_UnPause();
            // Opcional: Podrías bajar el volumen o resetear la posición al inicio
            //midiPlayer.MPTK_TickCurrent = 0;
            //Debug.Log("Director preparado...");

            midiPlayer.MPTK_Stop();
            midiPlayer.MPTK_TickCurrent = 0;
            calderonActive = false; // Resetear estados
            // Debug.Log("Director en posición correcta.");
            return;
        }

        // 2. Inicio de la música (al detectar movimiento)
        if (messageType == MessageType.Start)
        {
            _ = SetIndication("Comienzo");

            if (!midiPlayer.MPTK_IsPlaying)
                midiPlayer.MPTK_Play();
            else
                midiPlayer.MPTK_UnPause();

            // Debug.Log("Iniciando música...");
            return;
        }

        // 3. Finalización (Cut-off)
        if (messageType == MessageType.Stop)
        {
            _ = SetIndication("Finalización");
            //midiPlayer.MPTK_Stop();
            //Debug.Log("Final de la pieza.");
            midiPlayer.MPTK_Stop();
            // Importante: Asegurar que el volumen no se quede en 0
            if (midiPlayer.MPTK_Volume < 0.2f) midiPlayer.MPTK_Volume = 0.5f;
            // Debug.Log("Parando la música...");
            return;
        }

        // Gesto del calderon
        if (messageType == MessageType.Calderon)
        {
            _ = SetIndication("Calderón");
            calderonActive = true;
            return;
        }
        
        if (messageType == MessageType.OffCalderon)
        {
            _ = SetIndication("Fin Calderón", 500);
            calderonActive = false; // Necesitas una señal para apagarlo
            return;
        }

        // // Resto de gestos        
        // if (messageType == MessageType.Start) midiPlayer.MPTK_Play();
        // if (messageType == MessageType.Stop) midiPlayer.MPTK_Stop();
        // if (messageType == MessageType.Ready) midiPlayer.MPTK_UnPause();

        // Volumen 
        if (messageType == MessageType.VolumeUp)
        {

            _ = SetIndication("+ Volumen", 500);
            var powerNormalized = message[1];

            var power = powerNormalized / 100f;

            if (power + midiPlayer.MPTK_Volume >= 1.0f)
            {
                midiPlayer.MPTK_Volume = 1.0f;
            } else {
                midiPlayer.MPTK_Volume += power ;
            }

            // Debug.Log("Subiendo volumen...");
            return;
        }
        
        if (messageType == MessageType.VolumeDown)
        {
            _ = SetIndication("- Volumen", 500);
            
            var powerNormalized = message[1];

            var power = powerNormalized / 100f;

            if (midiPlayer.MPTK_Volume - power <= 0.2f)
            {
                midiPlayer.MPTK_Volume = 0.2f;
            } else {
                midiPlayer.MPTK_Volume -= power ;
            }

            // Debug.Log("Bajando volumen...");
            return;
        }
    }
}