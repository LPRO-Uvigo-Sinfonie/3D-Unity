using System.Collections.Generic;

public static class EstadoOrquesta
{
    public static string mapaSeleccionado = "Ninguno";

    // Nueva variable para la canción seleccionada
    public static string cancionSeleccionada = "Ninguna";

    // Lista de instrumentos con su estado ON/OFF
    public static Dictionary<string, bool> instrumentos = new Dictionary<string, bool>();
}