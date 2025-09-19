using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TetherConnection
{
    public LineRenderer line;
    public Tether otherTether;
    public bool isOwner;

    public TetherConnection(LineRenderer r, Tether o, bool owner)
    {
        line = r;
        otherTether = o;
        isOwner = owner;
    }
}

public class Tether : MonoBehaviour
{
    public static readonly List<Tether> AllTethers = new List<Tether>();

    // (A,B) ���� �ߺ� ������ Ű(InstanceID ���, ����)
    private static readonly HashSet<string> ExistingEdges = new HashSet<string>();

    [Header("���� ����")]
    public GameObject linePrefab;
    public float connectionRadius = 10f;
    public float lineAttachHeight = 1f;

    [Header("BaseCamp Link")]
    public bool linkToBaseCamp = true;          // ���̽��� ���ἱ ǥ�� ����
    public string baseTag = "OxygenSource";     // ���̽� �±�
    public float baseConnectRadius = 12f;       // ���̽� ���� �ݰ� (connectionRadius�� ���� ���)

    private LineRenderer baseLine;              // �״��꺣�̽� ���� ����
    private Transform baseTarget;               // ���� ���� ���� ���̽�(���� ����� ��)

    [Header("Materials")]
    public Material oxygenatedMaterial;
    public Material deactivatedMaterial;

    [SerializeField] private bool _isOxygenated = false;
    public bool IsOxygenated
    {
        get => _isOxygenated;
        set
        {
            if (_isOxygenated == value) return;
            _isOxygenated = value;
            UpdateVisuals(); // ����ÿ��� �ð� ����
        }
    }

    // Union-Find
    private Tether parent;
    [SerializeField] private int _rank = 0;

    private readonly List<TetherConnection> connections = new List<TetherConnection>();

    private void OnEnable()
    {
        if (!AllTethers.Contains(this)) AllTethers.Add(this);
        parent = this; // make-set
    }

    private void Start()
    {
        // ���� ����: Start���� ������ ����������,
        // ������ �ܺ�(�÷��̾� ����)���� BuildConnections() ��� ȣ�� ����.
        // ���� ȣȯ�� ���� Start������ �� �� ȣ���� ��.
        BuildConnections();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // �ν����Ϳ��� _isOxygenated �ٲ㵵 ��� �ݿ�
        UpdateVisuals();
    }
#endif

    private void OnDisable()
    {
        AllTethers.Remove(this);

        // ���� ������ �״�-�״� ���� ����
        foreach (var conn in connections)
        {
            if (conn.isOwner && conn.line)
            {
                string key = EdgeKey(this, conn.otherTether);
                ExistingEdges.Remove(key);
                Destroy(conn.line.gameObject);
            }
        }
        foreach (var conn in connections)
            conn.otherTether?.connections?.RemoveAll(c => c.otherTether == this);
        connections.Clear();

        // �� �״�-���̽� ���� ����
        if (baseLine)
        {
            Destroy(baseLine.gameObject);
            baseLine = null;
            baseTarget = null;
        }
    }

    // ----------------- Union-Find -----------------
    private Tether FindSet()
    {
        if (parent == this) return this;
        parent = parent.FindSet();
        return parent;
    }

    private static void Link(Tether a, Tether b)
    {
        if (a._rank < b._rank) a.parent = b;
        else if (a._rank > b._rank) b.parent = a;
        else { b.parent = a; a._rank++; }
    }

    private void UnionSets(Tether other)
    {
        var ra = FindSet();
        var rb = other.FindSet();
        if (ra != rb) Link(ra, rb);
    }

    // ----------------- Public API -----------------
    public List<TetherConnection> GetConnections() => connections;

    public void BuildConnections()
    {
        if (!linePrefab) return;

        // ��Ʈ�� �ĺ� ����
        var tetherGroups = new Dictionary<Tether, List<Tether>>();
        foreach (var other in AllTethers)
        {
            if (other == this) continue;
            if (Vector3.Distance(transform.position, other.transform.position) > connectionRadius) continue;

            var root = other.FindSet();
            if (!tetherGroups.TryGetValue(root, out var list))
            {
                list = new List<Tether>();
                tetherGroups[root] = list;
            }
            list.Add(other);
        }

        foreach (var kv in tetherGroups)
        {
            var group = kv.Value;
            var closest = group.OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
                               .FirstOrDefault();
            if (closest == null) continue;

            // ���� �����̸� ��ŵ
            if (FindSet() == closest.FindSet()) continue;

            // ���� �ߺ� ����
            string key = EdgeKey(this, closest);
            if (ExistingEdges.Contains(key)) continue;

            // ���� ����
            var lineObj = Instantiate(linePrefab);
            if (!lineObj.TryGetComponent<LineRenderer>(out var lr))
            {
                Destroy(lineObj);
                continue;
            }

            Vector3 startPos = transform.position + Vector3.up * lineAttachHeight;
            Vector3 endPos = closest.transform.position + Vector3.up * lineAttachHeight;
            DrawSaggingLine(lr, startPos, endPos);

            // ���� ���(�����), �����ڴ� this
            connections.Add(new TetherConnection(lr, closest, true));
            closest.connections.Add(new TetherConnection(lr, this, false));

            // ��Ƽ���� �ʱⰪ(��Ȱ��)
            if (deactivatedMaterial) lr.sharedMaterial = deactivatedMaterial;

            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;

            ExistingEdges.Add(key);
            UnionSets(closest);
        }

        // ���� ���� ���� �ð� ���� 1ȸ
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        foreach (var conn in connections)
        {
            if (!conn.line) continue;
            bool anyOxy = this.IsOxygenated || (conn.otherTether != null && conn.otherTether.IsOxygenated);
            var mat = anyOxy ? oxygenatedMaterial : deactivatedMaterial;
            if (mat != null) conn.line.sharedMaterial = mat;
        }
    }

