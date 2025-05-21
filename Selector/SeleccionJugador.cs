using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controla la interfaz de selección de personaje, permitiendo al usuario navegar
/// entre los diferentes personajes disponibles antes de comenzar el juego.
/// </summary>
/// <remarks>
/// Este componente gestiona la visualización de los personajes, sus descripciones
/// y la navegación entre ellos, así como la transición a la escena de juego
/// una vez que el usuario ha realizado su selección.
/// </remarks>
public class SeleccionJugador : MonoBehaviour
{
    /// <summary>
    /// Array de GameObjects que representan los modelos de los personajes seleccionables.
    /// </summary>
    public GameObject[] jugadores;
    
    /// <summary>
    /// Array de archivos de texto con las descripciones de cada personaje.
    /// </summary>
    public TextAsset[] descripciones;
    
    /// <summary>
    /// Componente de texto donde se mostrará la descripción del personaje seleccionado.
    /// </summary>
    public TextMeshProUGUI textoDescripcion;
    
    /// <summary>
    /// Fuente de audio para reproducir efectos sonoros.
    /// </summary>
    [SerializeField] private AudioSource audioSource;
    
    /// <summary>
    /// Clip de audio que se reproducirá al hacer clic en los botones.
    /// </summary>
    [SerializeField] private AudioClip audioClick;

    /// <summary>
    /// Índice del personaje actualmente seleccionado en el array de jugadores.
    /// </summary>
    public int jugadorElegido = 0;

    /// <summary>
    /// Inicializa el selector mostrando la descripción del personaje inicial.
    /// </summary>
    void Start()
    {
        ActualizarDescripcion();
    }

    /// <summary>
    /// Cambia al siguiente personaje en la lista de selección.
    /// </summary>
    /// <remarks>
    /// Oculta el personaje actual, incrementa el índice (con ciclo) y muestra el siguiente.
    /// Actualiza también la descripción correspondiente al nuevo personaje seleccionado.
    /// </remarks>
    public void JugadorSiguiente()
    {
        audioSource.PlayOneShot(audioClick);
        jugadores[jugadorElegido].SetActive(false);
        jugadorElegido = (jugadorElegido + 1) % jugadores.Length;
        jugadores[jugadorElegido].SetActive(true);
        ActualizarDescripcion();
    }

    /// <summary>
    /// Cambia al personaje anterior en la lista de selección.
    /// </summary>
    /// <remarks>
    /// Oculta el personaje actual, decrementa el índice (con ciclo) y muestra el anterior.
    /// Actualiza también la descripción correspondiente al nuevo personaje seleccionado.
    /// </remarks>
    public void JugadorAnterior()
    {
        audioSource.PlayOneShot(audioClick);
        jugadores[jugadorElegido].SetActive(false);
        jugadorElegido--;
        if (jugadorElegido < 0)
        {
            jugadorElegido += jugadores.Length;
        }
        jugadores[jugadorElegido].SetActive(true);
        ActualizarDescripcion();
    }

    /// <summary>
    /// Inicia el juego con el personaje seleccionado.
    /// </summary>
    /// <remarks>
    /// Guarda el índice del personaje elegido en PlayerPrefs y carga la escena de juego.
    /// </remarks>
    public void StartGame()
    {
        audioSource.PlayOneShot(audioClick);
        PlayerPrefs.SetInt("jugadorElegido", jugadorElegido + 1);
        TransicionEscena.Instancia.CargarEscena("Juego");
    }

    /// <summary>
    /// Actualiza el texto de descripción con la información del personaje seleccionado.
    /// </summary>
    void ActualizarDescripcion()
    {
        if (textoDescripcion != null && jugadorElegido < descripciones.Length)
        {
            textoDescripcion.text = descripciones[jugadorElegido].text;
        }
    }
    
    /// <summary>
    /// Reproduce el sonido de clic.
    /// </summary>
    /// <remarks>
    /// Método útil para ser llamado desde botones de la interfaz.
    /// </remarks>
    public void ReproducirAudioClick()
    {
        audioSource.PlayOneShot(audioClick);
    }
}
