using UnityEngine;

[CreateAssetMenu(fileName = "New Mineral Data", menuName = "Game/Mineral Data")]
public class MineralData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string mineralName;      // �ڿ� �̸� (A, B, C...)
    public Material mineralMaterial; // �ڿ� ť�꿡 ������ ���� Material

    [Header("���� ������")]
    public float weight;            // �ڿ��� ����
    public int price;               // �ڿ��� �Ǹ� ����
    public float spawnProbability;  // ���ڿ��� ������ Ȯ�� (����ġ)
}