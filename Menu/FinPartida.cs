using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la pantalla de fin de partida mostrando el resultado de victoria o derrota.
/// </summary>
/// <remarks>
/// Esta clase configura la interfaz de usuario y los efectos de sonido
/// en función del resultado de la partida (victoria o derrota).
/// </remarks>
public class FinPartida : MonoBehaviour
{
    /// <summary>
    /// Imagen de fondo para la pantalla de fin de partida.
    /// </summary>
    public Image fondoImagen;

    /// <summary>
    /// Sprite a mostrar cuando el jugador gana.
    /// </summary>
    public Sprite fondoVictoria;

    /// <summary>
    /// Sprite a mostrar cuando el jugador pierde.
    /// </summary>
    public Sprite fondoDerrota;

    /// <summary>
    /// Texto que mostrará "VICTORIA!!!" o "DERROTA...".
    /// </summary>
    public TextMeshProUGUI resultadoTexto;

    /// <summary>
    /// Configuración de audio para la pantalla de fin de partida.
    /// </summary>
    [Header("Audio")]
    /// <summary>
    /// Clip de audio que se reproduce cuando el jugador gana.
    /// </summary>
    public AudioClip sonidoVictoria;   // Clip de audio para victoria

    /// <summary>
    /// Clip de audio que se reproduce cuando el jugador pierde.
    /// </summary>
    public AudioClip sonidoDerrota;    // Clip de audio para derrota

    /// <summary>
    /// Clip de audio que se reproduce al hacer clic en botones.
    /// </summary>
    public AudioClip sonidoClick;       // Clip de audio para el menú principal

    /// <summary>
    /// Componente para reproducir los efectos de sonido.
    /// </summary>
    private AudioSource audioSource;    // Componente AudioSource    

    /// <summary>
    /// Se ejecuta al iniciar el componente.
    /// Configura la interfaz y reproduce el sonido según el resultado de la partida.
    /// </summary>
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Inicializar el componente AudioSource
        audioSource = GetComponent<AudioSource>();

        bool esVictoria = PlayerPrefs.GetInt("esVictoria", 0) == 1;
        PlayerPrefs.DeleteKey("esVictoria");
        PlayerPrefs.Save();

        if (esVictoria)
        {
            // Configurar UI para victoria
            fondoImagen.sprite = fondoVictoria;
            resultadoTexto.text = "VICTORIA!!!";

            // Reproducir sonido de victoria
            if (sonidoVictoria != null)
            {
                audioSource.PlayOneShot(sonidoVictoria);
            }
        }
        else
        {
            // Configurar UI para derrota
            fondoImagen.sprite = fondoDerrota;
            resultadoTexto.text = "DERROTA...";

            // Reproducir sonido de derrota
            if (sonidoDerrota != null)
            {
                audioSource.PlayOneShot(sonidoDerrota);
            }
        }
    }


    /// <summary>
    /// Carga el menú principal del juego.
    /// </summary>
    /// <remarks>
    /// Reproduce un sonido de clic y utiliza la clase TransicionEscena para cargar la escena "menu_ppal".
    /// </remarks>
    public void MenuPpal()
    {
        audioSource.PlayOneShot(sonidoClick);
        TransicionEscena.Instancia.CargarEscena("menu_ppal");
    }

    /// <summary>
    /// Cierra la aplicación.
    /// </summary>
    /// <remarks>
    /// Reproduce un sonido de clic y termina la ejecución del juego.
    /// Este método solo funciona en builds compiladas, no en el editor de Unity.
    /// </remarks>
    public void Salir()
    {
        audioSource.PlayOneShot(sonidoClick);
        Application.Quit();
    }
}
