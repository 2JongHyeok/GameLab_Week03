using UnityEngine;
using System.Collections.Generic;

public class PlayerOxygen : MonoBehaviour
{
    [Header("��� ����")]
    public float maxOxygen = 100f;
    public float currentOxygen;
    public float oxygenRegenRate = 10f;
    public float oxygenDepleteRate = 5f;

    [Header("UI & Visuals")]
    public Transform oxygenGaugePivot;
    public float gaugeMaxWidth = 1f;
    public LineRenderer oxygenLinkLine;
    public float lineAttachHeight = 1f;

    // ����/������ ����: HashSet + �ڽ� Collider ���
    private readonly HashSet<Collider> nearbyOxygenSources = new HashSet<Collider>();
    private Transform currentOxygenSource;
    private bool isReceivingOxygen;          // �� �� ������ ��� ����� ���� (���� ����)
    private bool prevReceivingOxygenState;   // �� ���� ��ȭ �α׿�

    void Start()
    {
        currentOxygen = maxOxygen;
        if (oxygenLinkLine != null)
            oxygenLinkLine.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1) ���޿� Ž�� & ���� ���� ���
        isReceivingOxygen = FindClosestOxygenSource(out currentOxygenSource);

        // 2) ��ҷ� ����
        float delta = (isReceivingOxygen ? oxygenRegenRate : -oxygenDepleteRate) * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen + delta, 0f, maxOxygen);

        // 3) ���־� ����
        UpdateOxygenLinkLine(isReceivingOxygen, currentOxygenSource);
        UpdateOxygenGauge();

        // (�ɼ�) ���� ��ȭ�ÿ��� �α�
        if (prevReceivingOxygenState != isReceivingOxygen)
        {
            if (isReceivingOxygen) Debug.Log("��� ���� ���� (�ٿ� Ȯ��)");
            else Debug.Log("��� ���� �ߴ� (�ٿ� ���)");
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

    // ���� ����� ��ȿ ���޿� Ž�� (�����Ÿ� ��)
    private bool FindClosestOxygenSource(out Transform best)
    {
        best = null;
        float bestDistSq = float.MaxValue;
        Vector3 p = transform.position;

        // null ����(�ı��� ������Ʈ) �� ��ȸ
        // HashSet�� �� �� ������ ���� ��ȸ (�ʿ�� List ��ȯ �� ���� ������ ����)
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
                // �ڽ� �ݶ��̴� ����
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

        // �ʿ�� ��Ƽ����/��/���� �ʱ�ȭ ���� ����
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
