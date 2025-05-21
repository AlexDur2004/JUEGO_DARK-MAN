using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using MTAssets.EasyMinimapSystem;
using System;

/// <summary>
/// Sistema principal que gestiona todos los fantasmas del juego, incluyendo su control por el jugador.
/// </summary>
/// <remarks>
/// Esta clase es responsable de coordinar las interacciones entre los fantasmas, manejar 
/// el cambio de control entre ellos, procesar sus estados (activo/inactivo), y gestionar eventos 
/// relacionados con los fantasmas como cambios de control, derrotas y resurrecciones.
/// Implementa un patrón Singleton para asegurar una única instancia global.
/// </remarks>
public class SistemaGestionFantasmas : Singleton<SistemaGestionFantasmas>
{
    [Header("Configuración")]
    /// <summary>
    /// Tiempo de espera obligatorio entre cambios de control de fantasma.
    /// </summary>
    [SerializeField] private float cooldownCambioFantasma = 20f;
    
    /// <summary>
    /// Identificador del fantasma que está siendo controlado por el jugador (1-4).
    /// </summary>
    [SerializeField][Range(1, 4)] private int numeroFantasmaControladoPorJugador = 1;
    
    /// <summary>
    /// Referencia a la cámara compartida que utilizarán todos los fantasmas.
    /// Esta cámara debe tener los componentes Camera y MinimapCamera configurados.
    /// </summary>
    /// <remarks>
    /// La cámara se asigna desde el Inspector en lugar de crearse dinámicamente para garantizar
    /// que su referencia persista entre sesiones de juego, evitando problemas con componentes
    /// como MinimapRenderer que necesitan mantener referencias válidas durante todo el ciclo
    /// de vida del juego, incluyendo reinicios de niveles.
    /// </remarks>
    [SerializeField] private Camera camaraCompartidaAsignada;

    /// <summary>
    /// Evento que se dispara cuando el jugador cambia el control a otro fantasma.
    /// </summary>
    /// <remarks>
    /// El parámetro entero es el número identificador (1-4) del nuevo fantasma controlado.
    /// </remarks>
    public event Action<int> OnFantasmaControlCambiado;
    
    /// <summary>
    /// Evento que se dispara cuando un fantasma es derrotado o revivido.
    /// </summary>
    /// <remarks>
    /// El primer parámetro entero es el número identificador (1-4) del fantasma.
    /// El segundo parámetro booleano indica si fue derrotado (true) o revivido (false).
    /// </remarks>
    public event Action<int, bool> OnFantasmaDerrotado;

    /// <summary>
    /// Contenedor que gestiona todos los fantasmas del juego, tanto activos como inactivos.
    /// </summary>
    private ObjectPool<FantasmaBase> poolFantasmas = new();
    
    /// <summary>
    /// Timestamp del último momento en que el jugador cambió de fantasma.
    /// </summary>
    /// <remarks>
    /// Se utiliza para calcular el cooldown entre cambios de fantasma.
    /// </remarks>
    private float tiempoUltimoCambioJugador;
    
    /// <summary>
    /// Referencia directa al fantasma actualmente controlado por el jugador.
    /// </summary>
    private FantasmaBase fantasmaControladoActual;

    /// <summary>
    /// Inicialización del sistema de gestión de fantasmas.
    /// </summary>
    /// <remarks>
    /// Configura el temporizador para permitir un cambio inmediato de fantasma al inicio del juego
    /// y carga las preferencias del usuario sobre qué fantasma debe controlar.
    /// </remarks>
    protected override void Awake()
    {
        base.Awake();
        tiempoUltimoCambioJugador = -cooldownCambioFantasma;             // Permite un cambio inmediato al inicio del juego
        numeroFantasmaControladoPorJugador = PlayerPrefs.GetInt("jugadorElegido");
    }

    /// <summary>
    /// Registra un fantasma en el sistema con su número identificador.
    /// </summary>
    /// <param name="fantasma">El fantasma a registrar</param>
    /// <param name="numero">El número identificador único del fantasma (1-4)</param>
    /// <remarks>
    /// Este método se llama generalmente desde el Start() de cada fantasma.
    /// Registra el fantasma en el pool y le asigna el control si corresponde 
    /// según las preferencias del jugador.
    /// </remarks>
    public void RegistrarFantasma(FantasmaBase fantasma, int numero)
    {
        if (fantasma == null) return;

        // Registrar en el pool y asignar control si corresponde
        poolFantasmas.Registrar(fantasma);
        bool esControlado = numero == numeroFantasmaControladoPorJugador;
        fantasma.AsignarControlador(esControlado);
        
        // Actualizar referencia al fantasma controlado si corresponde
        if (esControlado)
        {
            fantasmaControladoActual = fantasma;
            OnFantasmaControlCambiado?.Invoke(numeroFantasmaControladoPorJugador);
        }
    }

