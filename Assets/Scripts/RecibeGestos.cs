using System.Net;
using System.Net.Sockets;
using MidiPlayerTK;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

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
    // Para manejar los hilos de forma segura
    private byte[] lastMessage;
    private bool newMessageReceived = false;
    private readonly object lockObject = new object();

    void Start()
    {
        Debug.Log("Buenas noches caballero");
        if (midiPlayer == null)
            midiPlayer = FindFirstObjectByType<MidiFilePlayer>();

        calderonActive = false; // Corregido: ya no sombrea la variable global

        tcpListener = new TcpListener(IPAddress.Any, 5005);
        tcpListener.Start();

        udpClient = new UdpClient(5005);
        
        running = true;
        
        _ = TcpWorker();
        _ = UDPWorker();
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
                    
                    lock (lockObject)
                    {
                        lastMessage = message;
                        newMessageReceived = true;
                    }
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
                lock (lockObject)
                {
                    lastMessage = r.Buffer;
                    newMessageReceived = true;
                }
            } catch (Exception e) { 
                Debug.LogError(e);
            }
        }
    }

    void Update()
    {
        // 1. Procesar mensajes TCP/UDP en el hilo principal
        if (newMessageReceived)
        {
            byte[] msg;
            lock (lockObject)
            {
                msg = lastMessage;
                newMessageReceived = false;
            }
            HandleGesture(msg);
        }

        // 2. Lógica del Calderón (KeepNoteOff evita que las notas se detengan)
        // Si calderonActive es true, KeepNoteOff debe ser true.
        midiPlayer.MPTK_KeepNoteOff = calderonActive;
    }

    void HandleGesture(byte[] message)
    {

        // Comprobación de que el messageType es válido
        if (!Enum.IsDefined(typeof(MessageType), message[0])) return;
        
        var messageType = (MessageType) message[0];
        // Debug.Log("Gesto recibido: " + messageType);
        
        // 1. Estado de Preparación
        if (messageType == MessageType.Ready)
        {
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
            calderonActive = true;
            return;
        }
        
        if (messageType == MessageType.OffCalderon)
        {
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
            midiPlayer.MPTK_Volume += 0.25f;
            // Debug.Log("Subiendo volumen...");
            return;
        }
        
        if (messageType == MessageType.VolumeDown)
        {
            midiPlayer.MPTK_Volume -= 0.15f;
            // Debug.Log("Bajando volumen...");
            return;
        }
    }
}