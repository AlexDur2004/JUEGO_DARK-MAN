using UnityEngine;
using System.Collections;

/// <summary>
/// Implementación de una habilidad que crea una barrera defensiva delante del personaje.
/// La barrera aparece desde el suelo con una animación y se hunde al desactivarse.
/// </summary>
/// <remarks>
/// Esta habilidad hereda de la clase base HabilidadBase para implementar una barrera
/// defensiva que el jugador puede activar para protegerse. La barrera emerge del suelo
/// y desaparece hundiéndose en el mismo con animaciones fluidas.
/// </remarks>
public class HabilidadBarrera : HabilidadBase
{    /// <summary>
    /// Distancia a la que se colocará la barrera delante del personaje.
    /// </summary>
    [SerializeField] private float distanciaDelante = 2f;
    
    /// <summary>
    /// Prefab que se usará para instanciar la barrera defensiva.
    /// </summary>
    [SerializeField] private GameObject prefabBarrera;
    
    /// <summary>
    /// Tiempo en segundos que tarda la barrera en emerger completamente del suelo.
    /// </summary>
    [SerializeField] private float tiempoAparicion = 0.5f;

    /// <summary>
    /// Referencia a la instancia actual de la barrera en la escena.
    /// </summary>
    private GameObject barreraActual;

    /// <summary>
    /// Inicializa la habilidad creando una instancia desactivada de la barrera.
    /// </summary>
    /// <remarks>
    /// Este método se llama cuando la habilidad se inicializa por primera vez.
    /// Crea una instancia del prefab de la barrera en estado desactivado
    /// para optimizar su uso posterior.
    /// </remarks>
    protected override void InicializarHabilidad()
    {
        // Inicializar la barrera al inicio si no existe ya
        if (!barreraActual && prefabBarrera != null)
        {
            // Crear el objeto de la barrera pero desactivado
            barreraActual = Instantiate(prefabBarrera);
            barreraActual.SetActive(false);
        }
    }

    /// <summary>
    /// Implementación del método abstracto que activa el efecto de la habilidad.
    /// </summary>
    /// <remarks>
    /// Este método se llama cuando el jugador activa la habilidad.
    /// Invoca el método ActivarBarrera para crear la barrera defensiva.
    /// </remarks>
    public override void AplicarEfectoHabilidad()
    {
        ActivarBarrera();
    }

    /// <summary>
    /// Activa la barrera defensiva colocándola delante del personaje.
    /// </summary>
    /// <remarks>
    /// Este método coloca la barrera a la distancia especificada, 
    /// la orienta correctamente y activa la animación de aparición.
    /// Además programa la desactivación automática después del tiempo establecido.
    /// </remarks>
    private void ActivarBarrera()
    {
        // Verificar si la barrera existe, si no, crearla
        if (!barreraActual)
        {
            barreraActual = Instantiate(prefabBarrera);
            barreraActual.SetActive(false);
        }

        // Calcular la posición delante del personaje
        Vector3 direccionFrente = transform.forward;
        Vector3 posicionBarrera = transform.position + (direccionFrente * distanciaDelante);

        // Posicionar y orientar la barrera
        barreraActual.transform.position = posicionBarrera;

        // Aplicamos la rotación base mirando en la dirección del personaje
        barreraActual.transform.rotation = Quaternion.LookRotation(direccionFrente);

        // Aplicamos una rotación adicional de 90 grados en el eje Y
        barreraActual.transform.Rotate(0, 90, 0);

        // Guardar la escala original para la animación
        Vector3 escalaOriginal = barreraActual.transform.localScale;

        // Iniciar con altura cero pero manteniendo el ancho y profundidad
        barreraActual.transform.localScale = new Vector3(
            escalaOriginal.x,
            0, // Altura inicial en cero
            escalaOriginal.z
        );

        // Activar la barrera para que sea visible
        barreraActual.SetActive(true);

        // Iniciar la animación de emergencia desde el suelo
        StartCoroutine(AnimarAparicionDesdeAbajo(escalaOriginal));

        // Programamos la desactivación automática usando duracionBase de la clase padre
        Invoke("DesactivarBarrera", duracionBase);
    }

