using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MiningRock : MonoBehaviour
{
    [Header("Настройки UI")]
    public Canvas worldCanvas;
    public Slider miningSlider;

    [Header("Геймплей")]
    public float sliderSpeed = 2f;
    [Range(0.1f, 0.5f)]
    public float successZoneSize = 0.2f;
    public int requiredHits = 3;

    [Header("Результат")]
    public GameObject rawRockVisual;
    public GameObject gemPrefab;
    public Transform spawnPoint;

    [Header("Аудио")]
    public AudioSource audioSource;
    public AudioClip hitSuccessClip;
    public AudioClip hitFailClip;
    public AudioClip rockBreakClip;

    [Header("События")]
    public UnityEvent OnMiningComplete;

    // Внутренние переменные
    private bool _isMinigameActive = false;
    private float _currentSliderValue = 0f;
    private int _currentSuccessHits = 0;
    private float _timeCounter = 0f;

    void Awake()
    {
        if (worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(false);
            Debug.Log("[MiningRock] Canvas скрыт при загрузке сцены");
        }
        else
        {
            Debug.LogError("[MiningRock] World Canvas НЕ НАЗНАЧЕН в Inspector!");
        }
    }

    void Start()
    {
        Debug.Log($"[MiningRock] Инициализация на объекте: {gameObject.name}");

        if (miningSlider == null)
        {
            Debug.LogError("[MiningRock] miningSlider НЕ НАЗНАЧЕН в Inspector!");
        }
        else
        {
            miningSlider.minValue = 0f;
            miningSlider.maxValue = 1f;
            miningSlider.wholeNumbers = false;
            miningSlider.interactable = false;
            miningSlider.value = 0f;
        }

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("[MiningRock] НЕТ КОЛЛАЙДЕРА на камне!");
        }
        else
        {
            if (col.isTrigger)
            {
                Debug.LogWarning("[MiningRock] Коллайдер помечен как Trigger! Сними галочку Is Trigger.");
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("[MiningRock] Нет Rigidbody. Добавь с Is Kinematic = true.");
        }

        if (HitFeedbackUI.Instance == null)
        {
            Debug.LogWarning("[MiningRock] HitFeedbackUI.Instance == null! Уведомления не будут работать.");
        }
        else
        {
            Debug.Log("[MiningRock] HitFeedbackUI найден — уведомления работают");
        }
    }

    void Update()
    {
        if (_isMinigameActive && miningSlider != null)
        {
            _timeCounter += Time.deltaTime * sliderSpeed;
            _currentSliderValue = (Mathf.Sin(_timeCounter) + 1f) / 2f;
            miningSlider.value = _currentSliderValue;
        }
    }

    public void RegisterHit()
    {
        Debug.Log($"[MiningRock] RegisterHit() вызван! Активна: {_isMinigameActive}");

        if (!_isMinigameActive)
        {
            StartMinigame();
            return;
        }

        CheckHitAccuracy();
    }

    private void StartMinigame()
    {
        _isMinigameActive = true;
        _currentSuccessHits = 0;
        _timeCounter = 0f;
        _currentSliderValue = 0f;

        if (worldCanvas != null) worldCanvas.gameObject.SetActive(true);
        if (miningSlider != null) miningSlider.value = 0f;

        Debug.Log("[MiningRock] === МИНИ-ИГРА НАЧАЛАСЬ ===");

        if (HitFeedbackUI.Instance != null)
        {
            HitFeedbackUI.Instance.ShowMessage("Мини-игра началась! Бей в зелёную зону!", Color.white);
        }
    }

    private void CheckHitAccuracy()
    {
        float minSuccess = 0.5f - (successZoneSize / 2f);
        float maxSuccess = 0.5f + (successZoneSize / 2f);

        Debug.Log($"[MiningRock] Проверка: ползунок={_currentSliderValue:F3}, зона=[{minSuccess:F3}—{maxSuccess:F3}]");

        if (_currentSliderValue >= minSuccess && _currentSliderValue <= maxSuccess)
        {
            // ✅ УДАЧНЫЙ УДАР
            _currentSuccessHits++;
            PlaySound(hitSuccessClip);

            Debug.Log($"[MiningRock] ✅ УДАЧНЫЙ УДАР! ({_currentSuccessHits}/{requiredHits})");

            if (HitFeedbackUI.Instance != null)
            {
                HitFeedbackUI.Instance.ShowSuccess($"УДАЧНЫЙ УДАР {_currentSuccessHits}/{requiredHits}!");
            }

            if (_currentSuccessHits >= requiredHits)
            {
                CompleteMining();
            }
        }
        else
        {
            // ❌ ПРОМАХ
            _currentSuccessHits = Mathf.Max(0, _currentSuccessHits - 1);
            PlaySound(hitFailClip);

            Debug.Log($"[MiningRock] ❌ МИМО! Прогресс: {_currentSuccessHits}");

            if (HitFeedbackUI.Instance != null)
            {
                string failMessage = _currentSuccessHits > 0
                    ? $"МИМО! Прогресс: {_currentSuccessHits}/{requiredHits}"
                    : "МИМО! Прогресс сброшен!";
                HitFeedbackUI.Instance.ShowFail(failMessage);
            }
        }
    }

    private void CompleteMining()
    {
        Debug.Log("[MiningRock] === КАМЕНЬ ОБРАБОТАН! ===");
        _isMinigameActive = false;

        if (worldCanvas != null) worldCanvas.gameObject.SetActive(false);
        PlaySound(rockBreakClip);

        if (HitFeedbackUI.Instance != null)
        {
            HitFeedbackUI.Instance.ShowComplete("КАМЕНЬ ОБРАБОТАН! +1 ИСКРА");
        }

        if (rawRockVisual != null) rawRockVisual.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (gemPrefab != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(gemPrefab, spawnPos, Quaternion.identity);
            Debug.Log("[MiningRock] Самоцвет заспавнен!");
        }

        OnMiningComplete?.Invoke();

        // >>> Вызов GameManager для добавления искры
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddSpark();
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