using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Implementación de habilidades específicas para el personaje Pacman (DarkMan).
/// </summary>
/// <remarks>
/// Esta clase gestiona un conjunto de 4 habilidades diferentes que el jugador puede usar:
/// - Clon: Crea un clon de Pacman que distrae a los fantasmas
/// - Aturdimiento: Aturde a todos los fantasmas cercanos durante un tiempo
/// - Teletransporte: Mueve al jugador una distancia hacia adelante
/// - Blindado: Activa la invencibilidad temporal del jugador
/// 
/// Las habilidades se seleccionan aleatoriamente al activarse.
/// </remarks>
public class HabilidadesPacman : HabilidadBase
{
    [Header("Configuración de Habilidades Pacman")]
    /// <summary>
    /// Prefab utilizado para crear el clon de Pacman.
    /// </summary>
    [SerializeField] private GameObject prefabClon;
    
    /// <summary>
    /// Radio en unidades del área de efecto del aturdimiento.
    /// </summary>
    [SerializeField] private float radioAturdimiento = 5f;
    
    /// <summary>
    /// Distancia máxima que el jugador puede teletransportarse en unidades.
    /// </summary>
    [SerializeField] private float distanciaTP = 10f;
    
    /// <summary>
    /// Referencia al componente Pacman del jugador.
    /// </summary>
    private Pacman pacman;
    
    /// <summary>
    /// Indica qué habilidad está actualmente seleccionada.
    /// </summary>
    /// <remarks>
    /// Los valores representan: 0=clon, 1=aturdimiento, 2=tp, 3=blindado, -1=ninguna
    /// </remarks>
    private int habilidadSeleccionada = -1;    /// <summary>
    /// Inicializa las referencias y configuración específica de Pacman.
    /// </summary>
    /// <remarks>
    /// Este método obtiene la referencia al componente Pacman y
    /// desactiva la activación por tecla que viene de HabilidadBase,
    /// ya que las habilidades se activan mediante otros mecanismos.
    /// </remarks>
    protected override void Start()
    {
        base.Start();
        pacman = GetComponent<Pacman>();
        teclaHabilidad = KeyCode.None; // Desactiva la activación por tecla que viene de HabilidadBase
    }

    /// <summary>
    /// Implementa el método abstracto de HabilidadBase.
    /// </summary>
    /// <remarks>
    /// Selecciona una habilidad aleatoria entre las 4 disponibles y la activa.
    /// </remarks>
    public override void AplicarEfectoHabilidad()
    {
        habilidadSeleccionada = UnityEngine.Random.Range(0, 4);
        ActivarHabilidad();
    }

    /// <summary>
    /// Implementa el método abstracto de HabilidadBase.
    /// </summary>
    /// <remarks>
    /// No es necesario realizar acciones específicas para remover efectos,
    /// ya que el estado de las habilidades se controla mediante 
    /// habilidadSeleccionada y tiempoTranscurrido.
    /// </remarks>
    public override void RemoverEfectoHabilidad()
    {
        // No es necesario hacer nada aquí, ya que el estado de las habilidades
        // se controla mediante habilidadSeleccionada y tiempoTranscurrido
    }    /// <summary>
    /// Sobrescribe la corrutina base para manejar el ciclo de vida de las habilidades específicas de Pacman.
    /// </summary>
    /// <remarks>
    /// Esta implementación personalizada:
    /// 1. Activa la habilidad aleatoria seleccionada
    /// 2. Mantiene el efecto durante la duración configurada
    /// 3. Limpia los efectos al terminar
    /// 4. Gestiona el período de cooldown
    /// </remarks>
    /// <returns>Un enumerador para la corrutina.</returns>
    protected override IEnumerator GestionarHabilidadCoroutine()
    {
        tiempoTranscurrido = 0.1f; // Iniciamos con un valor mayor que 0 para activar la habilidad

        // Ejecuta la habilidad específica según el índice seleccionado
        switch (habilidadSeleccionada)
        {
            case 0: EjecutarHabilidadClon(); break;
            case 1: EjecutarHabilidadAturdimiento(); break;
            case 2: EjecutarHabilidadTP(); break;
            case 3: EjecutarHabilidadBlindado(); break;
            default:
                yield break; // Salimos si la habilidad no es válida
        }

        /// Mantiene la habilidad activa durante la duración configurada
        yield return new WaitForSeconds(duracionBase);

        /// Limpia los efectos al terminar y reinicia los estados
        RemoverEfectoHabilidad();
        tiempoTranscurrido = 0f;
        corrutinaHabilidadActual = null;
        habilidadSeleccionada = -1;

        /// Inicia el período de enfriamiento (cooldown) y espera hasta que termine
        tiempoRestante = cooldownBase;
        while (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            yield return null;
        }
        tiempoRestante = 0f;
    }

