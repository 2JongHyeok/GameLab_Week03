using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public abstract class DangerousPlant : MonoBehaviour
{
    [Header("���� ����")]
    public float attackCooldown = 3f;   // ���� �ֱ�
    public LayerMask playerLayer;       // �÷��̾� ���̾�

    protected Transform player;         // ������ �÷��̾�
    private bool isPlayerInRange = false;
    private bool isAttacking = false;
    [SerializeField] protected PlayerHealth ph;
    protected PlayerController pc;
    protected InGameManager igm;
    protected MyUIManager uim;

    protected virtual void Awake()
    {
        // SphereCollider�� �ڵ����� ���� ������ ����
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
        // �÷��̾� ���̾����� Ȯ��
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
            StopAllCoroutines(); // ���� �ߴ�
            isAttacking = false;
        }
    }

    private IEnumerator AttackLoop()
    {
        isAttacking = true;

        while (isPlayerInRange)
        {
            Attack(); // �ڽ� Ŭ�������� ����
            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
    }

    // �ڽ� Ŭ�������� �ݵ�� �����ؾ� ��
    protected abstract void Attack();
}
