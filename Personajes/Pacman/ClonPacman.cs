using UnityEngine;

/// <summary>
/// Implementación de un clon temporal de Pacman que se autodestruye después de un tiempo determinado.
/// </summary>
/// <remarks>
/// Esta clase representa un clon temporal de Pacman que tiene una duración de vida limitada.
/// Se utiliza como parte de las habilidades de DarkMan para crear señuelos o distracciones.
/// El clon se destruye automáticamente cuando su tiempo de vida llega a cero o cuando recibe daño mortal.
/// </remarks>
public class ClonPacman : Personaje
{
    [Header("Configuración")]
    /// <summary>Tiempo en segundos que el clon permanecerá activo antes de autodestruirse</summary>
    [SerializeField] private float duracionVida = 5f;

    /// <summary>Contador de tiempo de vida restante del clon</summary>
    private float tiempoVida;

    /// <summary>
    /// Inicializa el clon de Pacman cuando se crea.
    /// </summary>
    /// <remarks>
    /// Este método configura el tiempo de vida inicial del clon y establece su etiqueta (tag)
    /// para que pueda ser identificado correctamente por otros componentes del juego.
    /// Llama al método Start de la clase base para inicializar las propiedades heredadas.
    /// </remarks>
    protected override void Start()
    {
        base.Start();
        tiempoVida = duracionVida;
        gameObject.tag = "ClonPacman";
    }

    /// <summary>
    /// Configura la duración de vida del clon de Pacman.
    /// </summary>
    /// <param name="duracion">Tiempo en segundos que el clon permanecerá activo</param>
    /// <remarks>
    /// Este método permite ajustar dinámicamente el tiempo de vida del clon.
    /// Puede ser llamado desde la habilidad que crea el clon u otros sistemas
    /// para personalizar la duración según el nivel de habilidad o mejoras del jugador.
    /// </remarks>
    public void Inicializar(float duracion)
    {
        duracionVida = duracion;
        tiempoVida = duracion;
    }

    /// <summary>
    /// Actualiza el estado del clon cada frame y verifica si debe autodestruirse.
    /// </summary>
    /// <remarks>
    /// Este método se ejecuta una vez por frame y:
    /// - Actualiza el contador de tiempo de vida restante
    /// - Verifica si el tiempo ha expirado para iniciar la autodestrucción
    /// - Llama al Update de la clase base para mantener la funcionalidad heredada
    /// </remarks>
    protected override void Update()
    {
        base.Update();

        tiempoVida -= Time.deltaTime;
        if (tiempoVida <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Implementa el comportamiento del clon cuando su vida llega a cero por daño recibido.
    /// </summary>
    /// <remarks>
    /// Implementación del método abstracto definido en la clase base Personaje.
    /// Cuando el clon recibe daño fatal:
    /// 1. Reproduce el sonido de muerte usando el AudioManager
    /// 2. Inicia la secuencia de destrucción del clon
    /// 
    /// A diferencia de otros personajes, el clon no tiene animación de muerte
    /// ni respawn, simplemente se destruye inmediatamente.
    /// </remarks>
    protected override void ProcesarMuerte()
    {
        ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Muerte);
        Destroy(gameObject);
    }
}