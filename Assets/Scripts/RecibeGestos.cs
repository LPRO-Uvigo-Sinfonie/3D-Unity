using System.Net;
using System.Net.Sockets;
using System.Text;
using MidiPlayerTK;
using UnityEngine;

public class RecibeGestos : MonoBehaviour
{
    public MidiFilePlayer midiPlayer;
    public bool calderonActive;

    UdpClient client;
    int port = 5005;

    // Para manejar los hilos de forma segura
    private string lastMessage = "";
    private bool newMessageReceived = false;
    private readonly object lockObject = new object();

    void Start()
    {
        Debug.Log("Buenas noches caballero");
        if (midiPlayer == null)
            midiPlayer = FindObjectOfType<MidiFilePlayer>();

        calderonActive = false; // Corregido: ya no sombrea la variable global

        client = new UdpClient(port);
        client.BeginReceive(Receive, null);
    }

    void Receive(System.IAsyncResult result)
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        byte[] data = client.EndReceive(result, ref ip);
        string message = Encoding.UTF8.GetString(data);

        // Guardamos el mensaje de forma segura para el hilo principal
        lock (lockObject)
        {
            lastMessage = message;
            newMessageReceived = true;
        }

        client.BeginReceive(Receive, null);
    }

    void Update()
    {
        // 1. Procesar mensajes UDP en el hilo principal
        if (newMessageReceived)
        {
            string msg;
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

    void HandleGesture(string message)
    {
        Debug.Log("Gesto recibido: " + message);

        // Gesto del calderon
        if (message == "CALDERON") calderonActive = true;
        else if (message == "OFF_CALDERON") calderonActive = false; // Necesitas una señal para apagarlo

        // Resto de gestos        
        if (message == "START") midiPlayer.MPTK_Play();
        if (message == "STOP") midiPlayer.MPTK_Stop();
        if (message == "READY") midiPlayer.MPTK_UnPause();

        // Volumen (Corregido el error de las llaves {})
        if (message == "VOLUME_UP")
        {
            midiPlayer.MPTK_Volume += 0.50f;
            Debug.Log("Subiendo volumen...");
        }
        if (message == "VOLUME_DOWN")
        {
            midiPlayer.MPTK_Volume -= 0.15f;
        }
    }
}