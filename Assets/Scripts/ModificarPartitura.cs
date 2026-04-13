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
    
        var selectedPaths = StandaloneFileBrowser.OpenFilePanel("Seleccionar", "", "pdf", true);

        if (selectedPaths.Length == 0) return;
        
        var pdfFolderPath = Path.Join(Directory.GetCurrentDirectory(), "pdf");
            
        var pdfPart1Path = Path.Join(pdfFolderPath, $"{cancion}-1.pdf");
        var pdfPart2Path = Path.Join(pdfFolderPath, $"{cancion}-2.pdf");
        
        var selectedPdfPart1Path = selectedPaths[0];

        File.Copy(selectedPdfPart1Path, pdfPart1Path);
        
        if (selectedPaths.Length != 2) return;
        
        var selectedPdfPath2Path = selectedPaths[1];
        
        File.Copy(selectedPdfPath2Path, pdfPart2Path);
        
    }
    
}
