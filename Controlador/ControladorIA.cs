using UnityEngine;
using UnityEngine.AI;
using Utilidades;

/// <summary>
/// Clase que gestiona el comportamiento de la IA para personajes fantasmas y Pacman
/// </summary>
/// <remarks>
/// Implementa distintos comportamientos para los personajes controlados por IA, incluyendo
/// detección de objetivos, seguimiento, patrullaje y ataques. Adapta el comportamiento
/// según el tipo de personaje (Fantasma o DarkMan).
/// </remarks>
/// <seealso cref="Personaje"/>
/// <seealso cref="FantasmaBase"/>
/// <seealso cref="DetectionUtils"/>
/// <seealso cref="SpawnerLaberinto"/>
public class ControladorIA : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    /// <summary>
    /// Radio para detectar a Pacman
    /// </summary>
    /// <remarks>
    /// Define la distancia máxima a la que un personaje puede detectar a Pacman 
    /// y decidir perseguirlo.
    /// </remarks>
    [SerializeField] private float radioDeteccionPacman = 3f;

    /// <summary>
    /// Radio para detectar fantasmas aliados
    /// </summary>
    /// <remarks>
    /// Define la distancia máxima a la que un personaje puede detectar a otros fantasmas aliados
    /// para coordinar ataques o comportamiento en grupo.
    /// </remarks>
    [SerializeField] private float radioDeteccionAliados = 5f;

    /// <summary>
    /// Radio para detectar premios y frutas
    /// </summary>
    /// <remarks>
    /// Define la distancia máxima a la que un personaje puede detectar y perseguir premios.
    /// </remarks>
    [SerializeField] private float radioDeteccionPremios = 4f;

    /// <summary>
    /// Distancia mínima para realizar un ataque
    /// </summary>
    /// <remarks>
    /// Define la proximidad necesaria a un objetivo para iniciar un ataque.
    /// </remarks>
    [SerializeField] private float distanciaMinimaAtaque = 2f;

    // Referencias y estado
    /// <summary>
    /// Referencia al personaje que controla esta IA
    /// </summary>
    /// <remarks>
    /// Componente Personaje asociado a este GameObject que será controlado por la IA.
    /// </remarks>
    private Personaje personajeControlado;

    /// <summary>
    /// Punto objetivo actual hacia el que se mueve
    /// </summary>
    /// <remarks>
    /// Posición en el mundo a la que el personaje intentará moverse.
    /// </remarks>
    private Vector3 objetivoActual;

    /// <summary>
    /// Capa de los fantasmas para detección
    /// </summary>
    /// <remarks>
    /// Máscara de capas utilizada para detectar únicamente objetos en la capa de Fantasmas.
    /// </remarks>
    private LayerMask capaFantasma;

    /// <summary>
    /// Capa de los premios para detección
    /// </summary>
    /// <remarks>
    /// Máscara de capas utilizada para detectar únicamente objetos en la capa de Premios.
    /// </remarks>
    private LayerMask capaPremios;

    /// <summary>
    /// Referencia en caché al transform
    /// </summary>
    /// <remarks>
    /// Guarda una referencia al transform del GameObject para evitar múltiples llamadas a GetComponent.
    /// </remarks>
    private Transform cachedTransform;

    /// <summary>
    /// Buffers reutilizables para detección (evitan garbage collection)
    /// </summary>
    /// <remarks>
    /// Arrays preasignados que se utilizan para almacenar resultados de Physics.OverlapSphere
    /// sin generar nuevas asignaciones de memoria, optimizando el rendimiento.
    /// </remarks>
    private static Collider[] bufferAliadosCercanos = new Collider[10];
    private static Collider[] bufferPremiosCercanos = new Collider[5];

    /// <summary>
    /// Estado del seguimiento al jugador
    /// </summary>
    /// <remarks>
    /// Variables que controlan si la IA está siguiendo al jugador y por cuánto tiempo lo ha hecho.
    /// </remarks>
    private bool siguiendoJugador = false;
    private float tiempoSiguiendoJugador = 0f;

    /// <summary>
    /// Duración máxima del seguimiento al jugador en segundos
    /// </summary>
    /// <remarks>
    /// Determina cuánto tiempo seguirá la IA al jugador antes de volver al comportamiento normal.
    /// </remarks>
    private const float DURACION_SEGUIMIENTO = 15f;

    /// <summary>
    /// Temporizador para cambiar de objetivo automáticamente
    /// </summary>
    /// <remarks>
    /// Sistema para generar nuevos objetivos de patrullaje periódicamente.
    /// </remarks>
    [SerializeField]
    /// <summary>
    /// Tiempo entre cambios de objetivo (segundos)
    /// </summary>
    /// <remarks>
    /// Intervalo de tiempo entre la generación de nuevos puntos de patrullaje.
    /// </remarks>
    private float tiempoEntreObjetivos = 5f;

    /// <summary>
    /// Contador del tiempo transcurrido
    /// </summary>
    /// <remarks>
    /// Lleva el registro del tiempo desde el último cambio de objetivo.
    /// </remarks>
    private float temporizadorObjetivo = 0f;

    /// <summary>
    /// Inicialización de componentes
    /// </summary>
    /// <remarks>
    /// Se ejecuta al inicio para configurar todas las referencias y valores iniciales
    /// necesarios para el funcionamiento del controlador de IA.
    /// </remarks>
    private void Start()
    {
        // Inicializar componentes esenciales
        cachedTransform = transform;
        capaFantasma = LayerMask.GetMask("Fantasma");
        capaPremios = LayerMask.GetMask("Premio");
        personajeControlado = GetComponent<Personaje>();

        objetivoActual = cachedTransform.position;
    }

    /// <summary>
    /// Actualización del comportamiento cada frame
    /// </summary>
    /// <remarks>
    /// Maneja los temporizadores para seguimiento y cambio de objetivos,
    /// y coordina la ejecución del comportamiento de IA apropiado.
    /// </remarks>
    private void Update()
    {
        if (personajeControlado == null || personajeControlado.EstaInactivo()) return;

        // Actualizar tiempo de seguimiento si está activo
        if (siguiendoJugador)
        {
            tiempoSiguiendoJugador += Time.deltaTime;
            if (tiempoSiguiendoJugador >= DURACION_SEGUIMIENTO)
            {
                DetenerSeguimientoJugador();
            }
        }
        // Actualizar temporizador de objetivos cuando no está siguiendo al jugador
        else
        {
            temporizadorObjetivo += Time.deltaTime;
            if (temporizadorObjetivo >= tiempoEntreObjetivos)
            {
                GenerarNuevoObjetivo();
                temporizadorObjetivo = 0f;
            }
        }

        ProcesarComportamiento();
    }

    /// <summary>
    /// Procesa el comportamiento general del personaje
    /// </summary>
    /// <remarks>
    /// Función principal que determina qué debe hacer el personaje en el frame actual,
    /// considerando su estado, tipo y objetivos cercanos. Actúa como un despachador
    /// que selecciona y ejecuta el comportamiento adecuado.
    /// </remarks>
    private void ProcesarComportamiento()
    {
        Vector3 posicionActual = cachedTransform.position;

        // Determinar el objetivo de movimiento según el estado actual
        Vector3 objetivoEspecifico = Vector3.zero;
        bool tieneObjetivoEspecifico = false;

        // Si está siguiendo al jugador, obtener su posición como objetivo
        if (siguiendoJugador)
        {
            Personaje personajeJugador = ControladorJugador.Instancia?.GetPersonajeControlado();
            if (personajeJugador != null && personajeJugador.gameObject.activeInHierarchy)
            {
                Vector3 posicionJugador = personajeJugador.transform.position;
                float distanciaAlJugador = Vector3.Distance(posicionActual, posicionJugador);

                // Si estamos muy cerca del jugador, no hace falta moverse más
                if (distanciaAlJugador <= distanciaMinimaAtaque * 0.5f)
                    return;

                objetivoEspecifico = posicionJugador;
                tieneObjetivoEspecifico = true;
            }
            else
            {
                // El jugador ya no existe, detener seguimiento
                siguiendoJugador = false;
            }
        }

        // Si no tenemos un objetivo válido, generamos uno
        if (objetivoActual == Vector3.zero) GenerarNuevoObjetivo();

        // Ejecutar comportamiento según tipo de personaje
        if (gameObject.CompareTag("Fantasma"))
        {
            ComportamientoFantasma(tieneObjetivoEspecifico ? objetivoEspecifico : objetivoActual);
        }
        else if (gameObject.CompareTag("DarkMan"))
        {
            ComportamientoPacman(tieneObjetivoEspecifico ? objetivoEspecifico : objetivoActual);
        }
    }

    /// <summary>
    /// Comportamiento específico para fantasmas controlados por IA
    /// </summary>
    /// <param name="objetivoPersonalizado">Vector3 opcional que determina hacia dónde debe moverse el fantasma</param>
    /// <remarks>
    /// Implementa la lógica de comportamiento específica para los fantasmas,
    /// incluyendo persecución de Pacman, recolección de premios, y coordinación
    /// con otros fantasmas para realizar ataques en grupo.
    /// </remarks>
    private void ComportamientoFantasma(Vector3 objetivoPersonalizado = default)
    {
        Vector3 posicionActual = cachedTransform.position;
        bool estaEnModoSeguimiento = siguiendoJugador && objetivoPersonalizado != default;

        // 1. Prioridad: Verificar si hay premios cerca (excepto en modo seguimiento)
        if (!estaEnModoSeguimiento)
        {
            GameObject premioCercano = DetectarPremioCercano(posicionActual);
            if (premioCercano != null)
            {
                MoverHaciaObjetivo(premioCercano.transform.position);
                return;
            }
        }

        // 2. Obtener posición de Pacman (si existe)
        if (Pacman.Instancia == null || Pacman.Instancia.gameObject == null)
        {
            Patrullar(); // Si no hay Pacman, solo podemos patrullar
            return;
        }

        Vector3 posicionPacman = Pacman.Instancia.gameObject.transform.position;
        float distanciaAPacman = Vector3.Distance(posicionActual, posicionPacman);

        // 3. Modo seguimiento: seguir al jugador y atacar a Pacman si está cerca del jugador
        if (estaEnModoSeguimiento)
        {
            float distanciaPacmanJugador = Vector3.Distance(posicionPacman, objetivoPersonalizado);

            if (distanciaPacmanJugador <= radioDeteccionPacman && distanciaAPacman <= distanciaMinimaAtaque * 3)
            {
                MoverHaciaObjetivo(posicionPacman);
                personajeControlado.IntentarActivarHabilidad();
            }
            else
            {
                MoverHaciaObjetivo(objetivoPersonalizado);
            }
            return;
        }

        // 4. Modo normal: atacar o perseguir a Pacman si está cerca
        int contadorAliados = ContarAliadosCercanos(posicionActual);

        if (contadorAliados > 0 && distanciaAPacman <= distanciaMinimaAtaque * 3)
        {
            // Atacar cuando hay aliados cercanos
            MoverHaciaObjetivo(posicionPacman);
            personajeControlado.IntentarActivarHabilidad();
        }
        else if (distanciaAPacman <= radioDeteccionPacman)
        {
            // Solo perseguir a Pacman
            MoverHaciaObjetivo(posicionPacman);
        }
        else if (objetivoPersonalizado != default)
        {
            // Seguir objetivo personalizado
            MoverHaciaObjetivo(objetivoPersonalizado);
        }
        else
        {
            Patrullar();
        }
    }

    /// <summary>
    /// Cuenta cuántos fantasmas aliados activos hay cerca
    /// </summary>
    /// <param name="posicion">La posición desde la que buscar fantasmas aliados</param>
    /// <returns>Número de fantasmas aliados activos dentro del radio especificado</returns>
    /// <remarks>
    /// Detecta los fantasmas dentro del radio de detección, excluyendo al propio personaje,
    /// y verifica que estén activos para incluirlos en el conteo. Este método es fundamental
    /// para la coordinación de ataques en grupo.
    /// </remarks>
    private int ContarAliadosCercanos(Vector3 posicion)
    {
        int cantidad = DetectionUtils.DetectarObjetosNoAlloc(
            posicion, radioDeteccionAliados, capaFantasma, bufferAliadosCercanos);

        if (cantidad == 0) return 0;

        GameObject esteObjeto = gameObject;
        int contador = 0;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject objetoDetectado = bufferAliadosCercanos[i].gameObject;

            if (objetoDetectado != esteObjeto)
            {
                FantasmaBase fantasma = objetoDetectado.GetComponent<FantasmaBase>();
                if (fantasma != null && !fantasma.EstaInactivo())
                {
                    contador++;
                }
            }
        }

        return contador;
    }

    /// <summary>
    /// Detecta si hay un premio cercano y devuelve una referencia al mismo
    /// </summary>
    /// <param name="posicion">La posición desde la que buscar premios</param>
    /// <returns>El GameObject del premio más cercano o null si no hay ninguno</returns>
    /// <remarks>
    /// Busca objetos en la capa de premios dentro del radio de detección
    /// y determina cuál es el más cercano a la posición especificada.
    /// </remarks>
    private GameObject DetectarPremioCercano(Vector3 posicion)
    {
        if (capaPremios == 0)
            capaPremios = LayerMask.GetMask("Premio");

        int cantidad = DetectionUtils.DetectarObjetosNoAlloc(
            posicion, radioDeteccionPremios, capaPremios, bufferPremiosCercanos);

        if (cantidad == 0) return null;

        // Encontrar el premio más cercano
        GameObject premioCercano = null;
        float distanciaMinima = float.MaxValue;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject premio = bufferPremiosCercanos[i].gameObject;
            float distancia = Vector3.Distance(posicion, premio.transform.position);

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                premioCercano = premio;
            }
        }

        return premioCercano;
    }

    /// <summary>
    /// Mueve el personaje hacia una posición objetivo
    /// </summary>
    /// <param name="posicionObjetivo">La posición en el mundo hacia la que debe moverse el personaje</param>
    /// <remarks>
    /// Calcula la dirección hacia el objetivo y utiliza el componente Personaje
    /// para mover al personaje en esa dirección. Incluye manejo de errores
    /// para recuperar referencias si son nulas.
    /// </remarks>
    private void MoverHaciaObjetivo(Vector3 posicionObjetivo)
    {
        if (cachedTransform == null || personajeControlado == null)
        {
            // Intentar recuperar las referencias necesarias
            if (cachedTransform == null) cachedTransform = transform;
            if (personajeControlado == null) personajeControlado = GetComponent<Personaje>();

            // Verificar si la recuperación fue exitosa
            if (cachedTransform == null || personajeControlado == null)
            {
                Debug.LogError($"{gameObject.name}: No se puede mover hacia el objetivo - faltan referencias esenciales");
                return;
            }
        }

        Vector3 direccion = posicionObjetivo - cachedTransform.position;
        personajeControlado.MoverHacia(direccion.normalized);
    }

    /// <summary>
    /// Realiza el comportamiento de patrulla hacia el objetivo actual
    /// </summary>
    /// <remarks>
    /// Implementa un comportamiento básico de patrullaje, moviendo al personaje
    /// hacia el objetivo actual de patrulla. Si no hay un objetivo válido,
    /// genera uno nuevo automáticamente.
    /// </remarks>
    private void Patrullar()
    {
        // Verificar que tengamos las referencias necesarias
        if (cachedTransform == null || personajeControlado == null)
        {
            // Intentar recuperar las referencias
            if (cachedTransform == null) cachedTransform = transform;
            if (personajeControlado == null) personajeControlado = GetComponent<Personaje>();

            // Si aún no las tenemos, salir
            if (cachedTransform == null || personajeControlado == null) return;
        }

        // Verificar que tenemos un objetivo válido
        if (objetivoActual == Vector3.zero)
        {
            GenerarNuevoObjetivo();
            return;
        }

        Vector3 direccion = objetivoActual - cachedTransform.position;
        personajeControlado.MoverHacia(direccion.normalized);
    }

    /// <summary>
    /// Comportamiento específico para Pacman controlado por IA
    /// </summary>
    /// <param name="objetivoPersonalizado">Vector3 opcional que determina hacia dónde debe moverse Pacman</param>
    /// <remarks>
    /// Implementa la lógica de comportamiento específica para Pacman/DarkMan,
    /// con prioridades diferentes a las de los fantasmas. Prioriza la recolección
    /// de premios y el ataque a fantasmas vulnerables.
    /// </remarks>
    private void ComportamientoPacman(Vector3 objetivoPersonalizado = default)
    {
        Vector3 posicionActual = cachedTransform.position;

        // 1. Prioridad: Buscar premios cercanos
        GameObject premioCercano = DetectarPremioCercano(posicionActual);
        if (premioCercano != null)
        {
            MoverHaciaObjetivo(premioCercano.transform.position);
            return;
        }

        // 2. Buscar fantasmas no derribados cercanos para atacar
        int cantidad = DetectionUtils.DetectarObjetosNoAlloc(
            posicionActual, radioDeteccionPacman, capaFantasma, bufferAliadosCercanos);

        if (cantidad > 0)
        {
            for (int i = 0; i < cantidad; i++)
            {
                GameObject objetoDetectado = bufferAliadosCercanos[i].gameObject;
                FantasmaBase fantasma = objetoDetectado.GetComponent<FantasmaBase>();

                // Verificar que el fantasma existe y no está derribado
                if (fantasma != null && !fantasma.EstaInactivo())
                {
                    Vector3 posicionFantasma = objetoDetectado.transform.position;
                    MoverHaciaObjetivo(posicionFantasma);

                    // Intenta activar la habilidad si está suficientemente cerca
                    if (Vector3.Distance(posicionActual, posicionFantasma) <= distanciaMinimaAtaque)
                        personajeControlado.IntentarActivarHabilidad();

                    return;
                }
            }
        }

        // 3. Seguir objetivo personalizado si hay uno
        if (objetivoPersonalizado != default)
        {
            MoverHaciaObjetivo(objetivoPersonalizado);
        }
        // 4. Patrullar si no hay otras opciones
        else
        {
            Patrullar();
        }
    }

    /// <summary>
    /// Genera un nuevo punto objetivo para el patrullaje
    /// </summary>
    /// <remarks>
    /// Crea un nuevo punto de destino dentro del laberinto para el personaje.
    /// Prioriza el uso del SpawnerLaberinto para obtener posiciones válidas,
    /// pero incluye un método alternativo si éste no está disponible.
    /// </remarks>
    private void GenerarNuevoObjetivo()
    {
        // Restablecer el temporizador cuando se genera un nuevo objetivo
        temporizadorObjetivo = 0f;

        if (SpawnerLaberinto.Instancia != null)
        {
            objetivoActual = SpawnerLaberinto.Instancia.ObtenerPosicionAleatoriaEnLaberinto();
            return;
        }

        // Fallback solo si SpawnerLaberinto no está disponible
        Vector2 dimensiones = ObtenerDimensionesLaberinto();
        objetivoActual = new Vector3(
            Random.Range(0, dimensiones.x),
            0,
            Random.Range(0, dimensiones.y)
        );
    }

    /// <summary>
    /// Obtiene las dimensiones actuales del laberinto
    /// </summary>
    /// <returns>Vector2 que representa el ancho y alto del laberinto</returns>
    /// <remarks>
    /// Utiliza el GameManager para obtener las dimensiones del laberinto actual.
    /// Si el GameManager no está disponible, devuelve un valor predeterminado.
    /// </remarks>
    private Vector2 ObtenerDimensionesLaberinto()
    {
        if (GameManager.Instancia != null)
            return GameManager.Instancia.GetDimensionesLaberinto();

        return new Vector2(10, 10);
    }

    /// <summary>
    /// Se llama cuando el componente se activa
    /// </summary>
    /// <remarks>
    /// Inicializa el estado del controlador cuando se activa,
    /// generando un nuevo objetivo si es necesario y estableciendo
    /// un valor aleatorio para el temporizador para evitar que todos
    /// los personajes cambien de objetivo al mismo tiempo.
    /// </remarks>
    protected virtual void OnEnable()
    {
        if (cachedTransform != null && objetivoActual == Vector3.zero)
            GenerarNuevoObjetivo();

        temporizadorObjetivo = Random.Range(0f, tiempoEntreObjetivos * 0.5f);
    }

    /// <summary>
    /// Se llama cuando el componente se desactiva
    /// </summary>
    /// <remarks>
    /// Limpia el estado del controlador cuando se desactiva,
    /// deteniendo el movimiento del personaje y reiniciando
    /// el objetivo actual para que se genere uno nuevo al reactivarse.
    /// </remarks>
    protected virtual void OnDisable()
    {
        if (personajeControlado != null)
            personajeControlado.DetenerMovimiento();

        objetivoActual = Vector3.zero;
    }

    /// <summary>
    /// Activa el modo de seguimiento al jugador
    /// </summary>
    /// <remarks>
    /// Configura el personaje para que siga al jugador durante un período específico.
    /// También actualiza la interfaz de usuario para mostrar un indicador visual
    /// de que el fantasma está en modo seguimiento.
    /// </remarks>
    public void ActivarSeguimientoJugador()
    {
        siguiendoJugador = true;
        tiempoSiguiendoJugador = 0f;

        if (personajeControlado == null)
        {
            personajeControlado = GetComponent<Personaje>();
            if (personajeControlado == null) return;
        }

        // Notificar a la interfaz para mostrar indicador de seguimiento
        FantasmaBase fantasma = personajeControlado as FantasmaBase;
        if (fantasma != null)
        {
            InterfazJuego.Instancia?.ActualizarIconoSeguimiento(fantasma.GetNumeroFantasma(), true);
        }
    }

    /// <summary>
    /// Detiene el modo de seguimiento al jugador
    /// </summary>
    /// <remarks>
    /// Finaliza el modo de seguimiento al jugador, reinicia los temporizadores,
    /// genera un nuevo objetivo de patrulla y actualiza la interfaz para
    /// reflejar que el fantasma ya no está siguiendo al jugador.
    /// </remarks>
    public void DetenerSeguimientoJugador()
    {
        if (siguiendoJugador)
        {
            siguiendoJugador = false;
            tiempoSiguiendoJugador = 0f;

            // Al terminar el seguimiento, generar un nuevo objetivo
            // y reiniciar el temporizador
            GenerarNuevoObjetivo();

            // Notificar a la interfaz para ocultar indicador de seguimiento
            FantasmaBase fantasma = personajeControlado as FantasmaBase;
            if (fantasma != null)
            {
                InterfazJuego.Instancia?.ActualizarIconoSeguimiento(fantasma.GetNumeroFantasma(), false);
            }
        }
    }

    /// <summary>
    /// Método para indicar si el fantasma está siguiendo al jugador
    /// </summary>
    /// <returns>True si el personaje está en modo de seguimiento al jugador, False en caso contrario</returns>
    /// <remarks>
    /// Proporciona acceso de solo lectura al estado interno de seguimiento,
    /// permitiendo a otros componentes del juego conocer si este personaje
    /// está activamente persiguiendo al jugador.
    /// </remarks>
    public bool EstaSiguiendoJugador() => siguiendoJugador;
}