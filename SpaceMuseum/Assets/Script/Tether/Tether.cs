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

    // (A,B) 간선 중복 방지용 키(InstanceID 기반, 정렬)
    private static readonly HashSet<string> ExistingEdges = new HashSet<string>();

    [Header("연결 설정")]
    public GameObject linePrefab;
    public float connectionRadius = 10f;
    public float lineAttachHeight = 1f;

    [Header("BaseCamp Link")]
    public bool linkToBaseCamp = true;          // 베이스와 연결선 표시 여부
    public string baseTag = "OxygenSource";     // 베이스 태그
    public float baseConnectRadius = 12f;       // 베이스 감지 반경 (connectionRadius와 별도 운용)

    private LineRenderer baseLine;              // 테더↔베이스 전용 라인
    private Transform baseTarget;               // 현재 연결 중인 베이스(가장 가까운 것)

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
            UpdateVisuals(); // 변경시에만 시각 갱신
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
        // 과거 로직: Start에서 연결을 생성했으나,
        // 이제는 외부(플레이어 스폰)에서 BuildConnections() 즉시 호출 권장.
        // 하위 호환을 위해 Start에서도 한 번 호출해 둠.
        BuildConnections();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 인스펙터에서 _isOxygenated 바꿔도 즉시 반영
        UpdateVisuals();
    }
#endif

    private void OnDisable()
    {
        AllTethers.Remove(this);

        // 내가 소유한 테더-테더 라인 정리
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

        // ★ 테더-베이스 라인 정리
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

        // 루트별 후보 수집
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

            // 같은 집합이면 스킵
            if (FindSet() == closest.FindSet()) continue;

            // 간선 중복 방지
            string key = EdgeKey(this, closest);
            if (ExistingEdges.Contains(key)) continue;

            // 라인 생성
            var lineObj = Instantiate(linePrefab);
            if (!lineObj.TryGetComponent<LineRenderer>(out var lr))
            {
                Destroy(lineObj);
                continue;
            }

            Vector3 startPos = transform.position + Vector3.up * lineAttachHeight;
            Vector3 endPos = closest.transform.position + Vector3.up * lineAttachHeight;
            DrawSaggingLine(lr, startPos, endPos);

            // 연결 등록(양방향), 소유자는 this
            connections.Add(new TetherConnection(lr, closest, true));
            closest.connections.Add(new TetherConnection(lr, this, false));

            // 머티리얼 초기값(비활성)
            if (deactivatedMaterial) lr.sharedMaterial = deactivatedMaterial;

            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;

            ExistingEdges.Add(key);
            UnionSets(closest);
        }

        // 연결 생성 직후 시각 갱신 1회
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
        // 성능상 owner만 라인 위치 갱신
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

        // 1) 현재 타깃이 없거나 너무 멀어졌으면 재탐색
        if (baseTarget == null || Vector3.Distance(transform.position, baseTarget.position) > baseConnectRadius * 1.2f)
            baseTarget = FindNearestBase();

        bool inRange = baseTarget != null &&
                       Vector3.Distance(transform.position, baseTarget.position) <= baseConnectRadius;

        // 2) 범위 내면 라인 생성/갱신, 아니면 파기
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

            // 머티리얼: 기본은 테더의 산소 상태에 따름
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
        // 성능 상, 필요할 때만 전체 검색. 씬 내 "OxygenSource"들을 한 번에 스캔.
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
        // 반경 체크는 호출부(inRange)에서 한다.
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
        float sagAmount = Mathf.Max(0.1f, dist * 0.05f); // 거리 비례로 자연스러움

        var positions = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 straight = Vector3.Lerp(startPoint, endPoint, t);
            float sag = 4f * sagAmount * (t - t * t); // 포물선
            positions[i] = new Vector3(straight.x, straight.y - sag, straight.z);
        }

        lr.SetPositions(positions);
    }
}
