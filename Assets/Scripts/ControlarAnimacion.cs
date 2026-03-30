using UnityEngine;
using UnityEngine.InputSystem; // ¡IMPORTANTE! Añade esta línea

public class ControlarAnimacion : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // En el nuevo sistema se detecta así:
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            Debug.Log("Pulsada la X con el nuevo sistema");
            animator.Play("Animar_ready");
        }
    }
}
