using UnityEngine;
using UnityEngine.UI;
using Utilidades;
using System.Collections.Generic;
using System;   // Para optimizaciones con delegados y eventos

/// <summary>
/// Controla todos los elementos de interfaz de usuario durante el juego.
/// Gestiona la visualización de salud, cooldowns, indicadores de estado de los fantasmas
/// y la rotación de personajes controlados por el jugador.
/// </summary>
/// <remarks>
/// Implementado como Singleton para garantizar acceso global único desde cualquier parte del juego.
/// Se encarga de mostrar el estado de todos los personajes y actualizar dinámicamente la interfaz
/// según las interacciones del jugador y eventos del juego.
/// </remarks>
public class InterfazJuego : Singleton<InterfazJuego>
{
    /// <summary>
    /// Borde rojo que aparece cuando el personaje está herido.
    /// La opacidad aumenta a medida que la salud disminuye.
    /// </summary>
    public Image bordePantalla;

    [Header("Barras de Vida")]
    /// <summary>
    /// Sliders para mostrar la vida de cada personaje.
    /// </summary>
    public Slider[] barrasVida;
    
    /// <summary>
    /// Componentes de color de las barras de vida.
    /// Cambian de color según el porcentaje de vida (verde->amarillo->rojo).
    /// </summary>
    public Image[] barrasVidaFill;

    [Header("Barras de Habilidad")]
    /// <summary>
    /// Sliders para mostrar el cooldown de las habilidades.
    /// </summary>
    public Slider[] barrasHabilidad;

    [Header("Iconos de Fantasmas")]
    /// <summary>
    /// Iconos para mostrar qué fantasma está en cada posición de la interfaz.
    /// </summary>
    public Image[] iconosPosicionesInterfaz;
    
    /// <summary>
    /// Sprites para cada uno de los fantasmas.
    /// </summary>
    public Sprite[] spritesIconosFantasma;

    [Header("Indicadores de Estado de Fantasmas")]
    /// <summary>
    /// Imágenes siempre activas para mostrar estado (muerte/seguimiento).
    /// </summary>
    public Image[] iconosEstadoFantasmas;
    
    /// <summary>
    /// Sprite para indicar fantasma muerto.
    /// </summary>
    public Sprite spriteCalavera;
    
    /// <summary>
    /// Sprite para indicar fantasma en seguimiento.
    /// </summary>
    public Sprite spriteSeguimiento;
    
    /// <summary>
    /// Sprite transparente para cuando no hay estado especial.
    /// </summary>
    private Sprite spriteNormal;
    
    /// <summary>
    /// Constante para comparaciones de valores pequeños.
    /// </summary>
    private const float EPSILON = 0.01f;
    
    [Header("Barras de Cooldown")]
    /// <summary>
    /// Slider para mostrar el cooldown del cambio de fantasma.
    /// </summary>
    public Slider sliderCooldownCambioFantasma;
    
    /// <summary>
    /// Slider para mostrar el cooldown del botón SOS.
    /// </summary>
    public Slider sliderCooldownSOS;
    
    /// <summary>
    /// Slider para mostrar si la capacidad de revivir está disponible.
    /// </summary>
    public Slider sliderCapacidadRevivir;

    /// <summary>
    /// Número total de personajes en el juego.
    /// </summary>
    /// <remarks>
    /// Constante definida antes para poder usarla en la declaración de los arrays.
    /// </remarks>
    public const int NUM_PERSONAJES = 4;
    
    /// <summary>
    /// Mapeo entre posiciones UI y números de fantasma.
    /// </summary>
    private readonly int[] posicionesUI = new int[NUM_PERSONAJES];
    
    /// <summary>
    /// Mapeo entre números de fantasma y posiciones UI.
    /// </summary>
    private readonly int[] posicionesFantasma = new int[NUM_PERSONAJES + 1];

    /// <summary>
    /// Gradiente para colorear las barras de vida.
    /// </summary>
    private Gradient gradienteVida;

    /// <summary>
    /// Referencias a los fantasmas activos.
    /// </summary>
    private readonly Dictionary<int, FantasmaBase> fantasmas = new();

