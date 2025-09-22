using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    [Header("Movement")]
    private float rotationSpeed = 5f;
    public float jumpForce = 7f;
    private float originalMoveSpeed;
    private float currentSpeedMultiplier = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("World Wrapping")]
    public float worldSizeX = 300f;
    public float worldSizeZ = 300f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool jumpRequested = false;
    private bool isSprinting = false; // 달리기 상태 변수
    private bool isAlive = true;

    [Header("DangerousPlant")]
    private bool trapLaunchRequested = false;
    private float trapLaunchForce = 0f;
    private bool wasLaunchedByTrap = false; // 함정 발사 여부 추적
    private float landingDamage = 50f;
    [SerializeField] private PlayerHealth playerHealth;

    InGameManager gm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        gm = InGameManager.Instance;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
        originalMoveSpeed = gm.moveSpeed;
    }

    void Update()
    {
        // --- 기존 로직 ---
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        // --- [추가] 달리기 입력 체크 ---
        // 왼쪽 Shift 키를 누르고 있으면 달리기 상태로 변경
        isSprinting = Input.GetKey(KeyCode.LeftShift);
    }

    void FixedUpdate()
    {
        HandleMovementAndRotation();

        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }

        if (trapLaunchRequested)
        {
            rb.AddForce(Vector3.up * trapLaunchForce, ForceMode.Impulse);
            trapLaunchRequested = false;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (wasLaunchedByTrap && collision.gameObject.CompareTag("Ground"))
        {
            playerHealth?.TakeDamage(landingDamage);

            wasLaunchedByTrap = false; // 다시 초기화
        }
    }

    public void RequestTrapLaunch(float force)
    {
        trapLaunchForce = force;
        trapLaunchRequested = true;
        wasLaunchedByTrap = true;
    }
    public void ApplyWeightPenalty(float currentWeight, float maxWeight)
    {
        if (currentWeight <= maxWeight)
        {
            currentSpeedMultiplier = 1f; // 최대 무게 이하면 정상 속도
            return;
        }

        float overloadRatio = (currentWeight - maxWeight) / maxWeight;

        if (overloadRatio <= 0.51f) // 1% ~ 49% 초과
        {
            currentSpeedMultiplier = 0.5f; // 이동속도 50% 감소
        }
        else // 50% 이상 초과
        {
            currentSpeedMultiplier = 0.1f; // 이동속도 90% 감소
        }
    }
    void HandleMovementAndRotation()
    {
        // --- [수정] 현재 속도를 걷기/달리기 상태에 따라 결정 ---
        float baseSpeed = isSprinting ? gm.moveSpeed * 1.5f : gm.moveSpeed;
        float currentSpeed = baseSpeed * currentSpeedMultiplier;
        if (!isAlive)
        {
            currentSpeed = 0f;
        }
        // 카메라 기준 이동 방향 계산 (기존과 동일)
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        // 이동 적용 (수정된 속도 사용)
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;


        // --- [수정] 회전 로직 변경 ---
        // 이동 방향이 있을 때만(키를 누를 때만) 해당 방향을 바라보도록 회전
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }
    public void SetControllable(bool _isAlive)
    {
        isAlive = _isAlive;
    }

    public void PlayerDead()
    {
          transform.position = new Vector3(10, 1, 0); // fallback 위치
    }


}