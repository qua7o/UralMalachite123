using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PDedWalk : MonoBehaviour
{
    private Animator animator;
    public float speed = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
        }
    }

    void Update()
    {
        // Для теста: нажимайте Пробел для ходьбы
        if (Input.GetKey(KeyCode.Space))
        {
            speed = 1f;
        }
        else
        {
            speed = 0f;
        }

        // Передаем скорость в контроллер
        animator.SetFloat("Speed", speed);
    }
}