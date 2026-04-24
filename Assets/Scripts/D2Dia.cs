using DialogueEditor;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// Запускает диалог деда.
/// В текущей сцене этот компонент уже висит на ded5 и у него уже назначен DedConversation,
/// поэтому после замены скрипта диалог сможет стартовать сам при запуске сцены.
/// </summary>
public class D2Dia : MonoBehaviour
{
    [SerializeField] private NPCConversation DedConversation;

    [Header("Автозапуск")]
    [Tooltip("Запустить диалог автоматически при старте сцены")]
    public bool playOnStart = true;

    [Tooltip("Задержка перед стартом диалога, чтобы успел проснуться ConversationManager")]
    public float startDelay = 0.8f;

    [Header("Поведение")]
    [Tooltip("Запускать диалог при входе игрока в триггер")]
    public bool playOnTrigger = true;

    [Tooltip("Запускать только один раз")]
    public bool onlyOnce = true;

    private bool _hasStarted;

    private IEnumerator Start()
    {
        yield return null;

        if (DedConversation == null)
        {
            Debug.LogError($"[D2Dia] DedConversation НЕ назначен на объекте '{gameObject.name}'. Перетащи NPCConversation в поле Ded Conversation.");
            yield break;
        }

        if (ConversationManager.Instance == null)
        {
            Debug.LogError("[D2Dia] ConversationManager.Instance == null. В сцене должен быть объект ConversationManager.");
            yield break;
        }

        if (playOnStart)
        {
            yield return new WaitForSeconds(startDelay);
            TryStartDialogue("Start");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!playOnTrigger)
            return;

        bool isPlayer =
            other.GetComponentInParent<XROrigin>() != null ||
            other.CompareTag("Player") ||
            other.GetComponentInParent<Camera>() != null;

        if (isPlayer)
        {
            TryStartDialogue("Trigger");
        }
    }

    private void TryStartDialogue(string source)
    {
        if (onlyOnce && _hasStarted)
        {
            Debug.Log($"[D2Dia] Диалог уже запускался, пропуск. Source: {source}");
            return;
        }

        if (DedConversation == null)
        {
            Debug.LogError($"[D2Dia] Нельзя запустить диалог: DedConversation == null на '{gameObject.name}'.");
            return;
        }

        if (ConversationManager.Instance == null)
        {
            Debug.LogError("[D2Dia] Нельзя запустить диалог: ConversationManager.Instance == null.");
            return;
        }

        if (ConversationManager.Instance.IsConversationActive)
        {
            Debug.Log($"[D2Dia] Уже идёт другой диалог, пропуск. Source: {source}");
            return;
        }

        Debug.Log($"[D2Dia] Запускаю диалог деда. Source: {source}");
        ConversationManager.Instance.StartConversation(DedConversation);
        _hasStarted = true;
    }
}
