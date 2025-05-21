using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Clase base para todas las habilidades especiales del juego.
/// Proporciona el ciclo de vida completo de una habilidad: activación, duración y cooldown.
/// </summary>
/// <remarks>
/// Esta clase abstracta define la estructura básica para implementar habilidades especiales
/// en el juego. Maneja automáticamente el sistema de cooldown, duración del efecto,
/// y proporciona métodos abstractos que las clases derivadas deben implementar para
/// definir el comportamiento específico de cada habilidad.
/// </remarks>
public abstract class HabilidadBase : MonoBehaviour
{
    [Header("Configuración Base")]
    /// <summary>
    /// Tiempo de recarga de la habilidad en segundos.
    /// </summary>
    /// <remarks>
    /// Determina cuánto tiempo debe esperar el jugador después de usar la habilidad
    /// para poder volver a activarla. Valor predeterminado: 60 segundos.
    /// </remarks>
    [SerializeField] protected float cooldownBase = 60f;
    
    /// <summary>
    /// Tecla para activar la habilidad.
    /// </summary>
    /// <remarks>
    /// Define qué tecla del teclado activará esta habilidad cuando sea presionada.
    /// Valor predeterminado: Tecla E.
    /// </remarks>
    [SerializeField] protected KeyCode teclaHabilidad = KeyCode.E;
    
    /// <summary>
    /// Duración del efecto de la habilidad en segundos.
    /// </summary>
    /// <remarks>
    /// Especifica cuánto tiempo permanece activo el efecto de la habilidad
    /// una vez que se activa. Valor predeterminado: 5 segundos.
    /// </remarks>
    [SerializeField] protected float duracionBase = 5f;

    /// <summary>
    /// Tiempo restante para poder usar la habilidad nuevamente.
    /// </summary>
    /// <remarks>
    /// Este valor se reduce gradualmente desde cooldownBase hasta 0.
    /// Cuando llega a 0, la habilidad está disponible para usar nuevamente.
    /// </remarks>
    [HideInInspector] public float tiempoRestante;
    
    /// <summary>
    /// Tiempo transcurrido desde que se activó la habilidad.
    /// </summary>
    /// <remarks>
    /// Este valor aumenta desde 0 hasta duracionBase mientras la habilidad está activa.
    /// Se utiliza para determinar cuándo finalizar el efecto de la habilidad.
    /// </remarks>
    [HideInInspector] public float tiempoTranscurrido;
    
    /// <summary>
    /// Indica si la habilidad está actualmente en uso.
    /// </summary>
    /// <remarks>
    /// Propiedad calculada que determina si la habilidad está activa basándose
    /// en si el tiempo transcurrido es mayor que cero.
    /// </remarks>
    protected bool habilidadActiva => tiempoTranscurrido > 0f;
    
