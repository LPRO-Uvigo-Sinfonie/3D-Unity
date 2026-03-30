using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic; 

public class Menu : MonoBehaviour
{
    public GameObject menuOpciones;
    public GameObject menuPrincipal;
    private string mapaSeleccionado = "Null";

    [System.Serializable]
    public struct InstrumentoToggle
    {
        public string nombre;
        public Toggle toggle;
    }

    public List<InstrumentoToggle> listaInstrumentos;

    public void HabilitarPanelOpciones()
    {
        menuPrincipal.SetActive(false);
        menuOpciones.SetActive(true);
    }

    public void HabilitarPanelPrincipal()
    {
        menuOpciones.SetActive(false);
        menuPrincipal.SetActive(true);
    }

    public void SalirJuego()
    {
        Debug.Log("Finalizando ensayo...");
        Application.Quit(); 
    }

    // Seleccion de mapa
    public void SetMapa(string nombreDelMapa)
    {
        Debug.Log("Mapa seleccionado: " + nombreDelMapa);
        mapaSeleccionado = nombreDelMapa;       
    }

    public void IrAlMapa()
    {
        if (!string.IsNullOrEmpty(mapaSeleccionado) || mapaSeleccionado == "Null")
        {
            EstadoOrquesta.instrumentos.Clear();
            foreach (InstrumentoToggle item in listaInstrumentos)
            {
                EstadoOrquesta.instrumentos.Add(item.nombre, item.toggle.isOn);
            }

            EstadoOrquesta.mapaSeleccionado = mapaSeleccionado;
            SceneManager.LoadScene(mapaSeleccionado);
        }
        else
        {
            Debug.LogWarning("No se ha seleccionado un mapa.");
            return;
        }
    }
}
