using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Utilidades;

public class SpawnerLaberinto : Singleton<SpawnerLaberinto>
{
	/// <summary>
	/// Sección de prefabs utilizados en la construcción del laberinto
	/// </summary>
	[Header("Prefabs")]

	/// <summary>
	/// Prefab para el suelo del laberinto
	/// </summary>
	[SerializeField] private GameObject prefabSuelo;

	/// <summary>
	/// Prefab para las paredes del laberinto
	/// </summary>
	[SerializeField] private GameObject prefabPared;

	/// <summary>
	/// Prefab para los pilares en las esquinas
	/// </summary>
	[SerializeField] private GameObject prefabPilar;

	/// <summary>
	/// Prefab para el techo normal del laberinto
	/// </summary>
	[SerializeField] private GameObject prefabTecho;

	/// <summary>
	/// Prefab para techo con luz que ilumina el entorno
	/// </summary>
	[SerializeField] private GameObject prefabTechoLuzIlumina;

	/// <summary>
	/// Prefab para techo con luz decorativa (no ilumina)
	/// </summary>
	[SerializeField] private GameObject prefabTechoLuzNoIlumina;

	/// <summary>
	/// Ancho de cada celda del laberinto en unidades de mundo
	/// </summary>
	/// <value>Valor calculado automáticamente basado en el tamaño del prefab de suelo</value>
	public float anchoCelda { get; private set; }

	/// <summary>
	/// Altura de las paredes del laberinto
	/// </summary>
	private float alturaPared;

	/// <summary>
	/// Pool de paredes para reutilización de objetos
	/// </summary>
	private ObjectPool<GameObject> poolParedes = new ObjectPool<GameObject>();

	/// <summary>
	/// Pool de suelos para reutilización de objetos
	/// </summary>
	private ObjectPool<GameObject> poolSuelos = new ObjectPool<GameObject>();

	/// <summary>
	/// Pool de pilares para reutilización de objetos
	/// </summary>
	private ObjectPool<GameObject> poolPilares = new ObjectPool<GameObject>();

	/// <summary>
	/// Pool de techos para reutilización de objetos
	/// </summary>
	private ObjectPool<GameObject> poolTechos = new ObjectPool<GameObject>();

	/// <summary>
	/// Transform que agrupa todos los suelos
	/// </summary>
	private Transform contenedorSuelos;

	/// <summary>
	/// Transform que agrupa todas las paredes
	/// </summary>
	private Transform contenedorParedes;

	/// <summary>
	/// Transform que agrupa todos los pilares
	/// </summary>
	private Transform contenedorPilares;

	/// <summary>
	/// Transform que agrupa todos los techos
	/// </summary>
	private Transform contenedorTechos;

	/// <summary>
	/// Superficie de navegación para la IA
	/// </summary>
	private NavMeshSurface navMeshSurface;

	/// <summary>
	/// Máscara de capa para detectar obstáculos
	/// </summary>
	private int obstacleLayerMask;

	/// <summary>
	/// Enumera las posibles direcciones para posicionar paredes
	/// </summary>
	private enum DireccionPared { Izquierda, Derecha, Superior, Inferior }

	/// <summary>
	/// Inicializa los componentes necesarios para el funcionamiento del laberinto
	/// </summary>
	/// <remarks>
	/// Crea la jerarquía de objetos, configura los contenedores para los diferentes
	/// tipos de elementos del laberinto y obtiene las dimensiones de los prefabs.
	/// También inicializa el sistema de navegación NavMesh.
	/// </remarks>
	protected override void Awake()
	{
		base.Awake();

		// Crear contenedores y asignarlos como hijos del transform principal
		string[] nombres = { "Paredes", "Suelos", "Pilares", "Techos" };
		Transform[] contenedores = new Transform[4];

		for (int i = 0; i < nombres.Length; i++)
		{
			contenedores[i] = new GameObject(nombres[i]).transform;
			contenedores[i].parent = transform;
		}

		// Asignar referencias
		contenedorParedes = contenedores[0];
		contenedorSuelos = contenedores[1];
		contenedorPilares = contenedores[2];
		contenedorTechos = contenedores[3];

		// Obtener las dimensiones de los prefabs
		anchoCelda = prefabSuelo.GetComponent<Renderer>().bounds.size.x - 0.02f; // Ajuste para solapamiento
		alturaPared = prefabPared.GetComponent<Renderer>().bounds.size.y;

		// Configurar la superficie de navegación
		navMeshSurface = gameObject.GetComponent<NavMeshSurface>();
		navMeshSurface.collectObjects = CollectObjects.Children;

		obstacleLayerMask = ~LayerMask.GetMask("Suelo");
	}

