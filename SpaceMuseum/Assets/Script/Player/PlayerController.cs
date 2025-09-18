using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;
    private float jumpForce = 50f; // 점프 힘

    [Header("Ground Check")]
    public Transform groundCheck;   // 지면을 확인할 위치 (캐릭터 발밑)
    public float groundDistance = 0.2f; // 지면 확인 거리
    public LayerMask groundMask;      // 지면으로 인식할 레이어

    [Header("World Wrapping")]
    public float worldSizeX = 300f;
    public float worldSizeZ = 300f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private bool isGrounded; // 캐릭터가 땅에 닿았는지 여부
    private bool jumpRequested = false; // 점프 요청 플래그

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 지면 체크: 매 프레임 캐릭터 발밑에 구체를 생성해 땅과 닿는지 확인
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 입력 처리
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 점프 입력: 땅에 있을 때만 점프 요청을 할 수 있음
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        // 이동 및 회전 처리 (기존과 동일)
        HandleMovementAndRotation();

        // 점프 처리: 물리 업데이트 주기에서 실제 힘을 가함
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false; // 점프 요청 처리 후 플래그 초기화
        }

        // 월드 래핑 처리 (기존과 동일)
        WrapPosition();
    }

    void HandleMovementAndRotation()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;

        if (camForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward, Vector3.up);
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