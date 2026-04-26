using Unity.VisualScripting;
using System.Collections;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] Vector3 playerVelocity;
    [SerializeField] bool groundedPlayer;
    [SerializeField] float gravityValue;
    [SerializeField] GameObject activeChar;
    [SerializeField] float speed = 4;
    [SerializeField] float runSpeed = 7;
    [SerializeField] float rotateSpeed = 2;
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] bool isJumping;
    [SerializeField] float backwardsSpeedMultiplier = 0.5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float backwardsJumpForce = 7f;
    [SerializeField] float doubleJumpForce = 12f;
    [SerializeField] float groundDrag = 0.5f;

    [SerializeField] int jumpCount = 0;
    [SerializeField] int maxJumps = 2;
    
    [SerializeField] float pushPower = 2.0f; 
    
    // --- KICK SETTINGS ---
    [SerializeField] float kickPower = 18f; 
    [SerializeField] float kickUpwardForce = 0.5f;
    [SerializeField] string kickAnimationName = "Kicking";
    [SerializeField] string movingKickAnimationName = "MovingKick"; 
    private bool isKicking = false;
    [SerializeField] float kickReach = 0.8f; // Your preferred setting
    [SerializeField] float kickHitRadius = 0.4f; // Your preferred setting

    [SerializeField] Transform spawnPoint;
    [SerializeField] float deadZoneHeight = -20f;
    [SerializeField] float maxFallDistance = 30f;

    [SerializeField] string deathAnimationName = "Falling Back Death";
    [SerializeField] float deathDelay = 2.0f;
    private bool isDead = false;

    private Animator animator;
    private string currentAnimation = "";
    private float fallStartPosition;
    private bool wasGrounded;

    void Start()
    {
        // FIX: Allows SphereCast to detect collisions even if the player is touching the object
        Physics.queriesHitBackfaces = true; 

        gravityValue = -20;
        animator = activeChar.GetComponent<Animator>();

        if (spawnPoint == null)
        {
            GameObject tempSpawn = new GameObject("DefaultSpawnPoint");
            tempSpawn.transform.position = transform.position;
            spawnPoint = tempSpawn.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        groundedPlayer = controller.isGrounded;

        if (wasGrounded && !groundedPlayer)
        {
            fallStartPosition = transform.position.y;
        }

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -groundDrag;
            isJumping = false;
            jumpCount = 0;
        }

        if (!groundedPlayer && transform.position.y < fallStartPosition - maxFallDistance)
        {
            Respawn();
        }

        if (transform.position.y < deadZoneHeight)
        {
            Respawn();
        }

        playerVelocity.y += gravityValue * Time.deltaTime;

        float verticalInput = 0;
        if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
        if (Input.GetKey(KeyCode.S)) verticalInput = -1f;

        if (Input.GetKeyDown(KeyCode.Return) && !isKicking && groundedPlayer && verticalInput >= 0)
        {
            StartCoroutine(PerformKick(verticalInput > 0));
        }

        float rotationInput = 0;
        if (Input.GetKey(KeyCode.A)) rotationInput = -1f;
        if (Input.GetKey(KeyCode.D)) rotationInput = 1f;
        transform.Rotate(0, rotationInput * rotateSpeed, 0);

        Vector3 forward = transform.TransformDirection(Vector3.forward);

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0;
        float currentSpeed = isRunning ? runSpeed : speed;
        float speedMultiplier = verticalInput < 0 ? backwardsSpeedMultiplier : 1f;
        float curSpeed = currentSpeed * verticalInput * speedMultiplier;

        if (!isKicking)
        {
            animator.SetFloat("Direction", verticalInput);
            animator.SetBool("IsRunning", isRunning);

            if (Input.GetButtonDown("Jump") && (groundedPlayer || jumpCount < maxJumps))
            {
                isJumping = true;
                jumpCount++;

                if (jumpCount > 1)
                {
                    playerVelocity.y = doubleJumpForce;
                    ForceAnimationRestart("Jump");
                }
                else
                {
                    if (verticalInput < 0)
                    {
                        PlayAnimation("JumpBackwards");
                        playerVelocity.y = backwardsJumpForce;
                        jumpCount = maxJumps;
                    }
                    else
                    {
                        PlayAnimation("Jump");
                        playerVelocity.y = jumpHeight * jumpForce;
                    }
                }
            }
        }

        Vector3 movement = forward * curSpeed;
        movement.y = playerVelocity.y;
        controller.Move(movement * Time.deltaTime);

        if (!isKicking)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                controller.minMoveDistance = 0.001f;
                if (isJumping == false)
                {
                    if (Input.GetKey(KeyCode.S)) PlayAnimation("WalkBackwards");
                    else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) 
                        PlayAnimation(isRunning ? "Run" : "Walk");
                }
            }
            else
            {
                controller.minMoveDistance = 0.001f;
                if (isJumping == false) PlayAnimation("Idle");
            }
        }

        wasGrounded = groundedPlayer;
    }

    private IEnumerator PerformKick(bool isMovingForward)
    {
        isKicking = true;
        string animToPlay = isMovingForward ? movingKickAnimationName : kickAnimationName;
        PlayAnimation(animToPlay);
        
        yield return new WaitForSeconds(0.4f); 

        RaycastHit hit;
        
        // 1. Start the ray SLIGHTLY BEHIND the center of the player
        // This ensures that even if you are touching the ball, the sphere "sweeps" into it.
        Vector3 rayStart = transform.position + controller.center - (transform.forward * 0.3f);
        
        // 2. We increase the reach to cover the 0.3f we moved back, plus your desired 1.0f reach
        float totalReach = kickReach + 0.3f;

        // 3. Perform the sweep
        if (Physics.SphereCast(rayStart, kickHitRadius, transform.forward, out hit, totalReach))
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body != null && !body.isKinematic && hit.collider.CompareTag("Kickable"))
            {
                // Force calculation
                Vector3 kickDir = (transform.forward + Vector3.up * kickUpwardForce).normalized;
                body.AddForce(kickDir * kickPower, ForceMode.Impulse);
            }
        }
    
        yield return new WaitForSeconds(0.9f); 
        isKicking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trap") && !isDead)
        {
            StartCoroutine(Die());
        }
    }

    public IEnumerator Die()
    {
        isDead = true;
        PlayAnimation(deathAnimationName);
        yield return new WaitForSeconds(deathDelay);
        Respawn();
        isDead = false;
        PlayAnimation("Idle");
    }

    private void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        playerVelocity = Vector3.zero;
        controller.enabled = true;
    }

    void PlayAnimation(string animationName)
    {
        if (currentAnimation != animationName)
        {
            animator.Play(animationName);
            currentAnimation = animationName;
        }
    }

    void ForceAnimationRestart(string animationName)
    {
        animator.Play(animationName, -1, 0f);
        currentAnimation = animationName;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;

        bool isKickable = hit.gameObject.CompareTag("Kickable");
        bool isMovable = hit.gameObject.CompareTag("Movable");

        if (!isKicking && (isMovable || isKickable))
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.AddForce(pushDir * pushPower, ForceMode.VelocityChange);
        }
        else if (isKicking && isMovable)
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.AddForce(pushDir * pushPower, ForceMode.VelocityChange);
        }
    }
}