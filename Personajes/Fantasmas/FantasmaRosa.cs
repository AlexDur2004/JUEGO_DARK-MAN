using UnityEngine;
using System.Collections;

/// <summary>
/// Implementación del fantasma rosa, especializado en velocidad y ataques con jeringuilla.
/// </summary>
/// <remarks>
/// El fantasma rosa se caracteriza por tener una habilidad de velocidad que utiliza
/// el componente HabilidadRapidez para activar efectos de aceleración. 
/// La habilidad muestra efectos de partículas cuando está activa para dar feedback visual al jugador.
/// 
/// Este fantasma depende de tener un componente HabilidadRapidez asignado en el mismo
/// GameObject, que se referencia automáticamente como habilidadAsociada desde la clase base.
/// </remarks>
public class FantasmaRosa : FantasmaBase
{
    [Header("Efectos de Partículas")]
    /// <summary>
    /// Prefab de partículas para el efecto visual de velocidad.
    /// </summary>
    [SerializeField] private GameObject prefabParticulaRapidez;

    /// <summary>
    /// Sistema de partículas instanciado para el efecto de rapidez.
    /// </summary>
    private ParticleSystem particulasRapidez;

    /// <summary>
    /// Valor específico para el blend tree de animación que representa el ataque con jeringuilla.
    /// </summary>
    public override float TipoAtaqueAnimator => 0f;

    /// <summary>
    /// Inicializa componentes específicos del fantasma rosa y crea el sistema de partículas.
    /// </summary>
    /// <remarks>
    /// Este método realiza las siguientes acciones:
    /// 1. Llama al método Start de la clase base para inicializar componentes comunes
    /// 2. Instancia el sistema de partículas para los efectos de rapidez si el prefab está asignado
    /// </remarks>
    protected override void Start()
    {
        base.Start();

        // Instanciar partículas de rapidez directamente desde el prefab asignado en el inspector
        if (prefabParticulaRapidez != null)
        {
            GameObject particulaInstanciada = Instantiate(prefabParticulaRapidez, transform.position, Quaternion.identity, transform);
            particulasRapidez = particulaInstanciada.GetComponent<ParticleSystem>();
            if (particulasRapidez != null)
                particulasRapidez.Stop();
        }
    }

    /// <summary>
    /// Actualiza el movimiento y gestiona el estado del sistema de partículas según
    /// el estado de la habilidad de rapidez.
    /// </summary>
    /// <remarks>
    /// Este método realiza las siguientes acciones cada frame:
    /// 1. Detecta la entrada del jugador para el movimiento
    /// 2. Comprueba si la habilidad de rapidez está activa mediante habilidadAsociada.EstaActiva()
    /// 3. Actualiza el Animator con el estado de movimiento y la velocidad correcta
    /// 4. Controla el sistema de partículas sincronizándolo con el estado de la habilidad
    /// 
    /// Este enfoque permite que la lógica de activación y desactivación de la habilidad
    /// sea manejada por la clase HabilidadBase, mientras que el fantasma rosa solo
    /// responde al estado actual mediante efectos visuales y modificando la velocidad.
    /// </remarks>
    protected override void Update()
    {
        // Detectar movimiento
        Vector2 direccion = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Verificar si la habilidad de rapidez está activa
        bool corriendo = habilidadAsociada != null && habilidadAsociada.EstaActiva();

        // Actualizar el Animator
        ActualizarMovimiento(direccion, corriendo);

        // Controlar partículas de rapidez según el estado de la habilidad
        if (habilidadAsociada != null && particulasRapidez != null)
        {
            // Activar partículas solo cuando la habilidad está activa y no están reproduciéndose
            if (habilidadAsociada.EstaActiva() && !particulasRapidez.isPlaying)
                particulasRapidez.Play();
            // Detener partículas cuando la habilidad está inactiva y están reproduciéndose
            else if (!habilidadAsociada.EstaActiva() && particulasRapidez.isPlaying)
                particulasRapidez.Stop();
        }
    }
}