
#nullable enable
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using MidiPlayerTK;
using UnityPdfViewer;

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
        CargarPdf();
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
    
    void CargarPdf()
    {
        if (MidiPlayerGlobal.MPTK_ListMidi != null)
        {
            // Recuperamos el nombre de la canción guardada en el menú
            var cancion = EstadoOrquesta.cancionSeleccionada;

            if (cancion == "Ninguna") cancion = "paquito-chocolatero";

            var pdfFolderPath = Path.Join(Directory.GetCurrentDirectory(), "pdf");
            
            var pdfPath = Path.Join(pdfFolderPath, $"{cancion}.pdf");            
            // Debug.Log(pdfPart1Path);
            
            if (!File.Exists(pdfPath)) return;
            
            var pdfViewerUI1 = GameObject.Find("PdfViewer1").GetComponent<PdfViewerUI>();
            pdfViewerUI1.LoadPDF(pdfPath);

            var pdfViewerUI2 = GameObject.Find("PdfViewer2").GetComponent<PdfViewerUI>();
            pdfViewerUI2.LoadPDF(pdfPath);
            
            if (pdfViewerUI2.navigator.CurrentPage == pdfViewerUI2.navigator.TotalPages - 1)
            {
                pdfViewerUI2.pdfImage.texture = null;
            } else
            {
                pdfViewerUI2.NextPage();
            }
            
            pdfViewerUI1.nextButton.onClick.AddListener(() =>
            {

                pdfViewerUI1.NextPage();
                
                if (pdfViewerUI1.navigator.CurrentPage == pdfViewerUI1.navigator.TotalPages - 2)
                {
                    pdfViewerUI1.nextButton.interactable = false;
                }
                
                if (pdfViewerUI2.navigator.CurrentPage == pdfViewerUI2.navigator.TotalPages - 2)
                {
                    pdfViewerUI2.pdfImage.texture = null;
                    return;
                }
                
                pdfViewerUI2.NextPage();
                pdfViewerUI2.NextPage();
            });
            
            pdfViewerUI1.previousButton.onClick.AddListener(() =>
            {

                if (!pdfViewerUI1.nextButton.interactable)
                {
                    pdfViewerUI1.nextButton.interactable = true;
                }
                
                pdfViewerUI1.PreviousPage();
                
                if (pdfViewerUI2.pdfImage.texture == null)
                {
                    pdfViewerUI2.GoToPage(pdfViewerUI2.navigator.CurrentPage);
                    return;
                }
                pdfViewerUI2.PreviousPage();
                pdfViewerUI2.PreviousPage();
            });
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