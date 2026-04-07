using UnityEngine;
using TMPro;
using MidiPlayerTK;
using System.Collections.Generic;
using UnityEngine.UI;

public class SelectorMidiPro : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject botonPrefab;  // Tu botón con un componente TMP_Text
    public Transform contenedor;    // El objeto "Content" del Scroll View

    void Start()
    {
        GenerarListaDeCanciones();
    }

    void GenerarListaDeCanciones()
    {
        // Limpiamos el contenedor (por si acaso)
        foreach (Transform hijo in contenedor) Destroy(hijo.gameObject);

        if (MidiPlayerGlobal.MPTK_ListMidi != null)
        {
            Debug.Log("--- GENERANDO LISTA DE " + MidiPlayerGlobal.MPTK_ListMidi.Count + " CANCIONES ---");

            foreach (MPTKListItem midi in MidiPlayerGlobal.MPTK_ListMidi)
            {
                // 1. Instanciamos el botón en el "Content"
                GameObject nuevoBoton = Instantiate(botonPrefab, contenedor);
                nuevoBoton.SetActive(true);

                // 2. Le ponemos el nombre de la canción
                string nombreCancion = midi.Label;
                nuevoBoton.GetComponentInChildren<TMP_Text>().text = nombreCancion;

                // 3. Le asignamos la función de selección al hacer clic
                nuevoBoton.GetComponent<Button>().onClick.AddListener(() => GuardarSeleccion(nombreCancion));
            }

            // Seleccionamos la primera por defecto (opcional)
            if (MidiPlayerGlobal.MPTK_ListMidi.Count > 0)
                EstadoOrquesta.cancionSeleccionada = MidiPlayerGlobal.MPTK_ListMidi[0].Label;
        }
    }

    void GuardarSeleccion(string nombre)
    {
        EstadoOrquesta.cancionSeleccionada = nombre;
        Debug.Log("<color=cyan>EstadoOrquesta actualizado:</color> " + nombre);

        // Aquí podrías añadir una pequeña animación o sonido de "Click"
    }
}


/*using UnityEngine;
using TMPro;
using MidiPlayerTK;
using System.Collections.Generic;

public class SelectorMidi : MonoBehaviour
{
    public TMP_Dropdown miDropdown;

    void Start()
    {
        CargarListaDeCanciones();
    }

    void CargarListaDeCanciones()
    {
        miDropdown.ClearOptions();

        List<string> nombresCanciones = new List<string>();

        // Accedemos a la lista global de Maestro
        Debug.Log("¿Hay lista de midis?");
        if (MidiPlayerGlobal.MPTK_ListMidi != null)
        {
            Debug.Log("--- LISTA DE MIDIS ENCONTRADOS (" + MidiPlayerGlobal.MPTK_ListMidi.Count + ") ---");
            foreach (MPTKListItem midi in MidiPlayerGlobal.MPTK_ListMidi)
            {
                Debug.Log("MIDI Index: " + midi.Index + " - Nombre: " + midi.Label);
                nombresCanciones.Add(midi.Label);
            }
        }

        miDropdown.AddOptions(nombresCanciones);

        // Al inicio, guardamos la primera opción por defecto en EstadoOrquesta
        if (miDropdown.options.Count > 0)
        {
            GuardarSeleccion(miDropdown);
        }

        miDropdown.onValueChanged.AddListener(delegate {
            GuardarSeleccion(miDropdown);
        });
    }

    void GuardarSeleccion(TMP_Dropdown dropdown)
    {
        // CAMBIO CLAVE: Guardamos en la clase estática EstadoOrquesta
        EstadoOrquesta.cancionSeleccionada = dropdown.options[dropdown.value].text;
        Debug.Log("Canción guardada en EstadoOrquesta: " + EstadoOrquesta.cancionSeleccionada);
    }
}*/