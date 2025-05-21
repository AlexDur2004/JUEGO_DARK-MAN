using UnityEngine;
using System.Collections;
using MTAssets.EasyMinimapSystem;

/// <summary>
/// Habilidad que permite a los fantasmas revelar la posición de Pacman.
/// </summary>
/// <remarks>
/// Esta habilidad resalta la posición de Pacman en el minimapa para que los
/// fantasmas puedan localizarlo más fácilmente. Conceptualmente representa
/// una onda expansiva de luz que se expande y revela la posición del jugador.
/// </remarks>
public class HabilidadRevelar : HabilidadBase
{    
    /// <summary>
    /// Referencia al componente MinimapItem de DarkMan/Pacman.
    /// </summary>
    /// <remarks>
    /// Este componente representa al jugador en el minimapa.
    /// Se utilizará para destacar su posición durante el efecto de la habilidad.
    /// </remarks>
    private MinimapItem pacmanMinimapItem;
    
    /// <summary>
    /// Referencia al controlador principal del minimapa.
    /// </summary>
    /// <remarks>
    /// Este componente permite manipular elementos en el minimapa,
    /// como destacar u ocultar elementos específicos.
    /// </remarks>
    private MinimapRenderer minimapRenderer;    /// <summary>
    /// Inicializa la habilidad obteniendo y guardando las referencias necesarias.
    /// </summary>
    /// <remarks>
    /// Este método obtiene las referencias a los componentes necesarios:
    /// - La instancia de Pacman (o DarkMan) en la escena
    /// - Su componente MinimapItem para representación en el minimapa
    /// - El MinimapRenderer que controla la visualización del minimapa
    /// </remarks>
    protected override void InicializarHabilidad()
    {        
        var pacman = Pacman.Instancia;
        pacmanMinimapItem = pacman.GetComponent<MinimapItem>();
        minimapRenderer = Personaje.ObtenerMinimapRenderer();
    }
    
    /// <summary>
    /// Aplica el efecto de la habilidad: destacar a DarkMan en el minimapa.
    /// </summary>
    /// <remarks>
    /// Este método hace que el ítem de Pacman/DarkMan quede resaltado en el minimapa,
    /// permitiendo que los fantasmas puedan ver claramente su ubicación.
    /// </remarks>
    public override void AplicarEfectoHabilidad()
    {
        // Añadir a DarkMan como elemento destacado en el minimapa
        minimapRenderer.AddMinimapItemToBeHighlighted(pacmanMinimapItem);
    }
    
    /// <summary>
    /// Quita el destacado de DarkMan en el minimapa.
    /// </summary>
    /// <remarks>
    /// Este método elimina el resaltado del ítem de Pacman/DarkMan en el minimapa,
    /// haciendo que vuelva a su visualización normal cuando termina el efecto.
    /// </remarks>
    public override void RemoverEfectoHabilidad()
    {
        // Quitar a DarkMan como elemento destacado en el minimapa
        minimapRenderer.RemoveMinimapItemOfHighlight(pacmanMinimapItem);
    }
}
