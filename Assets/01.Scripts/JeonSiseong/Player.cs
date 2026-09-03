using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    Vector2 dir;

    Rigidbody2D rb;

    [SerializeField] float moveSpeed = 3f;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        dir = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)
            dir += Vector2.up;
        if (Keyboard.current.leftArrowKey.isPressed)
            dir += Vector2.left;
        if (Keyboard.current.rightArrowKey.isPressed)
            dir += Vector2.right;
        if (Keyboard.current.downArrowKey.isPressed)
            dir += Vector2.down;

        dir = dir.normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = dir * moveSpeed;
    }
}
