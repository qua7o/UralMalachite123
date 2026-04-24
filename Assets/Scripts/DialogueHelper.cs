using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DialogueEditor;


/// <summary>
/// Вспомогательный класс для удобной работы с диалогами
/// </summary>
public static class DialogueHelper
{
    /// <summary>
    /// Безопасный запуск диалога с проверками
    /// </summary>
    public static bool TryStart(NPCConversation conversation, string debugName = "")
    {
        if (conversation == null)
        {
            Debug.LogWarning($"[Dialogue] Пустая ссылка на диалог: {debugName}");
            return false;
        }

        if (ConversationManager.Instance == null)
        {
            Debug.LogError("[Dialogue] ConversationManager не найден в сцене!");
            return false;
        }

        if (ConversationManager.Instance.IsConversationActive)
        {
            Debug.Log($"[Dialogue] Уже идёт диалог, пропуск: {debugName}");
            return false;
        }

        ConversationManager.Instance.StartConversation(conversation);
        Debug.Log($"[Dialogue] Запущен: {debugName}");
        return true;
    }

    /// <summary>
    /// Установить параметры для диалога
    /// </summary>
    public static void SetBoolParameter(string paramName, bool value)
    {
        if (ConversationManager.Instance != null)
        {
            ConversationManager.Instance.SetBool(paramName, value);
        }
    }

    public static void SetIntParameter(string paramName, int value)
    {
        if (ConversationManager.Instance != null)
        {
            ConversationManager.Instance.SetInt(paramName, value);
        }
    }

    /// <summary>
    /// Установить несколько параметров сразу
    /// </summary>
    public static void SetParameters(Dictionary<string, object> parameters)
    {
        if (ConversationManager.Instance == null) return;

        foreach (var kvp in parameters)
        {
            if (kvp.Value is bool b)
                ConversationManager.Instance.SetBool(kvp.Key, b);
            else if (kvp.Value is int i)
                ConversationManager.Instance.SetInt(kvp.Key, i);
        }
    }

    /// <summary>
    /// Завершить текущий диалог
    /// </summary>
    public static void EndCurrentDialogue()
    {
        if (ConversationManager.Instance != null)
        {
            ConversationManager.Instance.EndConversation();
        }
    }

    /// <summary>
    /// Проверка: активен ли диалог
    /// </summary>
    public static bool IsDialogueActive()
    {
        return ConversationManager.Instance != null &&
               ConversationManager.Instance.IsConversationActive;
    }
}