using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [SerializeField] SkillManager skillManager;



    Vector2 dir;

    //Rigidbody2D rb;

    //[SerializeField] float moveSpeed = 3f;



    void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        // 모바일/헤드리스 실행에는 키보드 장치가 없을 수 있습니다.
        if (Keyboard.current == null || skillManager == null) return;
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

        // 임시 스킬 사용

        if(Keyboard.current.aKey.wasPressedThisFrame)
        {
            skillManager.UsePoison(transform.position);
        }


        if(Keyboard.current.sKey.wasPressedThisFrame)
        {
            skillManager.UseLightning(transform.position);
        }


        if(Keyboard.current.dKey.wasPressedThisFrame)
        {
            skillManager.UseFire(transform.position);
        }

    }

    //private void FixedUpdate()
    //{
    //    rb.linearVelocity = dir * moveSpeed;
    //}
}
