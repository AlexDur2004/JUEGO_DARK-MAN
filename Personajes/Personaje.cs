using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using MTAssets.EasyMinimapSystem;

/// <summary>
/// Clase base para todos los personajes del juego (DarkMan/Pacman y fantasmas).
/// Implementa la funcionalidad compartida por todos los personajes jugables y no jugables.
/// </summary>
/// <remarks>
/// Esta clase abstracta contiene la funcionalidad compartida de:
/// - Movimiento mediante NavMeshAgent
/// - Sistema de vida y daño
/// - Gestión de estados (aturdido, invencible)
/// - Integración con armas y habilidades especiales
/// 
/// Las clases derivadas deben implementar el método ProcesarMuerte() y
/// pueden sobrescribir otros métodos virtuales para personalizar comportamientos.
/// </remarks>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class Personaje : MonoBehaviour
{   [Header("Estadísticas Base")]
    /// <summary>Puntos de vida máximos del personaje</summary>
    [SerializeField] protected int vidaMaxima = 3;
    
    /// <summary>Velocidad base de movimiento en unidades por segundo</summary>
    [SerializeField] protected float velocidadMovimiento = 3.5f;
    
    /// <summary>Duración predeterminada del estado de invulnerabilidad en segundos</summary>
    [SerializeField] protected float tiempoInvencibilidad = 3f;
    
    /// <summary>Puntos de vida actuales del personaje</summary>
    protected int vidaActual;
    
    /// <summary>Indica si el personaje está temporalmente aturdido y no puede moverse</summary>
    protected bool estaAturdido;
    
    /// <summary>Indica si el personaje es temporalmente inmune al daño</summary>
    protected bool esInvencible;
    
    /// <summary>Flag para habilitar o deshabilitar el movimiento del personaje</summary>
    protected bool puedeMoverse = true;
    
    /// <summary>Indica si el personaje está derribado (inactivo tras perder toda su vida)</summary>
    protected bool derribado = false;    /// <summary>Agente de navegación para pathfinding y movimiento</summary>
    protected NavMeshAgent navMeshAgent;
    
    /// <summary>Referencia al controlador de IA (solo para personajes no jugadores)</summary>
    protected ControladorIA controladorIA;
    
    /// <summary>Array de colliders del personaje y sus objetos hijos</summary>
    protected Collider[] collidersPersonaje;
    
    /// <summary>Componente de audio para reproducir sonidos</summary>
    protected AudioSource audioSource;
    
    /// <summary>Componente para escuchar sonidos 3D (activo solo en personaje controlado)</summary>
    protected AudioListener audioListener;
    
    /// <summary>Componente Animator para controlar animaciones del personaje</summary>
    protected Animator animator;

    /// <summary>Arma equipada por el personaje</summary>
    [NonSerialized] public ArmaBase arma;
    
    /// <summary>Habilidad especial del personaje</summary>
    [NonSerialized] public HabilidadBase habilidadAsociada;

    /// <summary>Referencia compartida al renderizador del minimapa para todos los personajes</summary>
    private static MinimapRenderer minimapRendererCompartido;

    /// <summary>
    /// Inicializa los componentes y estadísticas del personaje.
    /// </summary>
    /// <remarks>
    /// Este método se ejecuta al iniciar el objeto y configura:
    /// - El agente de navegación con la velocidad base
    /// - Las estadísticas iniciales (vida, etc.)
    /// - Referencias a componentes necesarios (arma, habilidad, audio, etc.)
    /// </remarks>
    protected virtual void Start()
    {
        // Configura el agente de navegación con la velocidad base
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = velocidadMovimiento;

        // Inicializa las estadísticas y obtiene referencias a componentes
        vidaActual = vidaMaxima;
        arma = GetComponentInChildren<ArmaBase>();
        habilidadAsociada = GetComponent<HabilidadBase>();
        controladorIA = GetComponent<ControladorIA>();

        audioSource = GetComponent<AudioSource>();
        audioListener = GetComponent<AudioListener>();

        animator = GetComponent<Animator>();

        // Obtener todos los colliders del personaje y guardarlos
        collidersPersonaje = GetComponentsInChildren<Collider>();
    }    /// <summary>
    /// Reproduce un efecto de sonido utilizando el AudioManager del juego.
    /// </summary>
    /// <param name="tipo">El tipo de sonido a reproducir desde el catálogo del AudioManager</param>
    /// <remarks>
    /// El método verifica que exista un AudioManager válido y que el personaje tenga un AudioSource
    /// antes de intentar reproducir el sonido. El volumen y características se configuran según
    /// los parámetros definidos en el AudioManager.
    /// </remarks>
    public void ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido tipo)
    {
        // Verifica que exista un AudioManager y que este personaje tenga un AudioSource
        if (Utilidades.AudioManager.Instancia != null && audioSource != null)
        {
            // Obtiene la configuración del sonido del AudioManager
            var config = Utilidades.AudioManager.Instancia.ObtenerSonidoConfig(tipo);

            // Si hay un clip disponible, lo reproduce en el AudioSource del personaje
            if (config.clip != null)
            {
                // Configura el AudioSource y reproduce
                audioSource.clip = config.clip;
                audioSource.volume = config.volumen;
                audioSource.Play();
            }
        }
    }
    /// <summary>
    /// Mueve el personaje en la dirección especificada usando el sistema NavMesh.
    /// </summary>
    /// <param name="direccion">Vector de dirección en la que se desea mover al personaje</param>
    /// <remarks>
    /// Este método es invocado tanto por la IA como por el control del jugador.
    /// El movimiento se ignora si el personaje está aturdido, deshabilitado o derribado.
    /// La dirección se normaliza y se restringe al plano horizontal (XZ).
    /// </remarks>
    public virtual void MoverHacia(Vector3 direccion)
    {
        // Ignora órdenes de movimiento durante aturdimiento, si está deshabilitado o derribado
        // El estado 'derribado' ahora se verifica a través del sistema de gestión
        if (estaAturdido || !puedeMoverse || EstaInactivo()) return;

        // Normaliza la dirección y elimina el componente vertical para movimiento en plano
        direccion = new Vector3(direccion.x, 0, direccion.z).normalized;
        if (direccion == Vector3.zero) return;

        // Configura el NavMeshAgent para que rote automáticamente
        navMeshAgent.updateRotation = true;

        // Activa el NavMeshAgent y establece un punto de destino cercano
        // en la dirección indicada para simular movimiento continuo
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(transform.position + direccion * 2.0f);
    }    /// <summary>
    /// Detiene inmediatamente cualquier movimiento del personaje.
    /// </summary>
    /// <remarks>
    /// Este método se utiliza durante aturdimientos, muertes o cambios de control.
    /// Verifica que el NavMeshAgent exista, esté habilitado y conectado a la NavMesh
    /// antes de intentar detener el movimiento.
    /// </remarks>
    public void DetenerMovimiento()
    {
        // Verifica que el agente exista, esté habilitado y conectado a la NavMesh
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.SetDestination(transform.position);
        }
    }    /// <summary>
    /// Cambia el tipo de control del personaje entre jugador humano e IA.
    /// </summary>
    /// <param name="esJugador">Indica si el personaje será controlado por el jugador (true) o por la IA (false)</param>
    /// <remarks>
    /// Este método es crítico para la mecánica de posesión de personajes.
    /// Ajusta el controlador, la cámara y el audioListener según el tipo de control asignado.
    /// </remarks>
    public virtual void AsignarControlador(bool esJugador = false)
    {
        // Desactiva la IA si existe
        if (controladorIA) controladorIA.enabled = !esJugador;

        // Control de jugador
        if (esJugador)
        {
            ControladorJugador.Instancia.ControlarPersonaje(this);
        }
        else if (ControladorJugador.Instancia?.GetPersonajeControlado() == this)
        {
            ControladorJugador.Instancia.ControlarPersonaje(null);
        }

        // Configura la cámara y el AudioListener según el tipo de control
        ConfigurarCamaraJugador(esJugador);
        if (audioListener) audioListener.enabled = esJugador;
    }

    /// <summary>
    /// Configura la cámara según el personaje controlado.
    /// </summary>
    /// <param name="activar">Indica si se debe activar (true) o desactivar (false) la cámara para este personaje</param>
    /// <remarks>
    /// Método virtual que las clases derivadas deben implementar con la lógica específica para su tipo.
    /// Por ejemplo, DarkMan puede usar una cámara en tercera persona mientras que los fantasmas usan otra vista.
    /// </remarks>
    protected virtual void ConfigurarCamaraJugador(bool activar) { }    /// <summary>
    /// Sistema central de daño: gestiona el daño recibido, reduce la vida y verifica muerte.
    /// </summary>
    /// <param name="daño">Cantidad de daño a aplicar al personaje</param>
    /// <remarks>
    /// Este método se llama cuando cualquier personaje recibe daño.
    /// Ignora el daño si el personaje es invencible o está inactivo.
    /// Si la vida llega a 0 o menos, activa el proceso de muerte.
    /// </remarks>
    public virtual void RecibirDaño(int daño)
    {
        // Ignora daño durante invencibilidad o si ya está derribado/inactivo
        if (esInvencible || EstaInactivo()) return;

        ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Golpe);

        // Reduce la vida actual y la mantiene como mínimo en 0
        vidaActual = Mathf.Max(0, vidaActual - daño);

        // Si la vida llega a 0 o menos, activa el proceso de muerte
        if (vidaActual <= 0)
            ProcesarMuerte();
    }

    /// <summary>
    /// Define el comportamiento específico al morir para cada tipo de personaje.
    /// </summary>
    /// <remarks>
    /// Método abstracto que cada clase derivada debe implementar.
    /// Define comportamientos como animaciones, efectos visuales/sonoros y proceso de respawn.
    /// </remarks>
    protected abstract void ProcesarMuerte();

    /// <summary>
    /// Sistema genérico para aplicar estados temporales con duración específica.
    /// </summary>
    /// <param name="iniciar">Acción a ejecutar al inicio del estado temporal</param>
    /// <param name="finalizar">Acción a ejecutar al finalizar el estado temporal</param>
    /// <param name="duracion">Duración del estado temporal en segundos</param>
    /// <returns>Enumerador para la corrutina</returns>
    /// <remarks>
    /// Usa acciones (delegates) para definir el comportamiento al inicio y fin del estado.
    /// Para duración cero o negativa, aplica y finaliza inmediatamente.
    /// </remarks>
    protected IEnumerator AplicarEstadoTemporal(Action iniciar, Action finalizar, float duracion)
    {
        // Para duración cero o negativa, aplica y finaliza inmediatamente
        if (duracion <= 0f)
        {
            iniciar?.Invoke();
            finalizar?.Invoke();
            yield break;
        }

        // Activa el estado, espera la duración configurada y luego lo finaliza
        iniciar?.Invoke();
        yield return new WaitForSeconds(duracion);
        finalizar?.Invoke();
    }

    /// <summary>
    /// Activa el estado de invencibilidad por un tiempo determinado.
    /// </summary>
    /// <param name="duracion">Duración de la invencibilidad en segundos (0 para usar el valor predeterminado)</param>
    /// <remarks>
    /// Protege al personaje de recibir daño durante la duración establecida.
    /// Si no se especifica una duración, utiliza el valor de tiempoInvencibilidad definido en la clase.
    /// </remarks>
    public virtual void ActivarInvencibilidad(float duracion = 0f) =>
        StartCoroutine(AplicarEstadoTemporal(
            () => esInvencible = true,  // Al iniciar: activa invencibilidad
            () => esInvencible = false, // Al finalizar: desactiva invencibilidad
            duracion > 0 ? duracion : tiempoInvencibilidad // Usa la duración especificada o la predeterminada
        ));

    /// <summary>
    /// Aplica un efecto de aturdimiento que impide el movimiento temporalmente.
    /// </summary>
    /// <param name="duracion">Duración del aturdimiento en segundos</param>
    /// <remarks>
    /// Útil para habilidades de control como el aturdimiento de DarkMan.
    /// Durante el aturdimiento, el personaje no podrá moverse y se detendrá cualquier movimiento en curso.
    /// </remarks>
    public virtual void Aturdir(float duracion) =>
        StartCoroutine(AplicarEstadoTemporal(
            () =>
            {
                estaAturdido = true;     // Marca como aturdido
                DetenerMovimiento();     // Detiene cualquier movimiento en curso
            },
            () => estaAturdido = false,  // Desactiva el estado de aturdimiento
            duracion
        ));

    /// <summary>
    /// Intenta activar la habilidad especial si está disponible y no está aturdido.
    /// </summary>
    /// <remarks>
    /// Este método conecta el sistema de personajes con el sistema de habilidades.
    /// Verifica si el personaje puede usar su habilidad y no está aturdido antes de activarla.
    /// Para los fantasmas, reproduce un sonido de habilidad específico cuando no son controlados
    /// por el jugador, utilizando su número de fantasma para identificar el audio.
    /// </remarks>
    public virtual void IntentarActivarHabilidad()
    {
        if (!estaAturdido && habilidadAsociada?.PuedeUsarHabilidad() == true)
        {
            habilidadAsociada.ActivarHabilidad();

            // Si es un fantasma, reproduce el audio de habilidad usando el identificador
            // SOLO si no es el personaje controlado por el jugador actualmente
            if (this is FantasmaBase fantasma && ControladorJugador.Instancia?.GetPersonajeControlado() != this)
            {
                int indiceAudio = fantasma.GetNumeroFantasma() - 1; // Ajusta si tu identificador empieza en 1
                if (Utilidades.AudioQueueManager.Instancia != null)
                    Utilidades.AudioQueueManager.Instancia.ReproducirHabilidad(indiceAudio);
            }
        }
    }

    /// <summary>
    /// Obtiene o crea una referencia compartida al renderizador del minimapa.
    /// </summary>
    /// <returns>Referencia al componente MinimapRenderer encontrado en la escena</returns>
    /// <remarks>
    /// Permite que todos los personajes accedan al mismo minimapa sin múltiples búsquedas.
    /// Utiliza un campo estático para almacenar la referencia y evitar búsquedas repetidas.
    /// </remarks>
    public static MinimapRenderer ObtenerMinimapRenderer() =>
        minimapRendererCompartido ??= FindFirstObjectByType<MinimapRenderer>();

    // PROPIEDADES Y MÉTODOS DE ACCESO
    /// <summary>
    /// Obtiene la velocidad actual de movimiento del personaje.
    /// </summary>
    /// <returns>Velocidad de movimiento en unidades por segundo</returns>
    public float VelocidadMovimiento => velocidadMovimiento;
    
    /// <summary>
    /// Establece directamente una nueva velocidad de movimiento.
    /// </summary>
    /// <param name="nuevaVelocidad">Nueva velocidad base en unidades por segundo</param>
    /// <remarks>
    /// Actualiza tanto la variable interna como el NavMeshAgent si está disponible.
    /// </remarks>
    public void AjustarVelocidad(float nuevaVelocidad)
    {
        velocidadMovimiento = nuevaVelocidad;
        if (navMeshAgent != null)
            navMeshAgent.speed = velocidadMovimiento;
    }

    /// <summary>
    /// Aplica un multiplicador a la velocidad actual del personaje.
    /// </summary>
    /// <param name="multiplicador">Factor por el que multiplicar la velocidad actual</param>
    /// <remarks>
    /// Útil para aplicar bonificaciones o penalizaciones temporales de velocidad.
    /// </remarks>
    public void ModificarVelocidad(float multiplicador) =>
        AjustarVelocidad(velocidadMovimiento * multiplicador);

    /// <summary>
    /// Obtiene los puntos de vida actuales del personaje.
    /// </summary>
    /// <returns>Puntos de vida actuales</returns>
    public int GetVidaActual() => vidaActual;
    
    /// <summary>
    /// Obtiene los puntos de vida máximos del personaje.
    /// </summary>
    /// <returns>Puntos de vida máximos</returns>
    public int GetVidaMaxima() => vidaMaxima;
    
    /// <summary>
    /// Calcula el porcentaje de vida restante del personaje.
    /// </summary>
    /// <returns>Porcentaje de vida (0.0 a 1.0)</returns>
    /// <remarks>
    /// Retorna 0 si la vida máxima es cero o negativa para evitar divisiones por cero.
    /// </remarks>
    public float GetPorcentajeVida() => vidaMaxima > 0 ? (float)vidaActual / vidaMaxima : 0f;

    /// <summary>
    /// Obtiene la tecla asignada para activar la habilidad del personaje.
    /// </summary>
    /// <returns>Código de tecla para activar la habilidad, o KeyCode.None si no hay habilidad asignada</returns>
    public KeyCode GetTeclaHabilidad() => habilidadAsociada ? habilidadAsociada.GetTeclaHabilidad() : KeyCode.None;

    /// <summary>
    /// Inicializa el personaje cuando su GameObject se activa.
    /// </summary>
    /// <remarks>
    /// Restablece los valores por defecto del personaje:
    /// - Restaura la vida al máximo
    /// - Desactiva estados especiales (invencible, aturdido)
    /// - Habilita el movimiento
    /// - Resetea las animaciones relacionadas con la muerte
    /// </remarks>
    protected virtual void OnEnable()
    {
        // Reinicia la vida al máximo al activar el objeto
        vidaActual = vidaMaxima;
        esInvencible = false;
        estaAturdido = false;
        puedeMoverse = true;

        if (animator != null)
            animator.SetBool("Muerte Medico", false);
    }    /// <summary>
    /// Establece el estado derribado del personaje y aplica los efectos correspondientes.
    /// </summary>
    /// <param name="derribadoNuevo">Valor booleano que indica si el personaje debe estar derribado (true) o no (false)</param>
    /// <param name="reinicioSistema">Indica si se está reiniciando el sistema (true) para omitir la animación de revivir</param>
    /// <remarks>
    /// Cuando el personaje es derribado:
    /// - Se detiene su movimiento
    /// - Se desactiva su controlador de IA si lo tiene
    /// - Se quita el control del jugador si lo tenía
    /// - Se desactivan todos sus colliders
    /// 
    /// Cuando el personaje se recupera:
    /// - Se resetean las animaciones de muerte
    /// - Se inicia la animación de revivir
    /// - Se programa una restauración completa tras finalizar la animación
    /// </remarks>
    public virtual void SetDerribado(bool derribadoNuevo, bool reinicioSistema = false)
    {
        derribado = derribadoNuevo;

        if (derribado)
        {
            // Detener movimiento y desactivar control de IA
            DetenerMovimiento();
            if (controladorIA != null)
                controladorIA.enabled = false;

            // Si este era el personaje controlado por el jugador, quitar control
            if (ControladorJugador.Instancia?.GetPersonajeControlado() == this)
                ControladorJugador.Instancia.ControlarPersonaje(null);

            // Desactivar todos los colliders del personaje
            GestionarColliders(false);
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("Muerte Medico", false);
                animator.SetTrigger("Revivir");
            }

            // Determinar el tiempo de restauración: inmediato si es reinicio de sistema, 
            // o duración completa de la animación en caso normal
            float tiempoRestauracion = reinicioSistema ? 0f : 11.4f;

            // Aquí NO ponemos derribado = false todavía, lo haremos al final de la animación
            StartCoroutine(RestaurarFisicoTrasRevivir(tiempoRestauracion));
        }
    }

    /// <summary>
    /// Espera un tiempo determinado antes de restaurar el estado físico del personaje tras revivir.
    /// </summary>
    /// <param name="tiempo">Tiempo en segundos a esperar antes de completar la resurrección</param>
    /// <returns>Enumerador para la corrutina</returns>
    /// <remarks>
    /// Este método se utiliza para sincronizar la restauración completa del personaje
    /// con el final de la animación de resurrección. Tras completarse:
    /// - Desactiva el estado derribado permitiendo el movimiento
    /// - Restaura la vida al máximo
    /// - Reactiva el controlador de IA si no está siendo controlado por el jugador
    /// - Reactiva todos los colliders del personaje
    /// 
    /// Si el tiempo es cero, omite la espera, útil para reinicio del sistema.
    /// </remarks>
    private IEnumerator RestaurarFisicoTrasRevivir(float tiempo)
    {
        // Solo esperamos si el tiempo es mayor que cero
        if (tiempo > 0)
            yield return new WaitForSeconds(tiempo);

        derribado = false; // Ahora sí permitimos movimiento

        // Al levantarse, restauramos la vida y reactivamos la IA si no está siendo controlado por el jugador
        vidaActual = vidaMaxima;

        if (ControladorJugador.Instancia?.GetPersonajeControlado() != this && controladorIA != null)
            controladorIA.enabled = true;

        GestionarColliders(true);
    }

    /// <summary>
    /// Verifica si el personaje está inactivo en el sistema de gestión.
    /// </summary>
    /// <returns>True si el personaje está inactivo, False en caso contrario</returns>
    /// <remarks>
    /// Método virtual que por defecto retorna false.
    /// Las clases derivadas deben implementar su propia lógica según su sistema de gestión.
    /// Principalmente utilizado por los fantasmas que tienen un sistema especial de inactividad.
    /// </remarks>
    public virtual bool EstaInactivo()
    {
        // Por defecto, solo los fantasmas utilizan este sistema
        // Las clases derivadas pueden implementar su propia lógica
        return false;
    }

    /// <summary>
    /// Activa o desactiva todos los colliders del personaje y sus objetos hijos.
    /// </summary>
    /// <param name="activar">True para activar los colliders, False para desactivarlos</param>
    /// <remarks>
    /// Si los colliders no han sido inicializados previamente, los busca y almacena.
    /// Este método es útil para desactivar colisiones durante estados especiales como
    /// la muerte o estados de invulnerabilidad, o reactivarlas cuando el personaje vuelve a la normalidad.
    /// </remarks>
    protected void GestionarColliders(bool activar)
    {
        // Si no se han inicializado los colliders, buscarlos ahora
        if (collidersPersonaje == null || collidersPersonaje.Length == 0)
        {
            collidersPersonaje = GetComponentsInChildren<Collider>();
        }

        // Activar o desactivar todos los colliders
        foreach (Collider collider in collidersPersonaje)
        {
            if (collider != null)
            {
                collider.enabled = activar;
            }
        }
    }

    /// <summary>
    /// Obtiene la referencia al componente Animator del personaje.
    /// </summary>
    /// <returns>Componente Animator del personaje</returns>
    /// <remarks>
    /// Utilizado por sistemas externos que necesitan acceder al Animator del personaje,
    /// como el sistema de animación de habilidades o el controlador de jugador.
    /// </remarks>
    public Animator GetAnimator() => animator; // Método para obtener el Animator del personaje

    /// <summary>
    /// Actualiza las variables de movimiento en el Animator del personaje.
    /// </summary>
    /// <param name="direccion">Vector de dirección en 2D (horizontal/vertical)</param>
    /// <param name="corriendo">Indica si el personaje está corriendo (true) o caminando (false)</param>
    /// <remarks>
    /// Establece los parámetros del Animator para controlar las animaciones de movimiento:
    /// - PosX: Componente horizontal de la dirección (-1 a 1)
    /// - PosY: Componente vertical de la dirección (-1 a 1)
    /// - Running: Indica si está corriendo o no
    /// </remarks>
    protected void ActualizarMovimiento(Vector2 direccion, bool corriendo)
    {
        if (animator != null)
        {
            animator.SetFloat("PosX", direccion.x);
            animator.SetFloat("PosY", direccion.y);
            animator.SetBool("Running", corriendo);
        }
    }

    /// <summary>
    /// Método llamado una vez por frame. Actualiza el estado de movimiento del personaje.
    /// </summary>
    /// <remarks>
    /// En la clase base, captura la entrada del usuario desde los ejes horizontales y verticales,
    /// y actualiza las animaciones de movimiento. Las clases derivadas pueden sobrescribir este método
    /// para implementar comportamientos específicos como la lógica de control del jugador o IA.
    /// </remarks>
    protected virtual void Update()
    {
        Vector2 direccion = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        ActualizarMovimiento(direccion, false);
    }
}