    /// <summary>
    /// Procesa la derrota de un fantasma, moviéndolo a la lista de inactivos y notificando a los observadores.
    /// Si el fantasma derrotado era el controlado por el jugador, busca otro fantasma disponible.
    /// </summary>
    /// <param name="fantasma">El fantasma que ha sido derrotado</param>
    public void FantasmaDerrotado(FantasmaBase fantasma)
    {
        if (fantasma == null || !EstaFantasmaActivo(fantasma)) return;
            
        // Notificar que el fantasma ha sido derribado
        bool esFantasmaControlado = fantasma.GetNumeroFantasma() == numeroFantasmaControladoPorJugador;
        Debug.Log($"[SistemaGestionFantasmas] Fantasma #{fantasma.GetNumeroFantasma()} derrotado. Era controlado: {esFantasmaControlado}");
        
        // Actualizar estado en lista de fantasmas - pero mantener el objeto activo
        if (poolFantasmas.ContieneActivo(fantasma)) 
        {
            // Mover a la lista de inactivos SIN desactivar el GameObject
            poolFantasmas.Desactivar(fantasma, false);
            Debug.Log($"[SistemaGestionFantasmas] Fantasma #{fantasma.GetNumeroFantasma()} movido a inactivos. Activos: {poolFantasmas.CountActivos}, Inactivos: {poolFantasmas.CountInactivos}");
        }
        
        // Notificar a los observadores (como la interfaz de usuario) que el fantasma ha sido derrotado
        OnFantasmaDerrotado?.Invoke(fantasma.GetNumeroFantasma(), true);
        
        // Si era el fantasma controlado, buscar otro
        if (esFantasmaControlado)
        {
            fantasmaControladoActual = null;
            ManejarMuerteFantasmaControlado(fantasma);
        }
    }

    /// <summary>
    /// Comprueba si un fantasma está activo y no derribado</summary>
    /// <param name="fantasma">El fantasma a verificar</param>
    /// <returns>True si el fantasma existe y está en la lista de activos (no derribado), False en caso contrario</returns>
    /// <remarks>
    /// Este método se utiliza para determinar si un fantasma está disponible para ser controlado
    /// o para realizar acciones. Los fantasmas inactivos están derribados y no pueden ser controlados.
    /// </remarks>
    private bool EstaFantasmaActivo(FantasmaBase fantasma)
    {
        if (fantasma == null) return false;
        
        // Verificar si está en el pool de activos (por definición, no está derribado)
        return poolFantasmas.ContieneActivo(fantasma);
    }

