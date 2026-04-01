using UnityEngine;
using System.Collections.Generic;
using MidiPlayerTK;

public class GestorOrquesta : MonoBehaviour
{
    [System.Serializable]
    public struct InstrumentoObjeto
    {
        public string nombre;
        public GameObject objetoPadre;
    }

    [Header("Referencias de Sonido")]
    public MidiFilePlayer midiPlayer; 

    [Header("Configuración de Instrumentos en Escena")]
    public List<InstrumentoObjeto> instrumentosEnEscena;

    void Start()
    {
        ConfigurarMidi();      
        ConfigurarOrquesta();  
    }

    void ConfigurarMidi()
    {
        if (midiPlayer == null)
        {
            Debug.LogError("¡Falta el MidiFilePlayer en el GestorOrquesta!");
            return;
        }

        // Recuperamos el nombre de la canción guardada en el menú
        string cancion = EstadoOrquesta.cancionSeleccionada;

        if (!string.IsNullOrEmpty(cancion))
        {
            // Asignamos el nombre del MIDI
            midiPlayer.MPTK_MidiName = cancion;
        }
        else
        {
            Debug.LogWarning("No hay ninguna canción seleccionada en EstadoOrquesta.");
        }
    }

    void ConfigurarOrquesta()
    {
        foreach (InstrumentoObjeto inst in instrumentosEnEscena)
        {
            if (EstadoOrquesta.instrumentos.ContainsKey(inst.nombre))
            {
                bool estado = EstadoOrquesta.instrumentos[inst.nombre];
                if (inst.objetoPadre != null)
                {
                    inst.objetoPadre.SetActive(estado);
                }
            }
            else
            {
                if (inst.objetoPadre != null) inst.objetoPadre.SetActive(true);
            }
        }
    }
}



/*using UnityEngine;
using System.Collections.Generic;

public class GestorOrquesta : MonoBehaviour
{
    [System.Serializable]
    public struct InstrumentoObjeto
    {
        public string nombre;
        public GameObject objetoPadre;
    }

    [Header("Configuración de Instrumentos en Escena")]
    public List<InstrumentoObjeto> instrumentosEnEscena;

    void Start()
    {
        ConfigurarOrquesta();
    }

    void ConfigurarOrquesta()
    {
        // IMPORTANTE: Asegúrate de usar 'in' y no 'en' (que es español)
        foreach (InstrumentoObjeto inst in instrumentosEnEscena)
        {
            if (EstadoOrquesta.instrumentos.ContainsKey(inst.nombre))
            {
                bool estado = EstadoOrquesta.instrumentos[inst.nombre];

                if (inst.objetoPadre != null)
                {
                    inst.objetoPadre.SetActive(estado);
                }
            }
            else
            {
                // Si no está en el diccionario, lo dejamos activo por defecto
                if (inst.objetoPadre != null) inst.objetoPadre.SetActive(true);
            }
        }
    } 
} */