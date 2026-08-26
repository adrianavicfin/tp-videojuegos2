using UnityEngine;
//using UnityEngine.InputSystem;

namespace CosmosCritters
{

    public class movement : MonoBehaviour
    {
        public float moveForce;
        public float maxVel = 5f;
        public float jumpForce;
        public Rigidbody2D rb;

        private float inputMove;
        private bool isGrounded;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputMove = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            //inputMove = 0f;

            //if (Keyboard.current != null)
            //{
            //    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            //    {
            //        inputMove -= 1f;
            //    }

            //    if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            //    {
            //        inputMove += 1f;
            //    }
            //}

            //if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            //{
            //    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            //    isGrounded = false;
            //}

            //if (inputMove != 0f)
            //{
            //    rb.AddForce(Vector2.right * inputMove * moveForce);
            //}

            //rb.linearVelocity = new Vector2(
            //    Mathf.Clamp(rb.linearVelocity.x, -maxVel, maxVel),
            //    rb.linearVelocity.y
            //);
        }



        void OnCollisionStay2D(Collision2D collision)
        {
            isGrounded = true;
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            isGrounded = false;
        }

    }
}
