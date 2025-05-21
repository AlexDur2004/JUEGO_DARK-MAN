using UnityEngine;
using System.Collections;

/// <summary>
/// Implementación del fantasma verde, especializado en habilidades de revelación y ataques con martillo.
/// </summary>
/// <remarks>
/// El fantasma verde tiene una habilidad especial para revelar elementos en el juego,
/// lo que lo convierte en un personaje útil para descubrir secretos o enemigos ocultos.
/// </remarks>
public class FantasmaVerde : FantasmaBase
{    
    /// <summary>
    /// Valor específico para el blend tree de animación que representa el ataque con martillo.
    /// </summary>
    public override float TipoAtaqueAnimator => 1f;
}