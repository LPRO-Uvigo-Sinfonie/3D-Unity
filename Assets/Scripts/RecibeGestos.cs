using System.Net;
using System.Net.Sockets;
using System.Text;
using MidiPlayerTK;
using UnityEngine;
using System.IO;
using System;
using System.Threading;

public class RecibeGestos : MonoBehaviour
{
    public MidiFilePlayer midiPlayer;
    public bool calderonActive;
    private bool running = false;

    private TcpListener listener;
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

        listener = new TcpListener(IPAddress.Any, 5005);
        listener.Start();

        running = true;

        Thread w = new Thread(worker);
        w.Start();
    }

    private void worker()
    {
        while (this.running)
        {
            try
            {
                var s = this.listener.AcceptSocket();                        // waits for 'client' to 'connect'

                while (s.Connected)
                {
                    byte[] d = new byte[s.ReceiveBufferSize];

                    int length = s.Receive(d);

                    if (length == 0) break;

                    string message = Encoding.UTF8.GetString(d);

                    lock (lockObject)
                    {
                        lastMessage = message;
                        newMessageReceived = true;
                    }
                    
                }

                s.Close();
            }
            catch (Exception) { 

            }
        }
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

        // 1. Estado de Preparación
        if (message == "READY")
        {
            // Si el MIDI estaba pausado, lo prepara
            //midiPlayer.MPTK_UnPause();
            // Opcional: Podrías bajar el volumen o resetear la posición al inicio
            //midiPlayer.MPTK_TickCurrent = 0;
            //Debug.Log("Director preparado...");

            midiPlayer.MPTK_Stop();
            midiPlayer.MPTK_TickCurrent = 0;
            calderonActive = false; // Resetear estados
            Debug.Log("Director en posición correcta.");
        }

        // 2. Inicio de la música (al detectar movimiento)
        if (message == "START")
        {
            if (!midiPlayer.MPTK_IsPlaying)
                midiPlayer.MPTK_Play();
            else
                midiPlayer.MPTK_UnPause();

            Debug.Log("Iniciando música...");
        }

        // 3. Finalización (Cut-off)
        if (message == "STOP")
        {
            //midiPlayer.MPTK_Stop();
            //Debug.Log("Final de la pieza.");
            midiPlayer.MPTK_Stop();
            // Importante: Asegurar que el volumen no se quede en 0
            if (midiPlayer.MPTK_Volume < 0.2f) midiPlayer.MPTK_Volume = 0.5f;
            Debug.Log("Parando la música...");
        }

        // Gesto del calderon
        if (message == "CALDERON") calderonActive = true;
        else if (message == "OFF_CALDERON") calderonActive = false; // Necesitas una señal para apagarlo

        // Resto de gestos        
        if (message == "START") midiPlayer.MPTK_Play();
        if (message == "STOP") midiPlayer.MPTK_Stop();
        if (message == "READY") midiPlayer.MPTK_UnPause();

        // Volumen 
        if (message == "VOLUME_UP")
        {
            midiPlayer.MPTK_Volume += 0.25f;
            Debug.Log("Subiendo volumen...");
        }
        else if (message == "VOLUME_DOWN")
        {
            midiPlayer.MPTK_Volume -= 0.15f;
        }
    }
}