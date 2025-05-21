using UnityEngine;
using System.Collections;

/// <summary>
/// Implementación del algoritmo recursivo para generar laberintos aleatorios.
/// </summary>
/// <remarks>
/// Esta clase hereda de GeneradorLaberintoBase e implementa un algoritmo de generación
/// de laberintos utilizando recursividad. El algoritmo visita cada celda del laberinto 
/// y decide aleatoriamente si crear paredes, generando así un laberinto completamente 
/// aleatorio pero con caminos válidos garantizados.
/// </remarks>
public class GeneradorLaberintoRecursivo : GeneradorLaberintoBase
{
	/// <summary>
	/// Probabilidad de generar una pared en cada posible ubicación (rango 0-100).
	/// </summary>
	/// <remarks>
	/// Un valor más alto generará un laberinto con más paredes y posiblemente más
	/// difícil. Un valor más bajo creará un laberinto más abierto con menos obstáculos.
	/// El valor predeterminado es 80%.
	/// </remarks>
	private float probabilidadPared = 80f;

	/// <summary>
	/// Constructor que inicia la generación del laberinto.
	/// </summary>
	/// <param name="filas">Número de filas que tendrá el laberinto.</param>
	/// <param name="columnas">Número de columnas que tendrá el laberinto.</param>
	/// <remarks>
	/// Este constructor realiza las siguientes acciones:
	/// 1. Llama al constructor base para inicializar la matriz de celdas
	/// 2. Genera las paredes del borde exterior del laberinto
	/// 3. Inicia el algoritmo recursivo desde la esquina superior izquierda (0,0)
	/// </remarks>
	public GeneradorLaberintoRecursivo(int filas, int columnas)
		: base(filas, columnas)
	{
		GenerarParedesBorde();          // Primero genera los bordes exteriores
		VisitarCelda(0, 0, Direccion.Inicio);  // Comienza la generación recursiva desde la esquina superior izquierda
	}

	/// <summary>
	/// Permite ajustar la probabilidad de generar paredes.
	/// </summary>
	/// <param name="probabilidad">Valor entre 0 y 100 que determina la probabilidad de crear paredes.</param>
	/// <remarks>
	/// Este método permite cambiar la densidad del laberinto a generar. Utiliza
	/// Mathf.Clamp para garantizar que el valor se encuentre dentro del rango válido (0-100).
	/// - Un valor de 0 generaría un laberinto completamente abierto sin paredes internas.
	/// - Un valor de 100 generaría un laberinto con el máximo número de paredes posibles.
	/// </remarks>
	public void SetProbabilidadPared(float probabilidad)
	{
		probabilidadPared = Mathf.Clamp(probabilidad, 0f, 100f);
	}

	/// <summary>
	/// Genera las paredes del borde exterior del laberinto.
	/// </summary>
	/// <remarks>
	/// Este método crea las paredes perimetrales del laberinto, asegurando que 
	/// exista un límite claro en todos los bordes. Específicamente:
	/// 1. Crea paredes en los bordes izquierdo y derecho del laberinto
	/// 2. Crea paredes en los bordes superior e inferior del laberinto
	/// 
	/// Estas paredes son necesarias para definir los límites del área jugable
	/// y evitar que los personajes salgan del laberinto.
	/// </remarks>
	private void GenerarParedesBorde()
	{
		// Paredes izquierda y derecha del laberinto
		for (int fila = 0; fila < CantidadFilas; fila++)
		{
			ObtenerCeldaLaberinto(fila, 0).ParedDerecha = true;
			ObtenerCeldaLaberinto(fila, CantidadColumnas - 1).ParedIzquierda = true;
			}
		
		// Paredes superior e inferior del laberinto
		for (int columna = 0; columna < CantidadColumnas; columna++)
		{
			ObtenerCeldaLaberinto(0, columna).ParedSuperior = true;
			ObtenerCeldaLaberinto(CantidadFilas - 1, columna).ParedInferior = true;
		}
	}

