using UnityEngine;
using System.Collections;
using Utilidades;

/// <summary>
/// Componente que maneja la lógica de colisión con frutas y aplica beneficios a los personajes.
/// Las frutas pueden ser normales o especiales, con distintos efectos según su tipo.
/// </summary>
public class FrutaColisionable : MonoBehaviour
{
    [Header("Efectos")]
    /// <summary>
    /// Cantidad de segundos que se reducirá el tiempo de recarga de las habilidades al recoger una fruta normal.
    /// </summary>
    [SerializeField] private float reduccionCooldown = 10f;
    
    /// <summary>
    /// Duración en segundos del efecto de aumento de daño otorgado por las frutas especiales.
    /// </summary>
    [SerializeField] private float tiempoBuffoAtaque = 15f;
    
    /// <summary>
    /// Factor por el que se multiplica el daño base del arma al recoger una fruta especial.
    /// </summary>
    [SerializeField] private float multiplicadorAtaque = 1.5f;

    /// <summary>
    /// Referencia al arma que actualmente tiene el buffo de ataque activo. Es null cuando no hay ningún buffo aplicado.
    /// </summary>
    private ArmaBase armaBuffeada = null;

    /// <summary>
    /// Indica si esta fruta es especial basándose en su tag.
    /// Las frutas especiales aumentan el daño del arma temporalmente.
    /// </summary>
    public bool EsEspecial => CompareTag("FrutaEspecial");

    /// <summary>
    /// Se ejecuta cuando un objeto entra en el trigger de la fruta.
    /// Detecta si es un personaje jugable y aplica el efecto correspondiente según el tipo de fruta.
    /// </summary>
    /// <param name="other">Collider del objeto que ha entrado en contacto con la fruta.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Validaciones rápidas
        if (!other.CompareTag("DarkMan") && !other.CompareTag("Fantasma")) return;
        
        // Obtener el componente Personaje del objeto o sus padres
        Personaje personaje = other.GetComponentInParent<Personaje>();
        if (personaje == null) return;

        // Aplicar efecto según tipo de fruta
        if (EsEspecial && personaje.arma && armaBuffeada == null)
            StartCoroutine(BuffoAtaqueCoroutine(personaje.arma));
        else if (!EsEspecial)
            ReducirCooldown(personaje);

        personaje.ReproducirEfectoSonido(Utilidades.AudioManager.TipoSonido.Premio);

        // Notificar recogida
        GeneradorPremios.Instancia?.FrutaRecogida(gameObject);
    }

    /// <summary>
    /// Reduce el tiempo de recarga de la habilidad del personaje.
    /// Solo se aplica si la habilidad no está actualmente activa.
    /// </summary>
    /// <param name="personaje">Personaje cuya habilidad verá reducido su tiempo de recarga.</param>
    private void ReducirCooldown(Personaje personaje)
    {
        if (personaje.habilidadAsociada != null && !personaje.habilidadAsociada.EstaActiva())
            personaje.habilidadAsociada.ReducirCooldown(reduccionCooldown);
    }

    /// <summary>
    /// Corrutina que aplica un aumento temporal de daño al arma del personaje.
    /// Al terminar el tiempo especificado, restaura el daño original.
    /// </summary>
    /// <param name="arma">Arma a la que se aplicará el buffo de daño.</param>
    /// <returns>Enumerador para la corrutina.</returns>
    private IEnumerator BuffoAtaqueCoroutine(ArmaBase arma)
    {
        if (!arma) yield break;

        armaBuffeada = arma;
        int dañoOriginal = arma.ObtenerDañoBase();

        // Aplicar buffo
        arma.ModificarDañoBase(Mathf.RoundToInt(dañoOriginal * multiplicadorAtaque));
        yield return new WaitForSeconds(tiempoBuffoAtaque);

        // Restaurar valores originales
        arma.ModificarDañoBase(dañoOriginal);
        armaBuffeada = null;
    }
}