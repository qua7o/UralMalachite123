using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("💰 Экономика")]
    public int sparks = 0;
    public TextMeshProUGUI sparksCounterUI;
    public Image sparkIconUI;

    [Header("🎨 Настройки анимации искры")]
    [Tooltip("Цвет вспышки при получении искры")]
    public Color sparkFlashColor = Color.yellow;

    [Tooltip("Цвет вспышки при трате искры")]
    public Color sparkSpendColor = Color.red;

    [Tooltip("Длительность вспышки в секундах")]
    public float flashDuration = 0.15f;

    [Header("📜 Квесты")]
    [Tooltip("Флаг: первая искра добыта")]
    public bool firstGemMined = false;

    [Tooltip("Флаг: диалог о первой искре показан")]
    public bool firstGemDialogueShown = false;

    [Tooltip("НПС дедушка")]
    public NPCController grandfatherNPC;

    [Header("🎯 Прогресс игры")]
    [Tooltip("Всего искр собрано")]
    public int totalSparksCollected = 0;

    [Tooltip("Количество выполненных квестов")]
    public int completedQuests = 0;

    void Awake()
    {
        // Синглтон паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();

        // Скрываем UI искр если их ещё нет
        if (sparksCounterUI)
            sparksCounterUI.gameObject.SetActive(sparks > 0);
        if (sparkIconUI)
            sparkIconUI.gameObject.SetActive(sparks > 0);
    }

    // =============================================
    // === СИСТЕМА ИСКОР (валюты) ===
    // =============================================

    /// <summary>
    /// Добавить искру
    /// </summary>
    public void AddSpark()
    {
        sparks++;
        totalSparksCollected++;

        // Показываем UI при первой искре
        if (sparks == 1)
        {
            if (sparksCounterUI)
                sparksCounterUI.gameObject.SetActive(true);
            if (sparkIconUI)
                sparkIconUI.gameObject.SetActive(true);

            ShowTitle("Первая искра!");
        }

        // Запускаем диалог с дедушкой при первой искре
        if (!firstGemMined)
        {
            firstGemMined = true;
            OnFirstGemObtained();
        }

        // Анимация вспышки
        if (sparkIconUI != null)
        {
            StartCoroutine(FlashSparkCoroutine(sparkFlashColor));
        }

        UpdateUI();
    }

    /// <summary>
    /// Добавить несколько искр сразу
    /// </summary>
    public void AddSparks(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddSpark();
        }
    }

    /// <summary>
    /// Проверка: хватает ли искр на покупку
    /// </summary>
    public bool CanAfford(int cost)
    {
        return sparks >= cost;
    }

    /// <summary>
    /// Списать искры. Возвращает true если успешно
    /// </summary>
    public bool SpendSparks(int cost)
    {
        if (sparks < cost)
        {
            Debug.Log($"[GameManager] Не хватает искорок! Нужно: {cost}, есть: {sparks}");
            ShowTitle("Недостаточно искр!");
            return false;
        }

        sparks -= cost;
        Debug.Log($"[GameManager] Списано {cost} искорок. Осталось: {sparks}");

        // Анимация вспышки красным (трата)
        if (sparkIconUI != null)
        {
            StartCoroutine(FlashSparkCoroutine(sparkSpendColor));
        }

        UpdateUI();
        return true;
    }

    /// <summary>
    /// Получить текущее количество искр
    /// </summary>
    public int GetSparks()
    {
        return sparks;
    }

    // =============================================
    // === СОБЫТИЯ КВЕСТОВ ===
    // =============================================

    /// <summary>
    /// Вызывается при получении первой искры
    /// </summary>
    private void OnFirstGemObtained()
    {
        Debug.Log("[GameManager] Первая искра получена!");

        if ( !firstGemDialogueShown)
        {
            firstGemDialogueShown = true;

            // Небольшая задержка перед диалогом
            StartCoroutine(StartFirstGemDialogueDelayed());
        }
    }

    private IEnumerator StartFirstGemDialogueDelayed()
    {
        yield return new WaitForSeconds(1f);

       
            grandfatherNPC.PlayQuestDialogue("FirstGem");
        
    }

    /// <summary>
    /// Выполнить квест
    /// </summary>
    public void CompleteQuest(string questName)
    {
        completedQuests++;
        Debug.Log($"[GameManager] Квест выполнен: {questName}. Всего: {completedQuests}");
        ShowTitle($"Квест выполнен: {questName}!");
    }

    // =============================================
    // === АНИМАЦИИ И UI ===
    // =============================================

    /// <summary>
    /// Вспышка иконки искры
    /// </summary>
    private IEnumerator FlashSparkCoroutine(Color flashColor)
    {
        if (sparkIconUI == null) yield break;

        Color originalColor = sparkIconUI.color;
        sparkIconUI.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        sparkIconUI.color = originalColor;
    }

    /// <summary>
    /// Показать заголовок/уведомление
    /// </summary>
    public void ShowTitle(string text)
    {
        Debug.Log($"[TITLES] {text}");
        // Здесь можно добавить отображение UI-уведомления
        // Например: titleText.text = text; titleAnimator.Play("Show");
    }

    /// <summary>
    /// Обновить UI
    /// </summary>
    private void UpdateUI()
    {
        if (sparksCounterUI)
        {
            sparksCounterUI.text = $"{sparks}";
        }
    }

    // =============================================
    // === УТИЛИТЫ ===
    // =============================================

    /// <summary>
    /// Сохранить прогресс (заглушка)
    /// </summary>
    public void SaveGame()
    {
        PlayerPrefs.SetInt("Sparks", sparks);
        PlayerPrefs.SetInt("TotalSparksCollected", totalSparksCollected);
        PlayerPrefs.SetInt("CompletedQuests", completedQuests);
        PlayerPrefs.SetInt("FirstGemMined", firstGemMined ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("[GameManager] Игра сохранена");
    }

    /// <summary>
    /// Загрузить прогресс (заглушка)
    /// </summary>
    public void LoadGame()
    {
        sparks = PlayerPrefs.GetInt("Sparks", 0);
        totalSparksCollected = PlayerPrefs.GetInt("TotalSparksCollected", 0);
        completedQuests = PlayerPrefs.GetInt("CompletedQuests", 0);
        firstGemMined = PlayerPrefs.GetInt("FirstGemMined", 0) == 1;

        UpdateUI();
        Debug.Log("[GameManager] Игра загружена");
    }

    /// <summary>
    /// Сбросить прогресс
    /// </summary>
    public void ResetProgress()
    {
        sparks = 0;
        totalSparksCollected = 0;
        completedQuests = 0;
        firstGemMined = false;
        firstGemDialogueShown = false;

        UpdateUI();
        PlayerPrefs.DeleteAll();

        Debug.Log("[GameManager] Прогресс сброшен");
    }

    // =============================================
    // === ОТЛАДКА ===
    // =============================================

#if UNITY_EDITOR
    // Методы для быстрой отладки через Inspector

    [ContextMenu("Add Spark (Debug)")]
    public void Debug_AddSpark()
    {
        AddSpark();
    }

    [ContextMenu("Add 10 Sparks (Debug)")]
    public void Debug_Add10Sparks()
    {
        AddSparks(10);
    }

    [ContextMenu("Start Grandfather Dialogue (Debug)")]
    public void Debug_StartGrandfatherDialogue()
    {
        if (grandfatherNPC != null)
        {
            grandfatherNPC.PlayQuestDialogue("FirstGem");
        }
    }

    [ContextMenu("Save Game (Debug)")]
    public void Debug_SaveGame()
    {
        SaveGame();
    }

    [ContextMenu("Load Game (Debug)")]
    public void Debug_LoadGame()
    {
        LoadGame();
    }
#endif
}