using UnityEngine;
using System.Collections;

/// <summary>
/// Clase para gestionar el comportamiento de un arma en el juego.
/// Implementa sistema de daño, ataques críticos y detección de enemigos.
/// </summary>
/// <remarks>
/// Esta clase base proporciona la funcionalidad común para todas las armas del juego,
/// incluyendo detección de objetivos, cálculo de daño y animaciones de ataque.
/// </remarks>
/// <seealso cref="Personaje"/>
/// <seealso cref="FantasmaBase"/>
/// <seealso cref="ControladorIA"/>
/// <seealso cref="GestorNumerosDaño"/>
public class ArmaBase : MonoBehaviour
{
    [Header("Configuración Base")]
    /// <summary>
    /// Daño base que inflige el arma
    /// </summary>
    /// <remarks>
    /// Valor numérico que representa la cantidad de daño que se aplica al objetivo.
    /// Este valor puede ser modificado durante el juego con ModificarDañoBase().
    /// </remarks>
    [SerializeField] private int dañoBase = 1;

    /// <summary>
    /// Probabilidad de realizar un golpe crítico (rango 0-1)
    /// </summary>
    /// <remarks>
    /// Un valor entre 0 y 1 que representa la probabilidad de que un ataque sea crítico.
    /// 0 significa que nunca habrá críticos, 1 significa que todos los ataques serán críticos.
    /// </remarks>
    [SerializeField] private float probabilidadCritico = 0.3f;

    /// <summary>
    /// Multiplicador aplicado al daño base cuando ocurre un crítico
    /// </summary>
    /// <remarks>
    /// Factor por el cual se multiplica el daño base cuando un ataque resulta ser crítico.
    /// </remarks>
    [SerializeField] private float multiplicadorCritico = 2f;

    [Header("Sistema de Detección")]
    /// <summary>Collider que detecta enemigos en el rango frontal</summary>
    /// <remarks>
    /// Este collider se utiliza para detectar cuando un enemigo está dentro del rango
    /// de ataque del arma, principalmente usado por el sistema de IA.
    /// </remarks>
    [SerializeField] private Collider colliderDeteccion;

    /// <summary>Tag del objetivo a atacar</summary>
    /// <remarks>
    /// Identifica qué tipo de objetos pueden ser atacados por esta arma.
    /// Por defecto es "Fantasma", pero puede cambiar según el tipo de personaje que use el arma.
    /// </remarks>
    private string tagObjetivo = "Fantasma";

    /// <summary>Indica si hay un ataque en proceso actualmente</summary>
    /// <remarks>
    /// Bandera que evita que se inicie un nuevo ataque mientras uno está en curso.
    /// Se usa para controlar la cadencia de ataques.
    /// </remarks>
    private bool ataqueEnProceso = false;

    /// <summary>Referencias a componentes relacionados</summary>
    /// <remarks>
    /// Estas referencias se usan para coordinar el comportamiento del arma
    /// con el personaje que la porta
    /// </remarks>
    private Personaje personajeAsociado;
    private ControladorIA controladorIA;
    private Animator animator;
    /// <summary>Collider del arma ubicado en este mismo objeto</summary>
    private Collider colliderArma;

    /// <summary>
    /// Determina si el arma está siendo controlada por el jugador
    /// </summary>
    /// <returns>True si está siendo controlada por el jugador, False si es controlada por IA</returns>
    private bool EsControlJugador => controladorIA != null && !controladorIA.enabled;

    /// <summary>
    /// Inicializa los componentes y configuración del arma
    /// </summary>
    /// <remarks>
    /// Se ejecuta al iniciar el objeto y configura todas las referencias necesarias,
    /// ajusta los colliders y establece el objetivo según el tipo de personaje
    /// </remarks>
    private void Start()
    {
        // Inicializar componentes
        personajeAsociado = GetComponentInParent<Personaje>();
        controladorIA = personajeAsociado?.GetComponent<ControladorIA>();
        animator = personajeAsociado.GetAnimator();

        // Asignar colliders
        colliderArma = GetComponent<Collider>();

        // Configuración inicial
        if (personajeAsociado is FantasmaBase)
            tagObjetivo = "DarkMan";

        // Los colliders siempre deben estar activos
        colliderArma.enabled = true;
        colliderDeteccion.enabled = true;
    }

