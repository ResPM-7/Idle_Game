using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllIn1SpriteShader
{
    public class All1DemoUrpCamMove : MonoBehaviour
    {
        [SerializeField] private float speed = 5;
        [SerializeField] private float keySpeed = 5;
        private Vector2 input = Vector3.zero;
        private Rigidbody2D rb;

        public void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void FixedUpdate()
        {
            input.x = AllIn1InputSystem.GetMouseXAxis();
            input.y = AllIn1InputSystem.GetMouseYAxis();

            // Arrow keys
            if (AllIn1InputSystem.GetKey(KeyCode.RightArrow)) input.x += keySpeed;
            if (AllIn1InputSystem.GetKey(KeyCode.LeftArrow)) input.x -= keySpeed;
            if (AllIn1InputSystem.GetKey(KeyCode.UpArrow)) input.y += keySpeed;
            if (AllIn1InputSystem.GetKey(KeyCode.DownArrow)) input.y -= keySpeed;

            // WASD keys
            if (AllIn1InputSystem.GetKey(KeyCode.D)) input.x += keySpeed;
            if (AllIn1InputSystem.GetKey(KeyCode.A)) input.x -= keySpeed;
            if (AllIn1InputSystem.GetKey(KeyCode.W)) input.y += keySpeed;
            if (AllIn1InputSystem.GetKey(KeyCode.S)) input.y -= keySpeed;

#if UNITY_6000_0_OR_NEWER
			rb.linearVelocity = input * (speed * Time.fixedDeltaTime);
#else
			rb.velocity = input * (speed * Time.fixedDeltaTime);
#endif
		}
	}
}