    /// <summary>
    /// Reactiva un fantasma previamente derrotado, moviéndolo de la lista de inactivos a activos y restaurando su estado.
    /// Este método es utilizado cuando el jugador u otro mecanismo revive a un fantasma derribado.
    /// </summary>
    /// <param name="fantasma">El fantasma a revivir</param>
    /// <param name="reinicioCompleto">Si es true, reinicia completamente el fantasma incluyendo su IA y animación. 
    /// Si es false, solo realiza una reactivación básica.</param>
    /// <param name="reproducirSonido">Si es true, reproduce el sonido de revivir. Por defecto es true.</param>
    public void RevivirFantasma(FantasmaBase fantasma, bool reinicioCompleto = false, bool reproducirSonido = true)
    {
        if (fantasma == null) return;

        // Si está en la lista de inactivos, simplemente moverlo a activos
        if (poolFantasmas.ContieneInactivo(fantasma))
        {          
            // Restaurar el estado interno del fantasma
            fantasma.SetDerribado(false, reinicioCompleto);
            
            // Mover a la lista de activos
            poolFantasmas.Activar(fantasma, false);
            
            // Notificar a los observadores
            OnFantasmaDerrotado?.Invoke(fantasma.GetNumeroFantasma(), false);
            
            // Reproducir sonido de revivir si corresponde
            if (reproducirSonido)
            {
                fantasma.ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Revivir);
            }
        }
    }

    /// <summary>
    /// Solicita cambiar al fantasma controlado por el jugador a otro con un número identificador específico.
    /// </summary>
    /// <param name="fantasmaActual">El fantasma actualmente controlado</param>
    /// <param name="numeroFantasmaSolicitado">El número del fantasma al que se desea cambiar</param>
    /// <remarks>
    /// Este método verifica si es posible realizar el cambio (cooldown, existencia del fantasma)
    /// y delega al método sobrecargado que maneja dos instancias de fantasmas.
    /// </remarks>
    public void SolicitarCambioFantasma(FantasmaBase fantasmaActual, int numeroFantasmaSolicitado)
    {
        // Evitar cambiar al mismo fantasma o si hay cooldown
        if (numeroFantasmaControladoPorJugador == numeroFantasmaSolicitado || 
            Time.time < tiempoUltimoCambioJugador + cooldownCambioFantasma) 
            return;

        // Buscar el fantasma solicitado
        FantasmaBase fantasmaSolicitado = GetFantasma(numeroFantasmaSolicitado);
        if (fantasmaSolicitado != null)
            SolicitarCambioFantasma(fantasmaActual, fantasmaSolicitado);
    }

    /// <summary>
    /// Cambia el control del jugador entre dos fantasmas específicos.
    /// </summary>
    /// <param name="fantasmaActual">El fantasma actualmente controlado por el jugador</param>
    /// <param name="fantasmaSolicitado">El fantasma que recibirá el control</param>
    /// <remarks>
    /// Este método realiza la transferencia efectiva del control, actualizando todas las 
    /// referencias necesarias, notificando a los observadores y activando efectos visuales/sonoros.
    /// También establece el temporizador de cooldown para evitar cambios demasiado frecuentes.
    /// </remarks>
    public void SolicitarCambioFantasma(FantasmaBase fantasmaActual, FantasmaBase fantasmaSolicitado)
    {
        if (fantasmaSolicitado == null) return;

        // Quitar control al fantasma actual y asignar al nuevo
        if (fantasmaActual != null)
            fantasmaActual.AsignarControlador(false);

        fantasmaSolicitado.AsignarControlador(true);

        // Actualizar variables de control
        numeroFantasmaControladoPorJugador = fantasmaSolicitado.GetNumeroFantasma();
        fantasmaControladoActual = fantasmaSolicitado;
        tiempoUltimoCambioJugador = Time.time;

        // Reiniciar cooldown SOS y notificar del cambio
        ControladorJugador.Instancia.ReiniciarCooldownSOS();
        OnFantasmaControlCambiado?.Invoke(numeroFantasmaControladoPorJugador);

        // Reproducir sonido de cambio de fantasma desde el propio fantasma
        if (fantasmaControladoActual != null)
        {
            fantasmaControladoActual.ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.CambioPersonaje);
        }
    }

    /// <summary>
    /// Asigna un nuevo fantasma al jugador cuando el fantasma controlado muere.
    /// </summary>
    /// <param name="fantasmaMuerto">El fantasma que acaba de morir y era controlado por el jugador</param>
    /// <remarks>
    /// Este método selecciona aleatoriamente otro fantasma activo para darle el control al jugador
    /// y realiza la transición sin aplicar cooldown. También limpia efectos visuales como el borde rojo.
    /// </remarks>
    public void ManejarMuerteFantasmaControlado(FantasmaBase fantasmaMuerto)
    {
        if (fantasmaMuerto?.GetNumeroFantasma() != numeroFantasmaControladoPorJugador) return;

        // Obtener lista de fantasmas activos
        List<FantasmaBase> fantasmasDisponibles = GetFantasmasActivos();
        if (fantasmasDisponibles.Count == 0) return;

        // Selecciona un fantasma aleatorio de los disponibles (activos y no derribados)
        FantasmaBase nuevoFantasma = fantasmasDisponibles[UnityEngine.Random.Range(0, fantasmasDisponibles.Count)];

        // Limpiar el efecto de borde rojo en la interfaz
        var interfaz = InterfazJuego.Instancia;
        if (interfaz?.bordePantalla != null)
            interfaz.bordePantalla.color = new Color(1, 0, 0, 0); // Transparente

        // Cambiar al nuevo fantasma sin cooldown
        float tempCooldown = cooldownCambioFantasma;
        cooldownCambioFantasma = 0;
        SolicitarCambioFantasma(fantasmaMuerto, nuevoFantasma);
        cooldownCambioFantasma = tempCooldown;
    }

    /// <summary>
    /// Obtiene la lista de todos los fantasmas activos en el sistema (no derribados).
    /// </summary>
    /// <returns>Lista con las referencias a todos los fantasmas activos.</returns>
    /// <remarks>
    /// Un fantasma activo es aquel que no ha sido derrotado y puede ser controlado o realizar acciones.
    /// </remarks>
    public List<FantasmaBase> GetFantasmasActivos() => poolFantasmas.GetActivos();

    /// <summary>
    /// Obtiene la cantidad de fantasmas activos (no derribados) disponibles.
    /// </summary>
    /// <returns>Número de fantasmas activos.</returns>
    /// <remarks>
    /// Este método es útil para verificar condiciones de victoria o derrota,
    /// como cuando todos los fantasmas han sido eliminados.
    /// </remarks>
    public int GetFantasmasRestantes() => poolFantasmas.CountActivos;
    
    /// <summary>
    /// Obtiene el número identificador del fantasma controlado actualmente por el jugador.
    /// </summary>
    /// <returns>ID del fantasma controlado (1-4).</returns>
    /// <remarks>
    /// Este valor se actualiza cada vez que el jugador cambia de fantasma y se utiliza 
    /// para determinar qué fantasma debe recibir los inputs del usuario.
    /// </remarks>
    public int GetNumeroFantasmaControlado() => numeroFantasmaControladoPorJugador;
    
    /// <summary>
    /// Obtiene el fantasma controlado actualmente por el jugador.
    /// </summary>
    /// <returns>Referencia al fantasma controlado por el jugador o null si no hay ninguno disponible</returns>
    /// <remarks>
    /// Primero intenta usar la referencia directa si es válida. Si no es válida,
    /// busca el fantasma por su número identificador entre los fantasmas activos.
    /// </remarks>
    public FantasmaBase GetFantasmaControlado() 
    {
        // Si tenemos una referencia válida y activa, la usamos
        if (fantasmaControladoActual != null && fantasmaControladoActual.gameObject.activeInHierarchy)
            return fantasmaControladoActual;
            
        // Si no, intentamos obtener el fantasma por su número
        return numeroFantasmaControladoPorJugador >= 1 ? 
            GetFantasma(numeroFantasmaControladoPorJugador) : null;
    }

    /// <summary>
    /// Obtiene un fantasma específico por su número identificador.
    /// </summary>
    /// <param name="numeroFantasma">El número identificador del fantasma a buscar (1-4)</param>
    /// <returns>Referencia al fantasma si existe y está activo, o null si no se encuentra</returns>
    public FantasmaBase GetFantasma(int numeroFantasma)
    {
        return poolFantasmas.BuscarActivo(f => f?.GetNumeroFantasma() == numeroFantasma);
    }

    /// <summary>
    /// Devuelve el porcentaje de cooldown transcurrido para el cambio de fantasma (0-1).
    /// </summary>
    /// <returns>
    /// Valor entre 0 y 1, donde 0 significa que el cambio está disponible y 
    /// 1 significa que el cooldown está en su máximo (recién usado).
    /// </returns>
    public float GetPorcentajeCooldownCambioFantasma()
    {
        float tiempoTranscurrido = Time.time - tiempoUltimoCambioJugador;
        return tiempoTranscurrido >= cooldownCambioFantasma ? 0f : 1f - (tiempoTranscurrido / cooldownCambioFantasma);
    }

    /// <summary>
    /// Reinicia el sistema completo de fantasmas. 
    /// Reactiva todos los fantasmas, restaura su estado y reasigna el control al fantasma del jugador.
    /// Este método debe llamarse cuando se cambia de nivel, se reinicia el juego, o cuando
    /// se necesita restablecer todos los fantasmas a su estado inicial.
    /// </summary>
    public void ReiniciarSistema()
    {        
        // Primero activamos todos los fantasmas inactivos
        var fantasmasInactivos = poolFantasmas.GetInactivos().ToList();
        foreach (var fantasma in fantasmasInactivos)
        {
            if (fantasma != null)
            {
                // Utilizar el método RevivirFantasma con reinicio completo y sin reproducir sonido
                RevivirFantasma(fantasma, true, false);
            }
        }

        // Ahora todos los fantasmas están activos y no derribados

        // Asignar el control al fantasma correspondiente
        fantasmaControladoActual = null;
        foreach (var fantasma in poolFantasmas.GetActivos())
        {
            bool esControlado = fantasma.GetNumeroFantasma() == numeroFantasmaControladoPorJugador;
            fantasma.AsignarControlador(esControlado);
            
            if (esControlado)
                fantasmaControladoActual = fantasma;
        }
        
        // Reiniciar la capacidad de revivir en el controlador de jugador
        if (ControladorJugador.Instancia != null)
        {
            ControladorJugador.Instancia.ReiniciarCapacidadRevivir();
        }
            
        // Reiniciar el temporizador de cambio de fantasma
        tiempoUltimoCambioJugador = -cooldownCambioFantasma;  // Permite cambio inmediato
            
        // Actualizar UI notificando el fantasma actualmente controlado
        OnFantasmaControlCambiado?.Invoke(numeroFantasmaControladoPorJugador);
    }

    /// <summary>
    /// Comprueba si un fantasma específico está siendo controlado actualmente por el jugador.
    /// </summary>
    /// <param name="fantasma">El fantasma a comprobar</param>
    /// <returns>True si el fantasma existe y está siendo controlado por el jugador, false en caso contrario</returns>
    public bool EsFantasmaControlado(FantasmaBase fantasma)
    {
        if (fantasma == null) return false;
        return fantasma.GetNumeroFantasma() == numeroFantasmaControladoPorJugador;
    }
    
    /// <summary>
    /// Comprueba si un número de fantasma corresponde al fantasma actualmente controlado por el jugador.
    /// </summary>
    /// <param name="numeroFantasma">El número identificador del fantasma a comprobar</param>
    /// <returns>True si el número corresponde al fantasma controlado actualmente, false en caso contrario</returns>
    public bool EsFantasmaControlado(int numeroFantasma)
    {
        return numeroFantasma == numeroFantasmaControladoPorJugador;
    }

    /// <summary>
    /// Comprueba si un fantasma se encuentra en la lista de inactivos del pool.
    /// Los fantasmas inactivos son aquellos que han sido derrotados pero aún pueden ser revividos.
    /// </summary>
    /// <param name="fantasma">El fantasma a comprobar</param>
    /// <returns>True si el fantasma existe y está en la lista de inactivos</returns>
    public bool ContieneFantasmaInactivo(FantasmaBase fantasma)
    {
        return fantasma != null && poolFantasmas.ContieneInactivo(fantasma);
    }
    
    /// <summary>
    /// Busca un fantasma por su número identificador en cualquier estado (activo o inactivo).
    /// </summary>
    /// <param name="numeroFantasma">El número identificador del fantasma a buscar</param>
    /// <returns>La referencia al fantasma si se encuentra, o null si no existe</returns>
    /// <remarks>
    /// Este método busca primero en la lista de fantasmas activos por eficiencia,
    /// y si no lo encuentra ahí, busca en la lista de inactivos (fantasmas derribados).
    /// </remarks>
    public FantasmaBase BuscarFantasmaPorNumero(int numeroFantasma)
    {
        // Primero intentar encontrarlo en los activos (más común)
        var fantasma = poolFantasmas.BuscarActivo(f => f?.GetNumeroFantasma() == numeroFantasma);
        
        // Si no se encuentra entre los activos, buscar en los inactivos
        if (fantasma == null)
            fantasma = poolFantasmas.BuscarInactivo(f => f?.GetNumeroFantasma() == numeroFantasma);
            
        return fantasma;
    }

    /// <summary>
    /// Obtiene la cámara compartida utilizada por todos los fantasmas.
    /// </summary>
    /// <returns>
    /// Referencia a la cámara compartida que utilizan los fantasmas para la perspectiva de juego.
    /// </returns>
    /// <remarks>
    /// Utiliza la cámara asignada desde el Inspector. Esta cámara debe estar preconfigurada
    /// en la escena con todos los componentes necesarios (MinimapCamera, NieblaPorCamara, etc.).
    /// Si no hay una cámara asignada, mostrará un error en la consola.
    /// 
    /// Esta implementación soluciona problemas de persistencia de referencias entre reinicios de
    /// niveles. La asignación manual en el Inspector garantiza que los componentes dependientes
    /// como MinimapRenderer puedan acceder a la cámara correcta en todo momento, incluyendo
    /// después de reiniciar la partida, evitando así excepciones del tipo NullReferenceException.
    /// </remarks>
    public Camera ObtenerCamaraCompartida() => camaraCompartidaAsignada;
}