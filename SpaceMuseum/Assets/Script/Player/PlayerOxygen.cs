// PlayerOxygen.cs
using UnityEngine;

public class PlayerOxygen : MonoBehaviour
{
    public float maxOxygen = 100f;
    public float currentOxygen;
    public float oxygenRegenRate = 10f;
    public float oxygenDepleteRate = 5f;

    [Header("UI & Visuals")]
    public Transform oxygenGaugePivot;
    public Transform oxygenBlackGaugePivot;
    // ���� �̸��� MaxHeight -> MaxWidth �� �����Ͽ� ��Ȯ�ϰ� ��
    public float gaugeMaxWidth = 1f;  // ��Ұ� 100%�� ���� ���� �ʺ�

    void Start()
    {
        currentOxygen = maxOxygen;
    }

    void Update()
    {
        currentOxygen -= oxygenDepleteRate * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);

        UpdateOxygenGauge();
    }

    // ��� �������� ũ�⸦ ������Ʈ�ϴ� �Լ�
    void UpdateOxygenGauge()
    {
        if (oxygenGaugePivot == null) return;

        float oxygenRatio = Mathf.Max(0f, currentOxygen) / maxOxygen;

        oxygenGaugePivot.localScale = new Vector3(
            gaugeMaxWidth * oxygenRatio,
            oxygenGaugePivot.localScale.y,
            oxygenGaugePivot.localScale.z
        );

        // ���� ���� ( �ִ� ��ҷ��� �����ֱ� ���� Ʈ��)
        if (oxygenBlackGaugePivot == null) return;
        float missingOxygenRatio = 1f - oxygenRatio;

        oxygenBlackGaugePivot.localScale = new Vector3(
            gaugeMaxWidth * missingOxygenRatio,
            oxygenBlackGaugePivot.localScale.y,
            oxygenBlackGaugePivot.localScale.z
        );
    }
}