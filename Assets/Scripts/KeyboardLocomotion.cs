using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class KeyboardLocomotion : MonoBehaviour
{
    public float moveSpeed = 3f;
    private CharacterController controller; // ← поле класса
    private Transform head;

    void Start()
    {
        controller = GetComponent<CharacterController>(); // ✅ Без повторного объявления
        head = GetComponentInChildren<Camera>().transform;
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = head.right * x + head.forward * z;
        move.y = 0;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}
    

