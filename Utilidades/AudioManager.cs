using UnityEngine;
using System.Collections.Generic;

namespace Utilidades
{
    /// <summary>
    /// Sistema central de gestión de audio del juego, implementado como Singleton.
    /// Gestiona todos los sonidos del juego y los distribuye a los personajes.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        /// <summary>
        /// Tipos de efectos de sonido disponibles en el juego.
        /// Cada tipo corresponde a un clip de audio específico.
        /// </summary>
        public enum TipoSonido
        {
            Golpe,          // Sonido de golpe
            Ayuda,          // Sonido cuando un fantasma solicita ayuda
            Revivir,        // Sonido al revivir un fantasma
            CambioPersonaje, // Sonido al cambiar entre fantasmas
            Habilidad,       // Sonido al poseer un personaje
            Premio,         // Sonido al recoger una fruta
            Paso_ronda,      // Sonido al avanzar a la siguiente ronda
            Ataque,          // Sonido de atacar
            Muerte,         // Sonido al morir
            Tecleo,         // Sonido de tecleo para texto animado
            Click           // Sonido de clic en un botón
        }

        /// <summary>
        /// Configuración de sonido que contiene el tipo, clip de audio y su volumen asociado
        /// </summary>
        [System.Serializable]
        public class SonidoConfig
        {
            /// <summary>
            /// Tipo de sonido que representa esta configuración.
            /// </summary>
            public TipoSonido tipo;

            /// <summary>
            /// Clip de audio que se reproducirá para este tipo de sonido.
            /// </summary>
            public AudioClip clip;

            /// <summary>
            /// Volumen al que se reproducirá este sonido (rango de 0 a 1).
            /// </summary>
            [Range(0f, 1f)] public float volumen = 1f;

            /// <summary>
            /// Constructor que permite crear una configuración de sonido con un clip y volumen específicos.
            /// </summary>
            /// <param name="clip">El clip de audio para este sonido</param>
            /// <param name="volumen">El volumen para reproducir el clip</param>
            public SonidoConfig(AudioClip clip, float volumen)
            {
                this.clip = clip;
                this.volumen = volumen;
            }

            /// <summary>
            /// Constructor predeterminado sin parámetros.
            /// </summary>
            public SonidoConfig() { }
        }

        [Header("Configuración de Sonidos")]
        /// <summary>
        /// Colección de sonidos disponibles con sus configuraciones.
        /// </summary>
        [SerializeField] private SonidoConfig[] sonidosDisponibles;

        [Header("Configuración de Audio")]
        /// <summary>
        /// Fuente de audio para reproducir sonidos globales que no están asociados a un objeto específico.
        /// </summary>
        [SerializeField] private AudioSource audioSourceGlobal;

        /// <summary>
        /// Caché de configuraciones de sonido para acceso rápido mediante el tipo.
        /// </summary>
        private Dictionary<TipoSonido, SonidoConfig> cacheSonidos = new Dictionary<TipoSonido, SonidoConfig>();/// <summary>
                                                                                                               /// Inicializa el sistema de audio y prepara el diccionario para búsqueda rápida.
                                                                                                               /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Inicializar la caché de sonidos para acceso más rápido
            foreach (var sonido in sonidosDisponibles)
            {
                cacheSonidos[sonido.tipo] = sonido;
            }
        }

        /// <summary>
        /// Reproduce un sonido globalmente, sin asociarlo a un personaje específico.
        /// Útil para sonidos ambientales o de interfaz.
        /// </summary>
        /// <param name="tipo">Tipo de sonido a reproducir</param>
        public void ReproducirSonidoGlobal(TipoSonido tipo)
        {
            SonidoConfig config = ObtenerSonidoConfig(tipo);
            if (config.clip != null)
            {
                audioSourceGlobal.clip = config.clip;
                audioSourceGlobal.volume = config.volumen;
                audioSourceGlobal.Play();
            }
        }

        /// <summary>
        /// Obtiene la configuración de sonido para un tipo específico.
        /// </summary>
        /// <param name="tipo">Tipo de sonido cuya configuración se desea obtener</param>
        /// <returns>Configuración del sonido, o una configuración predeterminada si no existe</returns>
        public SonidoConfig ObtenerSonidoConfig(TipoSonido tipo)
        {
            if (cacheSonidos.TryGetValue(tipo, out SonidoConfig config))
                return config;

            return new SonidoConfig(null, 1f); // Valores por defecto
        }
    }
}
