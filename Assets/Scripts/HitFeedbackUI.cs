using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Отображает уведомления о прогрессе ударов в углу экрана игрока.
/// Вешается на Canvas, привязанный к VR-камере.
/// </summary>
public class HitFeedbackUI : MonoBehaviour
{
    // === СИНГЛТОН ===
    // Позволяет вызывать HitFeedbackUI.Instance.ShowMessage(...) из любого скрипта
    public static HitFeedbackUI Instance;

    [Header("Ссылки")]
    [Tooltip("Перетащи сюда текстовый элемент HitFeedbackText")]
    public TextMeshProUGUI feedbackText;

    [Header("Настройки")]
    [Tooltip("Время показа сообщения в секундах")]
    public float displayDuration = 2.0f;

    [Tooltip("Цвет текста при удачном ударе")]
    public Color successColor = new Color(0f, 1f, 0.53f, 1f); // Ярко-зелёный #00FF88

    [Tooltip("Цвет текста при промахе")]
    public Color failColor = new Color(1f, 0.3f, 0.3f, 1f); // Красноватый

    [Tooltip("Цвет текста при завершении обработки")]
    public Color completeColor = new Color(1f, 0.84f, 0f, 1f); // Золотой

    // Ссылка на текущую корутину, чтобы можно было прервать предыдущее сообщение
    private Coroutine _hideCoroutine;

    void Awake()
    {
        // Настройка синглтона
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[HitFeedbackUI] Синглтон инициализирован");
        }
        else
        {
            Debug.LogWarning("[HitFeedbackUI] Дубликат синглтона! Удаляю лишний.");
            Destroy(gameObject);
            return;
        }

        // Скрываем текст при старте
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
            Debug.Log("[HitFeedbackUI] Текст скрыт при загрузке");
        }
        else
        {
            Debug.LogError("[HitFeedbackUI] feedbackText НЕ НАЗНАЧЕН! " +
                           "Перетащи TextMeshProUGUI в поле Feedback Text.");
        }
    }

    /// <summary>
    /// Показывает сообщение на экране игрока.
    /// Вызывай из любого скрипта: HitFeedbackUI.Instance.ShowMessage("текст", цвет);
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="color">Цвет текста</param>
    public void ShowMessage(string message, Color color)
    {
        if (feedbackText == null)
        {
            Debug.LogError("[HitFeedbackUI] feedbackText == null! Сообщение не показано.");
            return;
        }

        // Если предыдущее сообщение ещё отображается — прерываем его скрытие
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }

        // Показываем текст
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;

        Debug.Log($"[HitFeedbackUI] Показано на экране: \"{message}\"");

        // Запускаем автоскрытие через displayDuration секунд
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    /// <summary>
    /// Удобные методы-обёртки для типичных ситуаций
    /// </summary>
    public void ShowSuccess(string message)
    {
        ShowMessage(message, successColor);
    }

    public void ShowFail(string message)
    {
        ShowMessage(message, failColor);
    }

    public void ShowComplete(string message)
    {
        ShowMessage(message, completeColor);
    }

    /// <summary>
    /// Корутина: ждёт displayDuration секунд, затем скрывает текст
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
            feedbackText.text = "";
        }

        _hideCoroutine = null;
    }
}
