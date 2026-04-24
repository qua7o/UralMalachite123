using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// ВРЕМЕННЫЙ скрипт для тестирования покупки.
/// Удали после проверки!
/// </summary>
public class TestShop : MonoBehaviour
{
    void Update()
    {
        // Нажми P чтобы купить кирку
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[TEST] Попытка покупки кирки...");
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.TryBuyPickaxe();
            }
        }

        // Нажми O чтобы добавить 5 искорок (для теста)
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (GameManager.Instance != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameManager.Instance.AddSpark();
                }
                Debug.Log($"[TEST] Добавлено 5 искорок. " +
                          $"Всего: {GameManager.Instance.GetSparks()}");
            }
        }
    }
}
