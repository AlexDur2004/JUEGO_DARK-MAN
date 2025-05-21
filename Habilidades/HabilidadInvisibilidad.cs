using UnityEngine;

/// <summary>
/// Habilidad que hace al personaje parcialmente invisible y lo vuelve invulnerable.
/// </summary>
/// <remarks>
/// Esta habilidad modifica el material del personaje para hacerlo semitransparente
/// y activa la invencibilidad durante su duración. Es útil para atravesar áreas 
/// peligrosas o evadir enemigos temporalmente.
/// </remarks>
public class HabilidadInvisibilidad : HabilidadBase
{
    /// <summary>
    /// Referencia al componente Renderer del personaje.
    /// </summary>
    /// <remarks>
    /// Este componente contiene el material que será modificado para
    /// lograr el efecto de transparencia.
    /// </remarks>
    [SerializeField] private Renderer rendererPersonaje;
    
    /// <summary>
    /// Referencia al material que será modificado.
    /// </summary>
    /// <remarks>
    /// Se almacena una referencia directa al material para poder
    /// modificar sus propiedades y restaurarlas posteriormente.
    /// </remarks>
    private Material material;

    /// <summary>
    /// Inicializa la habilidad guardando referencia al material principal.
    /// </summary>
    /// <remarks>
    /// Este método se ejecuta durante la inicialización y guarda una referencia
    /// al material del personaje que será modificado cuando se active la habilidad.
    /// </remarks>
    protected override void InicializarHabilidad()
    {
        // Guardar referencia al material principal
        if (rendererPersonaje != null && rendererPersonaje.material != null)
        {
            material = rendererPersonaje.material;
        }
    }    /// <summary>
    /// Aplica el efecto de invisibilidad al personaje.
    /// </summary>
    /// <remarks>
    /// Este método realiza dos acciones principales:
    /// 1. Activa la invencibilidad del personaje durante la duración establecida
    /// 2. Modifica el material del personaje para hacerlo semitransparente
    ///    configurando los parámetros de mezcla (blend) del shader y reduciendo
    ///    el valor alfa del color a aproximadamente un 23% (60/255).
    /// </remarks>
    public override void AplicarEfectoHabilidad()
    {
        // Activa invencibilidad durante la duración de la habilidad
        GetComponent<Personaje>()?.ActivarInvencibilidad(duracionBase);

        if (material != null)
        {
            // Configurar material como transparente
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;

            // Modificar solo el alfa sin cambiar el color
            Color colorActual = material.color;
            colorActual.a = 60f / 255f;
            material.color = colorActual;
        }
    }    /// <summary>
    /// Restaura la visibilidad normal del personaje.
    /// </summary>
    /// <remarks>
    /// Este método se llama cuando finaliza el efecto de la habilidad y:
    /// 1. Restaura el material a modo opaco (no transparente)
    /// 2. Reinicia los parámetros de blend del shader a sus valores por defecto
    /// 3. Restaura el valor alfa del color a 1.0 (completamente opaco)
    /// 
    /// El personaje regresa a su apariencia visual normal y la invencibilidad 
    /// habrá terminado automáticamente tras la duración establecida.
    /// </remarks>
    public override void RemoverEfectoHabilidad()
    {
        if (material != null)
        {
            // Restaurar material a modo opaco
            material.SetFloat("_Mode", 0); // Opaque mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHABLEND_ON");
            material.renderQueue = -1;

            // Restaurar opacidad completa manteniendo el color actual
            Color colorActual = material.color;
            colorActual.a = 1f; // 255 en escala 0-1 (opacidad total)
            material.color = colorActual;
        }
    }
}