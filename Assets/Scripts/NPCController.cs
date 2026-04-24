using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DialogueEditor;

public class NPCController : MonoBehaviour
{
    [Header("🎯 Настройки")]
    [Tooltip("Ссылка на игрока (XR Origin)")]
    public Transform playerTransform;

    [Tooltip("Компонент навигации")]
    public NavMeshAgent agent;

    [Tooltip("На каком расстоянии остановиться от игрока")]
    public float stopDistance = 2.5f;

    [Header("💬 Диалоги - Dialogue Editor")]
    [Tooltip("Основной диалог (по умолчанию)")]
    public NPCConversation defaultConversation;

    [Tooltip("Диалог после получения первой искры")]
    public NPCConversation firstGemConversation;

    [Tooltip("Диалог для покупки предметов")]
    public NPCConversation buyItemConversation;

    [Tooltip("Автоматически искать NPCConversation на этом объекте")]
    public bool findConversationOnSelf = true;

    [Header("🥽 VR-опции")]
    [Tooltip("Если false — НПС телепортируется вместо ходьбы")]
    public bool moveTowardsPlayer = false;

    [Tooltip("Дистанция телепорта (если moveTowardsPlayer = false)")]
    public float teleportDistance = 3f;

    [Tooltip("Разворачивать НПС лицом к игроку")]
    public bool facePlayerOnTalk = true;

    [Tooltip("Задержка перед диалогом для комфорта в VR")]
    public float dialogueStartDelay = 0.7f;

    [Header("🎲 Случайные фразы")]
    [Tooltip("Воспроизводить случайные фразы при приближении")]
    public bool playRandomPhrases = false;

    [Tooltip("Минимальное время между фразами")]
    public float minPhraseInterval = 10f;

    [Tooltip("Максимальное время между фразами")]
    public float maxPhraseInterval = 30f;

    [Tooltip("Список диалогов для случайного воспроизведения")]
    public List<NPCConversation> randomPhrases;

    // Приватные переменные
    private bool _isApproachingPlayer = false;
    private string _queuedDialogueID = "";
    private bool _isInDialogue = false;
    private NPCConversation _queuedConversation = null;
    private float _nextRandomPhraseTime = 0f;

    void Start()
    {
        // Инициализация NavMeshAgent
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // Поиск игрока по тегу
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning($"[NPC {gameObject.name}] Игрок с тегом 'Player' не найден!");
        }

        // Авто-поиск диалога на этом объекте
        if (findConversationOnSelf && defaultConversation == null)
            defaultConversation = GetComponent<NPCConversation>();

        if (defaultConversation == null && firstGemConversation == null)
            Debug.LogWarning($"[NPC {gameObject.name}] Не найден ни один NPCConversation!");

        // Подписка на событие завершения диалога
        ConversationManager.OnConversationEnded += OnDialogueEnded;

