using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class TimingSlider : MonoBehaviour
{
    public Slider slider;
    public float speed = 3f; 
    public float perfectMin = 0.6f;
    public float perfectMax = 0.8f; 

    private float currentValue = 0f;
    private bool movingForward = true;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        if (slider != null)
            slider.maxValue = 1f;
    }

    void Update()
    {
        
        if (movingForward)
        {
            currentValue += Time.deltaTime * speed;
            if (currentValue >= 1f)
            {
                currentValue = 1f;
                movingForward = false;
            }
        }
        else
        {
            currentValue -= Time.deltaTime * speed;
            if (currentValue <= 0f)
            {
                currentValue = 0f;
                movingForward = true;
            }
        }

        if (slider != null)
            slider.value = currentValue;
    }

    public bool IsInPerfectZone()
    {
        return currentValue >= perfectMin && currentValue <= perfectMax;
    }

    // Возвращает текущую позицию (для логгирования или визуального фидбэка)
    public float GetCurrentPosition() => currentValue;
}
