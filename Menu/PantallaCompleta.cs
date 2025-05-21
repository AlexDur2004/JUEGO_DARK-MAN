using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona las opciones de pantalla completa y resolución del juego.
/// </summary>
/// <remarks>
/// Esta clase permite al usuario seleccionar entre diferentes resoluciones
/// y activar/desactivar el modo de pantalla completa.
/// </remarks>
public class PantallaCompleta : MonoBehaviour
{
    /// <summary>
    /// Toggle para activar o desactivar el modo de pantalla completa.
    /// </summary>
    public Toggle toggle;
    
    /// <summary>
    /// Dropdown con las resoluciones disponibles para el dispositivo.
    /// </summary>
    public TMP_Dropdown resolucionesDisp; // texto de resoluciones disponibles
    
    /// <summary>
    /// Array con las resoluciones disponibles para el dispositivo.
    /// </summary>
    Resolution[] resoluciones;

    /// <summary>
    /// Se ejecuta al iniciar el componente.
    /// Configura el estado inicial del toggle según la configuración actual
    /// y carga las resoluciones disponibles.
    /// </summary>
    void Start()
    {
        if (Screen.fullScreen)
        {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }

        RevisarResolucion();
    }

    /// <summary>
    /// Activa o desactiva el modo de pantalla completa.
    /// </summary>
    /// <param name="fullscreen">Estado de pantalla completa a establecer.</param>
    /// <remarks>
    /// Cuando se establece a <c>true</c>, la aplicación se mostrará en modo pantalla completa.
    /// Cuando es <c>false</c>, se mostrará en modo ventana.
    /// </remarks>
    public void ActivarPantallaCompleta(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    /// <summary>
    /// Carga la lista de resoluciones disponibles en el dropdown.
    /// </summary>
    /// <remarks>
    /// Obtiene todas las resoluciones soportadas por el dispositivo,
    /// las muestra en el dropdown y selecciona la resolución actual o
    /// la última seleccionada por el usuario.
    /// </remarks>
    public void RevisarResolucion()
    {
        resoluciones = Screen.resolutions;
        resolucionesDisp.ClearOptions();
        List<string> opciones = new List<string>();
        int resolucionActual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            opciones.Add(opcion);

            if (Screen.fullScreen && resoluciones[i].width == Screen.currentResolution.width
                && resoluciones[i].height == Screen.currentResolution.height)
            {
                resolucionActual = i;
            }

        }

        resolucionesDisp.AddOptions(opciones);
        resolucionesDisp.value = resolucionActual;
        resolucionesDisp.RefreshShownValue();

        resolucionesDisp.value = PlayerPrefs.GetInt("numeroResolucion", 0);
    }

    /// <summary>
    /// Cambia la resolución de la pantalla según la selección del usuario.
    /// </summary>
    /// <param name="indiceResolucion">Índice de la resolución seleccionada en el dropdown.</param>
    /// <remarks>
    /// Guarda la preferencia del usuario en PlayerPrefs y aplica la nueva resolución.
    /// </remarks>
    public void CambiarResolucion(int indiceResolucion)
    {
        PlayerPrefs.SetInt("numeroResolucion", resolucionesDisp.value);
        Resolution resolucion = resoluciones[indiceResolucion];
        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);
    }
}
