using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Управляет покупкой кирки у NPC.
/// Вешается на NPC-торговца или на отдельный пустой объект в сцене.
/// 
/// Как использовать:
/// 1. Повесь на NPC или создай пустой объект "ShopSystem"
/// 2. Назначь префаб новой кирки в Inspector
/// 3. Назначь Transform игрока
/// 4. Из диалоговой системы вызови: ShopManager.Instance.TryBuyPickaxe()
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance; // Синглтон для удобного вызова

    [Header("Товар: Новая кирка")]
    [Tooltip("Префаб новой кирки (с XR Grab Interactable)")]
    public GameObject newPickaxePrefab;

    [Tooltip("Стоимость кирки в искорках")]
    public int pickaxePrice = 5;

    [Header("Спавн")]
    [Tooltip("Transform игрока (XR Origin) — кирка появится перед ним")]
    public Transform playerTransform;

    [Tooltip("Расстояние спавна перед игроком (метры)")]
    public float spawnDistance = 0.8f;

    [Tooltip("Высота спавна над полом (метры)")]
    public float spawnHeight = 1.0f;

    [Header("Состояние")]
    [Tooltip("Уже куплена? (только для отладки, не меняй вручную)")]
    [SerializeField] private bool _alreadyBought = false;

    [Header("Аудио (опционально)")]
    public AudioSource audioSource;
    public AudioClip purchaseSuccessClip;   // Звук успешной покупки
    public AudioClip purchaseFailClip;      // Звук "не хватает денег"

    /// <summary>
    /// Публичное свойство: куплена ли уже кирка.
    /// Можно использовать в диалоговой системе для проверки.
    /// </summary>
    public bool AlreadyBought => _alreadyBought;

    void Awake()
    {
        // Синглтон
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[ShopManager] Дубликат ShopManager! " +
                             "Удаляю лишний.");
            Destroy(this);
            return;
        }
    }

    void Start()
    {
        // === Проверки ===
        if (newPickaxePrefab == null)
        {
            Debug.LogError("[ShopManager] newPickaxePrefab НЕ НАЗНАЧЕН! " +
                           "Перетащи префаб новой кирки в Inspector.");
        }

        if (playerTransform == null)
        {
            // Пытаемся найти игрока автоматически
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("[ShopManager] Игрок найден автоматически по тегу 'Player'");
            }
            else
            {
                // Ищем XR Origin
                var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null)
                {
                    playerTransform = xrOrigin.transform;
                    Debug.Log("[ShopManager] Игрок найден как XR Origin");
                }
                else
                {
                    Debug.LogError("[ShopManager] playerTransform НЕ НАЗНАЧЕН " +
                                   "и не удалось найти автоматически!");
                }
            }
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[ShopManager] GameManager.Instance == null! " +
                           "Убедись что GameManager есть в сцене.");
        }

        Debug.Log($"[ShopManager] Инициализирован. " +
                  $"Цена кирки: {pickaxePrice} искорок");
    }

    // =================================================
    // === ГЛАВНЫЙ МЕТОД — ВЫЗЫВАЙ ИЗ ДИАЛОГА ===
    // =================================================

    /// <summary>
    /// Попытка купить кирку. Вызывается из диалоговой системы.
    /// 
    /// Проверяет:
    /// 1. Не куплена ли уже
    /// 2. Хватает ли искорок
    /// 
    /// Если всё ок — списывает искорки и спавнит кирку перед игроком.
    /// </summary>
    public void TryBuyPickaxe()
    {
        Debug.Log("[ShopManager] === TryBuyPickaxe() вызван ===");

        // --- Проверка 1: Уже куплена? ---
        if (_alreadyBought)
        {
            Debug.Log("[ShopManager] Кирка уже была куплена ранее!");

            if (HitFeedbackUI.Instance != null)
            {
                HitFeedbackUI.Instance.ShowMessage(
                    "У вас уже есть новая кирка!",
                    Color.yellow
                );
            }

            PlaySound(purchaseFailClip);
            return;
        }

        // --- Проверка 2: Есть ли GameManager? ---
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ShopManager] GameManager не найден! " +
                           "Покупка невозможна.");
            return;
        }

        // --- Проверка 3: Хватает ли искорок? ---
        int currentSparks = GameManager.Instance.GetSparks();

        if (!GameManager.Instance.CanAfford(pickaxePrice))
        {
            Debug.Log($"[ShopManager] Не хватает искорок! " +
                      $"Нужно: {pickaxePrice}, есть: {currentSparks}");

            if (HitFeedbackUI.Instance != null)
            {
                HitFeedbackUI.Instance.ShowFail(
                    $"Не хватает искорок!\n" +
                    $"Нужно: {pickaxePrice}, у вас: {currentSparks}"
                );
            }

            PlaySound(purchaseFailClip);
            return;
        }

        // --- Проверка 4: Есть ли префаб? ---
        if (newPickaxePrefab == null)
        {
            Debug.LogError("[ShopManager] Префаб кирки не назначен!");
            return;
        }

        // =================================
        // === ВСЁ ОК — СОВЕРШАЕМ ПОКУПКУ ===
        // =================================

        // Списываем искорки
        bool spent = GameManager.Instance.SpendSparks(pickaxePrice);

        if (!spent)
        {
            // На случай если что-то пошло не так между проверкой и списанием
            Debug.LogError("[ShopManager] SpendSparks вернул false! " +
                           "Покупка отменена.");
            return;
        }

        // Спавним кирку
        SpawnPickaxe();

        // Отмечаем как купленную
        _alreadyBought = true;

        // Звук
        PlaySound(purchaseSuccessClip);

        // Уведомление
        if (HitFeedbackUI.Instance != null)
        {
            HitFeedbackUI.Instance.ShowComplete(
                $"Новая кирка куплена!\n" +
                $"Потрачено: {pickaxePrice} искорок"
            );
        }

        Debug.Log($"[ShopManager] === ПОКУПКА УСПЕШНА! === " +
                  $"Потрачено: {pickaxePrice}, " +
                  $"Осталось: {GameManager.Instance.GetSparks()}");
    }

    // =================================================
    // === СПАВН КИРКИ ===
    // =================================================

    private void SpawnPickaxe()
    {
        if (playerTransform == null)
        {
            Debug.LogError("[ShopManager] playerTransform == null! " +
                           "Спавню кирку на позиции ShopManager.");
            Instantiate(newPickaxePrefab, transform.position, Quaternion.identity);
            return;
        }

        // Вычисляем позицию перед игроком
        // playerTransform.forward — направление куда смотрит игрок
        Vector3 spawnPosition = playerTransform.position
                                + playerTransform.forward * spawnDistance;

        // Устанавливаем высоту (чтобы кирка не спавнилась в полу или в потолке)
        spawnPosition.y = playerTransform.position.y + spawnHeight;

        // Поворот кирки — лежит боком перед игроком
        Quaternion spawnRotation = Quaternion.Euler(0, playerTransform.eulerAngles.y, 0);

        // Спавн!
        GameObject newPickaxe = Instantiate(
            newPickaxePrefab,
            spawnPosition,
            spawnRotation
        );

        Debug.Log($"[ShopManager] Кирка заспавнена на позиции: {spawnPosition}");

        // Даём кирке имя для удобства отладки
        newPickaxe.name = "NewPickaxe_Purchased";

        // Убеждаемся что Rigidbody не Kinematic (чтобы кирка упала)
        Rigidbody rb = newPickaxe.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            // Небольшой импульс вниз чтобы кирка красиво упала
            rb.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
        }
    }

    // =================================================
    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===
    // =================================================

    /// <summary>
    /// Можно ли купить кирку прямо сейчас?
    /// Полезно для диалоговой системы — чтобы показать/скрыть вариант покупки.
    /// </summary>
    public bool CanBuyPickaxe()
    {
        if (_alreadyBought) return false;
        if (GameManager.Instance == null) return false;
        return GameManager.Instance.CanAfford(pickaxePrice);
    }

    /// <summary>
    /// Возвращает строку с информацией о цене.
    /// Полезно для отображения в диалоге.
    /// </summary>
    public string GetPriceInfo()
    {
        int current = GameManager.Instance != null
            ? GameManager.Instance.GetSparks()
            : 0;

        return $"Цена: {pickaxePrice} искорок (у вас: {current})";
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
