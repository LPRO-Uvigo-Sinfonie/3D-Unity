using UnityEngine;
using UnityEngine.SceneManagement; 

public class Menu : MonoBehaviour
{
    public GameObject menuOpciones;
    public GameObject menuPrincipal;

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

    public void IrAlMapa()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
