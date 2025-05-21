using UnityEngine;
using MTAssets.EasyMinimapSystem;
using System.Collections;

/// <summary>
/// Clase base abstracta para todos los fantasmas del juego.
/// Define la funcionalidad común que comparten todos los personajes tipo fantasma.
/// </summary>
/// <remarks>
/// Los fantasmas son personajes especiales que pueden ser controlados por el jugador
/// o por la IA, y pueden intercambiarse durante el juego. Esta clase gestiona la
/// identificación, configuración de cámara, manejo de muerte y resurrección.
/// </remarks>
public abstract class FantasmaBase : Personaje
{
    [Header("Identificación")]
    /// <summary>
    /// ID único del fantasma (1-4).
    /// </summary>
    [SerializeField] private int numeroIdentificador = 0;
    
    /// <summary>
    /// Valor para el blend tree del Animator que determina el tipo de ataque.
    /// </summary>
    /// <remarks>
    /// Cada tipo de fantasma usa un valor diferente para seleccionar su animación de ataque única.
    /// </remarks>
    public virtual float TipoAtaqueAnimator => 0f;
    
    /// <summary>
    /// Referencia a la cámara del fantasma cuando está siendo controlado.
    /// </summary>
    [HideInInspector] public Camera camaraFantasma;
    
    /// <summary>
    /// Referencia al transform de la cabeza para posicionar la cámara.
    /// </summary>
    [SerializeField] protected Transform cabezaTransform;

    /// <summary>
    /// Inicialización y registro en el sistema de gestión de fantasmas.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // Registra el fantasma en el sistema de gestión si tiene un ID válido
        if (numeroIdentificador >= 1 && numeroIdentificador <= 4)
            SistemaGestionFantasmas.Instancia.RegistrarFantasma(this, numeroIdentificador);
    }

    /// <summary>
    /// Configura la cámara del fantasma para ver desde su perspectiva usando una cámara compartida con suavizado.
    /// </summary>
    /// <param name="activar">True para activar la cámara, False para desactivarla.</param>
    protected override void ConfigurarCamaraJugador(bool activar)
    {
        // Si no existe el sistema de gestión de fantasmas o no hay cabeza, no hacemos nada
        if (SistemaGestionFantasmas.Instancia == null) return;
        
        if (activar)
        {
            // Obtenemos (o creamos si no existe) la cámara compartida del sistema
            Camera camaraCompartida = SistemaGestionFantasmas.Instancia.ObtenerCamaraCompartida();
            
            if (camaraCompartida != null)
            {
                // Configuramos la cámara para que use el estabilizador
                Transform camaraTrans = camaraCompartida.transform;
                camaraTrans.SetParent(cabezaTransform);
                camaraTrans.localPosition = new Vector3(0f, 0.1f, 0.15f);
                camaraTrans.localRotation = Quaternion.identity;

                // Aseguramos que la cámara esté activa
                camaraCompartida.gameObject.SetActive(true);
                camaraCompartida.enabled = true;
                
                // Actualizamos la referencia a la cámara de este fantasma
                camaraFantasma = camaraCompartida;
            }
        }
        else
        {
            // Al desactivar, solo quitamos la referencia local pero no desactivamos la cámara
            // ya que podría estar siendo usada por otro fantasma
            camaraFantasma = null;
        }
    }

    /// <summary>
    /// Maneja la muerte del fantasma activando la animación correspondiente y notificando al sistema.
    /// </summary>
    protected override void ProcesarMuerte()
    {
        if (animator != null)
            animator.SetBool("Muerte Medico", true); // Activa la animación de muerte

        ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Muerte);

        // Establecer el estado de derribado
        SetDerribado(true);
        
        // Notificar al sistema de gestión de fantasmas
        SistemaGestionFantasmas.Instancia.FantasmaDerrotado(this);
    }

    /// <summary>
    /// Revive un fantasma que estaba derribado.
    /// </summary>
    /// <remarks>
    /// Los fantasmas específicos pueden sobrescribir este método para añadir efectos visuales
    /// o comportamientos especiales durante la resurrección.
    /// </remarks>
    public virtual void Revivir()
    {
        // Reproducir efecto de sonido
        ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Revivir);     
    }
    
    /// <summary>
    /// Determina si el fantasma está inactivo o derribado.
    /// </summary>
    /// <returns>True si el fantasma está inactivo o derribado, False en caso contrario.</returns>
    public override bool EstaInactivo()
    {
        // Verifica si está marcado como inactivo en el sistema de gestión
        return derribado || ( SistemaGestionFantasmas.Instancia != null && 
               SistemaGestionFantasmas.Instancia.ContieneFantasmaInactivo(this));
    }
    
    /// <summary>
    /// Devuelve el identificador numérico único del fantasma.
    /// </summary>
    /// <returns>Número identificador del fantasma (1-4).</returns>
    public int GetNumeroFantasma() => numeroIdentificador;
}