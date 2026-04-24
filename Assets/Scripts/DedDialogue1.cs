using DialogueEditor;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class DedDialogue1 : MonoBehaviour
{
    [SerializeField] NPCConversation DedConversation;

    [Header("Настройки")]
    [Tooltip("Запускать диалог только один раз?")]
    public bool onlyOnce = false;

    private bool _hasTriggered = false;

    private void Start()
    {
        // Проверка при старте — чтобы сразу увидеть проблему
        if (DedConversation == null)
        {
            Debug.LogError($"[DedDialogue1] DedConversation НЕ НАЗНАЧЕН " +
                           $"на объекте '{gameObject.name}'! " +
                           $"Перетащи NPCConversation в Inspector.");
        }

        if (ConversationManager.Instance == null)
        {
            Debug.LogError("[DedDialogue1] ConversationManager.Instance == null! " +
                           "Убедись что в сцене есть объект с компонентом ConversationManager. " +
                           "Обычно это объект 'ConversationManager' из ассета Dialogue Editor.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем что это именно игрок (XR Origin)
        if (other.gameObject.GetComponent<XROrigin>() != null)
        {
            // Если диалог уже был — не запускаем повторно
            if (onlyOnce && _hasTriggered)
            {
                Debug.Log("[DedDialogue1] Диалог уже был запущен ранее. Пропускаем.");
                return;
            }

            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        // === Проверка 1: ConversationManager ===
        if (ConversationManager.Instance == null)
        {
            Debug.LogError("[DedDialogue1] ОШИБКА: ConversationManager.Instance == null!\n" +
                           "РЕШЕНИЕ: Добавь в сцену объект с компонентом ConversationManager.\n" +
                           "1. Hierarchy → Create Empty → назови 'ConversationManager'\n" +
                           "2. Add Component → ConversationManager\n" +
                           "Или найди префаб ConversationManager в папке ассета Dialogue Editor.");
            return;
        }

        // === Проверка 2: DedConversation ===
        if (DedConversation == null)
        {
            Debug.LogError("[DedDialogue1] ОШИБКА: DedConversation == null!\n" +
                           "РЕШЕНИЕ: В Inspector объекта '" + gameObject.name + "'\n" +
                           "перетащи файл NPCConversation в поле 'Ded Conversation'.");
            return;
        }

        // === Всё ок — запускаем диалог ===
        Debug.Log("[DedDialogue1] Запускаю диалог...");
        ConversationManager.Instance.StartConversation(DedConversation);
        _hasTriggered = true;
    }
}
