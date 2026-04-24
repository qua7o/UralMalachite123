using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeTiming : MonoBehaviour
{
    public TimingSlider timingSlider; 
    public LayerMask mineralLayer;    
    public float hitDistance = 1.5f;  
    public void OnSwingAttempt()
    {
        if (timingSlider == null)
        {
            Debug.LogWarning("TimingSlider не назначен!");
            return;
        }

        bool inPerfectZone = timingSlider.IsInPerfectZone();
        float position = timingSlider.GetCurrentPosition();

        // Логика удара по минералу впереди
        RaycastHit hit;
        bool hitMineral = Physics.Raycast(
            transform.position,
            transform.forward,
            out hit,
            hitDistance,
            mineralLayer
        );

        if (hitMineral)
        {
            MineralController mineral = hit.collider.GetComponent<MineralController>();

            if (inPerfectZone)
            {
                Debug.Log($"✅ Идеальный удар! Позиция: {position:F2}");
                mineral?.BreakOffChunk(hit.point);
                // Добавить вибрацию, звук, эффект
            }
            else
            {
                Debug.Log($"⚠️ Удар, но не вовремя. Позиция: {position:F2}");
                // Можно: ничего не откалывать, или откалывать мелкий кусок
            }
        }
        else
        {
            Debug.Log($"💨 Махнул мимо. Позиция: {position:F2}");
        }
    }
}


