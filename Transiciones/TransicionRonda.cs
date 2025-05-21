using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Utilidades;

/// <summary>
/// Sistema que maneja las transiciones visuales entre rondas y pantalla de carga
/// </summary>
/// <remarks>
/// Implementa efectos visuales para las transiciones entre rondas, incluyendo
/// desvanecimientos y animaciones de texto. Utiliza el patrón Singleton para
/// proporcionar acceso global desde otros componentes del juego.
/// </remarks>
public class TransicionRonda : Singleton<TransicionRonda>
{
    /// <summary>
    /// Componentes de la interfaz de usuario para la transición
    /// </summary>
    [Header("Componentes")]
    
    /// <summary>
    /// Grupo de Canvas que controla la transparencia y las interacciones
    /// </summary>
    [SerializeField] private CanvasGroup canvasGroup;
    
    /// <summary>
    /// Componente de texto que muestra información durante la transición
    /// </summary>
    [SerializeField] private TextMeshProUGUI textoRonda;
    
    /// <summary>
    /// Imagen de fondo que oscurece la pantalla durante la transición
    /// </summary>
    [SerializeField] private Image fondoNegro;

    /// <summary>
    /// Parámetros configurables para la transición
    /// </summary>
    [Header("Configuración")]
    
    /// <summary>
    /// Velocidad a la que desaparece la transición (unidades por segundo)
    /// </summary>
    [SerializeField] private float velocidadFadeOut = 0.5f;
    
    /// <summary>
    /// Tiempo entre caracteres al animar texto (en segundos)
    /// </summary>
    [SerializeField] private float velocidadEscritura = 0.2f;

    /// <summary>
    /// Indica si hay una transición actualmente en proceso
    /// </summary>
    private bool estaEnTransicion = false;

