using UnityEngine;
using System;

/// <summary>
/// Maneja estados temporales evitando booleanos redundantes.
/// </summary>
public class EstadoTemporal
{
    private float duracion;
    private float tiempoRestante;
    private Action alActivar;
    private Action alDesactivar;
    private bool activado;

    /// <summary>
    /// Indica si el estado temporal está actualmente activo.
    /// </summary>
    public bool EstaActivo => tiempoRestante > 0;

    /// <summary>
    /// Porcentaje de tiempo restante respecto a la duración total (0 a 1).
    /// </summary>
    public float PorcentajeRestante => duracion > 0 ? tiempoRestante / duracion : 0;

    /// <summary>
    /// Tiempo restante en segundos hasta que el estado termine.
    /// </summary>
    public float TiempoRestante => tiempoRestante;

    /// <summary>
    /// Constructor de estado temporal.
    /// </summary>
    /// <param name="accionActivar">Acción a ejecutar cuando el estado se activa.</param>
    /// <param name="accionDesactivar">Acción a ejecutar cuando el estado se desactiva.</param>
    public EstadoTemporal(Action accionActivar = null, Action accionDesactivar = null)
    {
        alActivar = accionActivar;
        alDesactivar = accionDesactivar;
        tiempoRestante = 0;
        duracion = 0;
    }

    /// <summary>
    /// Activa el estado temporal por una duración específica.
    /// </summary>
    /// <param name="duracionEstado">Duración en segundos del estado temporal</param>
    public void Activar(float duracionEstado)
    {
        tiempoRestante = duracionEstado;
        duracion = duracionEstado;

        if (!activado)
        {
            activado = true;
            alActivar?.Invoke();
        }
    }

    /// <summary>
    /// Desactiva inmediatamente el estado temporal.
    /// </summary>
    public void Desactivar()
    {
        if (activado)
        {
            tiempoRestante = 0;
            activado = false;
            alDesactivar?.Invoke();
        }
    }

    /// <summary>
    /// Actualiza el tiempo restante del estado. Debe llamarse cada frame.
    /// </summary>
    public void Actualizar()
    {
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0)
            {
                Desactivar();
            }
        }
    }
}