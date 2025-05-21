using UnityEngine;

/// <summary>
/// Permite conservar un objeto de configuración entre cambios de escena.
/// </summary>
/// <remarks>
/// Esta clase asegura que solo exista una única instancia del objeto de opciones
/// en todas las escenas, evitando duplicados al cambiar de escena.
/// </remarks>
public class PasarConfiguracion : MonoBehaviour
{
    /// <summary>
    /// Se ejecuta al inicializar el objeto, antes de Start().
    /// Verifica si ya existe otro objeto de opciones y destruye este si es necesario,
    /// o lo marca como persistente entre escenas si es el primero.
    /// </summary>
    private void Awake() 
    {
        var noDestruirEntreEscenas = GameObject.FindGameObjectsWithTag("Opciones");

        if (noDestruirEntreEscenas.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
