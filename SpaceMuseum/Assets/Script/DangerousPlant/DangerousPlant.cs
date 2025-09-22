using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))] // Kinematic 권장
public abstract class DangerousPlant : MonoBehaviour
{
    [Header("공통 설정")]
    public float attackCooldown = 3f;
    public LayerMask playerLayer;

    protected Transform player;                // 범위 내 플레이어 루트
    protected PlayerController pc;
    [SerializeField] protected PlayerHealth ph; // 필요 시 직접 할당 가능

    protected bool isPlayerInRange = false;

    private float nextAttackTime = 0f;
    private int playerOverlapCount = 0;        // 다중 콜라이더 대응
    private int playerLayerMaskValue;          // 미세 최적화

    protected virtual void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // 트리거 충돌만 받을 것

        playerLayerMaskValue = playerLayer.value;
    }

    protected virtual void Start()
    {
        pc = PlayerController.Instance;
    }

    protected virtual void OnEnable()
    {
        if (PlantManager.Instance != null)
            PlantManager.Instance.RegisterPlant(this);
    }

    protected virtual void OnDisable()
    {
        if (PlantManager.Instance != null)
            PlantManager.Instance.UnregisterPlant(this);
        // 상태 리셋(안전)
        player = null;
        ph = null;
        isPlayerInRange = false;
        playerOverlapCount = 0;
    }

    // PlantManager가 매 프레임 호출
    public void OnUpdateTick()
    {
        Debug.Log("업데이트 불림.");
        // 플레이어가 비활성화/파괴로 사라진 경우 즉시 정리
        if (isPlayerInRange && (player == null || !player.gameObject.activeInHierarchy))
        {
            Debug.Log("플레이어 사라짐.");

            isPlayerInRange = false;
            playerOverlapCount = 0;
            ph = null;
        }

        if (isPlayerInRange && Time.time >= nextAttackTime)
        {
            Debug.Log("공격");

            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 레이어 확인
        if (((1 << other.gameObject.layer) & playerLayerMaskValue) == 0)
            return;
        Debug.Log("플레이어가 범위 안에 들어옴");
        // 플레이어 루트 & 체력 캐시
        if (player == null)
        {
            player = other.transform.root; // 상황에 따라 root가 아닌 특정 트랜스폼이면 교체
            if (ph == null)
                other.GetComponentInParent<PlayerHealth>()?.TryGetComponent(out ph);
            Debug.Log("체력 얻어옴");
        }

        playerOverlapCount++;
        isPlayerInRange = playerOverlapCount > 0;
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayerMaskValue) == 0)
            return;

        // 다중 콜라이더 대응
        playerOverlapCount = Mathf.Max(0, playerOverlapCount - 1);
        if (playerOverlapCount == 0)
        {
            isPlayerInRange = false;
            player = null;
            ph = null;
        }
    }

    // 각 식물 전용 공격 로직
    protected abstract void Attack();

#if UNITY_EDITOR
    private void OnValidate()
    {
        var col = GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }
#endif
}
