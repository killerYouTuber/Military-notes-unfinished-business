using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogError("HealthSlider: Slider component not found!");
        }
    }

    public float value
    {
        get { return slider != null ? slider.value : 0f; }
        set 
        { 
            if (slider != null)
            {
                slider.value = value;
            }
        }
    }
} 