using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Clase que gestiona las transiciones entre escenas del juego.
/// </summary>
/// <remarks>
/// Implementa un efecto de desvanecimiento (fade) para suavizar las transiciones
/// entre escenas. Utiliza un singleton para asegurar una única instancia
/// y proporcionar acceso global desde otros componentes.
/// </remarks>
public class TransicionEscena : MonoBehaviour
{
    /// <summary>
    /// Instancia única accesible globalmente (patrón Singleton)
    /// </summary>
    public static TransicionEscena Instancia;

    /// <summary>
    /// Sección de configuración de parámetros de la transición
    /// </summary>
    [Header("Configuración de la Transición")]
    
    /// <summary>
    /// Tiempo en segundos que tarda en aparecer/desaparecer la cortina de transición
    /// </summary>
    public float tiempoDesvanecimiento = 1.0f;
    
    /// <summary>
    /// Curva de animación que controla la suavidad de la transición
    /// </summary>
    /// <remarks>
    /// Una curva EaseInOut proporciona un inicio y final suaves para la transición
    /// </remarks>
    public AnimationCurve curvaTransicion = AnimationCurve.EaseInOut(0, 0, 1, 1);

    /// <summary>
    /// Sección de configuración visual
    /// </summary>
    [Header("Apariencia")]
    
    /// <summary>
    /// Color de la pantalla que cubre la transición entre escenas
    /// </summary>
    public Color colorCortina = Color.black;

    /// <summary>
    /// Grupo de Canvas que controla la transparencia del panel de transición
    /// </summary>
    private CanvasGroup panelTransicion;
    
    /// <summary>
    /// Imagen de fondo utilizada para la cortina de transición
    /// </summary>
    private Image imagenFondo;    /// <summary>
    /// Inicializa el sistema de transiciones al despertar el objeto
    /// </summary>
    /// <remarks>
    /// Configura la instancia singleton y obtiene las referencias a los
    /// componentes necesarios para realizar las transiciones.
    /// </remarks>
    private void Awake()
    {
        // Asegurarse de que solo haya una instancia de TransicionEscena
        Instancia = this;

        panelTransicion = GetComponentInChildren<CanvasGroup>();
        imagenFondo = GetComponentInChildren<Image>();
    }

    /// <summary>
    /// Inicia la carga de una nueva escena con efecto de transición
    /// </summary>
    /// <param name="nombreEscena">Nombre de la escena a cargar</param>
    /// <remarks>
    /// Este método es el punto de entrada principal para realizar transiciones
    /// entre escenas. Inicia la corrutina que maneja todo el proceso de transición.
    /// </remarks>
    public void CargarEscena(string nombreEscena)
    {
        StartCoroutine(RealizarTransicion(nombreEscena));
    }

    /// <summary>
    /// Ejecuta la secuencia completa de transición entre escenas
    /// </summary>
    /// <param name="nombreEscena">Nombre de la escena a cargar</param>
    /// <returns>Un enumerador para la corrutina</returns>
    /// <remarks>
    /// La secuencia consiste en:
    /// 1. Mostrar gradualmente la cortina (fadeout)
    /// 2. Cargar la nueva escena mientras la pantalla está cubierta
    /// 3. Ocultar gradualmente la cortina (fadein) para revelar la nueva escena
    /// </remarks>
    private IEnumerator RealizarTransicion(string nombreEscena)
    {
        // Asegurar que el panel de transición esté activo
        panelTransicion.gameObject.SetActive(true);

        // Mostrar la cortina (fadeout - la pantalla se oscurece)
        yield return StartCoroutine(MostrarCortina(true));

        // La pantalla ahora está completamente negra

        // Cargar la nueva escena mientras estamos en negro
        SceneManager.LoadScene(nombreEscena);

        // Esperar un momento para asegurarse de que la escena se ha cargado completamente
        yield return new WaitForSeconds(0.2f);

        // Ocultar la cortina (fadein - la nueva escena aparece gradualmente)
        yield return StartCoroutine(MostrarCortina(false));
    }

    /// <summary>
    /// Controla la animación de aparición o desaparición de la cortina de transición
    /// </summary>
    /// <param name="mostrar">True para mostrar la cortina (fadeout), False para ocultarla (fadein)</param>
    /// <returns>Un enumerador para la corrutina</returns>
    /// <remarks>
    /// Este método maneja la animación gradual de transparencia del panel de transición
    /// usando interpolación y una curva de animación personalizable para un efecto más suave.
    /// También gestiona el bloqueo de interacciones durante la transición.
    /// </remarks>
    private IEnumerator MostrarCortina(bool mostrar)
    {
        float tiempoInicial = Time.time;
        float tiempoFinal = tiempoInicial + tiempoDesvanecimiento;

        // Valores de alpha iniciales y finales
        // Si mostrar=true, vamos de transparente (0) a opaco (1) - fadeout
        // Si mostrar=false, vamos de opaco (1) a transparente (0) - fadein
        float alphaInicial = mostrar ? 0f : 1f;
        float alphaFinal = mostrar ? 1f : 0f;

        // Habilitar/deshabilitar interacción durante la transición
        panelTransicion.blocksRaycasts = mostrar;

        while (Time.time < tiempoFinal)
        {
            float t = (Time.time - tiempoInicial) / tiempoDesvanecimiento;
            float valorCurva = curvaTransicion.Evaluate(t);

            panelTransicion.alpha = Mathf.Lerp(alphaInicial, alphaFinal, valorCurva);

            yield return null;
        }

        // Asegurar que llegamos al valor final
        panelTransicion.alpha = alphaFinal;

        // Si estamos ocultando la cortina, desactivar el gameObject para evitar bloquear interacciones
        if (!mostrar)
        {
            panelTransicion.blocksRaycasts = false;
        }
    }
}