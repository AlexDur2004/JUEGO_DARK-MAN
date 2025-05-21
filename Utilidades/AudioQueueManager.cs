/// <summary>
/// Espacio de nombres para utilidades generales del juego.
/// </summary>
namespace Utilidades
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.UI;

    /// <summary>
    /// Gestor de cola de audio que reproduce consejos automáticamente y clips de habilidades bajo demanda.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa el patrón Singleton para garantizar una única instancia en todo el juego.
    /// Se encarga de organizar y reproducir clips de audio con visualización opcional en UI.
    /// </remarks>
    public class AudioQueueManager : Singleton<AudioQueueManager>
    {
        /// <summary>
        /// Colección de clips de audio que corresponden a habilidades del juego.
        /// </summary>
        [Header("Clips de habilidades")]
        public AudioClip[] clipsHabilidades;

        /// <summary>
        /// Colección de clips de audio que contienen consejos para el jugador.
        /// </summary>
        [Header("Clips de consejos")]
        public AudioClip[] clipsConsejos;

        /// <summary>
        /// Tiempo de espera entre cada consejo reproducido automáticamente, en segundos.
        /// </summary>
        [Header("Configuración")]
        public float tiempoEntreConsejos = 20f;

        /// <summary>
        /// Pausa entre cada mensaje de audio en segundos.
        /// </summary>
        /// <remarks>
        /// Define el tiempo de espera después de que termine un mensaje y antes de iniciar el siguiente.
        /// Esto da tiempo al jugador para procesar la información antes de escuchar el siguiente clip.
        /// </remarks>
        [SerializeField] private float pausaEntreMensajes = 1f;

        /// <summary>
        /// Referencia al objeto de UI que contiene el modelo 3D o representación visual del bot.
        /// </summary>
        /// <remarks>
        /// Este objeto se activa durante la reproducción de audio y se desactiva al finalizar.
        /// </remarks>
        [Header("UI Bot")]
        public GameObject botUIRoot;

        /// <summary>
        /// Componente AudioSource utilizado para la reproducción de clips.
        /// </summary>
        private AudioSource audioSource;
        
        /// <summary>
        /// Cola que almacena los clips de audio pendientes de reproducción.
        /// </summary>
        private Queue<AudioClip> colaClips = new Queue<AudioClip>();
        
        /// <summary>
        /// Índice del consejo actual en el array clipsConsejos.
        /// </summary>
        /// <remarks>
        /// Se inicializa con -1 para indicar que aún no se ha reproducido ningún consejo.
        /// </remarks>
        private int indiceConsejoActual = -1;

        /// <summary>
        /// Inicializa el componente AudioSource y configura el estado inicial de la UI.
        /// </summary>
        /// <remarks>
        /// Este método sobrescribe el método Awake de la clase base Singleton.
        /// Garantiza que exista un AudioSource y que el modelo 3D de la UI esté oculto al inicio.
        /// </remarks>
        protected override void Awake()
        {
            base.Awake();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Asegúrate de que el modelo 3D de la UI esté oculto al inicio
            if (botUIRoot != null)
                botUIRoot.SetActive(false);
        }

        /// <summary>
        /// Inicia las corrutinas para la reproducción de consejos y gestión de cola de audio.
        /// </summary>
        private void Start()
        {
            StartCoroutine(ReproducirConsejosAleatorios());
            StartCoroutine(GestionarCola());
        }

        /// <summary>
        /// Encola un clip de audio correspondiente a una habilidad para su reproducción.
        /// </summary>
        /// <param name="indice">Índice del clip de audio en el array clipsHabilidades.</param>
        /// <remarks>
        /// Este método verifica que el índice proporcionado sea válido antes de encolar el clip.
        /// Los clips encolados se reproducirán en el orden en que fueron agregados.
        /// </remarks>
        public void ReproducirHabilidad(int indice)
        {
            if (indice >= 0 && indice < clipsHabilidades.Length)
                colaClips.Enqueue(clipsHabilidades[indice]);
        }

        /// <summary>
        /// Corrutina que encola consejos para su reproducción de forma periódica.
        /// </summary>
        /// <returns>Un enumerador que permite la ejecución pausada de la corrutina.</returns>
        /// <remarks>
        /// La corrutina selecciona un consejo aleatorio la primera vez y luego continúa
        /// reproduciendo consejos de forma secuencial y cíclica con intervalos definidos.
        /// Los consejos se reproducen cada <see cref="tiempoEntreConsejos"/> segundos.
        /// </remarks>
        private IEnumerator ReproducirConsejosAleatorios()
        {
            // Si hay consejos disponibles, inicializa con un índice aleatorio
            if (clipsConsejos.Length > 0)
            {
                // Selecciona un índice inicial aleatorio solo la primera vez
                if (indiceConsejoActual == -1)
                    indiceConsejoActual = Random.Range(0, clipsConsejos.Length);
            }
            
            while (true)
            {
                yield return new WaitForSeconds(tiempoEntreConsejos);
                
                if (clipsConsejos.Length > 0)
                {
                    // Encola el consejo actual
                    colaClips.Enqueue(clipsConsejos[indiceConsejoActual]);
                    
                    // Incrementa el índice para el siguiente consejo (de forma circular)
                    indiceConsejoActual = (indiceConsejoActual + 1) % clipsConsejos.Length;
                }
            }
        }

        /// <summary>
        /// Corrutina que gestiona la reproducción secuencial de los clips de audio en la cola.
        /// </summary>
        /// <returns>Un enumerador que permite la ejecución pausada de la corrutina.</returns>
        /// <remarks>
        /// Esta corrutina verifica si hay clips pendientes en la cola y los reproduce uno tras otro.
        /// Durante la reproducción, activa el elemento visual de UI y lo desactiva al terminar.
        /// Espera a que termine la reproducción del clip actual antes de continuar con el siguiente.
        /// </remarks>
        private IEnumerator GestionarCola()
        {
            while (true)
            {
                // Usa el botUIRoot activo como indicador de reproducción en curso
                bool estaReproduciendo = botUIRoot != null && botUIRoot.activeSelf;
                
                if (!estaReproduciendo && colaClips.Count > 0)
                {
                    AudioClip clip = colaClips.Dequeue();

                    // Mostrar el modelo 3D en la UI
                    if (botUIRoot != null)
                        botUIRoot.SetActive(true);

                    audioSource.clip = clip;
                    audioSource.Play();
                    yield return new WaitForSeconds(clip.length);

                    // Pausa entre mensajes
                    yield return new WaitForSeconds(pausaEntreMensajes);

                    // Ocultar el modelo 3D en la UI
                    if (botUIRoot != null)
                        botUIRoot.SetActive(false);
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}