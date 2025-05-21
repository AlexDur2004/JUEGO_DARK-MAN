using UnityEngine;
using System.Collections;
using Utilidades;

/// <summary>
/// Implementación del fantasma rojo, especializado en ataques de tipo apuñalamiento.
/// </summary>
/// <remarks>
/// El fantasma rojo se caracteriza por tener una animación de ataque distintiva
/// y posiblemente otras habilidades específicas no implementadas en esta versión.
/// </remarks>
public class FantasmaRojo : FantasmaBase
{    
    /// <summary>
    /// Valor específico para el blend tree de animación que representa el ataque de apuñalamiento.
    /// </summary>
    public override float TipoAtaqueAnimator => 2f;
}