using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject menuOpciones;
    public GameObject menuPrincipal;
    private string mapaSeleccionado = "Null";

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
            SceneManager.LoadScene(mapaSeleccionado);
        }
    }
}
