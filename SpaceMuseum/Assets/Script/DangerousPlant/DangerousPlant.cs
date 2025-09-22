using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public abstract class DangerousPlant : MonoBehaviour
{
    [Header("���� ����")]
    public float attackCooldown = 3f;     // ���� �ֱ�
    public LayerMask playerLayer;         // �÷��̾� ���̾�

    // �̱���/���� (�ʿ� �� �ڵ� ����)
    [SerializeField] protected PlayerHealth ph;
    protected PlayerController pc;
    protected InGameManager igm;
    protected MyUIManager uim;

    // ���� ����
    protected Transform player;           // �÷��̾� ��Ʈ Transform
    private int playerOverlapCount = 0;   // Ʈ���� ���� �÷��̾� �ݶ��̴� ����
    private Coroutine attackCoroutine;

    protected virtual void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // Ʈ���ŷ� ����
    }

    private void Start()
    {
        pc = PlayerController.Instance;
        ph = ph ?? pc?.GetComponent<PlayerHealth>();
        igm = InGameManager.Instance;
        uim = MyUIManager.Instance;
    }

    // === Trigger ����/��Ż ���� ===
    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollider(other)) return;

        var root = GetRoot(other);

        // ���� ���� �� �÷��̾� ����
        if (player == null) player = root;
        if (root != player) return;

        playerOverlapCount++;

        // 0 -> 1 �� ���� ���� ���� (�ߺ� ����)
        if (playerOverlapCount == 1 && attackCoroutine == null)
            attackCoroutine = StartCoroutine(AttackLoop());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerCollider(other)) return;

        var root = GetRoot(other);
        if (root != player) return;

        playerOverlapCount = Mathf.Max(0, playerOverlapCount - 1);

        // 1 -> 0 �� ���� ���� ����
        if (playerOverlapCount == 0 && attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            player = null; // �ʿ��ϸ� �����ص� ��
        }
    }

    // �÷��̾� �Ǻ�: ���̾�(�ʼ�) + �ʿ��ϸ� �±ױ���
    private bool IsPlayerCollider(Collider c)
    {
        if ((playerLayer.value & (1 << c.gameObject.layer)) == 0) return false;
        // Tag���� ���� ������ �Ʒ� �� �� �߰�
        // if (!c.CompareTag("Player")) return false;
        return true;
    }

    // Rigidbody�� ������ �� Transform, ������ �ֻ��� Transform
    private Transform GetRoot(Collider c)
    {
        return c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform.root;
    }

    // === ���� ���� ===
    private IEnumerator AttackLoop()
    {
        // ù �� ���
        Attack();

        // ���� ��ٿ��
        while (playerOverlapCount > 0)
        {
            yield return new WaitForSeconds(attackCooldown);
            if (playerOverlapCount > 0)
                Attack();
        }

        attackCoroutine = null;
    }

    // �ڽ��� ����
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
