using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el comportamiento del menú de pausa durante el juego.
/// </summary>
/// <remarks>
/// Esta clase permite al jugador pausar el juego presionando la tecla Escape,
/// mostrando un menú con opciones para reanudar, volver al menú principal o salir del juego.
/// </remarks>
public class Pausa : MonoBehaviour
{
    /// <summary>
    /// Referencia al objeto GameObject del menú de pausa.
    /// </summary>
    public GameObject menuPausa;
    
    /// <summary>
    /// Indica si el juego está en pausa.
    /// </summary>
    /// <value><c>true</c> si el juego está en pausa; de lo contrario, <c>false</c>.</value>
    public static bool pausa = false;

    /// <summary>
    /// Se ejecuta una vez por cada frame.
    /// Detecta si se ha presionado la tecla Escape para pausar el juego.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pausa)
            {
                menuPausa.SetActive(true);
                pausa = true;

                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    /// <summary>
    /// Reanuda el juego después de estar en pausa.
    /// </summary>
    /// <remarks>
    /// Reproduce un sonido de clic, desactiva el menú de pausa,
    /// restaura la escala de tiempo normal y oculta el cursor.
    /// </remarks>
    public void Reanudar()
    {
        Utilidades.AudioManager.Instancia.ReproducirSonidoGlobal(Utilidades.AudioManager.TipoSonido.Click);

        menuPausa.SetActive(false);
        pausa = false;

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Vuelve al menú principal del juego.
    /// </summary>
    /// <remarks>
    /// Reproduce un sonido de clic y carga la escena "menu_ppal".
    /// La limpieza de datos se maneja en el Start() del menú principal.
    /// </remarks>
    public void MenuPpal()
    {
        // Restaurar la escala de tiempo normal antes de salir
        Time.timeScale = 1f;

        Utilidades.AudioManager.Instancia.ReproducirSonidoGlobal(Utilidades.AudioManager.TipoSonido.Click);

        // Cargar la escena del menú principal
        TransicionEscena.Instancia.CargarEscena("menu_ppal");
    }

    /// <summary>
    /// Cierra la aplicación.
    /// </summary>
    /// <remarks>
    /// Reproduce un sonido de clic y sale del juego.
    /// </remarks>
    public void Salir()
    {
        Utilidades.AudioManager.Instancia.ReproducirSonidoGlobal(Utilidades.AudioManager.TipoSonido.Click);
        Application.Quit();
    }
}