    /// <summary>
    /// Corrutina que anima la aparición de la barrera desde el suelo.
    /// </summary>
    /// <param name="escalaFinal">La escala final que tendrá la barrera cuando termine la animación.</param>
    /// <returns>Un enumerador para la corrutina.</returns>
    private IEnumerator AnimarAparicionDesdeAbajo(Vector3 escalaFinal)
    {
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoAparicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / tiempoAparicion;

            // Interpolar la altura de 0 a la altura final
            barreraActual.transform.localScale = new Vector3(
                escalaFinal.x,
                Mathf.Lerp(0, escalaFinal.y, progreso),
                escalaFinal.z
            );

            yield return null;
        }

        // Asegurarse de que al final tiene exactamente la escala deseada
        barreraActual.transform.localScale = escalaFinal;
    }

    /// <summary>
    /// Desactiva la barrera después de que ha transcurrido el tiempo de duración establecido.
    /// </summary>
    /// <remarks>
    /// En lugar de desactivar inmediatamente la barrera, inicia una animación
    /// de hundimiento para que la desaparición sea fluida.
    /// </remarks>
    private void DesactivarBarrera()
    {
        if (barreraActual && barreraActual.activeSelf)
        {
            // En lugar de desactivar inmediatamente, iniciamos la animación de hundimiento
            StartCoroutine(AnimarDesaparicionHundimiento());
        }
    }

    /// <summary>
    /// Corrutina que anima la desaparición de la barrera hundiéndose en el suelo.
    /// </summary>
    /// <returns>Un enumerador para la corrutina.</returns>
    private IEnumerator AnimarDesaparicionHundimiento()
    {
        // Guardamos la escala actual para la animación
        Vector3 escalaInicial = barreraActual.transform.localScale;

        // Guardamos la posición inicial
        Vector3 posicionInicial = barreraActual.transform.position;

        // Tiempo que toma hundirse en el suelo
        float tiempoDesaparicion = tiempoAparicion * 0.8f; // Un poco más rápido que la aparición
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoDesaparicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / tiempoDesaparicion;

            // Reducimos la escala en altura gradualmente
            barreraActual.transform.localScale = new Vector3(
                escalaInicial.x,
                escalaInicial.y * (1 - progreso),
                escalaInicial.z
            );

            // Bajamos la posición para dar efecto de hundimiento
            barreraActual.transform.position = new Vector3(
                posicionInicial.x,
                posicionInicial.y - (progreso * escalaInicial.y * 0.5f), // Bajamos hasta la mitad de la altura
                posicionInicial.z
            );

            yield return null;
        }

        // Al terminar la animación, desactivamos la barrera
        barreraActual.SetActive(false);

        // Restauramos la escala y posición originales para la próxima vez
        barreraActual.transform.localScale = escalaInicial;
        barreraActual.transform.position = posicionInicial;
    }

    /// <summary>
    /// Elimina la barrera defensiva cuando se finaliza el efecto de la habilidad.
    /// </summary>
    /// <remarks>
    /// Este método sobrescribe el método de la clase base para asegurar que la barrera
    /// se desactive adecuadamente, usando la animación de hundimiento si es posible
    /// o desactivándola directamente si el GameObject ya no está activo.
    /// </remarks>
    public override void RemoverEfectoHabilidad()
    {
        if (barreraActual && barreraActual.activeSelf)
        {
            // Verificar si el GameObject está activo antes de iniciar la corrutina
            if (gameObject.activeInHierarchy)
            {
                // En lugar de desactivar inmediatamente, iniciamos la animación de hundimiento
                StartCoroutine(AnimarDesaparicionHundimiento());
            }
            else
            {
                // Si el GameObject está inactivo, desactivar la barrera directamente
                barreraActual.SetActive(false);
            }

            // Cancela cualquier desactivación automática pendiente
            CancelInvoke("DesactivarBarrera");
        }
    }

    /// <summary>
    /// Limpia los recursos cuando este componente es destruido.
    /// </summary>
    /// <remarks>
    /// Este método se asegura de que la instancia de la barrera sea destruida adecuadamente
    /// para evitar pérdidas de memoria cuando el componente se destruye.
    /// </remarks>
    private void OnDestroy()
    {
        // Si la aplicación se está cerrando, no es necesario limpiar
        if (barreraActual != null)
        {
            Destroy(barreraActual);
        }
    }
}
