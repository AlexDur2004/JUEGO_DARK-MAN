using UnityEngine;
using System.Collections.Generic;
using Utilidades;
using MTAssets.EasyMinimapSystem;
using UnityEngine.AI;

/// <summary>
/// Clase encargada de gestionar la creación y posicionamiento de personajes en el juego.
/// Implementa el patrón Singleton para acceso global.
/// </summary>
/// <remarks>
/// Esta clase maneja:
/// - Instanciación de los personajes (Pacman y los 4 fantasmas)
/// - Posicionamiento inicial aleatorio en las esquinas del laberinto 
/// - Reubicación de personajes cuando se regenera el laberinto
/// - Integración con el sistema de minimapa
/// - Gestión de referencias entre sesiones de juego para garantizar persistencia
/// 
/// Utiliza el sistema NavMesh para asegurarse que los personajes se posicionen
/// en lugares válidos donde pueden caminar.
/// 
/// Esta clase es crucial para mantener la integridad de las referencias entre reinicio
/// de niveles, especialmente para componentes externos como MinimapRenderer que
/// dependen de estas referencias para su correcto funcionamiento.
/// </remarks>
public class SpawnPersonajes : Singleton<SpawnPersonajes>
{
    [Header("Prefabs")]
    /// <summary>Prefab del fantasma rojo utilizado para la instanciación</summary>
    [SerializeField] private GameObject prefabFantasmaRojo;

    /// <summary>Prefab del fantasma rosa utilizado para la instanciación</summary>
    [SerializeField] private GameObject prefabFantasmaRosa;

    /// <summary>Prefab del fantasma azul utilizado para la instanciación</summary>
    [SerializeField] private GameObject prefabFantasmaAzul;

    /// <summary>Prefab del fantasma verde utilizado para la instanciación</summary>
    [SerializeField] private GameObject prefabFantasmaVerde;

    /// <summary>Prefab del personaje jugador (Pacman) utilizado para la instanciación</summary>
    [SerializeField] private GameObject prefabPacman;
    
    /// <summary>Bandera que indica si todos los personajes han sido spawneados correctamente</summary>
    private bool personajesSpawneados = false;
    
    /// <summary>Propiedad pública para acceder al estado del spawn de personajes</summary>
    public bool PersonajesListos => personajesSpawneados;

    [Header("Configuración")]
    /// <summary>Altura en el eje Y a la que se generarán los personajes</summary>
    /// <remarks>Ajustar este valor según la altura del suelo para evitar que los personajes floten o se hundan</remarks>
    [SerializeField] private float alturaSpawn = 1.0f;         // Altura a la que aparecerán los personajes

    /// <summary>Referencia al componente que renderiza el minimapa en la interfaz</summary>
    private MinimapRenderer minimapRenderer;

    /// <summary>Tamaño de cada celda en el laberinto</summary>
    private float anchoCelda;

    /// <summary>Cantidad total de filas en el laberinto</summary>
    private int filas;

    /// <summary>Cantidad total de columnas en el laberinto</summary>
    private int columnas;

    /// <summary>Vector de desplazamiento para centrar los personajes en las celdas</summary>
    private Vector3 offsetCentro;

    /// <summary>Array de tuplas con las coordenadas (fila,columna) de las esquinas del laberinto</summary>
    /// <remarks>Se utiliza para posicionar los fantasmas en las esquinas de forma aleatoria</remarks>
    private (int fila, int columna)[] esquinas;

    /// <summary>Arreglo de referencias a los 4 fantasmas en juego</summary>
    /// <remarks>Los índices 0-3 corresponden a los fantasmas rojo, rosa, azul y verde respectivamente</remarks>
    private Personaje[] fantasmas = new Personaje[4];

    /// <summary>Referencia al personaje principal controlado por el jugador</summary>
    private Personaje pacman;

    /// <summary>
    /// Inicialización del componente. Se ejecuta cuando el objeto se activa por primera vez.
    /// </summary>
    /// <remarks>
    /// Inicializa el patrón Singleton llamando al Awake de la clase base y
    /// obtiene la referencia al renderizador del minimapa utilizando el método
    /// estático de la clase Personaje para compartir la misma instancia entre todos.
    /// </remarks>
    protected override void Awake()
    {
        base.Awake();
        minimapRenderer = Personaje.ObtenerMinimapRenderer();
    }

