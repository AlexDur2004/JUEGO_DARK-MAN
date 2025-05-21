using UnityEngine;
using System.Collections;

/// <summary>
/// Componente para reiniciar periódicamente un sistema de partículas.
/// </summary>
/// <remarks>
/// Este script busca un sistema de partículas en los hijos del GameObject 
/// y lo reinicia cada tres segundos. Útil para efectos visuales continuos
/// que necesitan ser refrescados periódicamente.
/// </remarks>
public class ReactivarParcticulas : MonoBehaviour
{
    /// <summary>
    /// Referencia al sistema de partículas que se reiniciará.
    /// </summary>
    private ParticleSystem ps;

    /// <summary>
    /// Se ejecuta al iniciar el componente.
    /// Obtiene la referencia al sistema de partículas e inicia la corrutina de reinicio.
    /// </summary>
    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        StartCoroutine(ReiniciarCadaCincoSegundos());
    }

    /// <summary>
    /// Corrutina que reinicia el sistema de partículas cada tres segundos.
    /// </summary>
    /// <remarks>
    /// Aunque el nombre de la función dice "cinco segundos", actualmente está configurado
    /// para reiniciar cada tres segundos. Elimina todas las partículas actuales y reinicia
    /// la emisión.
    /// </remarks>
    /// <returns>Un enumerador usado por la corrutina.</returns>
    IEnumerator ReiniciarCadaCincoSegundos()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
            }
        }
    }
}