    /// <summary>
    /// Referencia a la corrutina activa de la habilidad.
    /// </summary>
    /// <remarks>
    /// Almacena la referencia a la corrutina que gestiona el ciclo de vida
    /// de la habilidad, permitiendo detenerla si es necesario.
    /// </remarks>
    protected Coroutine corrutinaHabilidadActual;
      /// <summary>
    /// Propiedad que verifica si se puede usar la habilidad.
    /// </summary>
    /// <remarks>
    /// Retorna verdadero cuando el tiempo restante de cooldown es cero o menor,
    /// lo que indica que la habilidad ha terminado su período de recarga
    /// y está lista para ser activada nuevamente.
    /// </remarks>
    /// <value>
    /// <c>true</c> si la habilidad está disponible para usar (no está en cooldown); 
    /// <c>false</c> si aún está en período de espera.
    /// </value>
    protected bool PuedeUsar => tiempoRestante <= 0;    /// <summary>
    /// Inicializa los valores de la habilidad al comenzar.
    /// </summary>
    /// <remarks>
    /// Este método es llamado automáticamente por Unity cuando el GameObject
    /// es creado. Realiza dos acciones importantes:
    /// 1. Establece el tiempo restante de cooldown a cero, permitiendo que 
    ///    la habilidad sea utilizada inmediatamente al inicio del juego.
    /// 2. Llama al método InicializarHabilidad() que las clases derivadas
    ///    pueden implementar para realizar configuraciones específicas.
    /// </remarks>
    protected virtual void Start()
    {
        tiempoRestante = 0f;  // Inicia con la habilidad disponible
        InicializarHabilidad();
    }/// <summary>
    /// Método virtual que pueden sobrescribir las clases derivadas para inicializaciones específicas.
    /// </summary>
    /// <remarks>
    /// Las clases derivadas pueden implementar este método para realizar configuraciones específicas 
    /// de la habilidad al inicio. Por ejemplo, cargar prefabs, inicializar efectos visuales, etc.
    /// Por defecto no hace nada.
    /// </remarks>
    protected virtual void InicializarHabilidad() { }    /// <summary>
    /// Punto de entrada principal para activar la habilidad.
    /// Detiene cualquier instancia anterior y comienza una nueva.
    /// </summary>
    /// <remarks>
    /// Este método se debe llamar externamente cuando el jugador activa la habilidad.
    /// Se encarga de reproducir el sonido asociado y gestionar el ciclo de vida completo
    /// de la habilidad a través de la corrutina GestionarHabilidadCoroutine.
    /// </remarks>
    public void ActivarHabilidad()
    {
        // Detiene cualquier instancia previa de la corrutina si existe
        if (corrutinaHabilidadActual != null)
            StopCoroutine(corrutinaHabilidadActual);

        Utilidades.AudioManager.Instancia.ReproducirSonidoGlobal(Utilidades.AudioManager.TipoSonido.Habilidad);
            
        // Inicia la nueva corrutina de gestión de la habilidad
        corrutinaHabilidadActual = StartCoroutine(GestionarHabilidadCoroutine());
    }    /// <summary>
    /// Corrutina que maneja todo el ciclo de vida de la habilidad:
    /// activación, duración, desactivación y cooldown.
    /// </summary>
    /// <remarks>
    /// Esta corrutina implementa la lógica central de la vida de una habilidad:
    /// 1. Inicia el cooldown
    /// 2. Marca la habilidad como activa
    /// 3. Aplica el efecto específico
    /// 4. Mantiene el efecto durante la duración configurada
    /// 5. Remueve el efecto
    /// 6. Gestiona el tiempo de cooldown antes de permitir su reuso
    /// 
    /// Las clases derivadas pueden extender esta funcionalidad sobrescribiendo este método
    /// y llamando a base.GestionarHabilidadCoroutine() cuando sea apropiado.
    /// </remarks>
    /// <returns>Un enumerador que Unity utiliza para manejar la corrutina.</returns>
    protected virtual IEnumerator GestionarHabilidadCoroutine()
    {
        tiempoRestante = cooldownBase;
        tiempoTranscurrido = 0.1f; // Iniciamos con un valor mayor que 0 para activar la habilidad

        // Aplica el efecto específico de la habilidad (implementado en clases derivadas)
        AplicarEfectoHabilidad();

        // Mantiene el efecto activo durante la duración configurada e incrementa tiempoTranscurrido
        float tiempoEspera = 0.1f; // Actualizamos cada décima de segundo para mayor precisión
        while (tiempoTranscurrido < duracionBase)
        {
            yield return new WaitForSeconds(tiempoEspera);
            tiempoTranscurrido += tiempoEspera;
        }

        // Desactiva el efecto cuando termina la duración
        RemoverEfectoHabilidad();
        tiempoTranscurrido = 0f; // Al poner a 0 se desactiva la habilidad
        
        // Inicia el proceso de cooldown antes de poder usar la habilidad nuevamente
        yield return GestionarCooldown();
    }    /// <summary>
    /// Gestiona el tiempo de recarga de la habilidad con una cuenta regresiva.
    /// </summary>
    /// <param name="nombreHabilidad">Nombre opcional de la habilidad para registro.</param>
    /// <remarks>
    /// Esta corrutina maneja el período de cooldown después de usar una habilidad.
    /// Va reduciendo el tiempo restante gradualmente hasta llegar a cero,
    /// momento en el cual la habilidad vuelve a estar disponible para su uso.
    /// </remarks>
    /// <returns>Un enumerador que Unity utiliza para manejar la corrutina.</returns>
    protected IEnumerator GestionarCooldown(string nombreHabilidad = "")
    {
        tiempoRestante = cooldownBase;

        // Reduce el tiempo restante en intervalos de un segundo
        while (tiempoRestante > 0f)
        {
            yield return new WaitForSeconds(1f);
            tiempoRestante = Mathf.Max(0f, tiempoRestante - 1f);
        }

        // Marca la habilidad como disponible nuevamente
        tiempoRestante = 0f;
        
        corrutinaHabilidadActual = null;
    }