    /// <summary>
    /// Inicializa los componentes base.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        
        // Inicializar en un orden lógico
        InicializarGradienteVida(); // Primero creamos el gradiente
        CrearSpriteTransparente();  // Después los sprites necesarios
        InicializarPosicionesUI();  // Posiciones de los fantasmas
        InicializarBarras();        // Finalmente las barras con los datos ya disponibles
    }

    /// <summary>
    /// Crea un sprite transparente para los iconos en estado normal.
    /// </summary>
    private void CrearSpriteTransparente()
    {
        // Crear una textura 1x1 totalmente transparente
        Texture2D textura = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        textura.SetPixel(0, 0, Color.clear);
        textura.Apply();

        // Crear un sprite a partir de la textura
        spriteNormal = Sprite.Create(textura, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
    }

    /// <summary>
    /// Configura las posiciones iniciales de los fantasmas en la UI.
    /// </summary>
    /// <remarks>
    /// Establece el mapeo inicial entre posiciones de UI y fantasmas, asegurando
    /// que el fantasma principal seleccionado por el jugador se coloque en la
    /// primera posición de la interfaz.
    /// </remarks>
    private void InicializarPosicionesUI()
    {
        // Comprobación de seguridad para los arrays
        if (posicionesUI.Length < NUM_PERSONAJES || posicionesFantasma.Length <= NUM_PERSONAJES)
        {
            Debug.LogError("InterfazJuego: Error en inicialización de arrays de posiciones");
            return;
        }
        
        // Configuración inicial: posición UI i (0-based) contiene al fantasma i+1
        for (int i = 0; i < NUM_PERSONAJES; i++)
        {
            posicionesUI[i] = i + 1;            // posicionesUI[0] = 1, posicionesUI[1] = 2, etc.
            posicionesFantasma[i + 1] = i;      // posicionesFantasma[1] = 0, posicionesFantasma[2] = 1, etc.
        }

        // Obtiene directamente el fantasma principal desde PlayerPrefs
        int fantasmaPrincipal = PlayerPrefs.GetInt("jugadorElegido", 1);

        // Aseguramos que el fantasma principal siempre sea movido, aunque sea el 1
        if (fantasmaPrincipal >= 1)
        {
            // Posición original del fantasma principal en la UI (0-based)
            int posicionOriginal = posicionesFantasma[fantasmaPrincipal];
            // Número del fantasma que está actualmente en la posición UI 0
            int fantasmaPosicionCero = posicionesUI[0];

            // Intercambia las posiciones
            posicionesUI[0] = fantasmaPrincipal;                 // La posición UI 0 ahora contiene al fantasma principal
            posicionesUI[posicionOriginal] = fantasmaPosicionCero;// La posición original ahora contiene al fantasma que estaba en posición 0

            posicionesFantasma[fantasmaPrincipal] = 0;           // El fantasma principal ahora está en posición UI 0
            posicionesFantasma[fantasmaPosicionCero] = posicionOriginal; // El otro fantasma ahora está en la posición original
        }
    }

    /// <summary>
    /// Configura eventos y estado inicial - se ejecuta después de Awake.
    /// </summary>
    /// <remarks>
    /// Obtiene referencias a los fantasmas y se suscribe a los eventos
    /// relevantes del sistema de gestión de fantasmas.
    /// </remarks>
    private void Start()
    {
        // Obtiene referencias a los fantasmas disponibles en el nivel
        ObtenerFantasmas();

        // Suscripción a eventos del sistema de fantasmas usando una referencia cacheada
        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        if (sistemaGestion)
        {
            // Usar += para eventos y no olvidar desuscribirse cuando sea necesario
            sistemaGestion.OnFantasmaControlCambiado += CambiarFantasmaPrincipal;
            sistemaGestion.OnFantasmaDerrotado += ActualizarIndicadorFantasma;
        }
        else
        {
            Debug.LogWarning("InterfazJuego: No se encontró SistemaGestionFantasmas");
        }
    }

    /// <summary>
    /// Método para limpiar suscripciones a eventos.
    /// </summary>
    /// <remarks>
    /// Importante para evitar referencias persistentes que puedan causar memory leaks.
    /// </remarks>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Eliminar suscripciones a eventos para evitar referencias persistentes
        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        if (sistemaGestion)
        {
            sistemaGestion.OnFantasmaControlCambiado -= CambiarFantasmaPrincipal;
            sistemaGestion.OnFantasmaDerrotado -= ActualizarIndicadorFantasma;
        }
    }

    /// <summary>
    /// Obtiene referencias a todos los fantasmas disponibles.
    /// </summary>
    private void ObtenerFantasmas()
    {
        fantasmas.Clear();

        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        if (!sistemaGestion) return;

        // Obtener todos los fantasmas de una vez para mejorar rendimiento
        for (int i = 1; i <= NUM_PERSONAJES; i++)
        {
            FantasmaBase fantasma = sistemaGestion.GetFantasma(i);
            if (fantasma != null)
                fantasmas[i] = fantasma;
        }
    }

    /// <summary>
    /// Actualiza la interfaz cada frame.
    /// </summary>
    /// <remarks>
    /// Comprueba que haya referencias a los fantasmas y actualiza
    /// todos los elementos de interfaz visibles.
    /// </remarks>
    private void Update()
    {
        // Asegura que tengamos referencia a los fantasmas
        if (fantasmas.Count == 0)
        {
            ObtenerFantasmas();
            if (fantasmas.Count == 0) return;
        }

        // Optimización: solo actualizar UI visible
        if (gameObject.activeInHierarchy)
        {
            ActualizarBarrasUI();
            ActualizarBordePantalla();
            ActualizarBarrasCooldown();
        }
    }

    /// <summary>
    /// Actualiza las barras de vida y habilidad de todos los personajes.
    /// </summary>
    private void ActualizarBarrasUI()
    {
        // Cacheamos la longitud de los arrays para evitar accesos repetidos
        int barrasVidaLength = barrasVida.Length;
        int barrasHabilidadLength = barrasHabilidad.Length;
        
        // Actualiza barras para los fantasmas
        for (int posUI = 0; posUI < NUM_PERSONAJES; posUI++)
        {
            int numeroFantasma = posicionesUI[posUI];

            // Verifica que el fantasma exista
            if (!fantasmas.TryGetValue(numeroFantasma, out FantasmaBase fantasma) || fantasma == null)
                continue;

            // Actualiza barra de vida con color según porcentaje
            if (posUI < barrasVidaLength)
            {
                float porcentajeVida = fantasma.GetPorcentajeVida();
                barrasVida[posUI].value = porcentajeVida;
                barrasVidaFill[posUI].color = gradienteVida.Evaluate(porcentajeVida);
            }

            // Actualiza barra de cooldown de habilidad
            if (posUI < barrasHabilidadLength && fantasma.habilidadAsociada != null)
            {
                barrasHabilidad[posUI].value = fantasma.habilidadAsociada.GetPorcentajeCooldown();
            }

            // Actualiza el estado del icono del fantasma
            bool estaMuerto = fantasma.EstaInactivo(); // Usa el método apropiado para verificar si está derribado
            bool siguiendo = false;

            // Verifica si el fantasma está siguiendo (solo si está vivo)
            if (!estaMuerto)
            {
                ControladorIA controladorIA = fantasma.GetComponent<ControladorIA>();
                if (controladorIA != null)
                {
                    siguiendo = controladorIA.EstaSiguiendoJugador();
                }
            }

            ActualizarIconoEstado(posUI, estaMuerto, siguiendo);
        }

        // Actualiza la barra de vida de Pacman (posición 4)
        // Usamos el índice 4 (posición 5) que es la siguiente después de los 4 fantasmas
        var pacman = Pacman.Instancia;
        if (barrasVidaLength > 4 && pacman != null)
        {
            float porcentajeVida = pacman.GetPorcentajeVida();
            barrasVida[4].value = porcentajeVida;

            if (barrasVidaFill.Length > 4)
                barrasVidaFill[4].color = gradienteVida.Evaluate(porcentajeVida);
        }
    }

    /// <summary>
    /// Actualiza el borde rojo de la pantalla según la vida del fantasma principal.
    /// </summary>
    /// <remarks>
    /// A menor vida del personaje, más visible se hace el borde rojo,
    /// proporcionando feedback visual del estado de salud.
    /// </remarks>
    private void ActualizarBordePantalla()
    {
        if (bordePantalla == null)
            return;
            
        int numeroFantasmaPrincipal = posicionesUI[0];

        if (fantasmas.TryGetValue(numeroFantasmaPrincipal, out FantasmaBase fantasmaPrincipal) &&
            fantasmaPrincipal && fantasmaPrincipal.gameObject.activeInHierarchy)
        {
            // A menor vida, más visible es el borde rojo
            float porcentajeVida = fantasmaPrincipal.GetPorcentajeVida();
            float intensidadBorde = Mathf.Clamp01(1f - (porcentajeVida * 2f)) * 0.7f;
            bordePantalla.color = new Color(1, 0, 0, intensidadBorde);
        }
    }

    /// <summary>
    /// Actualiza las barras de cooldown del cambio de fantasma y SOS.
    /// </summary>
    private void ActualizarBarrasCooldown()
    {
        // Obtener referencia al sistema de gestión de fantasmas y controlador de jugador
        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        var controladorJugador = ControladorJugador.Instancia;
        
        if (sistemaGestion == null || controladorJugador == null) return;
        
        // Constante para comparar valores pequeños - evita crear un nuevo float en cada frame
        const float EPSILON = 0.01f;
        
        // Actualizar barra de cooldown de cambio de fantasma
        ActualizarSliderCooldown(sliderCooldownCambioFantasma, sistemaGestion.GetPorcentajeCooldownCambioFantasma(), EPSILON);
        
        // Actualizar barra de cooldown de SOS
        ActualizarSliderCooldown(sliderCooldownSOS, controladorJugador.GetPorcentajeCooldownSOS(), EPSILON);
        
        // Actualizar barra de capacidad de revivir
        // Si el jugador puede revivir, el valor es 0 (disponible)
        // Si no puede revivir, el valor es 1 (no disponible/usado)
        float valorRevivir = controladorJugador.PuedeRevivirEnRonda() ? 0f : 1f;
        ActualizarSliderCooldown(sliderCapacidadRevivir, valorRevivir, EPSILON);
    }
    
    /// <summary>
    /// Método auxiliar para actualizar cualquier slider de cooldown.
    /// </summary>
    /// <param name="slider">El slider a actualizar.</param>
    /// <param name="valor">Valor nuevo para el slider.</param>
    /// <param name="epsilon">Umbral para ocultar el fill cuando el valor es muy pequeño.</param>
    private void ActualizarSliderCooldown(Slider slider, float valor, float epsilon)
    {
        if (slider == null)
            return;
            
        slider.value = valor;
        
        // Ocultar el fill cuando el valor sea menor que epsilon
        if (slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.enabled = valor > epsilon;
            }
        }
    }

    /// <summary>
    /// Actualiza el estado de un icono según la posición en la UI o número de fantasma.
    /// </summary>
    /// <param name="posicionUI">Posición en la interfaz (0-based).</param>
    /// <param name="estaMuerto">Indica si el fantasma está muerto.</param>
    /// <param name="siguiendo">Indica si el fantasma está siguiendo al jugador.</param>
    /// <remarks>
    /// Este método unifica las funciones de gestión de iconos de estado.
    /// </remarks>
    private void ActualizarIconoEstado(int posicionUI, bool estaMuerto, bool siguiendo = false)
    {
        // posicionUI es 0-based (0 = primera posición visual en la UI, 1 = segunda, etc.)
        
        // Validaciones rápidas 
        if (posicionUI == 0 || iconosEstadoFantasmas == null || posicionUI >= iconosEstadoFantasmas.Length + 1) 
            return;

        // Comprueba que no sea el último fantasma vivo (para no mostrar calavera)
        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        if (sistemaGestion && sistemaGestion.GetFantasmasRestantes() <= 1 && !estaMuerto)
            return;

        // Actualiza el sprite según el estado (prioridad: muerto > siguiendo > normal)
        Image iconoEstado = iconosEstadoFantasmas[posicionUI-1];
        if (iconoEstado == null)
            return;
            
        if (estaMuerto)
        {
            iconoEstado.sprite = spriteCalavera;
            // Si está muerto, vacía su barra de habilidad
            int barrasHabilidadLength = barrasHabilidad.Length;
            if (posicionUI < barrasHabilidadLength && barrasHabilidad[posicionUI] != null)
                barrasHabilidad[posicionUI].value = 0f;
        }
        else if (siguiendo)
        {
            iconoEstado.sprite = spriteSeguimiento;
        }
        else
        {
            iconoEstado.sprite = spriteNormal;
        }
    }

    /// <summary>
    /// Actualiza el estado del indicador para un fantasma específico.
    /// </summary>
    /// <param name="numeroFantasma">Número identificador del fantasma.</param>
    /// <param name="muerto">Estado de muerte del fantasma.</param>
    public void ActualizarIndicadorFantasma(int numeroFantasma, bool muerto)
    {
        if (numeroFantasma < 1 || numeroFantasma > NUM_PERSONAJES || iconosEstadoFantasmas == null) return;

        int indiceUI = posicionesFantasma[numeroFantasma];

        // No mostrar calavera para el fantasma principal
        if (indiceUI == 0) return;

        // Obtener el estado de seguimiento actual
        bool siguiendo = false;
        if (!muerto && fantasmas.TryGetValue(numeroFantasma, out FantasmaBase fantasma) && fantasma != null)
        {
            ControladorIA controladorIA = fantasma.GetComponent<ControladorIA>();
            if (controladorIA != null)
            {
                siguiendo = controladorIA.EstaSiguiendoJugador();
            }
        }

        // Usar el método unificado para actualizar el icono
        ActualizarIconoEstado(indiceUI, muerto, siguiendo);
    }

    /// <summary>
    /// Crea el gradiente de color para las barras de vida (verde→amarillo→rojo).
    /// </summary>
    private void InicializarGradienteVida()
    {
        gradienteVida = new Gradient();

        GradientColorKey[] colores = new GradientColorKey[3];
        colores[0] = new GradientColorKey(Color.green, 0.7f);    // Verde para vida alta
        colores[1] = new GradientColorKey(Color.yellow, 0.3f);   // Amarillo para vida media
        colores[2] = new GradientColorKey(Color.red, 0.0f);      // Rojo para vida baja

        GradientAlphaKey[] alpha = new GradientAlphaKey[2];
        alpha[0] = new GradientAlphaKey(1f, 0f);
        alpha[1] = new GradientAlphaKey(1f, 1f);

        gradienteVida.SetKeys(colores, alpha);
    }

    /// <summary>
    /// Establece los valores iniciales para las barras de interfaz.
    /// </summary>
    private void InicializarBarras()
    {
        // Cachear longitudes para mejorar rendimiento
        int barrasVidaLength = barrasVida?.Length ?? 0;
        int barrasHabilidadLength = barrasHabilidad?.Length ?? 0;
        int barrasVidaFillLength = barrasVidaFill?.Length ?? 0;
        
        // Calcular una vez el color para 100% de vida
        Color colorVidaCompleta = gradienteVida.Evaluate(1f);
        
        for (int i = 0; i < NUM_PERSONAJES; i++)
        {
            if (i < barrasVidaLength && barrasVida[i] != null)
                barrasVida[i].value = 1f;

            if (i < barrasHabilidadLength && barrasHabilidad[i] != null)
                barrasHabilidad[i].value = 1f;

            if (i < barrasVidaFillLength && barrasVidaFill[i] != null)
                barrasVidaFill[i].color = colorVidaCompleta;
        }

        // Inicializar la barra de vida de Pacman (posición 5, índice 4)
        if (barrasVidaLength > 4 && barrasVida[4] != null)
        {
            barrasVida[4].value = 1f;

            if (barrasVidaFillLength > 4 && barrasVidaFill[4] != null)
                barrasVidaFill[4].color = colorVidaCompleta;
        }

        // Inicializar sliders de cooldown usando el método auxiliar
        const float valorInicial = 0f;
        
        ActualizarSliderCooldown(sliderCooldownCambioFantasma, valorInicial, 1f);

        // Inicializar los otros sliders de cooldown de la misma manera
        ActualizarSliderCooldown(sliderCooldownSOS, valorInicial, 1f);
        ActualizarSliderCooldown(sliderCapacidadRevivir, valorInicial, 1f);
    }

    /// <summary>
    /// Maneja el cambio de fantasma principal controlado por el jugador.
    /// </summary>
    /// <param name="nuevoFantasmaPrincipal">Número identificador del nuevo fantasma principal.</param>
    /// <remarks>
    /// Actualiza los mapeos de posición, guarda el cambio en PlayerPrefs y actualiza la interfaz.
    /// </remarks>
    public void CambiarFantasmaPrincipal(int nuevoFantasmaPrincipal)
    {
        // Verifica que el fantasma sea válido
        if (nuevoFantasmaPrincipal < 1 || nuevoFantasmaPrincipal > NUM_PERSONAJES) 
        {
            Debug.LogWarning($"InterfazJuego: Fantasma inválido para cambiar: {nuevoFantasmaPrincipal}");
            return;
        }

        // Actualiza el PlayerPrefs con el nuevo fantasma principal
        PlayerPrefs.SetInt("jugadorElegido", nuevoFantasmaPrincipal);
        PlayerPrefs.Save();

        // Si ya es el principal, solo actualiza la interfaz
        if (posicionesFantasma[nuevoFantasmaPrincipal] == 0)
        {
            ActualizarInterfazSegunPosiciones();
            return;
        }

        // Cálculo eficiente del intercambio
        int posicionNuevoPrincipal = posicionesFantasma[nuevoFantasmaPrincipal];
        int fantasmaAnteriorPrincipal = posicionesUI[0];

        // Intercambia las posiciones en la UI
        posicionesUI[0] = nuevoFantasmaPrincipal;
        posicionesUI[posicionNuevoPrincipal] = fantasmaAnteriorPrincipal;

        posicionesFantasma[nuevoFantasmaPrincipal] = 0;
        posicionesFantasma[fantasmaAnteriorPrincipal] = posicionNuevoPrincipal;

        // Actualiza la interfaz con las nuevas posiciones
        ActualizarInterfazSegunPosiciones();
    }

    /// <summary>
    /// Permite cambiar al fantasma en una posición específica de la UI.
    /// </summary>
    /// <param name="posicionUI">Posición en la interfaz del fantasma a controlar.</param>
    public void CambiarFantasmaPorPosicionUI(int posicionUI)
    {
        // Validar parámetro de entrada
        if (posicionUI < 0 || posicionUI >= posicionesUI.Length)
            return;
            
        // Obtiene el número del fantasma en esa posición
        int numeroFantasma = posicionesUI[posicionUI];
        
        // Verifica que el fantasma exista y esté activo
        if (!fantasmas.TryGetValue(numeroFantasma, out FantasmaBase fantasma) ||
            fantasma == null || fantasma.EstaInactivo()) 
            return;

        // Solicita el cambio de fantasma controlado
        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        if (sistemaGestion == null) 
            return;

        FantasmaBase fantasmaActual = sistemaGestion.GetFantasmaControlado();
        if (fantasmaActual != null)
            sistemaGestion.SolicitarCambioFantasma(fantasmaActual, numeroFantasma);
    }

    /// <summary>
    /// Devuelve el número identificador de un fantasma según su posición en la UI.
    /// </summary>
    /// <param name="posicionUI">Posición en la interfaz.</param>
    /// <returns>Número identificador del fantasma o -1 si la posición es inválida.</returns>
    public int ObtenerNumeroFantasmaDesdePosicionUI(int posicionUI)
    {
        // Validación para prevenir errores de índice
        if (posicionUI >= 0 && posicionUI < posicionesUI.Length)
            return posicionesUI[posicionUI];
        
        // Valor por defecto en caso de error
        Debug.LogWarning($"InterfazJuego: Posición UI inválida: {posicionUI}");
        return -1;
    }

    /// <summary>
    /// Actualiza los iconos de la interfaz según las posiciones actuales.
    /// </summary>
    private void ActualizarInterfazSegunPosiciones()
    {
        if (iconosPosicionesInterfaz == null || spritesIconosFantasma == null)
            return;
            
        // Cacheamos las longitudes para evitar múltiples accesos al array
        int iconosLength = iconosPosicionesInterfaz.Length;
        int spritesLength = spritesIconosFantasma.Length;
        
        // Actualiza los iconos de cada posición
        for (int posUI = 0; posUI < NUM_PERSONAJES && posUI < iconosLength; posUI++)
        {
            int numeroFantasma = posicionesUI[posUI];
            int indiceSprite = numeroFantasma - 1;

            if (indiceSprite >= 0 && indiceSprite < spritesLength &&
                iconosPosicionesInterfaz[posUI] != null)
            {
                iconosPosicionesInterfaz[posUI].sprite = spritesIconosFantasma[indiceSprite];
            }
        }

        // Actualiza los indicadores de fantasmas muertos
        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        if (sistemaGestion == null) return;

        for (int i = 1; i <= NUM_PERSONAJES; i++)
        {
            FantasmaBase fantasma = sistemaGestion.GetFantasma(i);
            bool estaMuerto = fantasma == null || fantasma.EstaInactivo();
            ActualizarIndicadorFantasma(i, estaMuerto);
        }
    }

    /// <summary>
    /// Inicializa los indicadores de estado de los fantasmas al inicio del juego.
    /// </summary>
    public void InicializarIndicadoresFantasmaMuerto()
    {
        if (iconosEstadoFantasmas == null) return;

        var sistemaGestion = SistemaGestionFantasmas.Instancia;
        if (!sistemaGestion) return;

        // Configura el estado inicial de cada indicador
        for (int i = 1; i <= NUM_PERSONAJES; i++)
        {
            if (posicionesFantasma[i] == 0) continue; // Omitir el fantasma principal

            FantasmaBase fantasma = sistemaGestion.GetFantasma(i);
            bool estaMuerto = fantasma == null || fantasma.EstaInactivo();
            bool siguiendo = false;

            // Verificar si está siguiendo (solo si está vivo)
            if (!estaMuerto && fantasma != null)
            {
                ControladorIA controladorIA = fantasma.GetComponent<ControladorIA>();
                if (controladorIA != null)
                {
                    siguiendo = controladorIA.EstaSiguiendoJugador();
                }
            }

            // Establecer el estado inicial del icono
            ActualizarIconoEstado(posicionesFantasma[i], estaMuerto, siguiendo);
        }
    }

    /// <summary>
    /// Actualiza el icono de seguimiento para un fantasma específico.
    /// </summary>
    /// <param name="numeroFantasma">Número identificador del fantasma.</param>
    /// <param name="activado">Estado de seguimiento (true si está siguiendo).</param>
    public void ActualizarIconoSeguimiento(int numeroFantasma, bool activado)
    {
        if (numeroFantasma < 1 || numeroFantasma >= posicionesFantasma.Length)
            return;
            
        int posicionUI = posicionesFantasma[numeroFantasma];
        
        // No actualizar el icono del fantasma principal (posición 0)
        if (posicionUI == 0) return;

        // Verifica si el fantasma está muerto (prioridad sobre seguimiento)
        bool estaMuerto = false;
        if (fantasmas.TryGetValue(numeroFantasma, out FantasmaBase fantasma))
        {
            estaMuerto = fantasma == null || fantasma.EstaInactivo();
        }

        // Si está muerto, mantenemos el icono de calavera (prioridad)
        if (estaMuerto) return;

        // Usar el método unificado para actualizar el icono
        ActualizarIconoEstado(posicionUI, false, activado);
    }
}