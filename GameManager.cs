using UnityEngine;
using System.Collections;
using Utilidades;
using MTAssets.EasyMinimapSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el flujo principal del juego, incluyendo las rondas, puntuación y estados del juego.
/// </summary>
/// <remarks>
/// Esta clase implementa el patrón Singleton para garantizar una única instancia accesible globalmente.
/// Se encarga de gestionar el estado general del juego, la progresión de rondas, y las condiciones de victoria/derrota.
/// </remarks>
public class GameManager : Singleton<GameManager>
{
    /// <summary>
    /// Configuraciones para cada ronda del juego.
    /// </summary>
    /// <remarks>
    /// Cada elemento del array representa una ronda con sus parámetros específicos.
    /// </remarks>
    [Header("Configuración de Rondas")]
    [SerializeField] private ConfiguracionRonda[] configuracionesRondas;
    
    /// <summary>
    /// Índice de la ronda actual del juego.
    /// </summary>
    /// <remarks>
    /// Comienza en 0 para la primera ronda e incrementa con cada progreso del juego.
    /// </remarks>
    private int rondaActual = 0;
    
    /// <summary>
    /// Dimensiones del laberinto actual en (filas, columnas).
    /// </summary>
    /// <remarks>
    /// Vector2 donde X representa las filas y Y representa las columnas del laberinto.
    /// </remarks>
    private Vector2 dimensionesLaberinto;

    /// <summary>
    /// Indica si estamos esperando a que los personajes se spawnen correctamente.
    /// </summary>
    private bool esperandoPersonajes = false;
    
    /// <summary>
    /// Contador para verificar periódicamente si los personajes están listos.
    /// </summary>
    private float temporizadorVerificacion = 0f;

