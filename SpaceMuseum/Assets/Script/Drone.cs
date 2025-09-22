using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public static Drone Instance { get; private set; }
    public int attackDamage = 10;
    public float retargetInterval = 3.0f;
    public LayerMask enemyMask;
    readonly HashSet<IEnemyTarget> inRange = new();
    [SerializeField] GameObject parentDrone;

    // ����
    public Transform firePoint;          // �ѱ� ��ġ(������ ��� ��ġ ���)
    public float laserThickness = 0.08f; // ť�� �β�
    public float laserLifetime = 0.05f;  // �ð�ȿ�� ���� �ð�
    private float lastFireTime = -999f;  // ���� ��ٿ� Ÿ�ӽ�����
    public IEnemyTarget current { get; private set; }   // EnemyAI ��� �������̽��� ����

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;
        var target = other.GetComponentInParent<IEnemyTarget>();
        if (target != null) inRange.Add(target);
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;
        var target = other.GetComponentInParent<IEnemyTarget>();
        if (target != null) inRange.Remove(target);
    }

    IEnumerator Start()
    {
        var wait = new WaitForSeconds(retargetInterval);
        while (true)
        {
            AcquireTarget();
            Attack();
            wait = new WaitForSeconds(retargetInterval);
            yield return wait;
        }
    }
    public void UnregisterTarget(IEnemyTarget target)
    {
        inRange.Remove(target);
    }
    void Attack()
    {
        if (current == null) return;

        Vector3 start = firePoint ? firePoint.position : transform.position;

        var col = (current as MonoBehaviour).GetComponent<Collider>();
        Vector3 target = col ? col.bounds.center : (current as MonoBehaviour).transform.position;

        Vector3 dir = (target - start);
        float dist = dir.magnitude;
        if (dist < 0.01f) return;
        dir /= dist;

        RaycastHit hit;
        float maxDist = dist + 0.5f;
        if (Physics.Raycast(start, dir, out hit, maxDist, enemyMask, QueryTriggerInteraction.Collide))
        {
            dist = hit.distance;

            var targets = hit.collider.GetComponentInParent<IEnemyTarget>();
            if (targets != null)
            {
                targets.OnDamagedFromDrone(attackDamage); // EnemyAI�� BossAI�� �˾Ƽ� ȣ���
            }
        }

        SpawnLaserVisual(start, dir, dist);
        lastFireTime = Time.time;
    }

    void SpawnLaserVisual(Vector3 start, Vector3 dir, float length)
    {
        // ť�� ����
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "LaserBeam";
        go.layer = LayerMask.NameToLayer("Ignore Raycast"); // �ڱ� �ڽ� ����ĳ��Ʈ ���� ����
        var col = go.GetComponent<Collider>();
        if (col) Destroy(col); // �ð�ȿ���� �� �Ŷ�� �ݶ��̴� ����

        // ��ġ/ȸ��/������: ����~���� �߰��� ���̸�ŭ �ø� ���簢��
        go.transform.SetPositionAndRotation(start + dir * (length * 0.5f), Quaternion.LookRotation(dir));
        go.transform.localScale = new Vector3(laserThickness, laserThickness, length);

        // ����(URP/HDRP/Built-in ȣȯ)
        var r = go.GetComponent<Renderer>();
        var m = r.material; // �� �ν��Ͻ���
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.red);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", Color.red);

        // �߱�(����)
        if (m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", Color.red * 2f);
        }

        // ���� ª�Ը� ǥ��
        Destroy(go, laserLifetime);
    }
    void AcquireTarget()
    {
        inRange.RemoveWhere(e => e == null || !(e as MonoBehaviour).gameObject.activeInHierarchy);

        float best = float.PositiveInfinity;
        IEnemyTarget bestTarget = null;
        var p = transform.position;

        foreach (var e in inRange)
        {
            float d = ((e as MonoBehaviour).transform.position - p).sqrMagnitude;
            if (d < best) { best = d; bestTarget = e; }
        }
        current = bestTarget;
    }

}
