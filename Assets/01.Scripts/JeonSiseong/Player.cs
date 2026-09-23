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

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            skillManager.UseAttackBuff(transform.position);
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            skillManager.UseFreeze(transform.position);
        }
    }

    //private void FixedUpdate()
    //{
    //    rb.linearVelocity = dir * moveSpeed;
    //}
}
