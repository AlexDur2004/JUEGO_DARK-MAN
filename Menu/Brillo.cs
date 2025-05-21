using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el ajuste de brillo en la interfaz de usuario.
/// </summary>
/// <remarks>
/// Esta clase permite ajustar el nivel de brillo de la pantalla 
/// mediante un slider, guardando la configuración en PlayerPrefs
/// y aplicando un filtro visual según el valor seleccionado.
/// </remarks>
public class Brillo : MonoBehaviour
{
    /// <summary>
    /// Control deslizante para ajustar el brillo.
    /// </summary>
    public Slider slider;
    
    /// <summary>
    /// Valor actual del control deslizante.
    /// </summary>
    public float sliderValue;
    
    /// <summary>
    /// Panel que se utiliza para mostrar el efecto de brillo.
    /// </summary>
    public Image panelBrillo;
    
    /// <summary>
    /// Valor calculado para el efecto de oscurecimiento.
    /// </summary>
    public float valorBlack;
    
    /// <summary>
    /// Valor calculado para el efecto de iluminación.
    /// </summary>
    public float valorWhite;

    /// <summary>
    /// Se ejecuta al inicio cuando se activa el objeto.
    /// Recupera el valor de brillo guardado y lo aplica al panel.
    /// </summary>
    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("Brillo", 0.5f);

        panelBrillo.color = new Color(panelBrillo.color.r, panelBrillo.color.g, panelBrillo.color.b, sliderValue / 3);
    }

    /// <summary>
    /// Se ejecuta una vez por cada frame.
    /// Calcula y aplica los efectos visuales de brillo según el valor del slider.
    /// </summary>
    void Update()
    {
        valorBlack = 1 - sliderValue - 0.5f;
        valorWhite = sliderValue - 0.5f;
        if (sliderValue < 0.5f)
        {
            panelBrillo.color = new Color(0, 0, 0, valorBlack);
        }
        if (sliderValue > 0.5f)
        {
            panelBrillo.color = new Color(255, 255, 255, valorWhite);
        }
    }
    
    /// <summary>
    /// Actualiza el nivel de brillo cuando el usuario modifica el slider.
    /// </summary>
    /// <param name="valor">El nuevo valor de brillo (entre 0 y 1).</param>
    /// <remarks>
    /// Guarda el valor en PlayerPrefs para mantener la configuración
    /// entre sesiones de juego y actualiza el panel de brillo.
    /// </remarks>
    public void ChangeSlider(float valor)
    {
        sliderValue = valor;
        PlayerPrefs.SetFloat("Brillo", sliderValue);
        panelBrillo.color = new Color(panelBrillo.color.r, panelBrillo.color.g, panelBrillo.color.b, sliderValue / 3);
    }
}