	/// <summary>
	/// Reutiliza o crea un nuevo objeto según sea necesario
	/// </summary>
	/// <param name="prefab">Prefab a instanciar</param>
	/// <param name="posicion">Posición donde colocar el objeto</param>
	/// <param name="rotacion">Rotación a aplicar al objeto</param>
	/// <param name="parent">Padre jerárquico al que añadir el objeto</param>
	/// <param name="pool">Pool de objetos donde buscar o registrar</param>
	/// <returns>GameObject instanciado o reutilizado</returns>
	/// <remarks>
	/// Este método implementa el patrón Object Pool para mejorar el rendimiento,
	/// evitando instanciar y destruir objetos constantemente.
	/// </remarks>
	public GameObject InstanciarOReutilizar(GameObject prefab, Vector3 posicion, Quaternion rotacion, Transform parent, ObjectPool<GameObject> pool)
	{
		// Buscar un objeto inactivo para reutilizar
		GameObject objeto = pool.GetInactivos().Find(obj => obj != null);

		if (objeto != null)
		{
			// Configurar el objeto reutilizado
			objeto.transform.SetPositionAndRotation(posicion, rotacion);
			objeto.transform.parent = parent;
			pool.Activar(objeto);
			return objeto;
		}

		// Crear un objeto nuevo
		GameObject nuevoObjeto = Instantiate(prefab, posicion, rotacion, parent);
		pool.Registrar(nuevoObjeto);
		return nuevoObjeto;
	}

