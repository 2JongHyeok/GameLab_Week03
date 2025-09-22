using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OxygenNetworkManager : MonoBehaviour
{
    public static OxygenNetworkManager Instance { get; private set; }

    [Header("��� ���� ����")]
    public float oxygenSourceRadius = 20f;

    [Tooltip("�� ���� ���ٸ� ù ��° �׸� �ΰ� ����ص� ��")]
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

        // 1) �ҽ� �ݰ� �� �״����� �õ�� ť�� ����
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

        // 2) BFS�� ����(����) ���� ����
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

        // 3) ��Ÿ ����: �ٲ�� �͸� setter ȣ��
        foreach (var t in Tether.AllTethers)
        {
            bool on = reachable.Contains(t);
            if (t.IsOxygenated != on)
                t.IsOxygenated = on;
        }
    }

    // (�ɼ�) ���� �ҽ� ���/����
    public void RegisterSource(Transform src)
    {
        if (src && !oxygenSources.Contains(src)) oxygenSources.Add(src);
    }

    public void UnregisterSource(Transform src)
    {
        if (src) oxygenSources.Remove(src);
    }
}
