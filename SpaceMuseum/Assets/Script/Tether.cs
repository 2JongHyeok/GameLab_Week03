// Tether.cs
using System.Collections.Generic;
using UnityEngine;

public class Tether : MonoBehaviour
{
    // ������ ��� �״� �ν��Ͻ��� �����ϱ� ���� static ����Ʈ
    public static List<Tether> AllTethers = new List<Tether>();

    // �� �״��� Ȱ��ȭ�� �� ȣ��˴ϴ�.
    private void OnEnable()
    {
        // �ڱ� �ڽ��� static ����Ʈ�� �߰�
        if (!AllTethers.Contains(this))
        {
            AllTethers.Add(this);
        }
    }

    // �� �״��� ��Ȱ��ȭ�ǰų� �ı��� �� ȣ��˴ϴ�.
    private void OnDisable()
    {
        // ����Ʈ���� �ڱ� �ڽ��� ����
        if (AllTethers.Contains(this))
        {
            AllTethers.Remove(this);
        }
    }
}