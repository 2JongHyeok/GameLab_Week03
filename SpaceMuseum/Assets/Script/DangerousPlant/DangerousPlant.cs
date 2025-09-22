using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public abstract class DangerousPlant : MonoBehaviour
{
    [Header("공통 설정")]
    public float attackCooldown = 3f;     // 공격 주기
    public LayerMask playerLayer;         // 플레이어 레이어

    // 싱글톤/참조 (필요 시 자동 연결)
    [SerializeField] protected PlayerHealth ph;
    protected PlayerController pc;
    protected InGameManager igm;
    protected MyUIManager uim;

    // 내부 상태
    protected Transform player;           // 플레이어 루트 Transform
    private int playerOverlapCount = 0;   // 트리거 안의 플레이어 콜라이더 개수
    private Coroutine attackCoroutine;

    protected virtual void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // 트리거로 동작
    }

    private void Start()
    {
        pc = PlayerController.Instance;
        ph = ph ?? pc?.GetComponent<PlayerHealth>();
        igm = InGameManager.Instance;
        uim = MyUIManager.Instance;
    }

    // === Trigger 진입/이탈 관리 ===
    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollider(other)) return;

        var root = GetRoot(other);

        // 최초 진입 시 플레이어 고정
        if (player == null) player = root;
        if (root != player) return;

        playerOverlapCount++;

        // 0 -> 1 될 때만 루프 시작 (중복 방지)
        if (playerOverlapCount == 1 && attackCoroutine == null)
            attackCoroutine = StartCoroutine(AttackLoop());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerCollider(other)) return;

        var root = GetRoot(other);
        if (root != player) return;

        playerOverlapCount = Mathf.Max(0, playerOverlapCount - 1);

        // 1 -> 0 될 때만 루프 종료
        if (playerOverlapCount == 0 && attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            player = null; // 필요하면 유지해도 됨
        }
    }

    // 플레이어 판별: 레이어(필수) + 필요하면 태그까지
    private bool IsPlayerCollider(Collider c)
    {
        if ((playerLayer.value & (1 << c.gameObject.layer)) == 0) return false;
        // Tag까지 쓰고 싶으면 아래 한 줄 추가
        // if (!c.CompareTag("Player")) return false;
        return true;
    }

    // Rigidbody가 있으면 그 Transform, 없으면 최상위 Transform
    private Transform GetRoot(Collider c)
    {
        return c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform.root;
    }

    // === 공격 루프 ===
    private IEnumerator AttackLoop()
    {
        // 첫 발 즉시
        Attack();

        // 이후 쿨다운마다
        while (playerOverlapCount > 0)
        {
            yield return new WaitForSeconds(attackCooldown);
            if (playerOverlapCount > 0)
                Attack();
        }

        attackCoroutine = null;
    }

    // 자식이 구현
    protected abstract void Attack();

    private void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        playerOverlapCount = 0;
        player = null;
    }
}