    /// <summary>
    /// Inicializa el juego cuando se carga la escena.
    /// </summary>
    /// <remarks>
    /// Ejecuta la transición para la primera ronda del juego.
    /// </remarks>
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Mostrar la transición para la primera ronda
        PrepararTransicionRonda($"RONDA  {rondaActual + 1}");
    }
    
    /// <summary>
    /// Comprueba periódicamente si los personajes están listos.
    /// </summary>
    /// <remarks>
    /// Verifica si el sistema de spawn de personajes ha completado su trabajo.
    /// Si los personajes no están listos, sigue esperando y verificando periódicamente.
    /// </remarks>
    private void Update()
    {
        // Si estamos esperando a que los personajes se spawnen
        if (esperandoPersonajes)
        {
            temporizadorVerificacion += Time.deltaTime;
            
            // Verificar cada 0.2 segundos para no sobrecargar
            if (temporizadorVerificacion >= 0.2f)
            {
                temporizadorVerificacion = 0f;
                
                // Verificar si los personajes están listos
                if (SpawnPersonajes.Instancia != null && SpawnPersonajes.Instancia.PersonajesListos)
                {
                    esperandoPersonajes = false;
                    ContinuarInicializacion();
                    Debug.Log("Personajes listos, continuando con la inicialización del juego");
                }
                else
                {
                    Debug.Log("Esperando a que los personajes estén listos...");
                }
            }
        }
        
        // Resto de la lógica del Update
        if (!esperandoPersonajes && !PacmanEstaActivo())
        {
            AvanzarSiguienteRonda();
        }
        // Comprobamos el estado de los fantasmas solo si Pacman está activo
        else if (!esperandoPersonajes && PacmanEstaActivo() && SistemaGestionFantasmas.Instancia?.GetFantasmasRestantes() == 0)
        {
            CargarEscenaFin(false);
        }
    }

    /// <summary>
    /// Avanza a la siguiente ronda del juego.
    /// </summary>
    /// <remarks>
    /// Se llama cuando Pacman muere (victoria para el jugador fantasma).
    /// Incrementa el contador de rondas y verifica si se han completado todas las rondas.
    /// Si no se han completado todas las rondas, prepara la transición a la siguiente.
    /// Si se han completado todas, carga la escena de fin de partida con victoria.
    /// </remarks>
    private void AvanzarSiguienteRonda()
    {
        // Avanzar a la siguiente ronda
        rondaActual++;

        // Verificar si hemos completado todas las rondas
        if (rondaActual >= configuracionesRondas.Length)
        {
            CargarEscenaFin(true); // Cargar la escena de fin de partida (victoria)
            return;
        }

        Utilidades.AudioManager.Instancia.ReproducirSonidoGlobal(Utilidades.AudioManager.TipoSonido.Paso_ronda);

        SistemaGestionFantasmas.Instancia?.ReiniciarSistema();

        // Preparar transición a la siguiente ronda
        PrepararTransicionRonda($"RONDA  {rondaActual + 1}");
    }

    /// <summary>
    /// Prepara y muestra la transición entre rondas.
    /// </summary>
    /// <param name="textoTransicion">Texto que se mostrará en la pantalla de transición.</param>
    /// <remarks>
    /// Utiliza el sistema de transición para mostrar la pantalla de carga entre rondas.
    /// Si el sistema de transición no está disponible, inicia el juego directamente.
    /// </remarks>
    private void PrepararTransicionRonda(string textoTransicion)
    {
        // Referencia local para optimizar y evitar búsqueda repetida
        var transicionRonda = TransicionRonda.Instancia;
        if (transicionRonda != null)
        {
            transicionRonda.MostrarPantallaCarga(textoTransicion, IniciarJuego);
        }
        else
        {
            // Si no hay sistema de transición, iniciar directamente
            IniciarJuego();
        }
    }

    /// <summary>
    /// Carga la escena de fin de partida con el resultado final.
    /// </summary>
    /// <param name="esVictoria">Indica si el resultado es victoria (true) o derrota (false).</param>
    /// <remarks>
    /// Almacena el resultado en PlayerPrefs para que la escena de fin de partida pueda 
    /// determinar qué mensaje mostrar, y luego carga esa escena.
    /// </remarks>
    private void CargarEscenaFin(bool esVictoria)
    {
        PlayerPrefs.SetInt("esVictoria", esVictoria ? 1 : 0);
        PlayerPrefs.Save();

        TransicionEscena.Instancia.CargarEscena("fin_partida");
    }

    /// <summary>
    /// Configura e inicia el juego para la ronda actual.
    /// </summary>
    /// <remarks>
    /// Realiza las siguientes tareas:
    /// <list type="bullet">
    /// <item>Genera un nuevo laberinto basado en la configuración de la ronda actual</item>
    /// <item>Posiciona a los personajes en el laberinto</item>
    /// <item>Inicia el proceso de espera hasta que los personajes estén listos</item>
    /// </list>
    /// </remarks>
    private void IniciarJuego()
    {
        ConfiguracionRonda configActual = configuracionesRondas[rondaActual];

        // Genera un nuevo laberinto basado en la configuración
        dimensionesLaberinto = SpawnerLaberinto.Instancia.GenerarLaberinto(
            configActual.filas,
            configActual.columnas
        );
        Debug.Log($"Dimensiones del laberinto: {dimensionesLaberinto.x} x {dimensionesLaberinto.y}");

        // Posiciona los personajes en el laberinto
        Debug.Log("Generando personajes, iniciando espera...");
        SpawnPersonajes.Instancia.GenerarPersonajes(configActual.filas, configActual.columnas);
        
        // Iniciar la espera para verificar que los personajes estén listos
        esperandoPersonajes = true;
        temporizadorVerificacion = 0f;
    }
    
    /// <summary>
    /// Continúa la inicialización del juego después de que los personajes estén listos.
    /// </summary>
    /// <remarks>
    /// Esta función se llama una vez que se ha verificado que todos los personajes
    /// se han spawneado correctamente, y completa el resto de la inicialización del juego.
    /// </remarks>
    private void ContinuarInicializacion()
    {
        // Ahora que los personajes están listos, continuar con la inicialización
        InterfazJuego.Instancia.InicializarIndicadoresFantasmaMuerto();
        
        // Asegurarse de que todos los personajes estén registrados en el minimapa (solo en la primera ronda)
        if (rondaActual == 0)
        {
            Debug.Log("Primera ronda: Actualizando registros del minimapa para todos los personajes");
            SpawnPersonajes.Instancia.ActualizarRegistrosMinimapa();
        }

        // Incrementa la dificultad un 10% por cada ronda
        float multiplicadorDificultad = 1f + (rondaActual * 0.1f);
        // Aumenta la velocidad de Pacman según el multiplicador si existe
        Pacman.Instancia.ModificarVelocidad(multiplicadorDificultad);

        // Escanea el laberinto para el sistema de navegación
        EscanerLaberinto.Instancia.EscanearLaberinto(dimensionesLaberinto);
        
        Debug.Log("Inicialización del juego completada después de verificar personajes.");
    }

    /// <summary>
    /// Comprueba si Pacman está activo en la escena.
    /// </summary>
    /// <returns>True si Pacman existe y está activo, False en caso contrario.</returns>
    /// <remarks>
    /// Esta comprobación se usa para determinar si el juego debe avanzar a la siguiente ronda.
    /// </remarks>
    public bool PacmanEstaActivo() => Pacman.Instancia != null && Pacman.Instancia.gameObject.activeSelf;

    /// <summary>
    /// Devuelve las dimensiones del laberinto actual.
    /// </summary>
    /// <returns>Un Vector2 donde X representa filas y Y representa columnas.</returns>
    public Vector2 GetDimensionesLaberinto() => dimensionesLaberinto;

    /// <summary>
    /// Devuelve el número de la ronda actual.
    /// </summary>
    /// <returns>El índice de la ronda actual (base cero).</returns>
    public int ObtenerNumeroRondaActual() => rondaActual;
}

/// <summary>
/// Estructura para almacenar la configuración de cada ronda del juego.
/// </summary>
/// <remarks>
/// Define los parámetros que controlan la generación del laberinto y 
/// sus características para cada ronda específica.
/// </remarks>
[System.Serializable]
public class ConfiguracionRonda
{
    /// <summary>
    /// Número de filas del laberinto.
    /// </summary>
    public int filas;
    
    /// <summary>
    /// Número de columnas del laberinto.
    /// </summary>
    public int columnas;
    
    /// <summary>
    /// Nivel de oscuridad del laberinto.
    /// </summary>
    /// <remarks>
    /// Valor entre 0 y 1, donde 0 representa iluminación completa y 1 oscuridad total.
    /// </remarks>
    public float oscuridad;
}