	/// <summary>
	/// Genera un nuevo laberinto con las dimensiones especificadas
	/// </summary>
	/// <param name="filas">Número de filas del laberinto</param>
	/// <param name="columnas">Número de columnas del laberinto</param>
	/// <returns>Vector2 con el ancho y alto total del laberinto generado</returns>
	/// <remarks>
	/// Este método es el punto central de la generación del laberinto.
	/// Crea el laberinto completo con suelos, paredes, pilares, techos y decoraciones.
	/// También construye el NavMesh para la navegación de la IA.
	/// </remarks>
	public Vector2 GenerarLaberinto(int filas, int columnas)
	{
		LimpiarLaberinto();

		// Limpia el historial de posiciones de objetos decorativos
		SpawnerDecoraciones.Instancia.LimpiarHistorialPosiciones();

		// Ya no es necesario calcular el máximo de objetos decorativos porque ahora es un valor fijo

		// Crea el algoritmo de generación de laberinto recursivo
		GeneradorLaberintoRecursivo generadorRecursivo = new GeneradorLaberintoRecursivo(filas, columnas);

		// El offset es la mitad del ancho de la celda
		float offset = anchoCelda / 2;

		// Genera suelos, techos y paredes para cada celda
		int contadorTechosNormales = 0;

		for (int fila = 0; fila < filas; fila++)
		{
			for (int columna = 0; columna < columnas; columna++)
			{
				float x = columna * anchoCelda;
				float z = fila * anchoCelda;

				// Instancia suelo para la celda actual
				InstanciarOReutilizar(prefabSuelo, new Vector3(x - offset, 0, z + offset), Quaternion.identity, contenedorSuelos, poolSuelos);

				GameObject prefabTechoElegido;

				// Cada 2 techos normales, coloca un techo con luz (alternando)
				if (contadorTechosNormales < 2)
				{
					prefabTechoElegido = prefabTecho;
					contadorTechosNormales++;
				}
				else
				{
					// Alterna entre los dos tipos de techo con luz
					prefabTechoElegido = (Random.value < 0.5f) ? prefabTechoLuzIlumina : prefabTechoLuzNoIlumina;
					contadorTechosNormales = 0;
				}

				InstanciarOReutilizar(prefabTechoElegido, new Vector3(x - offset, alturaPared, z - offset), Quaternion.Euler(180, 0, 0), contenedorTechos, poolTechos);

				// Obtiene información de la celda actual del generador
				CeldaLaberinto celda = generadorRecursivo.ObtenerCeldaLaberinto(fila, columna);

				// Instancia paredes según la configuración de la celda
				if (celda.ParedIzquierda)
					InstanciarOReutilizar(prefabPared, new Vector3(x + offset, 0, z - offset), Quaternion.Euler(0, 90, 0), contenedorParedes, poolParedes);

				if (celda.ParedInferior)
					InstanciarOReutilizar(prefabPared, new Vector3(x + offset, 0, z + offset), Quaternion.identity, contenedorParedes, poolParedes);

				if (celda.ParedDerecha)
					InstanciarOReutilizar(prefabPared, new Vector3(x - offset, 0, z + offset), Quaternion.Euler(0, 270, 0), contenedorParedes, poolParedes);

				if (celda.ParedSuperior)
					InstanciarOReutilizar(prefabPared, new Vector3(x - offset, 0, z - offset), Quaternion.Euler(0, 180, 0), contenedorParedes, poolParedes);

				// Genera objetos decorativos en la celda usando el singleton
				SpawnerDecoraciones.Instancia.GenerarObjetosDecorativos(celda, new Vector3(x, 0, z), anchoCelda);
			}
		}

		// Genera pilares en las intersecciones de paredes
		if (prefabPilar != null)
		{
			for (int fila = 0; fila <= filas; fila++)
			{
				for (int columna = 0; columna <= columnas; columna++)
				{
					bool necesitaPilar = false;

					// Siempre coloca pilares en los bordes
					if (fila == 0 || fila == filas || columna == 0 || columna == columnas)
					{
						necesitaPilar = true;
					}
					else
					{
						// Verifica las celdas adyacentes para determinar si necesita pilar
						CeldaLaberinto celdaActual = generadorRecursivo.ObtenerCeldaLaberinto(fila, columna);
						CeldaLaberinto celdaArriba = generadorRecursivo.ObtenerCeldaLaberinto(fila - 1, columna);
						CeldaLaberinto celdaDerecha = generadorRecursivo.ObtenerCeldaLaberinto(fila, columna - 1);
						CeldaLaberinto celdaDiagonal = generadorRecursivo.ObtenerCeldaLaberinto(fila - 1, columna - 1);

						// Simplificamos la verificación de paredes en una sola condición
						necesitaPilar = (celdaActual != null && (celdaActual.ParedSuperior || celdaActual.ParedDerecha)) ||
										(celdaArriba != null && (celdaArriba.ParedDerecha || celdaArriba.ParedInferior)) ||
										(celdaDerecha != null && (celdaDerecha.ParedIzquierda || celdaDerecha.ParedSuperior)) ||
										(celdaDiagonal != null && (celdaDiagonal.ParedInferior || celdaDiagonal.ParedIzquierda));
					}                       // Instancia el pilar si es necesario
					if (necesitaPilar)
					{
						float x = columna * anchoCelda - offset;
						float z = fila * anchoCelda - offset;
						InstanciarOReutilizar(prefabPilar, new Vector3(x, 0, z), Quaternion.identity, contenedorPilares, poolPilares);
					}
				}
			}
		}

		// Combina las mallas de suelos para optimizar el rendimiento
		GameObject suelosCombinados = CombinarMesh(contenedorSuelos, "SuelosCombinados");

		// Excluye los techos del navmesh temporalmente
		Transform padreOriginalTechos = contenedorTechos.parent;
		contenedorTechos.parent = null;

		// Construye el NavMesh para la navegación de la IA
		navMeshSurface.BuildNavMesh();

		// Restaura los techos
		contenedorTechos.parent = padreOriginalTechos;
		Destroy(suelosCombinados);

		// Calcula las dimensiones totales del laberinto
		float anchoTotal = columnas * anchoCelda;
		float altoTotal = filas * anchoCelda;

		return new Vector2(anchoTotal, altoTotal);
	}

	/// <summary>
	/// Verifica si una celda tiene paredes en alguna de sus cuatro direcciones
	/// </summary>
	/// <param name="celda">Celda del laberinto a comprobar</param>
	/// <returns>True si la celda tiene al menos una pared, False si no tiene ninguna</returns>
	public bool CeldaTieneParedes(CeldaLaberinto celda) => celda != null &&
		(celda.ParedIzquierda || celda.ParedDerecha || celda.ParedSuperior || celda.ParedInferior);

