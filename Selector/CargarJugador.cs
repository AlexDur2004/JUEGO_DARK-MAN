/*
========================================================================
Script para cargar el jugador elegido en el menú de selección de jugador

Trata de obtener el Int para saber el jugador seleccionado en el menú,
instancia el prefab del jugador escogido y lo spawnea en el punto
de spawn que sea conveniente. Incluir código en el juego para que 
funcione.
========================================================================
*/

using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Componente responsable de cargar el personaje seleccionado por el jugador en el menú de selección.
/// </summary>
/// <remarks>
/// Este script obtiene el índice del personaje seleccionado desde PlayerPrefs,
/// instancia el prefab correspondiente y lo posiciona en el punto de spawn designado.
/// También muestra el nombre del personaje seleccionado en un texto de la interfaz.
/// </remarks>
public class CargarJugador : MonoBehaviour
{
    /// <summary>
    /// Array de prefabs de los personajes disponibles para seleccionar.
    /// </summary>
    public GameObject[] jugadorPrefabs;
    
    /// <summary>
    /// Punto donde se instanciará el personaje seleccionado.
    /// </summary>
    public Transform puntoSpawn;
    
    /// <summary>
    /// Componente de texto para mostrar el nombre del personaje seleccionado.
    /// </summary>
    public TMP_Text texto;   // no necesario, para poner en pantalla cual es el elegido

    /// <summary>
    /// Se ejecuta una vez al inicio, carga e instancia el personaje seleccionado.
    /// </summary>
    /// <remarks>
    /// Recupera el índice del personaje elegido desde PlayerPrefs con la clave "jugadorElegido",
    /// instancia el prefab correspondiente en la posición del punto de spawn, y
    /// actualiza el texto de la interfaz con el nombre del personaje.
    /// </remarks>
    void Start()
    {
        int jugadorElegido = PlayerPrefs.GetInt("jugadorElegido");
        GameObject prefab = jugadorPrefabs[jugadorElegido];
        GameObject clon = Instantiate(prefab, puntoSpawn.position, Quaternion.identity);
        texto.text = prefab.name;
    }
}