	/// <summary>
	/// Método recursivo principal que visita cada celda y genera el laberinto.
	/// </summary>
	/// <param name="fila">Índice de fila de la celda actual.</param>
	/// <param name="columna">Índice de columna de la celda actual.</param>
	/// <param name="movimientoRealizado">Dirección desde la que se llegó a esta celda.</param>
	/// <remarks>
	/// Este es el método central del algoritmo de generación. Para cada celda:
	/// 1. Verifica en las cuatro direcciones posibles si hay celdas no visitadas
	/// 2. Si hay celdas disponibles, selecciona una al azar y continúa la recursión
	/// 3. Si no hay celdas disponibles, evalúa crear paredes aleatorias según la probabilidad configurada
	/// 4. Marca la celda como visitada
	/// 
	/// El algoritmo continúa recursivamente hasta que todas las celdas han sido visitadas,
	/// garantizando que el laberinto sea completamente explorable.
	/// </remarks>
	private void VisitarCelda(int fila, int columna, Direccion movimientoRealizado)
	{
		Direccion[] movimientosDisponibles = new Direccion[4];     // Posibles direcciones a tomar
		int cantidadMovimientosDisponibles = 0;                    // Contador de direcciones disponibles

		do
		{
			cantidadMovimientosDisponibles = 0;

			// Verifica si se puede mover a la derecha
			if (columna + 1 < CantidadColumnas - 1 && !ObtenerCeldaLaberinto(fila, columna + 1).Visitada)
			{
				movimientosDisponibles[cantidadMovimientosDisponibles] = Direccion.Derecha;
				cantidadMovimientosDisponibles++;
			}
			// Si no puede moverse, evalúa crear una pared izquierda aleatoriamente
			else if (!ObtenerCeldaLaberinto(fila, columna).Visitada &&
					 movimientoRealizado != Direccion.Izquierda &&
					 columna + 1 < CantidadColumnas &&
					 !ObtenerCeldaLaberinto(fila, columna).ParedIzquierda &&
					 Random.Range(0, 100) < probabilidadPared)
			{
				ObtenerCeldaLaberinto(fila, columna).ParedIzquierda = true;
				if (columna + 1 < CantidadColumnas)
				{
					ObtenerCeldaLaberinto(fila, columna + 1).Visitada = true;
				}
			}

			// Verifica si se puede mover hacia arriba
			if (fila + 1 < CantidadFilas - 1 && !ObtenerCeldaLaberinto(fila + 1, columna).Visitada)
			{
				movimientosDisponibles[cantidadMovimientosDisponibles] = Direccion.Superior;
				cantidadMovimientosDisponibles++;
			}
			// Si no puede moverse, evalúa crear una pared inferior aleatoriamente
			else if (!ObtenerCeldaLaberinto(fila, columna).Visitada &&
					 movimientoRealizado != Direccion.Inferior &&
					 fila + 1 < CantidadFilas &&
					 !ObtenerCeldaLaberinto(fila, columna).ParedInferior &&
					 Random.Range(0, 100) < probabilidadPared)
			{
				ObtenerCeldaLaberinto(fila, columna).ParedInferior = true;
				if (fila + 1 < CantidadFilas)
				{
					ObtenerCeldaLaberinto(fila + 1, columna).Visitada = true;
				}
			}

			// Verifica si se puede mover a la izquierda
			if (columna > 1 && !ObtenerCeldaLaberinto(fila, columna - 1).Visitada)
			{
				movimientosDisponibles[cantidadMovimientosDisponibles] = Direccion.Izquierda;
				cantidadMovimientosDisponibles++;
			}
			// Si no puede moverse, evalúa crear una pared derecha aleatoriamente
			else if (!ObtenerCeldaLaberinto(fila, columna).Visitada &&
					 movimientoRealizado != Direccion.Derecha &&
					 columna > 0 &&
					 !ObtenerCeldaLaberinto(fila, columna).ParedDerecha &&
					 Random.Range(0, 100) < probabilidadPared)
			{
				ObtenerCeldaLaberinto(fila, columna).ParedDerecha = true;
				if (columna > 0)
				{
					ObtenerCeldaLaberinto(fila, columna - 1).Visitada = true;
				}
			}

			// Verifica si se puede mover hacia abajo
			if (fila > 1 && !ObtenerCeldaLaberinto(fila - 1, columna).Visitada)
			{
				movimientosDisponibles[cantidadMovimientosDisponibles] = Direccion.Inferior;
				cantidadMovimientosDisponibles++;
			}
			// Si no puede moverse, evalúa crear una pared superior aleatoriamente
			else if (!ObtenerCeldaLaberinto(fila, columna).Visitada &&
					 movimientoRealizado != Direccion.Superior &&
					 fila > 0 &&
					 !ObtenerCeldaLaberinto(fila, columna).ParedSuperior &&
					 Random.Range(0, 100) < probabilidadPared)
			{
				ObtenerCeldaLaberinto(fila, columna).ParedSuperior = true;
				if (fila > 0)
				{
					ObtenerCeldaLaberinto(fila - 1, columna).Visitada = true;
				}
			}

			// Marca la celda actual como visitada
			ObtenerCeldaLaberinto(fila, columna).Visitada = true;

			// Si hay movimientos disponibles, elige uno al azar y continúa la recursión
			if (cantidadMovimientosDisponibles > 0)
			{
				Direccion direccionElegida = movimientosDisponibles[Random.Range(0, cantidadMovimientosDisponibles)];
				switch (direccionElegida)
				{
					case Direccion.Inicio:
						break;
					case Direccion.Derecha:
						VisitarCelda(fila, columna + 1, Direccion.Derecha);
						break;
					case Direccion.Superior:
						VisitarCelda(fila + 1, columna, Direccion.Superior);
						break;
					case Direccion.Izquierda:
						VisitarCelda(fila, columna - 1, Direccion.Izquierda);
						break;
					case Direccion.Inferior:
						VisitarCelda(fila - 1, columna, Direccion.Inferior);
						break;
				}
			}
		} while (cantidadMovimientosDisponibles > 0); // Continúa mientras haya direcciones disponibles
	}
}