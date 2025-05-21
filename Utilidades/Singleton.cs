using UnityEngine;

namespace Utilidades
{
    /// <summary>
    /// Implementación genérica del patrón Singleton para componentes MonoBehaviour.
    /// Garantiza que solo exista una instancia de una clase en el juego.
    /// </summary>
    /// <typeparam name="T">El tipo de componente que implementará el patrón Singleton.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// Propiedad estática para acceder a la única instancia desde cualquier parte del código.
        /// </summary>
        public static T Instancia { get; protected set; }

        /// <summary>
        /// Se llama automáticamente cuando el objeto se inicializa.
        /// Configura la instancia única y maneja los duplicados.
        /// </summary>
        protected virtual void Awake()
        {
            // Si no existe ninguna instancia, esta se convierte en la instancia única
            if (Instancia == null)
            {
                Instancia = this as T;
                DontDestroyOnLoad(gameObject); // Mantiene el objeto al cambiar de escena
            }
            // Si ya existe una instancia, destruye este objeto duplicado
            else if (Instancia != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Se llama automáticamente cuando el objeto se destruye.
        /// Limpia la referencia a la instancia si es necesario.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Solo limpia la referencia si se destruye la instancia activa
            if (Instancia == this)
                Instancia = null;
        }
    }
}