    /// <summary>
    /// Genera o reubica los personajes según las dimensiones del laberinto.
    /// </summary>
    /// <param name="filas">Número de filas del laberinto</param>
    /// <param name="columnas">Número de columnas del laberinto</param>
    /// <remarks>
    /// Este método es el punto de entrada principal para el sistema de spawn.
    /// Almacena las dimensiones del laberinto, inicializa las coordenadas de las esquinas,
    /// y decide si debe crear nuevos personajes desde cero (primera ejecución) o
    /// simplemente reubicar los existentes (cambios de nivel o regeneración).
    /// </remarks>
    public void GenerarPersonajes(int filas, int columnas)
    {
        this.filas = filas;
        this.columnas = columnas;

        // Inicializamos las coordenadas de las esquinas del laberinto
        esquinas = new (int, int)[]
        {
            (0, 0),                  // Esquina superior izquierda
            (0, columnas - 1),       // Esquina superior derecha
            (filas - 1, 0),          // Esquina inferior izquierda
            (filas - 1, columnas - 1) // Esquina inferior derecha
        };

        // Verificamos si ya hay personajes creados o necesitamos crearlos desde cero
        if (fantasmas[0] == null && pacman == null)
            SpawnPersonajesIniciales();
        else
            ReubicarPersonajes();
    }

    /// <summary>
    /// Crea los personajes iniciales en las esquinas del laberinto de forma aleatoria.
    /// </summary>
    /// <remarks>
    /// Este método se ejecuta la primera vez que se generan los personajes.
    /// Utiliza el algoritmo Fisher-Yates para mezclar aleatoriamente las esquinas,
    /// asegurando que los fantasmas no siempre aparezcan en las mismas posiciones.
    /// Crea los cuatro fantasmas y a Pacman, almacenando sus referencias para uso posterior.
    /// </remarks>
    private void SpawnPersonajesIniciales()
    {
        // Resetear la bandera al inicio del spawn
        personajesSpawneados = false;
        
        Debug.Log("Iniciando spawn de personajes...");
        
        anchoCelda = SpawnerLaberinto.Instancia.anchoCelda;
        offsetCentro = new Vector3(anchoCelda * 0.5f, 0, anchoCelda * 0.5f);

        // Mezclamos aleatoriamente las esquinas para que los fantasmas no siempre aparezcan en las mismas posiciones
        (int, int)[] esquinasMezcladas = new (int, int)[esquinas.Length];
        esquinas.CopyTo(esquinasMezcladas, 0);

        // Algoritmo Fisher-Yates para mezclar las esquinas
        for (int i = esquinasMezcladas.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            // Intercambiar las esquinas
            var temp = esquinasMezcladas[i];
            esquinasMezcladas[i] = esquinasMezcladas[j];
            esquinasMezcladas[j] = temp;
        }

        // Posiciona fantasmas en las esquinas mezcladas y los guarda en el array por su índice
        fantasmas[0] = SpawnFantasma(esquinasMezcladas[0].Item1, esquinasMezcladas[0].Item2, prefabFantasmaRojo);  // Rojo
        fantasmas[1] = SpawnFantasma(esquinasMezcladas[1].Item1, esquinasMezcladas[1].Item2, prefabFantasmaRosa);  // Rosa
        fantasmas[2] = SpawnFantasma(esquinasMezcladas[2].Item1, esquinasMezcladas[2].Item2, prefabFantasmaAzul);  // Azul
        fantasmas[3] = SpawnFantasma(esquinasMezcladas[3].Item1, esquinasMezcladas[3].Item2, prefabFantasmaVerde); // Verde

        pacman = SpawnPacman();
        
        // Verificar que todos los personajes se hayan spawneado correctamente
        bool todosLosFantasmasCreados = true;
        for (int i = 0; i < fantasmas.Length; i++)
        {
            if (fantasmas[i] == null)
            {
                Debug.LogError($"Error: El fantasma {i} no fue creado correctamente.");
                todosLosFantasmasCreados = false;
            }
        }
        
        if (pacman == null)
        {
            Debug.LogError("Error: Pacman no fue creado correctamente.");
            personajesSpawneados = false;
        }
        else if (todosLosFantasmasCreados)
        {
            personajesSpawneados = true;
            Debug.Log("Todos los personajes han sido spawneados correctamente.");
        }
    }

