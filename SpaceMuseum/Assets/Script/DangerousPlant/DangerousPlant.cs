using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public abstract class DangerousPlant : MonoBehaviour
{
    [Header("공통 설정")]
    public float attackCooldown = 3f;   // 공격 주기
    public LayerMask playerLayer;       // 플레이어 레이어

    protected Transform player;         // 감지된 플레이어
    private bool isPlayerInRange = false;
    private bool isAttacking = false;
    [SerializeField] protected PlayerHealth ph;
    protected PlayerController pc;
    protected InGameManager igm;
    protected MyUIManager uim;

    protected virtual void Awake()
    {
        // SphereCollider를 자동으로 감지 범위로 세팅
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }
    private void Start()
    {
        pc = PlayerController.Instance;
        igm = InGameManager.Instance; ;
        uim = MyUIManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 레이어인지 확인
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            player = other.transform;
            isPlayerInRange = true;

            if (!isAttacking)
                StartCoroutine(AttackLoop());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == player)
        {
            isPlayerInRange = false;
            player = null;
            StopAllCoroutines(); // 공격 중단
            isAttacking = false;
        }
    }

    private IEnumerator AttackLoop()
    {
        isAttacking = true;

        while (isPlayerInRange)
        {
            Attack(); // 자식 클래스에서 구현
            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
    }

    // 자식 클래스에서 반드시 구현해야 함
    protected abstract void Attack();
}
