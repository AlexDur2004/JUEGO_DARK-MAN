using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla las funciones básicas del menú principal del juego.
/// </summary>
/// <remarks>
/// Esta clase proporciona métodos para iniciar el juego y salir de la aplicación.
/// </remarks>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Carga la escena del selector de niveles o personajes.
    /// </summary>
    /// <remarks>
    /// Utiliza la clase TransicionEscena para cargar la escena "selector".
    /// </remarks>
    public void Jugar()
    {
        TransicionEscena.Instancia.CargarEscena("selector");
    }


    /// <summary>
    /// Cierra la aplicación.
    /// </summary>
    /// <remarks>
    /// Este método solo funciona en builds compiladas, no en el editor de Unity.
    /// </remarks>
    public void Salir()
    {
        Application.Quit();
    }
}