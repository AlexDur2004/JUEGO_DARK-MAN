using UnityEngine;
using System.Collections;

/// <summary>
/// Implementación del fantasma azul, especializado en ataques con martillo.
/// </summary>
/// <remarks>
/// Aunque actualmente tiene una implementación mínima, el fantasma azul
/// utiliza un tipo de ataque basado en martillo según su configuración
/// en el blend tree del Animator.
/// </remarks>
public class FantasmaAzul : FantasmaBase
{
    /// <summary>
    /// Valor específico para el blend tree de animación que representa el ataque con martillo.
    /// </summary>
    [HideInInspector]
    public override float TipoAtaqueAnimator => 1f;
}