using UnityEngine;

/// <summary>
/// Limpia los objetos persistentes entre escenas, exceptuando uno específico.
/// </summary>
/// <remarks>
/// Esta clase se encarga de eliminar los objetos marcados como DontDestroyOnLoad,
/// excepto uno específico que se desea conservar entre escenas.
/// </remarks>
public class LimpiaEscena : MonoBehaviour
{
    /// <summary>
    /// Nombre del objeto que se debe conservar entre escenas.
    /// </summary>
    public string opciones = "TraspasoObjetos";

    /// <summary>
    /// Se ejecuta al iniciar el componente.
    /// Llama al método para limpiar los objetos persistentes.
    /// </summary>
    void Start()
    {
        CleanDontDestroyOnLoad();
    }

    /// <summary>
    /// Elimina todos los objetos marcados como DontDestroyOnLoad, 
    /// excepto el objeto especificado y sus hijos.
    /// </summary>
    void CleanDontDestroyOnLoad()
    {
        GameObject objetoConservar = GameObject.Find(opciones);

        // Usamos el nuevo método recomendado
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name == null || obj.scene.name == "DontDestroyOnLoad")
            {
                if (obj != objetoConservar && !IsChildOf(obj, objetoConservar))
                {
                    Destroy(obj);
                }
            }
        }
    }

    /// <summary>
    /// Verifica si un objeto es hijo de otro objeto específico.
    /// </summary>
    /// <param name="obj">El objeto a verificar.</param>
    /// <param name="potentialParent">El posible objeto padre.</param>
    /// <returns><c>true</c> si el objeto es hijo del padre potencial; de lo contrario, <c>false</c>.</returns>
    bool IsChildOf(GameObject obj, GameObject potentialParent)
    {
        Transform current = obj.transform.parent;
        while (current != null)
        {
            if (current.gameObject == potentialParent)
                return true;
            current = current.parent;
        }
        return false;
    }
}