    /// <summary>
    /// Aplica el efecto específico de la habilidad (ej: invisibilidad, velocidad).
    /// </summary>
    /// <remarks>
    /// Las clases derivadas deben implementar este método para definir el comportamiento 
    /// concreto cuando se activa la habilidad. Algunos ejemplos incluyen:
    /// - Aplicar modificadores de velocidad o fuerza al personaje
    /// - Crear escudos o barreras defensivas
    /// - Activar efectos visuales
    /// - Alterar el estado del personaje (invisibilidad, invulnerabilidad, etc.)
    /// </remarks>
    public abstract void AplicarEfectoHabilidad();
    
    /// <summary>
    /// Remueve o revierte el efecto de la habilidad cuando termina.
    /// </summary>
    /// <remarks>
    /// Las clases derivadas deben implementar este método para definir cómo 
    /// se revierten los efectos aplicados cuando la habilidad finaliza.
    /// Es importante que este método limpie correctamente todos los efectos
    /// para evitar que persistan después de la duración establecida.
    /// </remarks>
    public abstract void RemoverEfectoHabilidad();    /// <summary>
    /// Método llamado por Unity cuando el objeto se activa.
    /// Reinicia todos los valores de la habilidad cuando se activa, útil para cambios de ronda.
    /// </summary>
    /// <remarks>
    /// Este método se ejecuta automáticamente cuando el GameObject se activa.
    /// Realiza las siguientes acciones:
    /// 1. Reinicia el tiempo de cooldown a cero
    /// 2. Reinicia el tiempo transcurrido a cero
    /// 3. Cancela cualquier corrutina activa
    /// 4. Remueve efectos activos de la habilidad si los hubiera
    /// 
    /// Las clases derivadas pueden extender esta funcionalidad sobrescribiendo el método
    /// y llamando a base.OnEnable() para mantener este comportamiento base.
    /// </remarks>
    protected virtual void OnEnable()
    {
        // Reinicia el tiempo restante para que la habilidad esté disponible
        tiempoRestante = 0f;
        
        // Asegura que no haya tiempo transcurrido de uso de habilidad
        tiempoTranscurrido = 0f;
        
        // Cancela cualquier corrutina activa
        if (corrutinaHabilidadActual != null)
        {
            StopCoroutine(corrutinaHabilidadActual);
            corrutinaHabilidadActual = null;
        }
        
        // Asegura que cualquier efecto de habilidad esté removido
        if (habilidadActiva)
        {
            RemoverEfectoHabilidad();
        }
    }    /// <summary>
    /// Método llamado por Unity cuando el objeto se desactiva.
    /// Asegura que los efectos de la habilidad se remuevan si estaba activa.
    /// </summary>
    /// <remarks>
    /// Este método se ejecuta automáticamente cuando el GameObject se desactiva.
    /// Realiza las siguientes acciones:
    /// 1. Si la habilidad está activa, remueve su efecto
    /// 2. Reinicia el tiempo transcurrido a cero
    /// 3. Detiene cualquier corrutina en ejecución
    /// 
    /// Este método es crucial para evitar que los efectos de las habilidades
    /// permanezcan activos cuando el objeto que los controla se desactiva.
    /// </remarks>
    protected virtual void OnDisable()
    {
        // Si la habilidad está activa al desactivar el objeto, remover su efecto
        if (habilidadActiva)
        {
            RemoverEfectoHabilidad();
            tiempoTranscurrido = 0f;
        }
        
        // Detener cualquier corrutina en ejecución
        if (corrutinaHabilidadActual != null)
        {
            StopCoroutine(corrutinaHabilidadActual);
            corrutinaHabilidadActual = null;
        }
    }    /// <summary>
    /// Verifica si la habilidad puede usarse (no está en cooldown ni activa actualmente).
    /// </summary>
    /// <remarks>
    /// Este método combina dos condiciones:
    /// 1. La habilidad no debe estar en período de cooldown (PuedeUsar)
    /// 2. La habilidad no debe estar actualmente en uso (!habilidadActiva)
    /// </remarks>
    /// <returns>True si la habilidad está disponible para usar.</returns>
    public bool PuedeUsarHabilidad() => PuedeUsar && !habilidadActiva;      /// <summary>
    /// Calcula qué porcentaje del cooldown o duración ha completado.
    /// </summary>
    /// <remarks>
    /// Este método tiene dos comportamientos distintos:
    /// - Si la habilidad está activa: Devuelve 1 - (tiempo transcurrido / duración base)
    ///   Esto representa el tiempo restante de la habilidad como porcentaje.
    /// - Si está en cooldown: Devuelve 1 - (tiempo restante / cooldown base)
    ///   Esto representa el tiempo restante de cooldown como porcentaje.
    /// 
    /// El valor devuelto aumenta a medida que el tiempo pasa, siendo 0 al inicio
    /// y 1 cuando está completamente disponible o cuando el efecto ha terminado.
    /// </remarks>
    /// <returns>Valor entre 0 y 1 que representa el progreso del cooldown o duración.</returns>
    public float GetPorcentajeCooldown()
    {
        if (habilidadActiva)
            return duracionBase > 0 ? 1f - (tiempoTranscurrido / duracionBase) : 0f;
        else
            return cooldownBase > 0 ? 1f - (tiempoRestante / cooldownBase) : 1f;
    }    /// <summary>
    /// Reduce el tiempo de recarga pendiente.
    /// </summary>
    /// <param name="reduccion">Cantidad de segundos a reducir del cooldown.</param>
    /// <remarks>
    /// Este método permite aplicar reducciones al tiempo de cooldown restante,
    /// lo que es útil para implementar habilidades o items que aceleren
    /// la recarga de otras habilidades. Nunca reduce el cooldown por debajo de cero.
    /// </remarks>
    public void ReducirCooldown(float reduccion)
    {
        tiempoRestante = Mathf.Max(0f, tiempoRestante - reduccion);
    }    /// <summary>
    /// Métodos de utilidad y acceso para obtener información del estado de la habilidad.
    /// </summary>
    
