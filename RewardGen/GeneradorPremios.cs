using UnityEngine;
using System.Collections.Generic;
using Utilidades;
using System.Linq;

/// <summary>
/// Sistema que genera y gestiona las frutas (premios) que aparecen aleatoriamente en el laberinto.
/// Utiliza el patrón Object Pool para optimizar el rendimiento y controla la aparición
/// de frutas normales y especiales con intervalos configurables.
/// </summary>
public class GeneradorPremios : Singleton<GeneradorPremios>
{
    /// <summary>
    /// Configuración para un tipo específico de premio (fruta).
    /// </summary>
    [System.Serializable]
    private class ConfigPremio
    {
        /// <summary>
        /// Prefab del premio que será instanciado.
        /// </summary>
        public GameObject prefab;
        
        /// <summary>
        /// Cantidad de instancias de este premio en el pool.
        /// </summary>
        public int cantidadPool = 5;
        
        /// <summary>
        /// Tiempo en segundos entre apariciones consecutivas de este tipo de premio.
        /// </summary>
        public float intervaloAparicion = 7f;
        
        /// <summary>
        /// Tiempo en el que se generó el último premio de este tipo.
        /// </summary>
        [HideInInspector] public float ultimoTiempoGenerado;
        
        /// <summary>
        /// Contenedor para organizar todas las instancias de este tipo de premio en la jerarquía.
        /// </summary>
        [HideInInspector] public Transform contenedor;
    }

    [Header("Tipos de Premios")]
    /// <summary>
    /// Configuración para los premios normales (frutas comunes).
    /// </summary>
    [SerializeField] private ConfigPremio premioNormal = new() { cantidadPool = 6, intervaloAparicion = 5f };
    
    /// <summary>
    /// Configuración para los premios especiales (frutas especiales con efectos más poderosos).
    /// </summary>
    [SerializeField] private ConfigPremio premioEspecial = new() { cantidadPool = 4, intervaloAparicion = 10f };

    /// <summary>
    /// Pool compartido para todos los tipos de premios.
    /// </summary>
    private ObjectPool<GameObject> poolPremios = new();

    /// <summary>
    /// Inicializa las pools de frutas al comenzar.
    /// </summary>
    private void Start()
    {
        InicializarTipoPremio(premioNormal, "FrutasNormalesPool");
        InicializarTipoPremio(premioEspecial, "FrutasEspecialesPool");
    }

    /// <summary>
    /// Inicializa un tipo específico de premio creando sus instancias y configurándolas.
    /// </summary>
    /// <param name="config">Configuración del premio a inicializar.</param>
    /// <param name="nombreContenedor">Nombre para el GameObject contenedor de este tipo de premios.</param>
    private void InicializarTipoPremio(ConfigPremio config, string nombreContenedor)
    {
        // Crear contenedor
        config.contenedor = new GameObject(nombreContenedor).transform;
        config.contenedor.parent = transform;
        
        // Crear frutas para el pool
        for (int i = 0; i < config.cantidadPool; i++)
        {
            // Crear objeto y configurarlo manteniendo la rotación original del prefab
            var premioGO = Instantiate(config.prefab, Vector3.zero, config.prefab.transform.rotation, config.contenedor);

            // Registrar en el pool como inactivo
            poolPremios.Registrar(premioGO);
            poolPremios.Desactivar(premioGO);
        }
    }

    /// <summary>
    /// Verifica periódicamente si es momento de generar nuevas frutas.
    /// </summary>
    private void Update()
    {
        float tiempoActual = Time.time;
        VerificarYGenerarPremio(premioNormal, tiempoActual);
        VerificarYGenerarPremio(premioEspecial, tiempoActual);
    }

    /// <summary>
    /// Genera un premio si ha pasado suficiente tiempo desde la última generación.
    /// </summary>
    /// <param name="config">Configuración del tipo de premio a verificar.</param>
    /// <param name="tiempoActual">Tiempo actual del juego.</param>
    private void VerificarYGenerarPremio(ConfigPremio config, float tiempoActual)
    {
        if (tiempoActual >= config.ultimoTiempoGenerado + config.intervaloAparicion)
            ActivarPremioDisponible(config);
    }

    /// <summary>
    /// Activa un premio del pool especificado, colocándolo en una posición aleatoria del laberinto.
    /// </summary>
    /// <param name="config">Configuración del tipo de premio a activar.</param>
    private void ActivarPremioDisponible(ConfigPremio config)
    {
        // Verificar si hay un spawner válido
        var spawner = SpawnerLaberinto.Instancia;
        if (!spawner) return;

        // Buscar fruta del tipo adecuado
        string tagBuscado = config == premioEspecial ? "FrutaEspecial" : "Fruta";
        var premio = poolPremios.GetInactivos().FirstOrDefault(p => p.CompareTag(tagBuscado));
        if (premio == null) return;
        
        // Posicionar y activar
        premio.transform.position = spawner.ObtenerPosicionAleatoriaEnLaberinto();
        poolPremios.Activar(premio);
        
        // Reproducir sistemas de partículas si existen
        ParticleSystem[] sistemasParticulas = premio.GetComponentsInChildren<ParticleSystem>();
        foreach (var sistema in sistemasParticulas)
        {
            sistema.Stop(true);  // Detener para asegurar que empieza desde el principio
            sistema.Play(true);  // Reproducir la animación de partículas
        }
        
        config.ultimoTiempoGenerado = Time.time;
    }

    /// <summary>
    /// Maneja la recogida de una fruta por parte de un personaje,
    /// desactivándola y devolviéndola al pool.
    /// </summary>
    /// <param name="frutaGO">GameObject de la fruta recogida.</param>
    public void FrutaRecogida(GameObject frutaGO)
    {
        if (frutaGO != null && poolPremios.ContieneActivo(frutaGO))
            poolPremios.Desactivar(frutaGO);
    }
}