    /// <summary>
    /// Implementaciones específicas de cada habilidad
    /// </summary>

    /// <summary>
    /// Crea un clon de Pacman que distrae a los fantasmas.
    /// </summary>
    /// <remarks>
    /// Esta habilidad instancia un prefab del clon en la posición actual del jugador
    /// y lo inicializa con la duración base configurada. El clon actúa como una
    /// distracción para los enemigos.
    /// </remarks>
    private void EjecutarHabilidadClon()
    {
        if (prefabClon != null)
        {
            // Instancia e inicializa el clon con la duración configurada
            GameObject clonObj = Instantiate(prefabClon, transform.position, transform.rotation);
            ClonPacman clon = clonObj.GetComponent<ClonPacman>();
            if (clon != null) clon.Inicializar(duracionBase);
        }
    }

    /// <summary>
    /// Aturde a todos los fantasmas dentro de un radio específico.
    /// </summary>
    /// <remarks>
    /// Esta habilidad utiliza Physics.OverlapSphere para encontrar todos los objetos
    /// con la etiqueta "Fantasma" dentro del radio configurado, y llama al método
    /// Aturdir() de cada uno con la duración base. 
    /// 
    /// La variable fantasmasAturdidos lleva la cuenta de los fantasmas afectados,
    /// lo cual podría usarse para estadísticas o efectos audiovisuales adicionales.
    /// </remarks>
    private void EjecutarHabilidadAturdimiento()
    {
        int fantasmasAturdidos = 0;

        foreach (Collider fantasma in Physics.OverlapSphere(transform.position, radioAturdimiento))
        {
            if (fantasma.CompareTag("Fantasma"))
            {
                FantasmaBase fantasmaBase = fantasma.GetComponent<FantasmaBase>();
                if (fantasmaBase != null)
                {
                    fantasmaBase.Aturdir(duracionBase);
                    fantasmasAturdidos++;
                }
            }
        }
    }

    /// <summary>
    /// Teletransporta a Pacman hacia adelante una distancia determinada.
    /// </summary>
    /// <remarks>
    /// Esta habilidad mueve instantáneamente al jugador en la dirección que está mirando.
    /// Utiliza un raycast para detectar obstáculos:
    /// - Si encuentra un obstáculo, posiciona al jugador justo antes de este
    /// - Si no encuentra obstáculos, lo mueve la distancia máxima configurada
    /// 
    /// El offset de 0.5f evita que el jugador quede parcialmente dentro de superficies.
    /// </remarks>
    private void EjecutarHabilidadTP()
    {
        Vector3 posicionOriginal = transform.position;
        Vector3 direccion = transform.forward;

        // Verifica si hay obstáculos en la dirección del teletransporte
        transform.position = Physics.Raycast(posicionOriginal, direccion, out RaycastHit hit, distanciaTP)
            ? hit.point - direccion * 0.5f // Si hay obstáculo, se posiciona justo antes
            : posicionOriginal + direccion * distanciaTP; // Si no hay obstáculo, distancia completa
    }

    /// <summary>
    /// Activa el blindaje/invencibilidad en el personaje.
    /// </summary>
    /// <remarks>
    /// Esta habilidad hace que el personaje sea temporalmente invulnerable
    /// a los ataques de los fantasmas. Utiliza la función ActivarInvencibilidad
    /// del componente Pacman para aplicar este efecto durante la duración base.
    /// </remarks>
    private void EjecutarHabilidadBlindado()
    {
        if (pacman != null)
        {
            // Activa la invencibilidad durante la duración de la habilidad
            pacman.ActivarInvencibilidad(duracionBase);
        }
    }

    /// <summary>
    /// Devuelve el tiempo de enfriamiento para la interfaz de usuario.
    /// </summary>
    /// <returns>El valor de cooldown configurado en segundos.</returns>
    public float GetTiempoEnfriamiento() => cooldownBase;
    
    /// <summary>
    /// Devuelve la duración de la habilidad para la interfaz de usuario.
    /// </summary>
    /// <returns>El valor de duración configurado en segundos.</returns>
    public float GetDuracionHabilidad() => duracionBase;
}