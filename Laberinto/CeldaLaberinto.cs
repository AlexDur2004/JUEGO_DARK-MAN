using UnityEngine;
using System.Collections;

/// <summary>
/// Enumeración para representar las posibles direcciones de movimiento en el laberinto.
/// </summary>
/// <remarks>
/// Define las cinco direcciones posibles para el movimiento dentro del laberinto,
/// incluyendo un estado inicial para cuando no hay dirección definida.
/// </remarks>
public enum Direccion{
	/// <summary>Estado inicial, sin dirección específica.</summary>
	Inicio,
	/// <summary>Movimiento hacia la derecha (+X).</summary>
	Derecha,
	/// <summary>Movimiento hacia arriba (+Y).</summary>
	Superior,
	/// <summary>Movimiento hacia la izquierda (-X).</summary>
	Izquierda,
	/// <summary>Movimiento hacia abajo (-Y).</summary>
	Inferior
};

/// <summary>
/// Representa una celda individual del laberinto con información sobre sus paredes y estado.
/// </summary>
/// <remarks>
/// Esta clase almacena la información de cada celda del laberinto, incluyendo:
/// - El estado de presencia/ausencia de paredes en cada dirección
/// - Si la celda ha sido visitada durante el algoritmo de generación
/// - Las coordenadas de la celda en la matriz del laberinto
/// - Las posiciones 3D donde se deben instanciar las paredes
/// </remarks>
public class CeldaLaberinto {
	/// <summary>Indica si hay una pared en el lado izquierdo de la celda.</summary>
	private bool paredIzquierda = false;
	
	/// <summary>Indica si hay una pared en el lado derecho de la celda.</summary>
	private bool paredDerecha = false;
	
	/// <summary>Indica si hay una pared en el lado superior de la celda.</summary>
	private bool paredSuperior = false;
	
	/// <summary>Indica si hay una pared en el lado inferior de la celda.</summary>
	private bool paredInferior = false;
	
	/// <summary>Indica si esta celda ya fue procesada por el algoritmo de generación.</summary>
	private bool visitada = false;
	
	/// <summary>Coordenada de fila de la celda en la matriz del laberinto.</summary>
	private readonly int fila;
	
	/// <summary>Coordenada de columna de la celda en la matriz del laberinto.</summary>
	private readonly int columna;
	
	/// <summary>
	/// Posiciones en el mundo 3D para cada pared de la celda.
	/// </summary>
	
	/// <summary>
	/// Posición en el mundo 3D donde se debe instanciar la pared izquierda.
	/// </summary>
	public Vector3 PosicionParedIzquierda { get; set; }
	
	/// <summary>
	/// Posición en el mundo 3D donde se debe instanciar la pared derecha.
	/// </summary>
	public Vector3 PosicionParedDerecha { get; set; }
	
	/// <summary>
	/// Posición en el mundo 3D donde se debe instanciar la pared superior.
	/// </summary>
	public Vector3 PosicionParedSuperior { get; set; }
	
	/// <summary>
	/// Posición en el mundo 3D donde se debe instanciar la pared inferior.
	/// </summary>
	public Vector3 PosicionParedInferior { get; set; }

	/// <summary>
	/// Propiedades para acceder y modificar el estado de las paredes.
	/// </summary>
	
	/// <summary>
	/// Indica si existe una pared en el lado izquierdo de la celda.
	/// </summary>
	/// <value><c>true</c> si hay pared; <c>false</c> si no hay pared.</value>
	public bool ParedIzquierda {
		get => paredIzquierda;
		set => paredIzquierda = value;
	}

	/// <summary>
	/// Indica si existe una pared en el lado derecho de la celda.
	/// </summary>
	/// <value><c>true</c> si hay pared; <c>false</c> si no hay pared.</value>
	public bool ParedDerecha {
		get => paredDerecha;
		set => paredDerecha = value;
	}

	/// <summary>
	/// Indica si existe una pared en el lado superior de la celda.
	/// </summary>
	/// <value><c>true</c> si hay pared; <c>false</c> si no hay pared.</value>
	public bool ParedSuperior {
		get => paredSuperior;
		set => paredSuperior = value;
	}

	/// <summary>
	/// Indica si existe una pared en el lado inferior de la celda.
	/// </summary>
	/// <value><c>true</c> si hay pared; <c>false</c> si no hay pared.</value>
	public bool ParedInferior {
		get => paredInferior;
		set => paredInferior = value;
	}

	/// <summary>
	/// Propiedad para el estado de visita durante la generación del laberinto.
	/// </summary>
	/// <value>
	/// <c>true</c> si la celda ya fue visitada por el algoritmo de generación;
	/// <c>false</c> si aún no ha sido procesada.
	/// </value>
	/// <remarks>
	/// Esta propiedad es utilizada por el algoritmo de generación del laberinto
	/// para marcar qué celdas ya han sido procesadas y evitar repeticiones.
	/// </remarks>
	public bool Visitada {
		get => visitada;
		set => visitada = value;
	}

	/// <summary>
	/// Propiedades de solo lectura para acceder a las coordenadas de la celda.
	/// </summary>
	
	/// <summary>
	/// Obtiene la coordenada de fila de la celda en la matriz del laberinto.
	/// </summary>
	/// <value>Un número entero que representa el índice de fila.</value>
	public int Fila => fila;
	
	/// <summary>
	/// Obtiene la coordenada de columna de la celda en la matriz del laberinto.
	/// </summary>
	/// <value>Un número entero que representa el índice de columna.</value>
	public int Columna => columna;

	/// <summary>
	/// Constructor que inicializa la celda con sus coordenadas en la matriz.
	/// </summary>
	/// <param name="fila">Índice de fila de la celda.</param>
	/// <param name="columna">Índice de columna de la celda.</param>
	/// <remarks>
	/// Las coordenadas son inmutables una vez establecidas, ya que definen
	/// la posición única de la celda dentro del laberinto.
	/// </remarks>
	public CeldaLaberinto(int fila, int columna) {
		this.fila = fila;
		this.columna = columna;
	}
}
