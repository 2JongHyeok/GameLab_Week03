using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OxygenNetworkManager : MonoBehaviour
{
    public static OxygenNetworkManager Instance { get; private set; }

    [Header("산소 공급 설정")]
    public float oxygenSourceRadius = 20f;

    [Tooltip("한 개만 쓴다면 첫 번째 항목만 두고 사용해도 됨")]
    [SerializeField] private List<Transform> oxygenSources = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void UpdateOxygenNetwork()
    {
        if (Tether.AllTethers.Count == 0) return;

        var reachable = new HashSet<Tether>();
        var q = new Queue<Tether>();

        // 1) 소스 반경 내 테더들을 시드로 큐에 적재
        foreach (var src in oxygenSources)
        {
            if (src == null) continue;

            foreach (var t in Tether.AllTethers)
            {
                if (reachable.Contains(t)) continue;
                if (Vector3.Distance(src.position, t.transform.position) <= oxygenSourceRadius)
                {
                    reachable.Add(t);
                    q.Enqueue(t);
                }
            }
        }

        // 2) BFS로 라인(연결) 따라 전파
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            foreach (var conn in cur.GetConnections())
            {
                var nb = conn.otherTether;
                if (nb != null && reachable.Add(nb))
                    q.Enqueue(nb);
            }
        }

        // 3) 델타 적용: 바뀌는 것만 setter 호출
        foreach (var t in Tether.AllTethers)
        {
            bool on = reachable.Contains(t);
            if (t.IsOxygenated != on)
                t.IsOxygenated = on;
        }
    }

    // (옵션) 동적 소스 등록/해제
    public void RegisterSource(Transform src)
    {
        if (src && !oxygenSources.Contains(src)) oxygenSources.Add(src);
    }

    public void UnregisterSource(Transform src)
    {
        if (src) oxygenSources.Remove(src);
    }
}