        // Инициализация случайных фраз
        if (playRandomPhrases && randomPhrases.Count > 0)
            _nextRandomPhraseTime = Time.time + Random.Range(minPhraseInterval, maxPhraseInterval);
    }

    void OnDestroy()
    {
        // Отписка от событий (важно для избежания утечек памяти!)
        ConversationManager.OnConversationEnded -= OnDialogueEnded;
    }

    void Update()
    {
        // Логика приближения к игроку
        if (_isApproachingPlayer && moveTowardsPlayer && !_isInDialogue)
        {
            if (playerTransform != null && agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(playerTransform.position);

                if (Vector3.Distance(transform.position, playerTransform.position) <= stopDistance)
                {
                    StopAndPrepareTalk();
                }
            }
            else
            {
                // Если NavMeshAgent не настроен или не стоит на NavMesh, не зависаем.
                StopAndPrepareTalk();
            }
        }

        // Логика случайных фраз
        if (playRandomPhrases && randomPhrases.Count > 0 && !_isInDialogue)
        {
            if (Time.time >= _nextRandomPhraseTime && playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                if (dist <= 10f) // Только если игрок рядом
                {
                    PlayRandomPhrase();
                }
                _nextRandomPhraseTime = Time.time + Random.Range(minPhraseInterval, maxPhraseInterval);
            }
        }
    }

    /// <summary>
    /// Основной метод запуска диалога по ID
    /// Вызывается из GameManager или других скриптов
    /// </summary>
    public void PlayQuestDialogue(string dialogueID)
    {
        if (_isInDialogue)
        {
            Debug.LogWarning($"[NPC {gameObject.name}] Диалог уже идёт, пропуск: {dialogueID}");
            return;
        }

        // Выбираем нужный диалог по ID
        NPCConversation conversation = GetConversationByID(dialogueID);

        if (conversation == null)
        {
            Debug.LogWarning($"[NPC {gameObject.name}] Диалог '{dialogueID}' не найден! Используем default.");
            conversation = defaultConversation;
        }

        if (conversation == null)
        {
            Debug.LogError($"[NPC {gameObject.name}] Нет доступных диалогов!");
            return;
        }

        Debug.Log($"[NPC {gameObject.name}] → запуск диалога: {dialogueID}");
        StartDialogue(conversation);
    }

    /// <summary>
    /// Запуск конкретного диалога (если нужно передать напрямую)
    /// </summary>
    public void StartDialogue(NPCConversation conversation)
    {
        if (conversation == null)
        {
            Debug.LogError($"[NPC {gameObject.name}] Передан пустой диалог!");
            return;
        }

        _queuedConversation = conversation;

        // Исправление:
        // если moveTowardsPlayer = false, старый код ставил диалог в очередь,
        // но никогда не доходил до StopAndPrepareTalk()/LaunchDialogue().
        if (moveTowardsPlayer && agent != null && playerTransform != null)
        {
            _isApproachingPlayer = true;
            agent.isStopped = false;
        }
        else
        {
            _isApproachingPlayer = false;
            StopAndPrepareTalk();
        }
    }

    /// <summary>
    /// Мгновенный запуск диалога (без приближения)
    /// </summary>
    public void StartDialogueImmediate(NPCConversation conversation)
    {
        if (_isInDialogue || conversation == null) return;

        _queuedConversation = conversation;
        LaunchDialogue();
    }

    private void StopAndPrepareTalk()
    {
        _isApproachingPlayer = false;
        if (agent != null)
            agent.isStopped = true;

        // Телепортация если нужно
        if (!moveTowardsPlayer && playerTransform != null)
        {
            Vector3 spawnPos = playerTransform.position +
                               playerTransform.forward * -teleportDistance +
                               Vector3.up * 0.1f;
            transform.position = spawnPos;
        }

        // Поворот к игроку
        if (facePlayerOnTalk && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        StartCoroutine(StartDialogueWithDelay());
    }

    private IEnumerator StartDialogueWithDelay()
    {
        yield return new WaitForSeconds(dialogueStartDelay);
        LaunchDialogue();
    }

    private void LaunchDialogue()
    {
        if (_queuedConversation == null)
        {
            Debug.LogError($"[NPC {gameObject.name}] Не назначен диалог для запуска!");
            return;
        }

        if (ConversationManager.Instance == null)
        {
            Debug.LogError("[NPC] Не найден ConversationManager в сцене! " +
                         "Добавьте префаб ConversationManager из ассета Dialogue Editor.");
            return;
        }

        _isInDialogue = true;
        Debug.Log($"[NPC {gameObject.name}] Запуск диалога через ConversationManager");

        // Запуск диалога
        ConversationManager.Instance.StartConversation(_queuedConversation);
    }

    /// <summary>
    /// Вызывается автоматически при завершении диалога
    /// </summary>
    private void OnDialogueEnded()
    {
        _isInDialogue = false;
        _queuedDialogueID = "";
        _queuedConversation = null;

        if (agent != null)
            agent.isStopped = false;

        Debug.Log($"[NPC {gameObject.name}] Диалог завершён");
    }

    /// <summary>
    /// Воспроизведение случайной фразы
    /// </summary>
    private void PlayRandomPhrase()
    {
        if (randomPhrases == null || randomPhrases.Count == 0) return;

        NPCConversation randomDialogue = randomPhrases[Random.Range(0, randomPhrases.Count)];
        StartDialogueImmediate(randomDialogue);
    }

    /// <summary>
    /// Получить диалог по ID
    /// </summary>
    private NPCConversation GetConversationByID(string id)
    {
        switch (id.ToLower())
        {
            case "firstgem":
                return firstGemConversation ?? defaultConversation;

            case "buyitem":
                return buyItemConversation ?? defaultConversation;

            case "default":
            default:
                return defaultConversation;
        }
    }

    /// <summary>
    /// Принудительный сброс состояния (если диалог прервался)
    /// </summary>
    public void ResetDialogueState()
    {
        _isInDialogue = false;
        _isApproachingPlayer = false;
        _queuedDialogueID = "";
        _queuedConversation = null;

        if (agent != null)
            agent.isStopped = false;
    }

    /// <summary>
    /// Проверка: можно ли начать диалог
    /// </summary>
    public bool CanStartDialogue()
    {
        return !_isInDialogue &&
               ConversationManager.Instance != null &&
               (defaultConversation != null || firstGemConversation != null);
    }

    /// <summary>
    /// Интеракция с НПсом (вызывать из XR Ray Interactor)
    /// </summary>
    public void OnInteract()
    {
        if (!CanStartDialogue()) return;

        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (dist <= stopDistance * 1.5f)
            {
                // Игрок близко — сразу диалог
                NPCConversation conv = defaultConversation ?? firstGemConversation;
                StartDialogueImmediate(conv);
            }
            else
            {
                // Игрок далеко — сначала подойти
                StartDialogue(defaultConversation ?? firstGemConversation);
            }
        }
    }

#if UNITY_EDITOR
    // Отладочная информация в Editor
    private void OnDrawGizmosSelected()
    {
        // Показываем радиус взаимодействия
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Показываем направление на игрока
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
#endif
}