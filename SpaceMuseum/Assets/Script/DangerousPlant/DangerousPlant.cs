using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))] // Kinematic ����
public abstract class DangerousPlant : MonoBehaviour
{
    [Header("���� ����")]
    public float attackCooldown = 3f;
    public LayerMask playerLayer;

    protected Transform player;                // ���� �� �÷��̾� ��Ʈ
    protected PlayerController pc;
    [SerializeField] protected PlayerHealth ph; // �ʿ� �� ���� �Ҵ� ����

    protected bool isPlayerInRange = false;

    private float nextAttackTime = 0f;
    private int playerOverlapCount = 0;        // ���� �ݶ��̴� ����
    private int playerLayerMaskValue;          // �̼� ����ȭ

    protected virtual void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Ʈ���� �浹�� ���� ��

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
        // ���� ����(����)
        player = null;
        ph = null;
        isPlayerInRange = false;
        playerOverlapCount = 0;
    }

    // PlantManager�� �� ������ ȣ��
    public void OnUpdateTick()
    {
        Debug.Log("������Ʈ �Ҹ�.");
        // �÷��̾ ��Ȱ��ȭ/�ı��� ����� ��� ��� ����
        if (isPlayerInRange && (player == null || !player.gameObject.activeInHierarchy))
        {
            Debug.Log("�÷��̾� �����.");

            isPlayerInRange = false;
            playerOverlapCount = 0;
            ph = null;
        }

        if (isPlayerInRange && Time.time >= nextAttackTime)
        {
            Debug.Log("����");

            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ���̾� Ȯ��
        if (((1 << other.gameObject.layer) & playerLayerMaskValue) == 0)
            return;
        Debug.Log("�÷��̾ ���� �ȿ� ����");
        // �÷��̾� ��Ʈ & ü�� ĳ��
        if (player == null)
        {
            player = other.transform.root; // ��Ȳ�� ���� root�� �ƴ� Ư�� Ʈ�������̸� ��ü
            if (ph == null)
                other.GetComponentInParent<PlayerHealth>()?.TryGetComponent(out ph);
            Debug.Log("ü�� ����");
        }

        playerOverlapCount++;
        isPlayerInRange = playerOverlapCount > 0;
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayerMaskValue) == 0)
            return;

        // ���� �ݶ��̴� ����
        playerOverlapCount = Mathf.Max(0, playerOverlapCount - 1);
        if (playerOverlapCount == 0)
        {
            isPlayerInRange = false;
            player = null;
            ph = null;
        }
    }

    // �� �Ĺ� ���� ���� ����
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
