using UnityEngine;

/// <summary>
/// Habilidad que aumenta temporalmente la velocidad de movimiento del personaje.
/// </summary>
/// <remarks>
/// Esta clase implementa una habilidad que potencia la velocidad del personaje
/// multiplicándola por un factor configurable. Es útil para escapar de 
/// situaciones peligrosas o para alcanzar objetivos rápidamente.
/// </remarks>
public class HabilidadRapidez : HabilidadBase
{
    /// <summary>
    /// Factor por el que se multiplica la velocidad original del personaje.
    /// </summary>
    /// <remarks>
    /// Este valor determina cuánto aumentará la velocidad del personaje.
    /// Un valor de 2.0 duplica la velocidad, 1.5 la aumenta en un 50%, etc.
    /// </remarks>
    [SerializeField] private float multiplicadorVelocidad = 2f;
    
    /// <summary>
    /// Almacena la velocidad base del personaje para restaurarla después.
    /// </summary>
    private float velocidadOriginal;
    
    /// <summary>
    /// Referencia al componente Personaje que posee esta habilidad.
    /// </summary>
    private Personaje personaje;    /// <summary>
    /// Inicializa la habilidad obteniendo y guardando la referencia al personaje y su velocidad original.
    /// </summary>
    /// <remarks>
    /// Este método se llama durante la inicialización de la habilidad.
    /// Obtiene la referencia al componente Personaje y guarda su velocidad original
    /// para poder restaurarla cuando termine el efecto de la habilidad.
    /// </remarks>
    protected override void InicializarHabilidad()
    {
        personaje = GetComponent<Personaje>();
        velocidadOriginal = personaje.VelocidadMovimiento;
    }

    /// <summary>
    /// Aplica el boost de velocidad cuando se activa la habilidad.
    /// </summary>
    /// <remarks>
    /// Este método se llama cuando el jugador activa la habilidad.
    /// Utiliza el método ModificarVelocidad del componente Personaje
    /// para multiplicar la velocidad por el factor configurado.
    /// </remarks>
    public override void AplicarEfectoHabilidad() => 
        personaje.ModificarVelocidad(multiplicadorVelocidad);

    /// <summary>
    /// Restaura la velocidad original cuando termina el efecto de la habilidad.
    /// </summary>
    /// <remarks>
    /// Este método se llama automáticamente cuando finaliza la duración de la habilidad.
    /// Restaura la velocidad del personaje a su valor original para terminar el efecto.
    /// </remarks>
    public override void RemoverEfectoHabilidad() => 
        personaje.AjustarVelocidad(velocidadOriginal);
}