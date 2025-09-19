using UnityEngine;
using System.Collections.Generic;

public class PlayerOxygen : MonoBehaviour
{
    [Header("산소 설정")]
    public float maxOxygen = 100f;
    public float currentOxygen;
    public float oxygenRegenRate = 10f;
    public float oxygenDepleteRate = 5f;

    [Header("UI & Visuals")]
    public Transform oxygenGaugePivot;
    public float gaugeMaxWidth = 1f;
    public LineRenderer oxygenLinkLine;
    public float lineAttachHeight = 1f;

    // 성능/안정성 개선: HashSet + 자식 Collider 허용
    private readonly HashSet<Collider> nearbyOxygenSources = new HashSet<Collider>();
    private Transform currentOxygenSource;
    private bool isReceivingOxygen;          // ← 매 프레임 계산 결과만 저장 (리셋 없음)
    private bool prevReceivingOxygenState;   // ← 상태 변화 로그용

    void Start()
    {
        currentOxygen = maxOxygen;
        if (oxygenLinkLine != null)
            oxygenLinkLine.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1) 공급원 탐색 & 공급 여부 계산
        isReceivingOxygen = FindClosestOxygenSource(out currentOxygenSource);

        // 2) 산소량 갱신
        float delta = (isReceivingOxygen ? oxygenRegenRate : -oxygenDepleteRate) * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen + delta, 0f, maxOxygen);

        // 3) 비주얼 갱신
        UpdateOxygenLinkLine(isReceivingOxygen, currentOxygenSource);
        UpdateOxygenGauge();

        // (옵션) 상태 변화시에만 로그
        if (prevReceivingOxygenState != isReceivingOxygen)
        {
            if (isReceivingOxygen) Debug.Log("산소 공급 시작 (근원 확보)");
            else Debug.Log("산소 공급 중단 (근원 상실)");
            prevReceivingOxygenState = isReceivingOxygen;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OxygenSource") || other.CompareTag("Tether"))
            nearbyOxygenSources.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (nearbyOxygenSources.Contains(other))
            nearbyOxygenSources.Remove(other);
    }

    // 가장 가까운 유효 공급원 탐색 (제곱거리 비교)
    private bool FindClosestOxygenSource(out Transform best)
    {
        best = null;
        float bestDistSq = float.MaxValue;
        Vector3 p = transform.position;

        // null 정리(파괴된 오브젝트) 겸 순회
        // HashSet을 한 번 복사해 안전 순회 (필요시 List 변환 후 제거 등으로 대응)
        var toRemove = new List<Collider>();

        foreach (var col in nearbyOxygenSources)
        {
            if (col == null) { toRemove.Add(col); continue; }

            bool valid = false;

            if (col.CompareTag("OxygenSource"))
            {
                valid = true;
            }
            else if (col.CompareTag("Tether"))
            {
                // 자식 콜라이더 대응
                var tether = col.GetComponentInParent<Tether>();
                if (tether != null && tether.IsOxygenated)
                    valid = true;
            }

            if (!valid) continue;

            float distSq = (p - col.transform.position).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = col.transform;
            }
        }

        if (toRemove.Count > 0)
            foreach (var dead in toRemove) nearbyOxygenSources.Remove(dead);

        return best != null;
    }

    private void UpdateOxygenLinkLine(bool receiving, Transform source)
    {
        if (oxygenLinkLine == null) return;

        if (receiving && source != null)
        {
            if (!oxygenLinkLine.gameObject.activeSelf)
                oxygenLinkLine.gameObject.SetActive(true);

            Vector3 startPos = transform.position + Vector3.up * lineAttachHeight;
            Vector3 endPos = source.position + Vector3.up * lineAttachHeight;
            DrawSaggingLine(oxygenLinkLine, startPos, endPos);
        }
        else
        {
            if (oxygenLinkLine.gameObject.activeSelf)
                oxygenLinkLine.gameObject.SetActive(false);
        }
    }

    private void DrawSaggingLine(LineRenderer lr, Vector3 start, Vector3 end)
    {
        const int segments = 20;
        const float sagAmount = 0.2f;
        lr.positionCount = segments + 1;

        // 필요시 머티리얼/폭/색상 초기화 보강 가능
        var positions = new Vector3[segments + 1];
        for (int i = 0; i <= segments; ++i)
        {
            float t = i / (float)segments;
            Vector3 straight = Vector3.Lerp(start, end, t);
            float sag = 4f * sagAmount * (t - t * t);
            positions[i] = new Vector3(straight.x, straight.y - sag, straight.z);
        }
        lr.SetPositions(positions);
    }

    private void UpdateOxygenGauge()
    {
        if (!oxygenGaugePivot) return;
        float ratio = Mathf.Clamp01(maxOxygen > 0f ? currentOxygen / maxOxygen : 0f);
        var ls = oxygenGaugePivot.localScale;
        oxygenGaugePivot.localScale = new Vector3(gaugeMaxWidth * ratio, ls.y, ls.z);
    }
}