    /// <summary>
    /// Actualización por frame para controlar las entradas del jugador
    /// </summary>
    /// <remarks>
    /// Verifica si el jugador ha iniciado un ataque mediante clic izquierdo
    /// o pulsando la tecla C, y comienza la secuencia de ataque
    /// </remarks>
    private void Update()
    {
        if (ataqueEnProceso) return;

        // Control por jugador - activar ataque con clic
        if (EsControlJugador && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.C)))
        {
            StartCoroutine(RealizarAtaque());
        }
    }

    /// <summary>
    /// Realiza un ataque contra un personaje objetivo
    /// </summary>
    /// <param name="objetivo">El personaje que recibirá el daño</param>
    /// <remarks>
    /// Calcula si el ataque es crítico, aplica el daño correspondiente
    /// y muestra los efectos visuales del daño en pantalla
    /// </remarks>
    public void Atacar(Personaje objetivo)
    {
        // Sistema de críticos
        bool esCritico = Random.value < probabilidadCritico;
        int daño = esCritico ? Mathf.RoundToInt(dañoBase * multiplicadorCritico) : dañoBase;

        // Aplicar daño
        objetivo.RecibirDaño(daño);

        // Efectos visuales
        if (GestorNumerosDaño.Instancia != null)
        {
            GestorNumerosDaño.Instancia.MostrarNumeroDaño(daño, objetivo.transform.position);
        }
    }

    /// <summary>
    /// Ejecuta la secuencia completa de un ataque
    /// </summary>
    /// <returns>Un IEnumerator que permite usar esta función como coroutine</returns>
    /// <remarks>
    /// Controla la animación de ataque, efectos de sonido y tiempo de espera
    /// entre ataques. Además detecta el tipo de personaje para ajustar el tipo de ataque.
    /// </remarks>
    private IEnumerator RealizarAtaque()
    {
        if (ataqueEnProceso) yield break;
        ataqueEnProceso = true;

        if (personajeAsociado != null && animator != null)
        {
            float tipoAtaque = 0f;
            if (personajeAsociado is FantasmaBase fantasma)
            {
                tipoAtaque = fantasma.TipoAtaqueAnimator;
                animator.SetFloat("TipoAtaque", tipoAtaque);
            }

            animator.SetTrigger("Atacar");
            personajeAsociado.ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Ataque);
        }

        // Espera solo lo que dura la animación de ataque
        yield return new WaitForSeconds(3f);
        ataqueEnProceso = false;
    }

    /// <summary>
    /// Gestiona la detección de colisiones con otros objetos
    /// </summary>
    /// <param name="other">El collider con el que se ha producido la colisión</param>
    /// <remarks>
    /// Maneja dos casos principales:
    /// 1. Contacto durante un ataque activo: aplica daño al objetivo
    /// 2. Detección de enemigo por IA: inicia un ataque automático
    /// </remarks>
    private void OnTriggerEnter(Collider other)
    {
        // Validar objetivo por tag
        if (!other.CompareTag(tagObjetivo)) return;

        // Obtener referencia al personaje objetivo
        Personaje objetivo = other.GetComponent<Personaje>();
        if (objetivo == null)
        {
            objetivo = other.GetComponentInParent<Personaje>();
            if (objetivo == null)
                objetivo = other.GetComponentInChildren<Personaje>();
            if (objetivo == null) return; // No se encontró componente Personaje
        }

        // CASO 1: Contacto con el arma durante un ataque - aplicar daño
        if (ataqueEnProceso)
        {
            Atacar(objetivo);
            return;
        }

        // CASO 2: IA detecta enemigo - iniciar ataque automático
        // Solo si es controlado por IA y no está en medio de un ataque
        if (!EsControlJugador && !ataqueEnProceso)
        {
            // Verificamos que la colisión sea con el collider de detección
            if (colliderDeteccion != null &&
                other.bounds.Intersects(colliderDeteccion.bounds))
            {
                StartCoroutine(RealizarAtaque());
            }
        }
    }

    /// <summary>
    /// Obtiene el valor actual del daño base del arma
    /// </summary>
    /// <returns>El valor numérico del daño base</returns>
    public int ObtenerDañoBase() => dañoBase;

    /// <summary>
    /// Modifica el daño base del arma
    /// </summary>
    /// <param name="nuevoDaño">Nuevo valor de daño a asignar</param>
    public void ModificarDañoBase(int nuevoDaño) => dañoBase = nuevoDaño;

    /// <summary>
    /// Se ejecuta cuando el objeto es activado
    /// </summary>
    /// <remarks>
    /// Reinicia el estado del ataque para evitar que quede bloqueado
    /// </remarks>
    private void OnEnable()
    {
        // Reiniciamos el estado de ataque
        ataqueEnProceso = false;
    }
}