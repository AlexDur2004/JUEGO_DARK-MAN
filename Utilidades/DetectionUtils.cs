using UnityEngine;

namespace Utilidades
{
    /// <summary>
    /// Clase de utilidades para detectar objetos en el mundo del juego de manera eficiente.
    /// Proporciona métodos optimizados para minimizar la generación de basura en memoria.
    /// </summary>
    public static class DetectionUtils
    {
        /// <summary>
        /// Buffer reutilizable para reducir la generación de basura durante las detecciones.
        /// </summary>
        private static readonly Collider[] bufferResultados = new Collider[20];

        /// <summary>
        /// Detecta objetos cercanos y devuelve un nuevo array con los resultados.
        /// </summary>
        /// <param name="posicion">Posición central desde donde detectar objetos.</param>
        /// <param name="radio">Radio de detección en unidades de mundo.</param>
        /// <param name="capa">Máscara de capas para filtrar los objetos detectados.</param>
        /// <returns>Un array nuevo con los colisionadores encontrados.</returns>
        public static Collider[] DetectarObjetosCercanos(Vector3 posicion, float radio, LayerMask capa)
        {
            int numEncontrados = Physics.OverlapSphereNonAlloc(posicion, radio, bufferResultados, capa);
            
            if (numEncontrados == 0)
                return System.Array.Empty<Collider>();
            
            Collider[] resultados = new Collider[numEncontrados];
            System.Array.Copy(bufferResultados, resultados, numEncontrados);
            
            return resultados;
        }
        
        /// <summary>
        /// Detecta objetos cercanos sin crear nuevas asignaciones de memoria.
        /// </summary>
        /// <param name="posicion">Posición central desde donde detectar objetos.</param>
        /// <param name="radio">Radio de detección en unidades de mundo.</param>        /// <param name="capa">Máscara de capas para filtrar los objetos detectados.</param>
        /// <param name="resultados">Array donde se almacenarán los resultados.</param>
        /// <returns>El número de objetos detectados.</returns>
        public static int DetectarObjetosNoAlloc(Vector3 posicion, float radio, LayerMask capa, Collider[] resultados) =>
            Physics.OverlapSphereNonAlloc(posicion, radio, resultados, capa);
        
        /// <summary>
        /// Verifica rápidamente si hay al menos un objeto dentro del radio especificado.
        /// </summary>
        /// <param name="posicion">Posición central desde donde detectar objetos.</param>
        /// <param name="radio">Radio de detección en unidades de mundo.</param>
        /// <param name="capa">Máscara de capas para filtrar los objetos detectados.</param>
        /// <returns>True si se encuentra al menos un objeto, false en caso contrario.</returns>
        public static bool HayObjetosCercanos(Vector3 posicion, float radio, LayerMask capa)
        {
            return Physics.CheckSphere(posicion, radio, capa);
        }
    }
}