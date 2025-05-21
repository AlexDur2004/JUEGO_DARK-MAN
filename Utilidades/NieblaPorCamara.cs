using UnityEngine;

/// <summary>
/// Controla la niebla específica para una cámara individual en el juego.
/// </summary>
/// <remarks>
/// Esta clase permite configurar parámetros de niebla únicos para cada cámara,
/// independientemente de la configuración global. También permite incrementar
/// la densidad de la niebla con cada ronda del juego.
/// </remarks>
[RequireComponent(typeof(Camera))]
public class NieblaPorCamara : MonoBehaviour
{
    [Header("Configuración de Niebla")]
    /// <summary>Determina si la niebla está activa para esta cámara.</summary>
    [SerializeField] private bool activarNiebla = true;

    /// <summary>El color de la niebla para esta cámara.</summary>
    [SerializeField] private Color colorNiebla = new(0.5f, 0.5f, 0.5f, 1f);

    /// <summary>La densidad base de la niebla antes de aplicar incrementos por ronda.</summary>
    [SerializeField] private float densidadNieblaBase = 0.25f;

    /// <summary>El modo de niebla (lineal, exponencial o exponencial al cuadrado).</summary>
    [SerializeField] private FogMode modoNiebla = FogMode.ExponentialSquared;

    /// <summary>Cantidad de densidad adicional que se aplica por cada ronda del juego.</summary>
    [SerializeField] private float incrementoPorRonda = 0.1f;

    /// <summary>Estado de activación de la niebla original.</summary>
    private bool nieblaOriginalActiva;

    /// <summary>Color de la niebla original.</summary>
    private Color colorOriginal;

    /// <summary>Densidad de la niebla original.</summary>
    private float densidadOriginal;

    /// <summary>Modo de niebla original.</summary>
    private FogMode modoOriginal;

    /// <summary>Referencia a la cámara asociada al componente.</summary>
    private Camera camaraAsociada;

    /// <summary>
    /// Inicializa la referencia a la cámara asociada.
    /// </summary>
    private void Awake()
    {
        camaraAsociada = GetComponent<Camera>();
    }

    /// <summary>
    /// Se llama justo antes de renderizar la cámara.
    /// Guarda la configuración actual de niebla y aplica la configuración personalizada.
    /// </summary>
    private void OnPreRender()
    {
        if (!camaraAsociada.enabled) return;

        GuardarConfiguracionActual();
        AplicarNieblaPersonalizada();
    }

    /// <summary>
    /// Se llama inmediatamente después de renderizar la cámara.
    /// Restaura la configuración original de la niebla.
    /// </summary>
    private void OnPostRender()
    {
        if (!camaraAsociada.enabled) return;

        RestaurarConfiguracionOriginal();
    }

    /// <summary>
    /// Almacena la configuración global actual de la niebla antes de modificarla.
    /// </summary>
    private void GuardarConfiguracionActual()
    {
        nieblaOriginalActiva = RenderSettings.fog;
        colorOriginal = RenderSettings.fogColor;
        densidadOriginal = RenderSettings.fogDensity;
        modoOriginal = RenderSettings.fogMode;
    }

    /// <summary>
    /// Aplica la configuración de niebla personalizada para esta cámara.
    /// </summary>
    /// <remarks>
    /// Incrementa la densidad de la niebla basada en la ronda actual del juego
    /// si el GameManager está disponible.
    /// </remarks>
    private void AplicarNieblaPersonalizada()
    {
        float densidadFinal = densidadNieblaBase;

        // Incrementar la densidad de la niebla por cada ronda
        if (GameManager.Instancia != null)
        {
            densidadFinal += GameManager.Instancia.ObtenerNumeroRondaActual() * incrementoPorRonda;
        }

        RenderSettings.fog = activarNiebla;
        RenderSettings.fogColor = colorNiebla;
        RenderSettings.fogDensity = densidadFinal;
        RenderSettings.fogMode = modoNiebla;
    }

    /// <summary>
    /// Restaura la configuración original de la niebla que estaba activa
    /// antes de que esta cámara renderizara.
    /// </summary>
    private void RestaurarConfiguracionOriginal()
    {
        RenderSettings.fog = nieblaOriginalActiva;
        RenderSettings.fogColor = colorOriginal;
        RenderSettings.fogDensity = densidadOriginal;
        RenderSettings.fogMode = modoOriginal;
    }
}
