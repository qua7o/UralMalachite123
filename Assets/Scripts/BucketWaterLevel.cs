using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Вариант 2: Управляет видимостью воды внутри ведра.
/// Вешается на ведро (Bucket).
/// При контакте с зоной воды — включает объект воды внутри.
/// При переворачивании — выливает воду.
/// </summary>
public class BucketWaterLevel : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Объект воды внутри ведра (WaterInBucket)")]
    public GameObject waterVisual;

    [Header("Настройки")]
    [Tooltip("Тег зоны воды в реке")]
    public string waterTag = "Water";

    [Tooltip("Время набора воды (секунды)")]
    public float fillTime = 1.0f;

    [Tooltip("Угол наклона для выливания воды (градусы)")]
    public float pourAngle = 100f;

    [Header("Аудио")]
    public AudioSource audioSource;
    public AudioClip fillClip;       // Звук набора воды
    public AudioClip pourClip;       // Звук выливания воды

    [Header("Состояние (только для отладки)")]
    [SerializeField] private bool _isFilled = false;
    [SerializeField] private bool _isInWater = false;

    // Внутренние переменные
    private float _timeInWater = 0f;

    /// <summary>
    /// Публичное свойство: заполнено ли ведро.
    /// Другие скрипты могут проверять: bucket.IsFilled
    /// </summary>
    public bool IsFilled => _isFilled;

    void Start()
    {
        // Проверки
        if (waterVisual == null)
        {
            Debug.LogError("[BucketWater] waterVisual НЕ НАЗНАЧЕН! " +
                           "Перетащи объект WaterInBucket в поле Water Visual.");
            return;
        }

        // Убеждаемся что вода скрыта в начале
        waterVisual.SetActive(false);
        _isFilled = false;

        Debug.Log($"[BucketWater] Ведро инициализировано: {gameObject.name}");
    }

    void Update()
    {
        // Набор воды: считаем время в воде
        if (_isInWater && !_isFilled)
        {
            _timeInWater += Time.deltaTime;

            if (_timeInWater >= fillTime)
            {
                Fill();
            }
        }

        // Выливание: проверяем наклон ведра
        if (_isFilled)
        {
            CheckPouring();
        }
    }

    // === ТРИГГЕРЫ ВОДЫ ===

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(waterTag)) return;

        if (!_isFilled)
        {
            _isInWater = true;
            _timeInWater = 0f;
            Debug.Log("[BucketWater] Ведро погружено в воду...");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Дополнительная страховка: если Enter не сработал
        if (!other.CompareTag(waterTag)) return;

        if (!_isFilled && !_isInWater)
        {
            _isInWater = true;
            _timeInWater = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(waterTag)) return;

        _isInWater = false;
        _timeInWater = 0f;
        Debug.Log("[BucketWater] Ведро вынуто из воды");
    }

    // === ЗАПОЛНЕНИЕ ===

    private void Fill()
    {
        if (_isFilled) return; // Защита от повторного вызова

        _isFilled = true;
        _isInWater = false;

        // Показываем воду
        if (waterVisual != null)
        {
            waterVisual.SetActive(true);
        }

        // Звук
        PlaySound(fillClip);

        Debug.Log("[BucketWater] === ВЕДРО НАПОЛНЕНО! ===");

        // Уведомление на экране (если есть HitFeedbackUI)
        if (HitFeedbackUI.Instance != null)
        {
            HitFeedbackUI.Instance.ShowSuccess("Ведро наполнено водой!");
        }
    }

    // === ВЫЛИВАНИЕ ===

    private void CheckPouring()
    {
        // Проверяем угол наклона ведра
        // Vector3.up — мировой вектор вверх
        // transform.up — локальный вектор "вверх" ведра
        float angle = Vector3.Angle(Vector3.up, transform.up);

        // Если ведро наклонено больше чем pourAngle — вода выливается
        if (angle > pourAngle)
        {
            Pour();
        }
    }

    /// <summary>
    /// Выливает воду из ведра. Можно вызвать и вручную из других скриптов.
    /// </summary>
    public void Pour()
    {
        if (!_isFilled) return;

        _isFilled = false;

        // Скрываем воду
        if (waterVisual != null)
        {
            waterVisual.SetActive(false);
        }

        // Звук
        PlaySound(pourClip);

        Debug.Log("[BucketWater] Вода вылита!");

        if (HitFeedbackUI.Instance != null)
        {
            HitFeedbackUI.Instance.ShowFail("Вода вылита!");
        }
    }

    /// <summary>
    /// Использует воду (например, для полива или квеста).
    /// Вызывается из других скриптов.
    /// </summary>
    public void UseWater()
    {
        if (!_isFilled)
        {
            Debug.Log("[BucketWater] Ведро пустое — нечего использовать");
            return;
        }

        _isFilled = false;

        if (waterVisual != null)
        {
            waterVisual.SetActive(false);
        }

        PlaySound(pourClip);

        Debug.Log("[BucketWater] Вода использована!");

        if (HitFeedbackUI.Instance != null)
        {
            HitFeedbackUI.Instance.ShowComplete("Вода использована!");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