    /// <summary>
    /// Inicializa los componentes necesarios para las transiciones
    /// </summary>
    /// <remarks>
    /// Configura el singleton, obtiene las referencias a los componentes y
    /// establece el estado inicial del canvas como invisible para no bloquear
    /// las interacciones al inicio del juego.
    /// </remarks>
    protected override void Awake()
    {
        // Llama al método Awake de la clase base para inicializar el singleton
        base.Awake();

        // Carga optimizada de componentes con el operador ??=
        canvasGroup ??= GetComponent<CanvasGroup>();
        textoRonda ??= GetComponentInChildren<TextMeshProUGUI>();
        fondoNegro ??= GetComponentInChildren<Image>();

        // Inicializar canvas visible pero transparente para permitir interacciones
        // Esto permite que el resto del juego se inicie sin bloqueos
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>
    /// Muestra una pantalla de carga mientras inicia el juego en segundo plano
    /// </summary>
    /// <param name="texto">Texto a mostrar durante la carga</param>
    /// <param name="operacionCompletada">Acción que se invoca para iniciar el juego en paralelo</param>
    /// <remarks>
    /// Este método es el punto de entrada principal para mostrar una pantalla de carga.
    /// Detiene cualquier transición en curso, asegura que el canvas esté activo y
    /// comienza la animación de la pantalla de carga mientras ejecuta la operación
    /// de inicialización proporcionada en paralelo.
    /// </remarks>
    public void MostrarPantallaCarga(string texto, Action operacionCompletada)
    {
        // Detener cualquier transición anterior si estaba en curso
        if (estaEnTransicion)
        {
            StopAllCoroutines();
        }

        // Asegurar que el canvas esté activo
        gameObject.SetActive(true);
        canvasGroup.gameObject.SetActive(true);

        // Iniciar la nueva animación
        StartCoroutine(AnimarPantallaCarga(texto, operacionCompletada));
    }

    /// <summary>
    /// Anima la pantalla de carga mientras inicia el juego en segundo plano
    /// </summary>
    /// <param name="texto">Texto a mostrar durante la carga</param>
    /// <param name="operacionCompletada">Acción que se invoca para iniciar el juego</param>
    /// <returns>Un enumerador para la corrutina</returns>
    /// <remarks>
    /// Esta corrutina implementa la secuencia completa de animación de la pantalla de carga:
    /// 1. Inicia la operación de carga inmediatamente sin esperar animaciones
    /// 2. Muestra la pantalla de carga instantáneamente
    /// 3. Anima el texto con efecto de escritura
    /// 4. Espera un tiempo para que el jugador pueda leer el texto
    /// 5. Inicia el desvanecimiento de la pantalla
    /// 
    /// Prioriza que la carga del juego comience lo antes posible mientras
    /// proporciona retroalimentación visual al jugador.
    /// </remarks>
    private IEnumerator AnimarPantallaCarga(string texto, Action operacionCompletada)
    {
        estaEnTransicion = true;

        // Ejecutamos la operación de inicialización del juego INMEDIATAMENTE
        // sin esperar a completar la animación de entrada
        operacionCompletada?.Invoke();

        // Limpiamos el texto antes de empezar y preparamos el canvas
        textoRonda.text = "";
        canvasGroup.blocksRaycasts = true;

        // Mostramos inmediatamente la pantalla sin animación de entrada gradual
        canvasGroup.alpha = 1;

        // Animamos la escritura del texto con una velocidad un poco mayor para no retrasar la carga
        float velocidadOriginal = velocidadEscritura;
        velocidadEscritura *= 0.7f;
        yield return AnimarEscrituraTexto(texto);
        velocidadEscritura = velocidadOriginal; // Restauramos la velocidad original

        // Esperamos más tiempo para que el jugador pueda leer el texto completo 
        // y para que el juego termine de cargar correctamente
        yield return new WaitForSeconds(1.0f);

        // Iniciamos desvanecimiento mientras el juego sigue cargando
        StartCoroutine(DesvanecerTransicionEnSegundoPlano());
    }

    /// <summary>
    /// Realiza el efecto de fade out en segundo plano mientras el juego se inicializa
    /// </summary>
    /// <returns>Un enumerador para la corrutina</returns>
    /// <remarks>
    /// Esta corrutina implementa la animación de desvanecimiento (fadeout) de la pantalla de carga.
    /// Utiliza una velocidad acelerada para mejorar la experiencia del usuario y
    /// Time.unscaledDeltaTime para que el desvanecimiento sea independiente de la escala de tiempo
    /// del juego. Finaliza reiniciando el canvas a su estado original.
    /// </remarks>
    private IEnumerator DesvanecerTransicionEnSegundoPlano()
    {
        // Fade out más rápido para mejorar la experiencia de carga
        float velocidadFadeOutAcelerado = velocidadFadeOut * 2.5f; // Mucho más rápido

        // Aseguramos que el canvas esté visible antes de comenzar a desvanecer
        if (canvasGroup.alpha < 1)
            canvasGroup.alpha = 1;

        // Fade out (negro y texto desaparecen)
        while (canvasGroup.alpha > 0.01f) // Umbral cercano a cero para evitar problemas de precisión
        {
            // Utilizamos deltaTime no escalado para que sea independiente de timeScale
            canvasGroup.alpha -= Time.unscaledDeltaTime * velocidadFadeOutAcelerado;
            yield return null;
        }

        // Limpiamos y reseteamos todo
        ReiniciarCanvas();
        estaEnTransicion = false;
    }

    /// <summary>
    /// Reinicia el canvas a su estado inicial invisible
    /// </summary>
    /// <remarks>
    /// Este método restaura todos los componentes de la interfaz de usuario
    /// a su estado inicial, garantizando que la pantalla de transición quede
    /// completamente invisible y no bloquee ninguna interacción con el juego.
    /// Es llamado al finalizar una transición.
    /// </remarks>
    private void ReiniciarCanvas()
    {
        // Forzamos alpha a cero para garantizar invisibilidad
        canvasGroup.alpha = 0f;

        // Desactivamos interacciones
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Limpiamos el texto
        textoRonda.text = "";
    }

    /// <summary>
    /// Anima un texto para que aparezca letra por letra, simulando que se está escribiendo
    /// </summary>
    /// <param name="textoCompleto">El texto final que se mostrará</param>
    /// <returns>Un enumerador para la corrutina</returns>
    /// <remarks>
    /// Implementa un efecto visual donde el texto aparece letra por letra,
    /// como si se estuviera escribiendo en tiempo real. Además, reproduce
    /// un sonido de tecleo para cada carácter que no sea un espacio en blanco,
    /// mejorando la sensación de escritura con retroalimentación auditiva.
    /// </remarks>
    private IEnumerator AnimarEscrituraTexto(string textoCompleto)
    {
        // Limpiamos el texto inicial
        textoRonda.text = "";

        // Escribimos el texto letra por letra
        for (int i = 0; i < textoCompleto.Length; i++)
        {
            // Añadimos una letra
            textoRonda.text += textoCompleto[i];

            // Si el carácter es un espacio en blanco, no reproducimos sonido ni esperamos
            if (textoCompleto[i] != ' ')
            {
                // Reproducir sonido de tecleo para cada carácter
                Utilidades.AudioManager.Instancia.ReproducirSonidoGlobal(Utilidades.AudioManager.TipoSonido.Tecleo);

                // Esperamos un tiempo entre cada letra solo si no es un espacio
                yield return new WaitForSeconds(velocidadEscritura);
            }
        }
    }
}
