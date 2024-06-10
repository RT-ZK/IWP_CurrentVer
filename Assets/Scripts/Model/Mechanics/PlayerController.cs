using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using TMPro;
//using static UnityEditor.Rendering.CameraUI;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        // Max horizontal speed of the player.
        public float maxSpeed = 4.0f;
        public float crouchSpeed = 1.0f;
        public float magBootsSpeed = 1.50f;
        public float baseSpeed = 2.0f;
        private float playerSpeed;

        public bool playerCrouch;
        // Initial jump velocity at the start of a jump.
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        //Mouse UI
        public GameObject Cursor;
        private float cursorLimit = 2.0f;

        //Projectile Prefab
        public GameObject ProjectilePrefab;

        //Fire Rate Timer
        public float fireRate_ex = 3.0f, fireRate_pr = 0.3f, fireRate_tp = 0.5f, fireTimer1 = 0, fireTimer2 = 0, fireTimer3 = 0;

        //Magnetic Boots
        public bool magBoots;

        //Jetpack
        public bool JetpackActivate;
        private float JetpackResourceCap;
        public float JetpackForce;

        //Effectors
        private Vector3 externalForces;

        //Trap Variables
        private bool InAcid;
        private float AcidTimer;

        //Turret Variables
        private Vector3 launchForce;

        //Health UI
        public TMP_Text HealthUI;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            playerSpeed = baseSpeed;
            externalForces = Vector3.zero;
            launchForce = Vector3.zero;
            AcidTimer = 0;
            JetpackResourceCap = 100;

            playerCrouch = false;
            magBoots = false;
            InAcid = false;
        }

        protected override void Start()
        {
            Cursor.transform.position = new Vector2(1, 1);
        }

        protected override void Update()
        {
            HealthUI.text = "Health: " + health.GetHP();

            if (fireTimer1 > 0)
            {
                fireTimer1 -= Time.deltaTime;
            }
            else if (fireTimer2 > 0)
            {
                fireTimer2 -= Time.deltaTime;
            }
            else if (fireTimer3 > 0)
            {
                fireTimer3 -= Time.deltaTime;
            }

            if (InAcid)
            {
                AcidTimer += Time.deltaTime;
                if (AcidTimer >= 1.0f)
                {
                    health.Decrement(1);
                    Debug.Log(health.GetHP());
                    AcidTimer = 0;
                }
            }
            else if (!InAcid)
            {
                AcidTimer = 0;
            }

            if (controlEnabled)
            {
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump") && !playerCrouch && !magBoots)
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    playerSpeed = maxSpeed;
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    playerSpeed = crouchSpeed;
                    playerCrouch = true;
                }
                if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.C))
                {
                    playerSpeed = baseSpeed;
                    playerCrouch = false;
                }
                if (Input.GetKeyUp(KeyCode.M) && !magBoots)
                {
                    playerSpeed = magBootsSpeed;
                    magBoots = true;
                    model.player.body.mass = 0;
                    Debug.Log(magBoots);
                }
                else if (Input.GetKeyUp(KeyCode.M) && magBoots)
                {
                    playerSpeed = baseSpeed;
                    magBoots = false;
                    model.player.body.mass = 1;
                    Debug.Log(magBoots);
                }

                if (magBoots)
                {
                    //Collider Offset (-0.09, -0.14) Collider Scale (0.32, 0.54)
                    //raycastcolliderBottom.y -> -0.14 - ((0.54/2)-0.01)
                    //raycastcolliderLeft.x -> -0.09 - ((0.32/2)+0.01)
                    //raycastcolliderRight.x -> -0.09 + ((0.32/2)-0.01)
                    var raycastColliderBottom = new Vector2(transform.position.x - 0.09f, transform.position.y - 0.4f);
                    var raycastColliderLeft = new Vector2(transform.position.x - 0.26f, transform.position.y - 0.14f);
                    var raycastColliderRight = new Vector2(transform.position.x + 0.06f, transform.position.y - 0.14f);

                    if (Input.GetAxis("Horizontal") != 0)
                    {
                        //move.x = Input.GetAxis("Horizontal");
                        if (Input.GetAxis("Horizontal") > 0)
                        {
                            RaycastHit2D[] downHit = Physics2D.RaycastAll(raycastColliderRight, Vector2.down, 0.5f);

                            int counter = 0;
                            bool isHit = false;
                            while (counter < downHit.Length && !isHit)
                            {
                                if (downHit[counter].collider.GetComponent<TilemapCollider2D>() != null)
                                {
                                    move.x = Input.GetAxis("Horizontal");
                                    isHit = true;
                                }
                                counter++;
                            }
                            /*if (downHit.collider != null)
                            {
                                move.x = Input.GetAxis("Horizontal");
                            }*/
                        }
                        else if (Input.GetAxis("Horizontal") < 0)
                        {
                            RaycastHit2D[] downHit = Physics2D.RaycastAll(raycastColliderLeft, Vector2.down, 0.5f);

                            int counter = 0;
                            bool isHit = false;
                            while (counter < downHit.Length && !isHit)
                            {
                                if (downHit[counter].collider.GetComponent<TilemapCollider2D>() != null)
                                {
                                    move.x = Input.GetAxis("Horizontal");
                                    isHit = true;
                                }
                                counter++;
                            }
                            /*if (downHit.collider != null)
                            {
                                move.x = Input.GetAxis("Horizontal");
                                Debug.Log(downHit.collider);
                            }*/
                        }
                    }
                    else if (Input.GetAxis("Vertical") != 0)
                    {
                        RaycastHit2D[] leftHit = Physics2D.RaycastAll(raycastColliderBottom, Vector2.left, 0.18f);
                        RaycastHit2D[] rightHit = Physics2D.RaycastAll(raycastColliderBottom, Vector2.right, 0.18f);
                        int counter = 0;
                        bool isHit = false;
                        while (counter < leftHit.Length && !isHit)
                        {
                            if (leftHit[counter].collider.GetComponent<TilemapCollider2D>() != null)
                            {
                                move.y = Input.GetAxis("Vertical");
                                isHit = true;
                            }
                            counter++;
                        }
                        counter = 0;
                        while (counter < rightHit.Length && !isHit)
                        {
                            if (rightHit[counter].collider.GetComponent<TilemapCollider2D>() != null)
                            {
                                move.y = Input.GetAxis("Vertical");
                                isHit = true;
                            }
                            counter++;
                        }
                    }
                }
                else
                {
                    move.x = Input.GetAxis("Horizontal");
                }

                if (Input.GetKeyUp(KeyCode.J) && !JetpackActivate)
                {
                    JetpackActivate = true;
                    
                }
                else if (Input.GetKeyUp(KeyCode.J) && JetpackActivate)
                {
                    JetpackActivate = false;
                }


                //Mouse Cursor UI
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var newPos = transform.position;

                float newX = mouseWorldPos.x - transform.position.x;
                float newY = mouseWorldPos.y - (transform.position.y );
                float hyp = Mathf.Sqrt((newX) * (newX) + (newY) * (newY));
                newPos.x = (newX * cursorLimit / hyp) + transform.position.x;
                newPos.y = ((newY * cursorLimit / hyp) + transform.position.y) ;

                Cursor.transform.position = newPos;
                
                //Firing Projectile
                if (Input.GetMouseButtonUp(0))
                {
                    if (fireTimer1 <= 0)
                    {
                        
                        var projectilePos = transform.position;
                        projectilePos.x = (newX * 0.3f / hyp) + transform.position.x;
                        projectilePos.y = ((newY * 0.3f / hyp) + transform.position.y);
                        GameObject gb = Instantiate(ProjectilePrefab, projectilePos, Quaternion.identity);
                        Projectile projectile = gb.GetComponent<Projectile>();
                        projectile.playerScript = this;
                        
                        //Projectile Type
                        projectile.projectileType = 1;

                        //Projectile Damage Value
                        projectile.damageValue = 1;

                        //Projectile Speed
                        float bulletSpeed = 5.5f;
                        var projVel = Vector3.zero;
                        float xVel = bulletSpeed / cursorLimit * (Cursor.transform.position.x - transform.position.x);
                        projVel.x = xVel;
                        float yVel = bulletSpeed / cursorLimit * (Cursor.transform.position.y - transform.position.y);
                        projVel.y = yVel;
                        projectile.initialVel = projVel;

                        fireTimer1 = fireRate_ex;
                    }
                    

                }
                else if (Input.GetMouseButtonUp(1))
                {
                    if (fireTimer2 <= 0)
                    {
                        var projectilePos = transform.position;
                        /*
                        if (newX < 0)
                        {
                            projectilePos.x = Bounds.min.x - 0.11f;
                        }
                        else
                        {
                            projectilePos.x = Bounds.max.x + 0.11f;
                        }
                        if (newY < 0)
                        {
                            projectilePos.y = Bounds.min.y - 0.11f;
                        }
                        else
                        {
                            projectilePos.y = Bounds.max.y + 0.11f;
                        }
                        */
                        projectilePos.x = (newX * 0.3f / hyp) + transform.position.x;
                        projectilePos.y = ((newY * 0.3f / hyp) + transform.position.y);
                        GameObject gb = Instantiate(ProjectilePrefab, projectilePos, Quaternion.identity);
                        Projectile projectile = gb.GetComponent<Projectile>();
                        projectile.playerScript = this;

                        //Projectile Type
                        projectile.projectileType = 2;

                        //Projectile Damage Value
                        projectile.damageValue = 0;

                        //Projectile Speed
                        float bulletSpeed = 5.0f;
                        var projVel = Vector3.zero;
                        float xVel = bulletSpeed / cursorLimit * (Cursor.transform.position.x - transform.position.x);
                        projVel.x = xVel;
                        float yVel = bulletSpeed / cursorLimit * (Cursor.transform.position.y - transform.position.y);
                        projVel.y = yVel;
                        projectile.initialVel = projVel;

                        fireTimer2 = fireRate_pr;
                    }
                }
                else if (Input.GetKeyUp(KeyCode.T))
                {
                    if (fireTimer3 <= 0)
                    {

                        var projectilePos = transform.position;
                        projectilePos.x = (newX * 0.3f / hyp) + transform.position.x;
                        projectilePos.y = ((newY * 0.3f / hyp) + transform.position.y);
                        GameObject gb = Instantiate(ProjectilePrefab, projectilePos, Quaternion.identity);
                        Projectile projectile = gb.GetComponent<Projectile>();
                        projectile.playerScript = this;

                        //Projectile Type
                        projectile.projectileType = 3;

                        //Projectile Damage Value
                        projectile.damageValue = 0;

                        //Projectile Speed
                        float bulletSpeed = 2.5f;
                        var projVel = Vector3.zero;
                        float xVel = bulletSpeed / cursorLimit * (Cursor.transform.position.x - transform.position.x);
                        projVel.x = xVel;
                        float yVel = bulletSpeed / cursorLimit * (Cursor.transform.position.y - transform.position.y);
                        projVel.y = yVel;
                        projectile.initialVel = projVel;

                        fireTimer3 = fireRate_tp;
                    }


                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }


            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / playerSpeed);

            targetVelocity = move * playerSpeed;
        }

        protected override void FixedUpdate()
        {
            
            if(magBoots) //need to add if touching wall
            { 
                IsGrounded = true;  
                velocity.y = targetVelocity.y;
            }
            
            else {
                if (velocity.y < 0)
                    velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
                else
                    velocity += Physics2D.gravity * Time.deltaTime;
                IsGrounded = false;
            }

            velocity.y += externalForces.y * Time.deltaTime + launchForce.y;

            velocity.x = targetVelocity.x + externalForces.x * Time.deltaTime + launchForce.x;

            launchForce.y = Mathf.Sign(launchForce.y) * -1.5f * Time.deltaTime + launchForce.y;

            launchForce.x = Mathf.Sign(launchForce.x) * -1 * Time.deltaTime + launchForce.x;
            
            if (Mathf.Abs(launchForce.y) < 0.05)
            {
                launchForce.y = 0;
            }
            if (Mathf.Abs(launchForce.x) < 0.05)
            {
                launchForce.x = 0;
            }

            var deltaPosition = velocity * Time.deltaTime;

            var moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

            var move = moveAlongGround * deltaPosition.x;

            PerformMovement(move, false);

            move = Vector2.up * deltaPosition.y;

            PerformMovement(move, true);

            externalForces = Vector3.zero;
        }



        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }

        public void GameTeleport(Vector3 velocity, Vector3 position)
        {
            var newVelocity = velocity;
            if (Mathf.Abs(velocity.x * 0.10f) < 0.50f)
            {
                if (velocity.x < 0)
                {
                    newVelocity = new Vector3(newVelocity.x - 1.5f, newVelocity.y, newVelocity.z);
                }
                else if (velocity.x > 0)
                {
                    newVelocity = new Vector3(newVelocity.x + 1.5f, newVelocity.y, newVelocity.z);
                }
            }
            if (Mathf.Abs(velocity.y * 0.10f) < 0.50f)
            {
                if (velocity.y < 0)
                {
                    newVelocity = new Vector3(newVelocity.x, newVelocity.y - 2.5f, newVelocity.z);
                }
                else if (velocity.y > 0)
                {
                    newVelocity = new Vector3(newVelocity.x, newVelocity.y + 2.5f, newVelocity.z);
                }
            }
            model.player.Teleport(position - newVelocity * 0.10f);        
        }

        public void RegisterForce(Vector3 externalForce)
        {
            externalForces += externalForce;
        }

        public void RegisterSelfDamage(int modifier, int damageValue)
        {
            health.Decrement(damageValue * modifier);
        }

        public void RocketJump(Vector3 projposition)
        {
            float dist = Vector3.Distance(transform.position, projposition);
            Debug.Log(dist);
            if (dist < 2)
            {
                float y = Mathf.Clamp(0.7f/(transform.position.y - projposition.y), -0.55f, 0.55f);
                Debug.Log(transform.position.y + ", " + projposition.y);
                float x = Mathf.Clamp(0.7f/ (transform.position.x - projposition.x), -1, 1);
                Debug.Log(x);
                launchForce += (new Vector3(x, y, 0));
                Debug.Log(launchForce);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag.CompareTo("AcidPool") == 0)
            {
                Debug.Log("In Acid");
                InAcid = true;
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag.CompareTo("AcidPool") == 0)
            {
                InAcid = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<Projectile>() != null)
            {
                Projectile collidedProjectile = collision.gameObject.GetComponent<Projectile>();
                if (collidedProjectile.projectileType == 4)
                {
                    //Debug.Log("Projectile Type " + collidedProjectile.projectileType + " hit Player");
                    health.Decrement(collidedProjectile.damageValue);
                    //Debug.Log(health.GetHP());
                }
            }
        }
    }
}