    private void LateUpdate()
    {
        // ���ɻ� owner�� ���� ��ġ ����
        foreach (var conn in connections)
        {
            if (!conn.isOwner || !conn.line || conn.otherTether == null) continue;

            Vector3 start = transform.position + Vector3.up * lineAttachHeight;
            Vector3 end = conn.otherTether.transform.position + Vector3.up * lineAttachHeight;
            DrawSaggingLine(conn.line, start, end);
        }
        UpdateBaseCampLink();
    }

    private void UpdateBaseCampLink()
    {
        if (!linkToBaseCamp || !linePrefab) return;

        // 1) ���� Ÿ���� ���ų� �ʹ� �־������� ��Ž��
        if (baseTarget == null || Vector3.Distance(transform.position, baseTarget.position) > baseConnectRadius * 1.2f)
            baseTarget = FindNearestBase();

        bool inRange = baseTarget != null &&
                       Vector3.Distance(transform.position, baseTarget.position) <= baseConnectRadius;

        // 2) ���� ���� ���� ����/����, �ƴϸ� �ı�
        if (inRange)
        {
            if (baseLine == null)
            {
                var obj = Instantiate(linePrefab);
                if (!obj.TryGetComponent<LineRenderer>(out baseLine))
                {
                    Destroy(obj);
                    return;
                }
                baseLine.startWidth = 0.05f;
                baseLine.endWidth = 0.05f;
                if (deactivatedMaterial) baseLine.sharedMaterial = deactivatedMaterial;
            }

            Vector3 start = transform.position + Vector3.up * lineAttachHeight;
            Vector3 end = baseTarget.position + Vector3.up * lineAttachHeight;
            DrawSaggingLine(baseLine, start, end);

            // ��Ƽ����: �⺻�� �״��� ��� ���¿� ����
            var mat = IsOxygenated ? oxygenatedMaterial : deactivatedMaterial;
            if (mat) baseLine.sharedMaterial = mat;
        }
        else
        {
            if (baseLine)
            {
                Destroy(baseLine.gameObject);
                baseLine = null;
            }
            baseTarget = null;
        }
    }

    private Transform FindNearestBase()
    {
        // ���� ��, �ʿ��� ���� ��ü �˻�. �� �� "OxygenSource"���� �� ���� ��ĵ.
        GameObject[] bases = GameObject.FindGameObjectsWithTag(baseTag);
        if (bases == null || bases.Length == 0) return null;

        Transform best = null;
        float bestDistSq = float.MaxValue;
        Vector3 p = transform.position;

        foreach (var go in bases)
        {
            if (!go) continue;
            float d2 = (go.transform.position - p).sqrMagnitude;
            if (d2 < bestDistSq)
            {
                bestDistSq = d2;
                best = go.transform;
            }
        }
        // �ݰ� üũ�� ȣ���(inRange)���� �Ѵ�.
        return best;
    }

    private static string EdgeKey(Tether a, Tether b)
    {
        if (a == null || b == null) return string.Empty;
        int ia = a.GetInstanceID();
        int ib = b.GetInstanceID();
        return ia < ib ? $"{ia}-{ib}" : $"{ib}-{ia}";
    }

    private void DrawSaggingLine(LineRenderer lr, Vector3 startPoint, Vector3 endPoint)
    {
        const int segments = 20;
        lr.positionCount = segments + 1;

        float dist = Vector3.Distance(startPoint, endPoint);
        float sagAmount = Mathf.Max(0.1f, dist * 0.05f); // �Ÿ� ��ʷ� �ڿ�������

        var positions = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 straight = Vector3.Lerp(startPoint, endPoint, t);
            float sag = 4f * sagAmount * (t - t * t); // ������
            positions[i] = new Vector3(straight.x, straight.y - sag, straight.z);
        }

        lr.SetPositions(positions);
    }
}
