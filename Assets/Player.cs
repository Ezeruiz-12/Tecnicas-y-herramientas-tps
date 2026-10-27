using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;
    public Transform firePoint;
    public float vida = 10;
    public float speed = 6f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 100f;

    public float maxStamina = 100f;
    public float staminaDrain = 20f;
    public float staminaRecovery = 15f;

    public float weaponRange = 100f;
    public float damage = 25f;
    public int ammo = 10;

    private float currentStamina;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraPitch;

    void Start()
    {
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mirar con el mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Movimiento y estamina
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isMoving = x != 0f || z != 0f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift)
            && isMoving
            && currentStamina > 0f;

        float currentSpeed = isSprinting ? sprintSpeed : speed;

        if (isSprinting)
            currentStamina -= staminaDrain * Time.deltaTime;
        else
            currentStamina += staminaRecovery * Time.deltaTime;

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Disparar
        if (Input.GetMouseButtonDown(0))
            Shoot();

        // Suelo, salto y gravedad
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Shoot()
    {
        if (ammo <= 0)
        {
            Debug.Log("No quedan balas.");
            return;
        }

        ammo--;
        Debug.Log("Balas restantes: " + ammo);

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, weaponRange))
        {
            TargetHealth target = hit.collider.GetComponent<TargetHealth>();
            if (target != null)
                target.TakeDamage(damage);
        }
    }

    public void AddAmmo(int amount)
    {
        ammo += amount;
        Debug.Log("Balas: " + ammo);
    }

    public void RecibirDano(float cantidad)
    {
        vida -= cantidad;
    }
}