    /// <summary>
    /// Indica si la habilidad está actualmente en efecto.
    /// </summary>
    /// <remarks>
    /// Verifica si la habilidad se encuentra activa basándose en el valor
    /// de tiempoTranscurrido. Se considera activa si tiempoTranscurrido > 0.
    /// </remarks>
    /// <returns>True si la habilidad está activa.</returns>
    public bool EstaActiva() => habilidadActiva;
    
    /// <summary>
    /// Indica si la habilidad está en período de recarga.
    /// </summary>
    /// <remarks>
    /// Una habilidad está en cooldown cuando no puede usarse debido a que 
    /// el tiempo de recarga aún no ha finalizado (tiempoRestante > 0).
    /// </remarks>
    /// <returns>True si la habilidad está en cooldown.</returns>
    public bool EstaEnCooldown() => !PuedeUsar;
    
    /// <summary>
    /// Devuelve el tiempo total de recarga configurado.
    /// </summary>
    /// <remarks>
    /// Este valor representa la cantidad de segundos que debe esperar el jugador
    /// antes de poder volver a utilizar la habilidad después de usarla.
    /// </remarks>
    /// <returns>La duración base del cooldown en segundos.</returns>
    public float GetCooldownBase() => cooldownBase;
    
    /// <summary>
    /// Devuelve la tecla configurada para activar la habilidad.
    /// </summary>
    /// <remarks>
    /// Esta tecla se define en el inspector de Unity y permite al sistema
    /// de entrada identificar cuándo el jugador quiere activar esta habilidad.
    /// </remarks>
    /// <returns>El KeyCode asignado a la habilidad.</returns>
    public KeyCode GetTeclaHabilidad() => teclaHabilidad;
}