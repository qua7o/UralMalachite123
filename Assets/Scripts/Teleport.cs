using System.Collections;
using System; // для Action, если используется
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Teleporter : MonoBehaviour
{
    [Header("Координаты назначения")]
    public GameObject destination;
    public bool useYRotation = true;

    [Header("Эффект затемнения")]
    public CanvasGroup fadeOverlay; // Ссылка на CanvasGroup вашего UI-элемента
    public float fadeDuration = 0.9f; // Длительность затухания/появления

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Зашел в триггер " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Обнаружен " + other.name);
            StartCoroutine(TeleportWithFade(other.gameObject));
        }
    }

    IEnumerator TeleportWithFade(GameObject player)
    {
        // Затемнение экрана
        yield return FadeTo(1f); // затемнить

        // Выполняем телепортацию
        TeleportXR(player);

        // Возвращаем нормальную видимость
        yield return FadeTo(0f); // осветлить
    }

    void TeleportXR(GameObject player)
    {
        Debug.Log("Hello: " + player.name + " " + destination);
        if (destination != null)
        {
            player.transform.position = destination.transform.position;

            if (useYRotation)
            {
                Vector3 newRotation = player.transform.eulerAngles;
                newRotation.y = destination.transform.eulerAngles.y;
                player.transform.eulerAngles = newRotation;
            }
        }
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeOverlay == null)
        {
            Debug.LogWarning("FadeOverlay не назначен! Эффект затемнения отключен.");
            yield break;
        }

        float startAlpha = fadeOverlay.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeOverlay.alpha = targetAlpha;
    }
}