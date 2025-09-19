using UnityEngine;

[CreateAssetMenu(fileName = "New Mineral Data", menuName = "Game/Mineral Data")]
public class MineralData : ScriptableObject
{
    [Header("기본 정보")]
    public string mineralName;      // 자원 이름 (A, B, C...)
    public Material mineralMaterial; // 자원 큐브에 적용할 색상 Material

    [Header("게임 데이터")]
    public float weight;            // 자원의 무게
    public int price;               // 자원의 판매 가격
    public float spawnProbability;  // 상자에서 등장할 확률 (가중치)
}