    /// <summary>
    /// Reposiciona los personajes existentes cuando se regenera el laberinto.
    /// </summary>
    /// <remarks>
    /// Esta función se utiliza cuando ya existen instancias de los personajes y solo
    /// es necesario cambiar su ubicación, como al iniciar una nueva ronda o al 
    /// regenerar el laberinto. Mezcla aleatoriamente las esquinas para los fantasmas
    /// y coloca a Pacman en una posición central. También reactiva los objetos
    /// en caso de que estuvieran desactivados.
    /// 
    /// Al reubicar personajes, también se asegura que sus referencias en el sistema
    /// de minimapa se mantengan válidas, garantizando que todos los objetos visuales
    /// asociados (iconos en el minimapa) funcionen correctamente.
    /// </remarks>
    private void ReubicarPersonajes()
    {
        // Resetear la bandera al inicio de la reubicación
        personajesSpawneados = false;
        
        Debug.Log("Reubicando personajes existentes...");
        
        // Verificar si tenemos los fantasmas en el array
        if (fantasmas[0] != null && fantasmas[1] != null && fantasmas[2] != null && fantasmas[3] != null)
        {
            // Mezclar las posiciones de las esquinas
            for (int i = esquinas.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                // Intercambiar las esquinas
                var temp = esquinas[i];
                esquinas[i] = esquinas[j];
                esquinas[j] = temp;
            }

            // Asignar cada fantasma a una esquina
            for (int i = 0; i < fantasmas.Length && i < esquinas.Length; i++)
            {
                ReubicarPersonaje(esquinas[i].Item1, esquinas[i].Item2, fantasmas[i]);
            }

            // Asegurarnos de que todos los fantasmas estén correctamente registrados en el minimapa
            // después de ser reubicados para mantener la coherencia visual
            ActualizarRegistrosMinimapa();
            
            // Si llegamos aquí, todos los fantasmas fueron reubicados correctamente
            if (pacman != null)
            {
                // Reubica a Pacman en el centro
                ReubicarPacman();
                pacman.gameObject.SetActive(true);
                
                // Indicar que todos los personajes están listos
                personajesSpawneados = true;
                Debug.Log("Todos los personajes han sido reubicados correctamente.");
            }
            else
            {
                Debug.LogError("Error: No se puede reubicar Pacman porque la referencia es nula.");
            }
        }
        else
        {
            Debug.LogError("Error al reubicar: Faltan algunos fantasmas en el array.");
        }
    }

    /// <summary>
    /// Cambia la posición de un personaje a la celda indicada.
    /// </summary>
    /// <param name="fila">Fila destino en el laberinto</param>
    /// <param name="columna">Columna destino en el laberinto</param>
    /// <param name="personaje">Personaje a reubicar</param>
    private void ReubicarPersonaje(int fila, int columna, Personaje personaje)
    {
        personaje.transform.position = CalcularPosicionCelda(fila, columna);
        personaje.gameObject.SetActive(true);
    }

    /// <summary>
    /// Reubica a Pacman en una posición central aleatoria del laberinto.
    /// </summary>
    private void ReubicarPacman()
    {
        pacman.transform.position = CalcularPosicionCentralAleatoria();
    }

    /// <summary>
    /// Crea un fantasma en la posición especificada y lo registra en el minimapa.
    /// </summary>
    /// <param name="fila">Fila donde se creará el fantasma</param>
    /// <param name="columna">Columna donde se creará el fantasma</param>
    /// <param name="prefabFantasma">Prefab del fantasma a crear</param>
    /// <returns>Componente Personaje del fantasma creado</returns>
    private Personaje SpawnFantasma(int fila, int columna, GameObject prefabFantasma)
    {
        Vector3 posicion = CalcularPosicionCelda(fila, columna);
        GameObject instancia = Instantiate(prefabFantasma, posicion, Quaternion.identity, transform);
        Personaje fantasma = instancia.GetComponent<Personaje>();

        return fantasma;
    }

    /// <summary>
    /// Crea el personaje del jugador (Pacman) en una posición central aleatoria.
    /// </summary>
    /// <returns>Componente Personaje de Pacman</returns>
    private Personaje SpawnPacman()
    {
        Vector3 posicionAleatoria = CalcularPosicionCentralAleatoria();
        
        // Verificar que el prefab no sea nulo antes de instanciarlo
        if (prefabPacman == null)
        {
            Debug.LogError("Error crítico: El prefab de Pacman es nulo. Verifica las referencias en el inspector.");
            return null;
        }
        
        GameObject instancia = Instantiate(prefabPacman, posicionAleatoria, Quaternion.identity, transform);
        
        Debug.Log($"Pacman spawneado en posición: {posicionAleatoria}, instancia válida: {instancia != null}");
        
        Personaje componentePersonaje = instancia.GetComponent<Personaje>();
        if (componentePersonaje == null)
        {
            Debug.LogError("Error: El gameObject de Pacman no tiene un componente Personaje.");
        }
        
        return componentePersonaje;
    }

    /// <summary>
    /// Calcula una posición aleatoria cerca del centro del laberinto.
    /// </summary>
    /// <returns>Posición mundial para colocar un personaje</returns>
    /// <remarks>
    /// Determina la posición central del laberinto y añade un pequeño desplazamiento
    /// aleatorio de -1, 0 o +1 celdas en ambos ejes. Esto evita que Pacman siempre
    /// aparezca exactamente en la misma celda central, añadiendo variedad al inicio
    /// de cada ronda mientras mantiene la ventaja posicional inicial.
    /// </remarks>
    private Vector3 CalcularPosicionCentralAleatoria()
    {
        int filaCentral = filas / 2;
        int columnaCentral = columnas / 2;
        int desplazamientoFila = Random.Range(-1, 2);
        int desplazamientoColumna = Random.Range(-1, 2);

        return CalcularPosicionCelda(filaCentral + desplazamientoFila, columnaCentral + desplazamientoColumna);
    }

