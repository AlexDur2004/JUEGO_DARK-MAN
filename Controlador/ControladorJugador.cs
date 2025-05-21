using UnityEngine;
using Utilidades;

/// <summary>
/// Clase que gestiona los controles del jugador y el manejo de los personajes
/// </summary>
/// <remarks>
/// Esta clase implementa un singleton y se encarga de procesar las entradas del usuario,
/// gestionar el movimiento y rotación del personaje controlado, y coordinar acciones especiales
/// como solicitar ayuda a fantasmas o revivirlos.
/// </remarks>
/// <seealso cref="Singleton{T}"/>
/// <seealso cref="Personaje"/>
/// <seealso cref="FantasmaBase"/>
/// <seealso cref="InterfazJuego"/>
/// <seealso cref="SistemaGestionFantasmas"/>
public class ControladorJugador : Singleton<ControladorJugador>
{
    /// <summary>
    /// Personaje actualmente controlado por el jugador
    /// </summary>
    /// <remarks>
    /// Referencia al personaje que está siendo controlado activamente por el jugador
    /// en este momento. Puede ser un fantasma u otro tipo de personaje.
    /// </remarks>
    private Personaje personajeActual;

    /// <summary>
    /// Acumulador de rotación vertical para la cámara
    /// </summary>
    /// <remarks>
    /// Guarda el ángulo actual de rotación vertical de la cámara.
    /// Se utiliza para aplicar límites y evitar que la cámara gire demasiado.
    /// </remarks>
    private float rotacionX = 0f;

    [Header("Configuración de movimiento")]
    /// <summary>
    /// Sensibilidad del ratón para la rotación
    /// </summary>
    /// <remarks>
    /// Determina la velocidad a la que rota la cámara en respuesta al movimiento del ratón.
    /// Un valor más alto hace que la cámara gire más rápidamente.
    /// </remarks>
    public float sensibilidadRaton = 2f;

    /// <summary>
    /// Límite de rotación vertical (en grados)
    /// </summary>
    /// <remarks>
    /// Define el ángulo máximo que puede rotar la cámara hacia arriba o hacia abajo.
    /// </remarks>
    public float limiteVertical = 80f;

    /// <summary>
    /// Fuerza de gravedad aplicada al personaje
    /// </summary>
    /// <remarks>
    /// Valor de aceleración aplicado verticalmente al personaje cuando no está en el suelo.
    /// </remarks>
    public float gravedad = -9.8f;

    /// <summary>
    /// Velocidad actual de caída
    /// </summary>
    /// <remarks>
    /// Vector que representa la velocidad vertical actual del personaje, usado para simular la gravedad.
    /// </remarks>
    private Vector3 velocidadVertical = Vector3.zero;

    [Header("Configuración de Ayuda y Revivir")]
    /// <summary>
    /// Tiempo de espera entre solicitudes de ayuda (en segundos)
    /// </summary>
    /// <remarks>
    /// Define la duración del cooldown entre solicitudes de ayuda a los fantasmas.
    /// El jugador no podrá pedir ayuda nuevamente hasta que este tiempo haya transcurrido.
    /// </remarks>
    [SerializeField] private float cooldownAyuda = 10f;

    /// <summary>
    /// Tiempo de la última solicitud de ayuda
    /// </summary>
    /// <remarks>
    /// Almacena el momento en que se realizó la última solicitud de ayuda, para calcular si
    /// ha pasado suficiente tiempo para permitir una nueva solicitud.
    /// </remarks>
    private float tiempoUltimaAyuda = -999f;

    /// <summary>
    /// Indica si ya se usó la habilidad de revivir en esta ronda
    /// </summary>
    /// <remarks>
    /// Controla si el jugador puede revivir a un fantasma durante la ronda actual.
    /// Se establece a false después de revivir a un fantasma, y se restablece a true
    /// al comenzar una nueva ronda.
    /// </remarks>
    [SerializeField] private bool puedeRevivirEnRonda = true;

