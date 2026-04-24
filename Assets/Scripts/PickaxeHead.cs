using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeHead : MonoBehaviour
{
    public float impactForce = 1f;

    private void Start()
    {
        // Проверяем, что скрипт вообще загрузился
        Debug.Log($"[PickaxeHead] Скрипт инициализирован на объекте: " +
                  $"{gameObject.name}");

        // Проверяем наличие коллайдера
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[PickaxeHead] НЕТ КОЛЛАЙДЕРА на {gameObject.name}!");
        }
        else
        {
            Debug.Log($"[PickaxeHead] Коллайдер найден: {col.GetType().Name}, " +
                      $"IsTrigger = {col.isTrigger}");
            if (!col.isTrigger)
            {
                Debug.LogError("[PickaxeHead] КОЛЛАЙДЕР НЕ ТРИГГЕР! " +
                               "Установи Is Trigger = true!");
            }
        }

        // Проверяем Rigidbody на родителе
        Rigidbody rb = GetComponentInParent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("[PickaxeHead] НЕТ RIGIDBODY на кирке или её родителях!");
        }
        else
        {
            Debug.Log($"[PickaxeHead] Rigidbody найден на: {rb.gameObject.name}, " +
                      $"isKinematic = {rb.isKinematic}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PickaxeHead] OnTriggerEnter сработал! " +
                  $"Столкнулся с: {other.gameObject.name} " +
                  $"(Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        MiningRock rock = other.GetComponent<MiningRock>();
        if (rock == null)
        {
            rock = other.GetComponentInParent<MiningRock>();
        }

        if (rock != null)
        {
            Debug.Log("[PickaxeHead] >>> MiningRock НАЙДЕН! Регистрирую удар... <<<");
            rock.RegisterHit();
        }
        else
        {
            Debug.LogWarning($"[PickaxeHead] MiningRock НЕ найден на объекте " +
                             $"{other.gameObject.name} и его родителях");
        }
    }

    // Дополнительно: проверяем, есть ли ВООБЩЕ какие-то коллизии
    private void OnTriggerStay(Collider other)
    {
        // Этот лог будет спамить, но покажет, 
        // что триггер в принципе работает
        // Раскомментируй при необходимости:
        // Debug.Log($"[PickaxeHead] OnTriggerStay с: {other.gameObject.name}");
    }
}