using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilidades
{
    /// <summary>
    /// Interface común para todas las implementaciones de ObjectPool
    /// </summary>
    public interface IObjectPool
    {
        void Clear();
    }

    /// <summary>
    /// Implementación genérica del patrón Object Pool para gestionar objetos reutilizables.
    /// Mantiene dos conjuntos: uno de objetos activos y otro de inactivos.
    /// </summary>
    /// <typeparam name="T">Tipo de objeto que se va a gestionar en el pool.</typeparam>
    public class ObjectPool<T> : IObjectPool where T : class
    {
        private readonly HashSet<T> activos = new();
        private readonly HashSet<T> inactivos = new();

        /// <summary>
        /// Registra un nuevo objeto en el pool como activo.
        /// </summary>
        /// <param name="objeto">Objeto a registrar en el pool.</param>
        /// <returns>True si el objeto se registró correctamente, false si ya existía o es nulo.</returns>
        public bool Registrar(T objeto)
        {
            if (objeto == null) return false;

            if (!activos.Contains(objeto) && !inactivos.Contains(objeto))
            {
                activos.Add(objeto);
                SetActive(objeto, true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Activa un objeto que estaba inactivo en el pool, con la opción de activar o no su GameObject.
        /// </summary>
        /// <param name="objeto">Objeto a activar.</param>
        /// <param name="activarGameObject">Si es true, activa el GameObject; si es false, solo lo mueve en las listas.</param>
        /// <returns>True si el objeto se activó correctamente, false si no estaba en los inactivos o es nulo.</returns>
        public bool Activar(T objeto, bool activarGameObject = true)
        {
            if (objeto == null) return false;

            if (inactivos.Remove(objeto))
            {
                activos.Add(objeto);
                if (activarGameObject)
                    SetActive(objeto, true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Desactiva un objeto activo y lo mueve a los inactivos, con la opción de desactivar o no su GameObject.
        /// </summary>
        /// <param name="objeto">Objeto a desactivar.</param>
        /// <param name="desactivarGameObject">Si es true, desactiva el GameObject; si es false, solo lo mueve en las listas.</param>
        /// <returns>True si el objeto se desactivó correctamente, false si no estaba en los activos o es nulo.</returns>
        public bool Desactivar(T objeto, bool desactivarGameObject = true)
        {
            if (objeto == null) return false;

            if (activos.Remove(objeto))
            {
                inactivos.Add(objeto);
                if (desactivarGameObject)
                    SetActive(objeto, false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Método para activar/desactivar el objeto según su tipo
        /// </summary>
        private void SetActive(T objeto, bool activo)
        {
            // Si es un GameObject, usar SetActive directamente
            if (objeto is GameObject gameObject)
            {
                gameObject.SetActive(activo);
                return;
            }

            // Si es un MonoBehaviour, usar su gameObject
            if (objeto is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.gameObject.SetActive(activo);
                return;
            }
        }

        /// <summary>
        /// Obtiene una copia de la lista de objetos activos.
        /// </summary>
        /// <returns>Lista nueva con los objetos activos.</returns>
        public List<T> GetActivos() => new(activos);

        /// <summary>
        /// Obtiene una copia de la lista de objetos inactivos.
        /// </summary>
        /// <returns>Lista nueva con los objetos inactivos.</returns>
        public List<T> GetInactivos() => new(inactivos);

        /// <summary>
        /// Cantidad de objetos activos en el pool.
        /// </summary>
        public int CountActivos => activos.Count;

        /// <summary>
        /// Cantidad de objetos inactivos en el pool.
        /// </summary>
        public int CountInactivos => inactivos.Count;

        /// <summary>
        /// Cantidad total de objetos en el pool (activos e inactivos).
        /// </summary>
        public int CountTotal => activos.Count + inactivos.Count;

        /// <summary>
        /// Busca un objeto en los activos que cumpla con un predicado.
        /// </summary>
        /// <param name="predicado">Función para determinar si un objeto cumple con el criterio de búsqueda.</param>
        /// <returns>El primer objeto que cumple con el predicado o null si no se encuentra.</returns>
        public T BuscarActivo(Func<T, bool> predicado)
        {
            foreach (var obj in activos)
            {
                if (predicado(obj))
                    return obj;
            }
            return null;
        }

        /// <summary>
        /// Busca un objeto en los inactivos que cumpla con un predicado.
        /// </summary>
        /// <param name="predicado">Función para determinar si un objeto cumple con el criterio de búsqueda.</param>
        /// <returns>El primer objeto que cumple con el predicado o null si no se encuentra.</returns>
        public T BuscarInactivo(Func<T, bool> predicado)
        {
            foreach (var obj in inactivos)
            {
                if (predicado(obj))
                    return obj;
            }
            return null;
        }

        /// <summary>
        /// Busca un objeto en todos los objetos del pool (activos e inactivos) que cumpla con un predicado.
        /// </summary>
        /// <param name="predicado">Función para determinar si un objeto cumple con el criterio de búsqueda.</param>
        /// <returns>El primer objeto que cumple con el predicado o null si no se encuentra.</returns>
        public T BuscarCualquiera(Func<T, bool> predicado)
        {
            // Primero buscamos en los activos
            var objetoActivo = BuscarActivo(predicado);
            if (objetoActivo != null)
                return objetoActivo;

            // Si no se encuentra, buscamos en los inactivos
            return BuscarInactivo(predicado);
        }

        /// <summary>
        /// Verifica si un objeto está en la lista de objetos activos.
        /// </summary>
        /// <param name="objeto">Objeto a verificar.</param>
        /// <returns>True si el objeto está activo, false en caso contrario.</returns>
        public bool ContieneActivo(T objeto)
        {
            return activos.Contains(objeto);
        }
        /// <summary>
        /// Verifica si un objeto está en la lista de objetos inactivos.
        /// </summary>
        /// <param name="objeto">Objeto a verificar.</param>
        /// <returns>True si el objeto está inactivo, false en caso contrario.</returns>
        public bool ContieneInactivo(T objeto)
        {
            return inactivos.Contains(objeto);
        }

        /// <summary>
        /// Verifica si un objeto está registrado en el pool (ya sea activo o inactivo).
        /// </summary>
        /// <param name="objeto">Objeto a verificar.</param>
        /// <returns>True si el objeto está en el pool, false en caso contrario.</returns>
        public bool Contiene(T objeto)
        {
            return activos.Contains(objeto) || inactivos.Contains(objeto);
        }

        /// <summary>
        /// Obtiene el estado de un objeto en el pool.
        /// </summary>
        /// <param name="objeto">Objeto a verificar.</param>
        /// <returns>True si el objeto está activo, False si está inactivo, null si no está en el pool.</returns>
        public bool? ObtenerEstado(T objeto)
        {
            if (activos.Contains(objeto))
                return true;
            if (inactivos.Contains(objeto))
                return false;
            return null;
        }

        /// <summary>
        /// Limpia todas las referencias de objetos en el pool.
        /// </summary>
        public void Clear()
        {
            activos.Clear();
            inactivos.Clear();
        }

        /// <summary>
        /// Desactiva todos los objetos activos en el pool y los mueve a inactivos
        /// </summary>
        /// <param name="desactivarGameObjects">Si es true, desactiva los GameObjects; si es false, solo los mueve a inactivos.</param>
        public void DesactivarTodos(bool desactivarGameObjects = true)
        {
            // Crea una copia para evitar modificar la colección durante la iteración
            var objetosParaDesactivar = activos.ToList();
            objetosParaDesactivar.ForEach(obj => Desactivar(obj, desactivarGameObjects));
        }
    }
}