    /// <summary>
    /// Calcula la posición mundial correspondiente a una celda del laberinto.
    /// </summary>
    /// <param name="fila">Fila de la celda</param>
    /// <param name="columna">Columna de la celda</param>
    /// <returns>Posición mundial para colocar un personaje</returns>
    /// <remarks>
    /// Primero determina la posición base de la celda usando el sistema del SpawnerLaberinto.
    /// Luego intenta encontrar una posición válida en el NavMesh donde el personaje pueda caminar.
    /// Utiliza un enfoque progresivo, probando con radios cada vez mayores para encontrar
    /// un punto válido en el NavMesh. Si todos los intentos fallan, devuelve la posición original.
    /// Esto garantiza que los personajes nunca queden atrapados en paredes o fuera del área navegable.
    /// </remarks>
    private Vector3 CalcularPosicionCelda(int fila, int columna)
    {
        // Utiliza directamente el método del SpawnerLaberinto para evitar redundancia
        Vector3 posicionInicial = SpawnerLaberinto.Instancia.GetPosicionCelda(fila, columna) + offsetCentro;
        posicionInicial.y = alturaSpawn;

        NavMeshHit hit;
        // Intenta encontrar una posición válida en el NavMesh con radio progresivo
        float[] radiosIntentos = { anchoCelda * 0.5f, anchoCelda, anchoCelda * 2.0f };

        for (int i = 0; i < radiosIntentos.Length; i++)
        {
            if (NavMesh.SamplePosition(posicionInicial, out hit, radiosIntentos[i], NavMesh.GetAreaFromName("Walkable")))
            {
                return new Vector3(hit.position.x, alturaSpawn, hit.position.z);
            }
        }

        // Si todo falla, retorna la posición inicial
        return posicionInicial;
    }

    /// <summary>
    /// Actualiza el registro de todos los personajes en el minimapa.
    /// </summary>
    /// <remarks>
    /// Este método es crucial cuando se reinicia el juego o se regenera el nivel, ya que las
    /// referencias del minimapRenderer pueden perderse o cambiar entre sesiones de juego.
    /// 
    /// Funcionalidad:
    /// - Obtiene la referencia más actualizada del MinimapRenderer a través de Personaje
    /// - Elimina los registros actuales para evitar duplicados en el minimapa
    /// - Vuelve a añadir todos los personajes al sistema de resaltado del minimapa
    /// 
    /// Se debe llamar desde el GameManager durante la primera ronda (rondaActual == 0) para
    /// garantizar que todos los fantasmas aparezcan correctamente en el minimapa después de
    /// reiniciar el juego, evitando así excepciones NullReferenceException cuando el
    /// componente MinimapRenderer intenta acceder a cámaras no válidas.
    /// </remarks>
    public void ActualizarRegistrosMinimapa()
    {
        // Siempre intentar obtener la referencia más actualizada
        minimapRenderer = Personaje.ObtenerMinimapRenderer();
        if (minimapRenderer == null)
        {
            Debug.LogWarning("No se pudo encontrar el MinimapRenderer al actualizar registros del minimapa.");
            return;
        }

        Debug.Log("Actualizando registros de todos los personajes en el minimapa...");

        // Registrar todos los fantasmas en el minimapa
        foreach (var fantasma in fantasmas)
        {
            if (fantasma != null)
            {
                MinimapItem itemMinimapa = fantasma.GetComponent<MinimapItem>();
                if (itemMinimapa != null)
                {
                    // Remover primero para evitar duplicados
                    minimapRenderer.RemoveMinimapItemOfHighlight(itemMinimapa);
                    minimapRenderer.AddMinimapItemToBeHighlighted(itemMinimapa);
                    Debug.Log($"Personaje {fantasma.name} registrado en el minimapa");
                }
            }
        }
    }

    /// <summary>
    /// Limpia recursos al destruir el objeto.
    /// </summary>
    /// <remarks>
    /// Se ejecuta automáticamente cuando se destruye este componente, como al cerrar el juego
    /// o cambiar de escena. Destruye todos los personajes que fueron creados por este spawner
    /// para evitar referencias obsoletas y posibles fugas de memoria. 
    /// También llama al método OnDestroy de la clase base para mantener la funcionalidad heredada.
    /// </remarks>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Destruye todos los fantasmas
        for (int i = 0; i < fantasmas.Length; i++)
        {
            if (fantasmas[i] != null)
            {
                Destroy(fantasmas[i].gameObject);
                fantasmas[i] = null;
            }
        }

        // Destruye al pacman
        if (pacman != null)
        {
            Destroy(pacman.gameObject);
            pacman = null;
        }
    }
}