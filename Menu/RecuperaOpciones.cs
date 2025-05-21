using UnityEngine;

/// <summary>
/// Gestiona la referencia al panel de opciones y permite mostrarlo cuando sea necesario.
/// </summary>
/// <remarks>
/// Esta clase busca el objeto de opciones en la escena y proporciona métodos
/// para interactuar con él desde otros componentes.
/// </remarks>
public class RecuperaOpciones : MonoBehaviour
{
    /// <summary>
    /// Referencia al controlador de opciones del juego.
    /// </summary>
    public ControladorOpciones panelOpciones;

    /// <summary>
    /// Se ejecuta al iniciar el componente.
    /// Busca y almacena una referencia al controlador de opciones en la escena.
    /// </summary>
    void Start()
    {
        panelOpciones = GameObject.FindGameObjectWithTag("Opciones").GetComponent<ControladorOpciones>();    
    }

    /// <summary>
    /// Se ejecuta una vez por cada frame.
    /// </summary>
    void Update()
    {
        
    }

    /// <summary>
    /// Muestra el panel de opciones en la pantalla.
    /// </summary>
    /// <remarks>
    /// Activa el objeto GameObject que contiene la interfaz de opciones.
    /// </remarks>
    public void MostrarOpciones()
    {
        panelOpciones.pantallaOpciones.SetActive(true);
    }
}
