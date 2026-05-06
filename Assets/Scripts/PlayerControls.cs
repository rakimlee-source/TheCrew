using Unity.VisualScripting;
using System.Collections;
using UnityEngine;
using System;

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
    
    // --- PLATFORM TRACKING VARIABLES ---
    private GameObject currentPlatform;
    private Vector3 lastPlatformPosition;
    private Vector3 platformVelocity;

    // --- KICK SETTINGS ---
    [SerializeField] float kickPower = 18f; 
    [SerializeField] float kickUpwardForce = 0.5f;
    [SerializeField] string kickAnimationName = "Kicking";
    [SerializeField] string movingKickAnimationName = "MovingKick"; 
    private bool isKicking = false;
    [SerializeField] float kickReach = 0.8f; 
    [SerializeField] float kickHitRadius = 0.4f; 

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

        if (wasGrounded && !groundedPlayer) fallStartPosition = transform.position.y;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -groundDrag;
            isJumping = false;
            jumpCount = 0;
        }

        // --- CALCULATE PLATFORM VELOCITY ---
        if (groundedPlayer && currentPlatform != null)
        {
            platformVelocity = currentPlatform.transform.position - lastPlatformPosition;
            lastPlatformPosition = currentPlatform.transform.position;
        }
        else
        {
            platformVelocity = Vector3.zero;
            currentPlatform = null;
        }

        if (!groundedPlayer && transform.position.y < fallStartPosition - maxFallDistance) Respawn();
        if (transform.position.y < deadZoneHeight) Respawn();

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
                currentPlatform = null; // Clear platform on jump

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

        // --- MOVE COMMAND (Includes Platform Velocity) ---
        Vector3 movement = forward * curSpeed;
        movement.y = playerVelocity.y;
        
        // This is the line that fixes the "pulling through objects"
        controller.Move((movement * Time.deltaTime) + platformVelocity);

        if (!isKicking)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                if (isJumping == false)
                {
                    if (Input.GetKey(KeyCode.S)) PlayAnimation("WalkBackwards");
                    else PlayAnimation(isRunning ? "Run" : "Walk");
                }
            }
            else
            {
                if (isJumping == false) PlayAnimation("Idle");
            }
        }

        wasGrounded = groundedPlayer;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // NO PARENTING - Just identify the platform
        if (hit.gameObject.CompareTag("Moving") && hit.normal.y > 0.5f)
        {
            if (currentPlatform != hit.gameObject)
            {
                currentPlatform = hit.gameObject;
                lastPlatformPosition = currentPlatform.transform.position;
            }
        }

        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic || hit.moveDirection.y < -0.3f) return;

        if (hit.gameObject.CompareTag("Kickable") || hit.gameObject.CompareTag("Movable"))
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.AddForce(pushDir * pushPower, ForceMode.VelocityChange);
        }
    }

    // --- COROUTINES AND OTHER METHODS ---

    private IEnumerator PerformKick(bool isMovingForward)
    {
        isKicking = true;
        string animToPlay = isMovingForward ? movingKickAnimationName : kickAnimationName;
        PlayAnimation(animToPlay);
        yield return new WaitForSeconds(0.4f); 
        Vector3 rayStart = transform.position + controller.center - (transform.forward * 0.3f);
        float totalReach = kickReach + 0.3f;
        if (Physics.SphereCast(rayStart, kickHitRadius, transform.forward, out RaycastHit hit, totalReach))
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body != null && !body.isKinematic && hit.collider.CompareTag("Kickable"))
            {
                Vector3 kickDir = (transform.forward + Vector3.up * kickUpwardForce).normalized;
                body.AddForce(kickDir * kickPower, ForceMode.Impulse);
            }
        }
        yield return new WaitForSeconds(0.9f); 
        isKicking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trap") && !isDead) StartCoroutine(Die());
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
        currentPlatform = null; // Important to reset this
        controller.enabled = true;
    }

    void PlayAnimation(string animationName)
    {
        if (currentAnimation != animationName) { animator.Play(animationName); currentAnimation = animationName; }
    }

    void ForceAnimationRestart(string animationName)
    {
        animator.Play(animationName, -1, 0f); currentAnimation = animationName;
    }

    public void UpdateSpawnPoint(Vector3 newPosition, Quaternion newRotation)
    {
        spawnPoint.position = newPosition;
        spawnPoint.rotation = newRotation;
    }
}