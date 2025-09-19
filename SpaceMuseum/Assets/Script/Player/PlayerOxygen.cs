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
    // 변수 이름을 MaxHeight -> MaxWidth 로 변경하여 명확하게 함
    public float gaugeMaxWidth = 1f;  // 산소가 100%일 때의 막대 너비

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

    // 산소 게이지의 크기를 업데이트하는 함수
    void UpdateOxygenGauge()
    {
        if (oxygenGaugePivot == null) return;

        float oxygenRatio = Mathf.Max(0f, currentOxygen) / maxOxygen;

        oxygenGaugePivot.localScale = new Vector3(
            gaugeMaxWidth * oxygenRatio,
            oxygenGaugePivot.localScale.y,
            oxygenGaugePivot.localScale.z
        );

        // 검정 막대 ( 최대 산소량을 보여주기 위한 트릭)
        if (oxygenBlackGaugePivot == null) return;
        float missingOxygenRatio = 1f - oxygenRatio;

        oxygenBlackGaugePivot.localScale = new Vector3(
            gaugeMaxWidth * missingOxygenRatio,
            oxygenBlackGaugePivot.localScale.y,
            oxygenBlackGaugePivot.localScale.z
        );
    }
}