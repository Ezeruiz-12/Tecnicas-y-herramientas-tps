using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    public Transform firePoint;
    public float vida = 10;
    public float speed = 6f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 2f;
    public float mouseSensitivity = 2f;

    public float maxStamina = 100f;
    public float staminaDrain = 20f;
    public float staminaRecovery = 15f;

    public float weaponRange = 100f;
    public float damage = 25f;
    public int ammo = 10;

    public Transform cameraTransform;
    public float cameraDistance = 5f;
    public float cameraHeight = 1.5f;
    public float minPitch = -30f;
    public float maxPitch = 70f;
    public float turnSpeed = 720f;

    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundMask = ~0;

    [SerializeField] private float jumpBufferDuration = 0.15f;
    [SerializeField] private float coyoteDuration = 0.1f;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private float currentStamina;
    private float cameraPitch = 15f;
    private float yaw;

    private float inputX;
    private float inputZ;
    private bool isSprinting;
    private bool isGrounded;

    private float jumpBufferTimer;
    private float coyoteTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        yaw = transform.eulerAngles.y;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        cameraPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        bool isMoving = inputX != 0f || inputZ != 0f;
        isSprinting = Input.GetKey(KeyCode.LeftShift)
            && isMoving
            && currentStamina > 0f;

        animator.SetBool("Idle", !isMoving);
        animator.SetBool("Runing", isMoving);

        if (isSprinting)
            currentStamina -= staminaDrain * Time.deltaTime;
        else
            currentStamina += staminaRecovery * Time.deltaTime;

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        jumpBufferTimer -= Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferDuration;
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Quaternion orbit = Quaternion.Euler(cameraPitch, yaw, 0f);
        Vector3 pivot = transform.position + Vector3.up * cameraHeight;

        cameraTransform.position = pivot - orbit * Vector3.forward * cameraDistance;
        cameraTransform.rotation = orbit;
    }

    void FixedUpdate()
    {
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 velocity = rb.linearVelocity;

        Bounds bounds = capsule.bounds;

        isGrounded = velocity.y <= 0.1f && Physics.Raycast(
            bounds.center,
            Vector3.down,
            bounds.extents.y + groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);

        if (isGrounded)
            coyoteTimer = coyoteDuration;
        else
            coyoteTimer -= Time.fixedDeltaTime;

        Vector3 move = yawRotation * Vector3.ClampMagnitude(new Vector3(inputX, 0f, inputZ), 1f);

        if (move.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }

        move *= isSprinting ? sprintSpeed : speed;

        velocity.x = move.x;
        velocity.z = move.z;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            animator.SetTrigger("Jumping");

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        rb.linearVelocity = velocity;
    }

    public void RecibirDano(float cantidad)
    {
        vida -= cantidad;
    }
}