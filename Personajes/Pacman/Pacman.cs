using UnityEngine;
using System.Collections;
using System;
using Utilidades;

/// <summary>
/// Clase principal que representa al personaje Pacman controlado por el jugador.
/// </summary>
/// <remarks>
/// Este personaje es el protagonista principal del juego y utiliza un patrón Singleton
/// para garantizar que solo exista una instancia en todo momento.
/// Implementa mecánicas específicas como:
/// - Cálculo de vida basado en el número de ronda
/// - Sistema de habilidades especiales personalizadas
/// - Comportamiento al morir que afecta al flujo de juego
/// </remarks>
public class Pacman : Personaje
{
    /// <summary>
    /// Referencia estática a la única instancia de Pacman en el juego (Singleton).
    /// </summary>
    /// <remarks>
    /// Proporciona acceso global a la instancia de Pacman desde cualquier parte del código.
    /// Garantiza que solo exista una instancia en todo momento.
    /// </remarks>
    public static Pacman Instancia { get; private set; }

    /// <summary>
    /// Inicializa el patrón Singleton para el personaje Pacman.
    /// </summary>
    /// <remarks>
    /// Se ejecuta automáticamente al cargar el objeto en la escena y:
    /// - Asigna esta instancia como la instancia única si no existe otra
    /// - Destruye esta instancia si ya existe otra instancia de Pacman
    /// 
    /// Este método garantiza que solo exista un Pacman en la escena
    /// en cualquier momento, evitando comportamientos inesperados.
    /// </remarks>
    protected void Awake()
    {        
        if (Instancia == null) 
            Instancia = this;
        else if (Instancia != this)
            Destroy(gameObject);
    }

    /// <summary>
    /// Configura los valores iniciales específicos del personaje Pacman.
    /// </summary>
    /// <remarks>
    /// Extiende el método Start de la clase base con funcionalidades específicas:
    /// - Calcula y aplica un bonus de vida basado en el número de ronda actual
    /// - Suma 2 puntos de vida adicionales por cada ronda completada
    /// 
    /// Este sistema de escalado de vida permite que el personaje sea más resistente
    /// a medida que avanza en el juego, compensando la dificultad creciente.
    /// </remarks>
    protected override void Start()
    {
        base.Start();
        
        // Establecer valores específicos de Pacman
        // Aumentamos 2 puntos de vida por cada ronda actual
        int bonusRonda = GameManager.Instancia != null ? GameManager.Instancia.ObtenerNumeroRondaActual() * 2 : 0;
        vidaActual = vidaMaxima + bonusRonda;
    }

    /// <summary>
    /// Implementa el comportamiento cuando Pacman pierde toda su vida.
    /// </summary>
    /// <remarks>
    /// Esta implementación del método abstracto de la clase base:
    /// - Desactiva el GameObject de Pacman sin destruirlo
    /// - Esto permite que el GameManager pueda detectar la muerte y procesar
    ///   la transición a la siguiente ronda o el fin del juego
    /// 
    /// A diferencia de otros personajes, Pacman no se destruye permanentemente
    /// sino que se desactiva para poder ser reutilizado en la siguiente ronda.
    /// </remarks>
    protected override void ProcesarMuerte()
    {
        gameObject.SetActive(false); // Pasa a la siguiente ronda
    }

    /// <summary>
    /// Sobrescribe el método de activación de habilidad para utilizar el sistema
    /// de habilidades específico de Pacman.
    /// </summary>
    /// <remarks>
    /// Esta implementación especializada:
    /// - Verifica que la habilidad asociada sea del tipo HabilidadesPacman
    /// - Comprueba que la habilidad pueda usarse (enfriamiento, recursos, etc.)
    /// - Llama al método específico AplicarEfectoHabilidad en lugar del método genérico
    /// 
    /// Permite que Pacman tenga un sistema de habilidades más complejo que otros personajes,
    /// potencialmente incluyendo efectos aleatorios o múltiples habilidades en una.
    /// </remarks>
    public override void IntentarActivarHabilidad()
    {
        if (habilidadAsociada is HabilidadesPacman habilidadesPacman && habilidadAsociada.PuedeUsarHabilidad())
            habilidadesPacman.AplicarEfectoHabilidad();
    }
}