	/// <summary>
	/// Combina las mallas individuales en una sola para mejorar el rendimiento
	/// </summary>
	/// <param name="contenedor">Transform que contiene las mallas a combinar</param>
	/// <param name="nombre">Nombre para el objeto combinado resultante</param>
	/// <returns>GameObject que contiene la malla combinada o null si no hay mallas para combinar</returns>
	/// <remarks>
	/// Este método optimiza el renderizado mediante la técnica de combinación de mallas,
	/// reduciendo el número de draw calls y mejorando el rendimiento. Utiliza el formato UInt32
	/// para soportar mallas grandes con muchos vértices.
	/// </remarks>
	private GameObject CombinarMesh(Transform contenedor, string nombre)
	{
		MeshFilter[] meshFilters = contenedor.GetComponentsInChildren<MeshFilter>();
		if (meshFilters.Length == 0) return null;

		// Crear objeto combinado
		GameObject combinado = new GameObject(nombre);
		combinado.transform.parent = transform;

		// Añadir componentes necesarios
		MeshFilter meshFilter = combinado.AddComponent<MeshFilter>();
		Mesh meshCombinado = new Mesh();
		meshCombinado.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Soporte para mallas grandes

		// Crear instancias para combinar
		CombineInstance[] instancias = new CombineInstance[meshFilters.Length];
		Material material = null;

		// Configurar cada instancia
		for (int i = 0; i < meshFilters.Length; i++)
		{
			MeshFilter filtroActual = meshFilters[i];
			if (filtroActual == null || filtroActual.sharedMesh == null) continue;

			// Obtener el material del primer objeto válido
			if (material == null)
			{
				Renderer renderer = filtroActual.GetComponent<Renderer>();
				if (renderer != null) material = renderer.sharedMaterial;
			}

			instancias[i].mesh = filtroActual.sharedMesh;
			instancias[i].transform = filtroActual.transform.localToWorldMatrix;
		}

		// Combinar las mallas
		meshCombinado.CombineMeshes(instancias, true, true);
		meshFilter.sharedMesh = meshCombinado;

		// Añadir renderer y collider
		if (material != null) combinado.AddComponent<MeshRenderer>().sharedMaterial = material;
		combinado.AddComponent<MeshCollider>().sharedMesh = meshCombinado;

		return combinado;
	}

	/// <summary>
	/// Desactiva todos los objetos del laberinto actual
	/// </summary>
	/// <remarks>
	/// Desactiva los objetos en vez de destruirlos para permitir su reutilización,
	/// mejorando el rendimiento al generar un nuevo laberinto. También elimina
	/// las decoraciones y los datos del NavMesh.
	/// </remarks>
	private void LimpiarLaberinto()
	{
		// Desactivar todos los objetos en los pools mediante una sola llamada para cada uno
		poolParedes.DesactivarTodos();
		poolSuelos.DesactivarTodos();
		poolPilares.DesactivarTodos();
		poolTechos.DesactivarTodos();

		// Limpiar decoraciones y NavMesh
		SpawnerDecoraciones.Instancia.LimpiarDecoraciones();
		navMeshSurface.RemoveData();
	}

	/// <summary>
	/// Obtiene una posición aleatoria dentro de un área caminable del laberinto
	/// </summary>
	/// <returns>Vector3 con las coordenadas de una posición aleatoria válida dentro del NavMesh</returns>
	/// <remarks>
	/// Este método utiliza la triangulación del NavMesh para encontrar áreas caminables y
	/// selecciona un punto aleatorio dentro de un triángulo válido. Es útil para colocar
	/// elementos o enemigos en posiciones accesibles del laberinto.
	/// </remarks>
	/// <summary>
	/// Obtiene una posición aleatoria dentro del área caminable del laberinto
	/// </summary>
	/// <returns>Vector3 con una posición aleatoria válida dentro del NavMesh del laberinto</returns>
	/// <remarks>
	/// Este método utiliza la triangulación del NavMesh para garantizar que la posición generada
	/// sea válida para la navegación. Primero identifica todos los triángulos en áreas caminables
	/// y luego selecciona una posición aleatoria dentro de uno de estos triángulos usando
	/// coordenadas baricéntricas para una distribución uniforme.
	/// </remarks>
	public Vector3 ObtenerPosicionAleatoriaEnLaberinto()
	{
		// Obtenemos la triangulación completa del NavMesh
		NavMeshTriangulation datosMalla = NavMesh.CalculateTriangulation();

		// Definimos la máscara para áreas caminables (área por defecto "Walkable" = 0)
		int mascaraAreaCaminable = 1 << NavMesh.GetAreaFromName("Walkable");

		// Filtramos solo los triángulos que pertenecen al área caminable
		var triangulosValidos = new List<int>();
		for (int i = 0; i < datosMalla.indices.Length; i += 3)
		{
			Vector3 v0 = datosMalla.vertices[datosMalla.indices[i]];
			Vector3 v1 = datosMalla.vertices[datosMalla.indices[i + 1]];
			Vector3 v2 = datosMalla.vertices[datosMalla.indices[i + 2]];

			// Tomamos el centro del triángulo para verificar si está en área caminable
			Vector3 centro = (v0 + v1 + v2) / 3f;

			NavMeshHit resultadoMuestra;
			if (NavMesh.SamplePosition(centro, out resultadoMuestra, 0.1f, mascaraAreaCaminable))
			{
				triangulosValidos.Add(i); // Índice del triángulo
			}
		}

		if (triangulosValidos.Count == 0)
		{
			return Vector3.zero;
		}

		// Elegir un triángulo aleatorio válido
		int indiceTriangulo = triangulosValidos[Random.Range(0, triangulosValidos.Count)];

		Vector3 a = datosMalla.vertices[datosMalla.indices[indiceTriangulo]];
		Vector3 b = datosMalla.vertices[datosMalla.indices[indiceTriangulo + 1]];
		Vector3 c = datosMalla.vertices[datosMalla.indices[indiceTriangulo + 2]];

		// Calcular posición aleatoria dentro del triángulo
		return ObtenerPuntoAleatorioEnTriangulo(a, b, c);
	}

