using UnityEngine;
using Utilidades;
using DamageNumbersPro;

/// <summary>
/// Gestiona la creación y visualización de números de daño flotantes en el juego
/// </summary>
/// <remarks>
/// Este componente utiliza el patrón Singleton para proporcionar acceso global desde
/// otros sistemas. Se basa en DamageNumbersPro para crear textos 3D que muestran 
/// la cantidad de daño infligido a objetivos durante el combate.
/// </remarks>
public class GestorNumerosDaño : Singleton<GestorNumerosDaño>
{
    /// <summary>
    /// Configuración básica para los números de daño
    /// </summary>
    [Header("Configuración de Números de Daño")]
    
    /// <summary>
    /// Prefab para mostrar números de daño en 3D dentro del mundo de juego
    /// </summary>
    [SerializeField] private DamageNumberMesh prefabDaño;

    /// <summary>
    /// Configuración para ajuste fino de los números de daño
    /// </summary>
    [Header("Configuración Avanzada")]
    
    /// <summary>
    /// Altura sobre el objetivo donde aparecen los números de daño
    /// </summary>
    [SerializeField] private float alturaOffset = 0.5f;

    /// <summary>
    /// Muestra un número de daño en la posición especificada
    /// </summary>
    /// <param name="cantidad">Valor numérico del daño a mostrar</param>
    /// <param name="posicion">Posición en el mundo donde mostrar el número</param>
    /// <remarks>
    /// Crea una instancia del prefab de daño en la posición especificada, 
    /// ajustada verticalmente según el offset configurado. Configura la instancia
    /// para que no sea visible a través de paredes, mejorando así la percepción
    /// espacial del jugador.
    /// </remarks>
    public void MostrarNumeroDaño(int cantidad, Vector3 posicion)
    {
        if (prefabDaño == null || cantidad == 0) return;

        // Ajusta la posición para que el número aparezca sobre el objetivo
        Vector3 posicionAjustada = posicion + Vector3.up * alturaOffset;
        
        // Usar el método Spawn de DamageNumbersPro (la manera correcta según la documentación)
        DamageNumberMesh instancia = prefabDaño.Spawn(posicionAjustada, cantidad) as DamageNumberMesh;
        
        if (instancia != null)
        {
            // Deshabilitar la visibilidad a través de paredes
            instancia.renderThroughWalls = false;
        }
    }
}

