using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [SerializeField] private float m_JumpForce = 600f;                  // Amount of force added when the player jumps.
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.
		private float vsp;					// Vertical speed of the character
		private float vspLimit = 10f;		// Maximum/minimum vertical speed limit of the character
		private float gravity = 1f;         // Gravity multiplier
        
        public Text timerText; //Text that is displayed in top right of level
        private float timer; //Keeps track of time spent on level
        private int seconds;
        private int minutes;
        private int hours;
        private String minString; 
        private String secString;

        private int maxFallTime = 10;        // The maximum amount of seconds to spend before player falls to their death
        private bool fallCheck = false;     // whether we are checking if the player is falling or not

        void Start()
        {
            timer = 0.0f;
            //minutes = 0; //minutes needs to be initialised f
        }

        private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }


        private void FixedUpdate()
        {
            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                    m_Grounded = true;
            }
            m_Anim.SetBool("Ground", m_Grounded);

            // Set the vertical animation
            m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
            UpdateTimer();
            
        }

		public void Move(float move, bool crouch, bool jump, int flipped)
		{
            // Check if the player is falling indefinetly
            if(!m_Grounded)
                CheckFalling();
            // Set gravity to the value of flipped
            m_Rigidbody2D.gravityScale = -flipped * System.Math.Abs(m_Rigidbody2D.gravityScale);
			vsp = m_Rigidbody2D.velocity.y;
			// Check if the vertical speed will be between the upper and lower limits
			if ((vsp - flipped)<vspLimit && (vsp + flipped)>-vspLimit)
			{
				vsp = (flipped * gravity) + m_Rigidbody2D.velocity.y;
			}
            // If crouching, check to see if the character can stand up
            if (!crouch && m_Anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            m_Anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_AirControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = (crouch ? move*m_CrouchSpeed : move);

                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", Mathf.Abs(move));

				// Move the character (If the character is flipped then y-value(vsp) is negative)
				m_Rigidbody2D.velocity = new Vector2(move*m_MaxSpeed, vsp);


                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight)
                {
                    // ... flip the player.

                    Flip();
                }
                    // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (m_Grounded && jump && m_Anim.GetBool("Ground"))
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Anim.SetBool("Ground", false);

				// Add force to the vector based on whether player is flipped or not.
				if (flipped == -1)
					m_Rigidbody2D.AddForce (new Vector2 (0f, flipped * -m_JumpForce));
				else if (flipped == 1)
					m_Rigidbody2D.AddForce (new Vector2 (0f, flipped * -m_JumpForce*1.1f));
            }
        }
        

        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("EndTag")) //spaceship
                GameManager.LevelSuccess(hours, minutes, seconds);
            else if (other.gameObject.CompareTag("PickUp"))
                other.gameObject.SetActive(false);
        }

        void UpdateTimer()
        {
            //deltaTime is the amount of time the last frame took
            timer += Time.deltaTime;
            seconds = (int)timer - (minutes * 60); //total seconds, minus complete minutes
            minutes = ((int)timer) / 60;
            hours = ((int)timer) / (60*60);
            if (seconds < 10) //makes string on screen appear as '09' instead of '9' if 9 seconds have passed
                secString = TimeScript.timeFormatter(seconds);
            else
                secString = seconds.ToString();
            if (minutes < 10)
                minString = TimeScript.timeFormatter(minutes);
            else
                minString = minutes.ToString();
            timerText.text = minString + ":" + secString;
        }

        /**
         * Check if the player has been falling to their death for longer
         * than the maximum falling time.
         *
         */
        IEnumerator FallingToDeath()
        {
            // start the counter at 1 second
            int counter = 1;
            while(counter <= maxFallTime && fallCheck)
            {
                // if the player lands
                if (m_Grounded)
                {
                    fallCheck = false;
                    yield return false;
                }
                // the player is falling
                else
                {
                    yield return new WaitForSeconds(1);
                    counter++;
                }
            }
            if (fallCheck)
            {
                fallCheck = false;
                GameManager.PlayerDeath();
                yield return true;
            }
            
        }
        
        /** start the Falling coroutine if not locked.
         *
         */
        void CheckFalling()
        {
            if (!fallCheck)
            {
                fallCheck = true;
                StartCoroutine(FallingToDeath());
            }
        }
    }
}