	/// <summary>
	/// Clase encargada de generar y gestionar el laberinto del juego.
	/// </summary>
	/// <remarks>
	/// Implementa la funcionalidad para crear laberintos dinámicamente, incluyendo
	/// suelos, paredes, pilares y techos. También gestiona la navegación AI mediante
	/// NavMesh y optimiza el rendimiento mediante el uso de ob/// <summary>
	/// Genera un punto aleatorio dentro de un triángulo especificado por sus tres vértices
	/// </summary>
	/// <param name="a">Primer vértice del triángulo</param>
	/// <param name="b">Segundo vértice del triángulo</param>
	/// <param name="c">Tercer vértice del triángulo</param>
	/// <returns>Vector3 con un punto aleatorio dentro del triángulo</returns>
	/// <remarks>
	/// Este método utiliza coordenadas baricéntricas con raíz cuadrada para generar una
	/// distribución verdaderamente uniforme de puntos dentro del triángulo. La técnica
	/// estándar de coordenadas baricéntricas sin la raíz cuadrada tendería a concentrar
	/// puntos cerca de un vértice.
	/// </remarks>
	private Vector3 ObtenerPuntoAleatorioEnTriangulo(Vector3 a, Vector3 b, Vector3 c)
	{
		// Utilizamos coordenadas baricéntricas con raíz cuadrada para asegurar
		// una distribución uniforme dentro del triángulo
		float r1 = Mathf.Sqrt(Random.value);
		float r2 = Random.value;

		return (1 - r1) * a + r1 * (1 - r2) * b + r1 * r2 * c;
	}

	/// <summary>
	/// Calcula la posición mundial para una celda específica
	/// </summary>
	/// <param name="fila">Índice de fila de la celda</param>
	/// <param name="columna">Índice de columna de la celda</param>
	/// <returns>Vector3 con la posición mundial de la celda</returns>
	public Vector3 GetPosicionCelda(int fila, int columna)
	{
		return new Vector3(columna * anchoCelda, 0, fila * anchoCelda);
	}

	/// <summary>
	/// Verifica si una posición está dentro de los límites del laberinto
	/// </summary>
	/// <param name="posicion">Posición a verificar</param>
	/// <returns>True si la posición es válida, False en caso contrario</returns>
	public bool EsPosicionValida(Vector3 posicion)
	{
		return posicion.x >= 0 && posicion.z >= 0;
	}

	/// <summary>
	/// Encuentra el pilar más cercano a una posición dada
	/// </summary>
	/// <param name="posicion">Posición desde la que buscar el pilar más cercano</param>
	/// <returns>Vector3 con la posición del pilar más cercano</returns>
	/// <remarks>
	/// Utiliza cálculo de distancia al cuadrado para optimizar el rendimiento,
	/// evitando operaciones de raíz cuadrada innecesarias.
	/// </remarks>
	public Vector3 GetPosicionPilarMasCercano(Vector3 posicion)
	{
		Vector3 posicionMasCercana = Vector3.zero;
		float distanciaMinima = float.MaxValue;

		// Revisar todos los pilares activos
		foreach (var pilar in poolPilares.GetActivos())
		{
			// Calcular la distancia al cuadrado para evitar usar raíz cuadrada
			float distancia = (posicion - pilar.transform.position).sqrMagnitude;
			if (distancia < distanciaMinima)
			{
				distanciaMinima = distancia;
				posicionMasCercana = pilar.transform.position;
			}
		}

		return posicionMasCercana;
	}

	/// <summary>
	/// Limpia los objetos temporales cuando se destruye el script
	/// </summary>
	/// <remarks>
	/// Asegura una correcta limpieza de memoria al eliminar todos los
	/// objetos en los pools cuando el componente es destruido.
	/// </remarks>
	protected override void OnDestroy()
	{
		base.OnDestroy();

		// Limpiar todos los pools
		poolParedes.Clear();
		poolSuelos.Clear();
		poolPilares.Clear();
		poolTechos.Clear();
	}
}
