using UnityEngine;
using MTAssets.EasyMinimapSystem;
using System;
using System.Collections;
using Utilidades;

/// <summary>
/// Clase encargada de escanear el laberinto para visualizarlo en el minimapa.
/// </summary>
/// <remarks>
/// Implementa el patrón Singleton para asegurar una única instancia en toda la aplicación.
/// Esta clase se encarga de:
/// - Configurar el escáner del minimapa para adaptarse al tamaño del laberinto
/// - Posicionar correctamente el área de escaneo
/// - Ejecutar el proceso de escaneo para mostrar el laberinto en el minimapa
/// </remarks>
public class EscanerLaberinto : Singleton<EscanerLaberinto>
{
    /// <summary>
    /// Componente que realiza el escaneo del área para el minimapa.
    /// </summary>
    /// <remarks>
    /// Este componente pertenece al asset EasyMinimapSystem y se encarga
    /// de escanear objetos en un área determinada para mostrarlos en el minimapa.
    /// </remarks>
    private MinimapScanner minimapScanner;
    
    /// <summary>
    /// Desplazamiento aplicado a la posición de escaneo para mejorar la visualización.
    /// </summary>
    /// <remarks>
    /// Este valor se utiliza para ajustar la posición del escaneo y asegurar
    /// que todo el laberinto quede correctamente visualizado en el minimapa.
    /// </remarks>
    private const float offset = 2f;
      /// <summary>
    /// Se ejecuta al inicializarse el objeto. Configura el escáner del minimapa.
    /// </summary>
    /// <remarks>
    /// Este método:
    /// 1. Llama al método Awake() de la clase base para asegurar la implementación correcta del Singleton
    /// 2. Añade dinámicamente el componente MinimapScanner al GameObject
    /// 3. Configura la altura de escaneo a 10 unidades
    /// 4. Añade el objeto SpawnPersonajes a la lista de objetos a ignorar durante el escaneo
    /// </remarks>
    protected override void Awake()
    {
        base.Awake();
        minimapScanner = gameObject.AddComponent<MinimapScanner>();
        minimapScanner.scanHeight = 10f;
        minimapScanner.gameObjectsToIgnore.Add(SpawnPersonajes.Instancia.gameObject);
    }

    /// <summary>
    /// Escanea el laberinto con las dimensiones especificadas y lo muestra en el minimapa.
    /// </summary>
    /// <param name="dimensionesLaberinto">Vector2 que contiene el ancho (x) y alto (y) del laberinto en unidades de mundo.</param>
    /// <remarks>
    /// Este método realiza los siguientes pasos:
    /// 1. Calcula el tamaño máximo del laberinto (el mayor entre ancho y alto)
    /// 2. Determina el área de escaneo más adecuada para ese tamaño
    /// 3. Calcula el centro geométrico del laberinto
    /// 4. Posiciona el escáner considerando el tamaño y aplicando un offset
    /// 5. Ejecuta el escaneo para visualizar el laberinto en el minimapa
    /// 
    /// El resultado será un minimapa que muestra una representación visual
    /// del laberinto con sus pasajes y paredes.
    /// </remarks>
    public void EscanearLaberinto(Vector2 dimensionesLaberinto)
    {
        // Calcular el tamaño máximo del laberinto una sola vez
        float tamañoMaximo = Mathf.Max(dimensionesLaberinto.x, dimensionesLaberinto.y);

        // Ajustar el área de escaneo
        minimapScanner.scanArea = GetScanAreaMásAproximado(tamañoMaximo);

        // Calcular centro y posición de escaneo una sola vez
        Vector3 centroDelLaberinto = new Vector3(dimensionesLaberinto.x / 2f, 0f, dimensionesLaberinto.y / 2f);
        Vector3 posicionEscaneo = centroDelLaberinto - new Vector3(tamañoMaximo / 2f, 0, tamañoMaximo / 2f);

        // Ajustar su posición para centrar el escaneo con offset
        minimapScanner.transform.position = new Vector3(posicionEscaneo.x - offset, 0f, posicionEscaneo.z - offset);

        // Lanzar el escaneo
        minimapScanner.DoScanInThisAreaOfComponentAndShowOnMinimap();
    }

    /// <summary>
    /// Determina el área de escaneo más aproximada al tamaño del laberinto.
    /// </summary>
    /// <param name="tamaño">Tamaño máximo del laberinto en unidades del mundo.</param>
    /// <returns>El área de escaneo más adecuada para el tamaño especificado.</returns>
    /// <remarks>
    /// Este método:
    /// 1. Itera a través de todas las áreas de escaneo predefinidas en MinimapScanner.ScanArea
    /// 2. Extrae el valor numérico de cada área (eliminando el texto "Units")
    /// 3. Calcula la diferencia entre ese valor y el tamaño del laberinto
    /// 4. Selecciona el área que tenga la diferencia mínima
    /// 
    /// Esto garantiza que el área de escaneo seleccionada sea lo más cercana posible
    /// al tamaño real del laberinto, optimizando así la visualización en el minimapa.
    /// </remarks>
    private MinimapScanner.ScanArea GetScanAreaMásAproximado(float tamaño)
    {
        MinimapScanner.ScanArea áreaAproximada = MinimapScanner.ScanArea.Units40;
        float diferenciaMenor = float.MaxValue;

        foreach (MinimapScanner.ScanArea area in Enum.GetValues(typeof(MinimapScanner.ScanArea)))
        {
            float valor = (float)Convert.ToInt32(area.ToString().Replace("Units", ""));
            float diferencia = Mathf.Abs(valor - tamaño);
            if (diferencia < diferenciaMenor)
            {
                diferenciaMenor = diferencia;
                áreaAproximada = area;
            }
        }
        return áreaAproximada;
    }
}