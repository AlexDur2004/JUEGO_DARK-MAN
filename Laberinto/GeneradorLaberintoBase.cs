using UnityEngine;
using System.Collections;

/// <summary>
/// Clase abstracta base para implementar diferentes algoritmos de generación de laberintos.
/// </summary>
/// <remarks>
/// Esta clase proporciona la estructura básica y funcionalidad común para
/// todos los algoritmos de generación de laberintos. Define una matriz de celdas
/// y métodos para inicializar y acceder a ellas. Las clases derivadas deben implementar
/// los algoritmos específicos de generación aprovechando esta estructura.
/// </remarks>
public abstract class GeneradorLaberintoBase {
	/// <summary>
	/// Matriz con todas las celdas que forman el laberinto.
	/// </summary>
	/// <remarks>
	/// Estructura bidimensional que contiene todas las celdas del laberinto.
	/// Cada celda mantiene información sobre sus paredes y estado de visita.
	/// </remarks>
	private readonly CeldaLaberinto[,] celdasLaberinto;
	
	/// <summary>
	/// Número de filas del laberinto.
	/// </summary>
	private readonly int cantidadFilas;
	
	/// <summary>
	/// Número de columnas del laberinto.
	/// </summary>
	private readonly int cantidadColumnas;

	/// <summary>
	/// Propiedades de solo lectura para acceder a las dimensiones del laberinto.
	/// </summary>
	
	/// <summary>
	/// Obtiene el número de filas del laberinto.
	/// </summary>
	/// <value>Un entero que representa el número de filas.</value>
	public int CantidadFilas => cantidadFilas;
	
	/// <summary>
	/// Obtiene el número de columnas del laberinto.
	/// </summary>
	/// <value>Un entero que representa el número de columnas.</value>
	public int CantidadColumnas => cantidadColumnas;

	/// <summary>
	/// Constructor que inicializa la matriz de celdas con las dimensiones especificadas.
	/// </summary>
	/// <param name="filas">Número de filas que tendrá el laberinto.</param>
	/// <param name="columnas">Número de columnas que tendrá el laberinto.</param>
	/// <remarks>
	/// Este constructor realiza las siguientes acciones:
	/// 1. Guarda las dimensiones del laberinto
	/// 2. Crea una matriz de celdas con el tamaño apropiado
	/// 3. Llama a InicializarCeldas() para crear todas las celdas individuales
	/// </remarks>
	protected GeneradorLaberintoBase(int filas, int columnas) {
		cantidadFilas = filas;
		cantidadColumnas = columnas;
		celdasLaberinto = new CeldaLaberinto[filas, columnas];
		InicializarCeldas();
	}

	/// <summary>
	/// Crea todas las celdas del laberinto en su estado inicial.
	/// </summary>
	/// <remarks>
	/// Este método recorre todas las posiciones de la matriz y crea una nueva instancia
	/// de CeldaLaberinto para cada posición. Cada celda se inicializa con sus coordenadas
	/// correspondientes (fila, columna) para poder identificarla posteriormente.
	/// 
	/// En su estado inicial, las celdas no tienen paredes activas y no han sido visitadas.
	/// La configuración de las paredes se hará posteriormente durante el proceso de generación
	/// del laberinto en las clases derivadas.
	/// </remarks>
	private void InicializarCeldas() {
		for (int fila = 0; fila < cantidadFilas; fila++) {
			for (int columna = 0; columna < cantidadColumnas; columna++) {
				celdasLaberinto[fila, columna] = new CeldaLaberinto(fila, columna);
			}
		}
	}

	/// <summary>
	/// Obtiene una celda específica del laberinto o null si está fuera de los límites.
	/// </summary>
	/// <param name="fila">Índice de fila de la celda a obtener.</param>
	/// <param name="columna">Índice de columna de la celda a obtener.</param>
	/// <returns>
	/// La instancia de CeldaLaberinto en la posición especificada, o null si las 
	/// coordenadas están fuera de los límites del laberinto.
	/// </returns>
	/// <remarks>
	/// Este método realiza una comprobación de límites para evitar excepciones al
	/// intentar acceder a posiciones fuera del rango válido de la matriz.
	/// Es utilizado por los algoritmos de generación de laberintos para obtener
	/// y manipular las celdas de manera segura.
	/// </remarks>
	public CeldaLaberinto ObtenerCeldaLaberinto(int fila, int columna) {
		if (fila < 0 || fila >= cantidadFilas || columna < 0 || columna >= cantidadColumnas) {
			return null;
		}
		return celdasLaberinto[fila, columna];
	}
}