    /// <summary>
    /// Procesa los inputs del jugador cada frame
    /// </summary>
    /// <remarks>
    /// Se ejecuta una vez por frame y gestiona todas las entradas del usuario,
    /// delegando en métodos específicos para cada tipo de control (movimiento, rotación, habilidades).
    /// No se ejecuta si el juego está en pausa o no hay un personaje controlado.
    /// </remarks>
    private void Update()
    {
        if (Pausa.pausa) return;
        if (personajeActual == null) return;

        ProcesarInputGlobal();
        ControlarRotacion();
        ControlarMovimiento();
        ControlarHabilidad();
    }

    /// <summary>
    /// Maneja inputs globales como cambiar entre fantasmas, pedir ayuda o revivir
    /// </summary>
    /// <remarks>
    /// Procesa las combinaciones de teclas para realizar acciones globales:
    /// - Teclas numéricas (1-3): Cambiar al fantasma correspondiente
    /// - Q + número: Solicitar ayuda al fantasma correspondiente
    /// - R + número: Intentar revivir al fantasma correspondiente
    /// </remarks>
    private void ProcesarInputGlobal()
    {
        // Verificaciones iniciales
        FantasmaBase fantasmaActual = personajeActual as FantasmaBase;
        if (fantasmaActual == null || InterfazJuego.Instancia == null) return;

        bool teclaSOS = Input.GetKey(KeyCode.Q);
        bool teclaRevivir = Input.GetKey(KeyCode.R);

        // Procesar teclas 1-3 para cambiar fantasma, solicitar ayuda o revivir
        for (int i = 0; i <= 2; i++) // i = 0,1,2 para teclas 1,2,3
        {
            int tecla = i + 1; // tecla = 1,2,3 (posición UI)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                if (teclaSOS)
                {
                    // Solicitar ayuda al fantasma usando la posición de la UI (1,2,3)
                    // Esto llamará al fantasma que está en esa posición, no al fantasma con ese número
                    Debug.Log($"Detectada combinación Q+{i + 1}. Solicitando ayuda al fantasma en posición UI {tecla}");
                    fantasmaActual.ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Ayuda);
                    OrdenarSeguimientoFantasmaDirecto(tecla);
                }
                else if (teclaRevivir)
                {
                    // Intentar revivir al fantasma usando la posición de la UI (1,2,3)
                    Debug.Log($"Detectada combinación R+{i + 1}. Intentando revivir al fantasma en posición UI {tecla}");
                    RevivirFantasmaPorPosicionUI(tecla);
                }
                else
                    InterfazJuego.Instancia.CambiarFantasmaPorPosicionUI(tecla); // Cambiar al fantasma

                break; // Salir del bucle al detectar una tecla válida
            }
        }
    }

    /// <summary>
    /// Ordena a un fantasma de IA que siga al jugador (por número de fantasma directo)
    /// </summary>
    /// <param name="numeroFantasma">Número identificador del fantasma que debe seguir al jugador</param>
    /// <remarks>
    /// Activa el modo de seguimiento en el fantasma especificado por su número interno,
    /// siempre que esté activo, no sea el controlado por el jugador, y se respete el cooldown.
    /// </remarks>
    private void OrdenarSeguimientoFantasmaNumero(int numeroFantasma)
    {
        // Verificar cooldown
        if (Time.time < tiempoUltimaAyuda + cooldownAyuda)
        {
            return;
        }

        // Verificaciones básicas
        if (numeroFantasma < 1 || numeroFantasma >= InterfazJuego.NUM_PERSONAJES) return;

        Debug.Log($"OrdenarSeguimientoFantasmaNumero: Solicitando ayuda directamente al fantasma #{numeroFantasma}");

        // Obtener sistema y fantasma
        SistemaGestionFantasmas sistema = SistemaGestionFantasmas.Instancia;
        if (sistema == null) return;

        FantasmaBase fantasma = sistema.GetFantasma(numeroFantasma);

        // Verificar que el fantasma existe, está activo y no es el controlado por el jugador
        if (fantasma == null ||
            !fantasma.gameObject.activeInHierarchy ||
            numeroFantasma == sistema.GetNumeroFantasmaControlado())
        {
            Debug.Log($"No se puede activar el seguimiento para fantasma #{numeroFantasma}: no existe, no está activo o es el controlado por el jugador");
            return;
        }

        // Activar modo de seguimiento
        ControladorIA controladorIA = fantasma.GetComponent<ControladorIA>();
        if (controladorIA != null)
        {
            Debug.Log($"Activando seguimiento para fantasma #{numeroFantasma}");

            // Registrar tiempo de uso para cooldown
            tiempoUltimaAyuda = Time.time;

            // Activar seguimiento (actualizará el icono en la interfaz)
            controladorIA.ActivarSeguimientoJugador();
        }
    }

    /// <summary>
    /// Ordena a un fantasma de IA que siga al jugador (por posición UI)
    /// </summary>
    /// <param name="posicionUI">Posición del fantasma en la interfaz de usuario (1-3)</param>
    /// <remarks>
    /// Activa el modo de seguimiento en el fantasma especificado por su posición en la UI.
    /// Traduce la posición UI al número real del fantasma y verifica todas las condiciones
    /// necesarias antes de activar el seguimiento.
    /// </remarks>
    private void OrdenarSeguimientoFantasmaDirecto(int posicionUI)
    {
        // Verificar cooldown
        if (Time.time < tiempoUltimaAyuda + cooldownAyuda)
        {
            return;
        }

        // La posición UI viene como 1-based (1,2,3)
        // Verificaciones básicas
        if (InterfazJuego.Instancia == null ||
            posicionUI < 1 ||
            posicionUI >= InterfazJuego.NUM_PERSONAJES) return;

        // Obtenemos el número real del fantasma según la posición en UI
        int numeroFantasma = InterfazJuego.Instancia.ObtenerNumeroFantasmaDesdePosicionUI(posicionUI);

        Debug.Log($"OrdenarSeguimiento: posición UI {posicionUI} (1-based) corresponde al fantasma #{numeroFantasma}");

        // Obtener sistema y fantasma
        SistemaGestionFantasmas sistema = SistemaGestionFantasmas.Instancia;
        if (sistema == null) return;

        FantasmaBase fantasma = sistema.GetFantasma(numeroFantasma);

        // Verificar que el fantasma existe, está activo y no es el controlado por el jugador
        if (fantasma == null ||
            !fantasma.gameObject.activeInHierarchy ||
            numeroFantasma == sistema.GetNumeroFantasmaControlado())
        {
            Debug.Log($"No se puede activar el seguimiento para fantasma #{numeroFantasma} en posición UI {posicionUI}: no existe, no está activo o es el controlado por el jugador");
            return;
        }

        // Activar modo de seguimiento
        ControladorIA controladorIA = fantasma.GetComponent<ControladorIA>();
        if (controladorIA != null)
        {
            Debug.Log($"Activando seguimiento para fantasma #{numeroFantasma} en posición UI {posicionUI}");

            // Registrar tiempo de uso para cooldown
            tiempoUltimaAyuda = Time.time;

            // Activar seguimiento (actualizará el icono en la interfaz)
            controladorIA.ActivarSeguimientoJugador();
        }
    }

    /// <summary>
    /// Ordena a un fantasma de IA que siga al jugador (método alternativo)
    /// </summary>
    /// <param name="posicionUI">Posición del fantasma en la interfaz de usuario (1-3)</param>
    /// <remarks>
    /// Implementación alternativa que tiene el mismo comportamiento que OrdenarSeguimientoFantasmaDirecto.
    /// Se mantiene por compatibilidad con código existente.
    /// </remarks>
    private void OrdenarSeguimientoFantasma(int posicionUI)
    {
        // Verificar cooldown
        if (Time.time < tiempoUltimaAyuda + cooldownAyuda)
        {
            return;
        }

        // Verificaciones básicas
        if (InterfazJuego.Instancia == null ||
            posicionUI < 1 ||
            posicionUI >= InterfazJuego.NUM_PERSONAJES) return;

        // Obtener sistema y fantasma
        SistemaGestionFantasmas sistema = SistemaGestionFantasmas.Instancia;
        if (sistema == null) return;

        int numeroFantasma = InterfazJuego.Instancia.ObtenerNumeroFantasmaDesdePosicionUI(posicionUI);
        FantasmaBase fantasma = sistema.GetFantasma(numeroFantasma);

        // Verificar que el fantasma existe, está activo y no es el controlado por el jugador
        if (fantasma == null ||
            !fantasma.gameObject.activeInHierarchy ||
            numeroFantasma == sistema.GetNumeroFantasmaControlado()) return;

        // Activar modo de seguimiento
        ControladorIA controladorIA = fantasma.GetComponent<ControladorIA>();
        if (controladorIA != null)
        {
            // Registrar tiempo de uso para cooldown
            tiempoUltimaAyuda = Time.time;

            // Activar seguimiento (actualizará el icono en la interfaz)
            controladorIA.ActivarSeguimientoJugador();
        }
    }

    /// <summary>
    /// Detiene a todos los fantasmas que estén siguiendo al jugador
    /// </summary>
    /// <remarks>
    /// Recorre todos los fantasmas activos en el sistema y desactiva
    /// el modo de seguimiento en cada uno de ellos. Se utiliza cuando
    /// el jugador cambia de personaje o en otras situaciones donde
    /// se debe cancelar el seguimiento de todos los fantasmas.
    /// </remarks>
    private void DetenerFantasmasEnSeguimiento()
    {
        SistemaGestionFantasmas sistema = SistemaGestionFantasmas.Instancia;
        if (sistema == null) return;

        // Obtener todos los fantasmas activos
        var fantasmasActivos = sistema.GetFantasmasActivos();
        if (fantasmasActivos == null) return;

        // Detener el modo seguimiento de todos los fantasmas
        foreach (var fantasma in fantasmasActivos)
        {
            if (fantasma != null && fantasma.gameObject.activeInHierarchy)
            {
                ControladorIA controladorIA = fantasma.GetComponent<ControladorIA>();
                if (controladorIA != null)
                {
                    controladorIA.DetenerSeguimientoJugador();
                }
            }
        }
    }

    /// <summary>
    /// Gestiona el movimiento del personaje basado en los inputs
    /// </summary>
    /// <remarks>
    /// Procesa las entradas de los ejes horizontal y vertical (WASD/flechas)
    /// para mover al personaje en la dirección correspondiente, aplicando
    /// la velocidad de movimiento propia del personaje y la gravedad.
    /// </remarks>
    private void ControlarMovimiento()
    {
        CharacterController controller = personajeActual.GetComponent<CharacterController>();
        if (controller == null) return;

        // Obtiene los valores de input horizontal y vertical
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Si no hay movimiento significativo, evitamos cálculos
        if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
        {
            // Sólo aplicamos gravedad
            AplicarGravedad(controller);
            return;
        }

        // Calcula la dirección del movimiento relativa a la orientación del personaje
        Vector3 direccion = personajeActual.transform.right * horizontal + personajeActual.transform.forward * vertical;
        direccion.Normalize();

        // Aplica la velocidad de movimiento del personaje
        Vector3 movimiento = direccion * personajeActual.VelocidadMovimiento;

        // Aplica la gravedad
        AplicarGravedad(controller);

        // Realiza el movimiento final
        controller.Move((movimiento + velocidadVertical) * Time.deltaTime);
    }

    /// <summary>
    /// Aplica la gravedad al personaje
    /// </summary>
    /// <param name="controller">El CharacterController del personaje</param>
    /// <remarks>
    /// Ajusta la velocidad vertical del personaje para simular la gravedad.
    /// Si el personaje está en el suelo, aplica una pequeña fuerza hacia abajo
    /// para mantener el contacto. Si no está en el suelo, incrementa la velocidad
    /// de caída según la gravedad configurada.
    /// </remarks>
    private void AplicarGravedad(CharacterController controller)
    {
        if (controller.isGrounded)
            velocidadVertical.y = -0.5f;  // Pequeña fuerza hacia abajo para mantener contacto con el suelo
        else
            velocidadVertical.y += gravedad * Time.deltaTime;
    }

    /// <summary>
    /// Maneja la rotación del personaje y la cámara basada en el movimiento del ratón
    /// </summary>
    /// <remarks>
    /// Procesa el movimiento del ratón para rotar el personaje horizontalmente
    /// y la cámara verticalmente. Aplica límites a la rotación vertical para
    /// evitar que la cámara gire más allá de cierto ángulo hacia arriba o abajo.
    /// </remarks>
    private void ControlarRotacion()
    {
        // Obtiene la entrada del ratón
        float mouseX = Input.GetAxisRaw("Mouse X") * sensibilidadRaton;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensibilidadRaton;

        if (Mathf.Approximately(mouseX, 0f) && Mathf.Approximately(mouseY, 0f))
            return;

        // Rota el personaje horizontalmente
        personajeActual.transform.Rotate(Vector3.up * mouseX);

        // Obtiene la referencia a la cámara del personaje
        Camera camaraPersonaje = null;
        FantasmaBase fantasma = personajeActual as FantasmaBase;
        if (fantasma != null)
            camaraPersonaje = fantasma.camaraFantasma;

        if (camaraPersonaje != null)
        {
            // Aplica la rotación vertical a la cámara con límites
            rotacionX -= mouseY;
            rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);
            camaraPersonaje.transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        }
    }

    /// <summary>
    /// Activa la habilidad del personaje cuando se presiona la tecla asignada
    /// </summary>
    /// <remarks>
    /// Verifica si se ha pulsado la tecla correspondiente a la habilidad del
    /// personaje actual, y en ese caso, intenta activar dicha habilidad.
    /// Cada personaje puede tener una tecla diferente asignada para su habilidad.
    /// </remarks>
    private void ControlarHabilidad()
    {
        if (personajeActual == null) return;

        KeyCode teclaHabilidad = personajeActual.GetTeclaHabilidad();
        if (teclaHabilidad != KeyCode.None && Input.GetKeyDown(teclaHabilidad))
            personajeActual.IntentarActivarHabilidad();
    }

    /// <summary>
    /// Asigna un nuevo personaje para ser controlado por el jugador
    /// </summary>
    /// <param name="personaje">El personaje que pasará a ser controlado por el jugador</param>
    /// <remarks>
    /// Establece el personaje especificado como el personaje controlado actualmente
    /// por el jugador. Detiene cualquier fantasma que esté en modo seguimiento y
    /// restablece la rotación de la cámara.
    /// </remarks>
    public void ControlarPersonaje(Personaje personaje)
    {
        // Si estamos cambiando de personaje, detener fantasmas en seguimiento
        if (personaje != personajeActual)
        {
            DetenerFantasmasEnSeguimiento();
        }

        personajeActual = personaje;
        rotacionX = 0f;  // Resetea la rotación vertical

        if (personaje != null)
        {
            // Resetea la rotación de la cámara si es un fantasma
            FantasmaBase fantasma = personaje as FantasmaBase;
            if (fantasma?.camaraFantasma != null)
                fantasma.camaraFantasma.transform.localRotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Devuelve el personaje actualmente controlado por el jugador
    /// </summary>
    /// <returns>El personaje controlado por el jugador o null si no hay ninguno</returns>
    /// <remarks>
    /// Proporciona acceso al personaje que está siendo controlado actualmente
    /// por el jugador. Otros sistemas pueden usar este método para interactuar
    /// con el personaje del jugador.
    /// </remarks>
    public Personaje GetPersonajeControlado() => personajeActual;

    /// <summary>
    /// Devuelve el porcentaje de cooldown transcurrido para SOS (0-1)
    /// </summary>
    /// <returns>
    /// Valor entre 0 y 1: 0 = disponible, 1 = cooldown completo (recién usado)
    /// </returns>
    /// <remarks>
    /// Calcula qué fracción del tiempo de cooldown ha transcurrido desde la última
    /// solicitud de ayuda. Este valor puede ser utilizado por la interfaz de usuario
    /// para mostrar el estado del cooldown visualmente.
    /// </remarks>
    public float GetPorcentajeCooldownSOS()
    {
        float tiempoActual = Time.time;
        float tiempoTranscurrido = tiempoActual - tiempoUltimaAyuda;

        if (tiempoTranscurrido >= cooldownAyuda)
            return 0f; // Ya no hay cooldown, está disponible

        // Devuelve fracción que queda (1 = recién usado, 0 = disponible)
        return 1f - (tiempoTranscurrido / cooldownAyuda);
    }

    /// <summary>
    /// Reinicia el cooldown de SOS (usado cuando se cambia de fantasma)
    /// </summary>
    /// <remarks>
    /// Establece el tiempo de la última ayuda a un valor que permita usar
    /// la habilidad de SOS de forma inmediata. Se utiliza típicamente cuando
    /// el jugador cambia de personaje para permitir que use la habilidad sin esperar.
    /// </remarks>
    public void ReiniciarCooldownSOS()
    {
        tiempoUltimaAyuda = -cooldownAyuda; // Permite usar SOS inmediatamente
    }

    /// <summary>
    /// Revive a un fantasma inactivo por su posición en la UI
    /// </summary>
    /// <param name="posicionUI">Posición del fantasma en la interfaz de usuario (1-3)</param>
    /// <remarks>
    /// Intenta revivir al fantasma correspondiente a la posición especificada en la UI.
    /// Verifica que el fantasma exista, esté inactivo (derribado) y que el jugador
    /// aún tenga disponible la habilidad de revivir en la ronda actual.
    /// Solo se puede revivir un fantasma por ronda.
    /// </remarks>
    private void RevivirFantasmaPorPosicionUI(int posicionUI)
    {
        // Verificar si ya se usó la habilidad de revivir en esta ronda
        if (!puedeRevivirEnRonda)
        {
            Debug.Log("No se puede revivir más fantasmas en esta ronda. Límite alcanzado.");
            return;
        }

        // Verificaciones básicas
        if (InterfazJuego.Instancia == null ||
            posicionUI < 1 ||
            posicionUI >= InterfazJuego.NUM_PERSONAJES) return;

        // Obtenemos el número real del fantasma según la posición en UI
        int numeroFantasma = InterfazJuego.Instancia.ObtenerNumeroFantasmaDesdePosicionUI(posicionUI);

        Debug.Log($"RevivirFantasma: posición UI {posicionUI} (1-based) corresponde al fantasma #{numeroFantasma}");

        // Obtener sistema y buscar el fantasma (incluyendo inactivos)
        SistemaGestionFantasmas sistema = SistemaGestionFantasmas.Instancia;
        if (sistema == null) return;

        FantasmaBase fantasma = sistema.BuscarFantasmaPorNumero(numeroFantasma);

        // Verificar que el fantasma existe y está inactivo (derribado)
        if (fantasma == null)
        {
            Debug.Log($"No se puede revivir el fantasma #{numeroFantasma} en posición UI {posicionUI}: no existe");
            return;
        }

        if (!sistema.ContieneFantasmaInactivo(fantasma))
        {
            Debug.Log($"No se puede revivir el fantasma #{numeroFantasma} en posición UI {posicionUI}: no está derribado");
            return;
        }

        // Revivir al fantasma
        Debug.Log($"Reviviendo fantasma #{numeroFantasma} en posición UI {posicionUI}");
        sistema.RevivirFantasma(fantasma);

        // Marcar que ya se usó la habilidad de revivir en esta ronda
        puedeRevivirEnRonda = false;

        // Reproducir efecto de sonido desde el fantasma actualmente controlado
        FantasmaBase fantasmaActual = personajeActual as FantasmaBase;
        if (fantasmaActual != null)
            fantasmaActual.ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Revivir);
    }

    /// <summary>
    /// Reinicia la habilidad de revivir para una nueva ronda
    /// </summary>
    /// <remarks>
    /// Restablece la capacidad del jugador para revivir fantasmas al inicio
    /// de una nueva ronda. Debe llamarse cuando comienza una nueva ronda de juego.
    /// </remarks>
    public void ReiniciarCapacidadRevivir()
    {
        puedeRevivirEnRonda = true;
    }

    /// <summary>
    /// Devuelve si el jugador puede revivir en la ronda actual
    /// </summary>
    /// <returns>True si el jugador puede revivir a un fantasma en esta ronda, False en caso contrario</returns>
    /// <remarks>
    /// Indica si el jugador aún no ha usado su habilidad de revivir en la ronda actual.
    /// Esta información puede ser utilizada por la interfaz de usuario para mostrar
    /// la disponibilidad de la habilidad.
    /// </remarks>
    public bool PuedeRevivirEnRonda() => puedeRevivirEnRonda;
}