using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UI;

public class ModificarPartitura: MonoBehaviour
{

    public Button modifyBtn;

    private void Start()
    {
        modifyBtn.onClick.AddListener(SeleccionarYModificarPartitura);
    }

    private void SeleccionarYModificarPartitura()
    {
 
        var cancion = EstadoOrquesta.cancionSeleccionada;
    
        var selectedPaths = StandaloneFileBrowser.OpenFilePanel("Seleccionar", "", "pdf", false);

        if (selectedPaths.Length == 0) return;
        
        var pdfFolderPath = Path.Join(Directory.GetCurrentDirectory(), "pdf");
            
        var pdfPath = Path.Join(pdfFolderPath, $"{cancion}.pdf");
        
        var selectedPdfPath = selectedPaths[0];

        try
        {
            File.Copy(selectedPdfPath, pdfPath);
        }
        catch
        {
            File.Delete(pdfPath);
            File.Copy(selectedPdfPath, pdfPath);
        }
    }
    
}
