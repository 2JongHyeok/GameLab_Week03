using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;      // 걷는 속도
    public float sprintSpeed = 10f;   // 달리기 속도
    private float rotationSpeed = 5f;
    public float jumpForce = 7f;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
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

        WrapPosition();
    }

    void HandleMovementAndRotation()
    {
        // --- [수정] 현재 속도를 걷기/달리기 상태에 따라 결정 ---
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

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

    void WrapPosition()
    {
        Vector3 currentPosition = rb.position;
        float worldWidth = worldSizeX * 2;
        float worldDepth = worldSizeZ * 2;

        if (currentPosition.x > worldSizeX) currentPosition.x -= worldWidth;
        else if (currentPosition.x < -worldSizeX) currentPosition.x += worldWidth;

        if (currentPosition.z > worldSizeZ) currentPosition.z -= worldDepth;
        else if (currentPosition.z < -worldSizeZ) currentPosition.z += worldDepth;

        rb.position = currentPosition;
    }
}