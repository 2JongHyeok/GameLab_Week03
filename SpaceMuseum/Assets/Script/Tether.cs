// Tether.cs
using System.Collections.Generic;
using UnityEngine;

public class Tether : MonoBehaviour
{
    // 생성된 모든 테더 인스턴스를 추적하기 위한 static 리스트
    public static List<Tether> AllTethers = new List<Tether>();

    // 이 테더가 활성화될 때 호출됩니다.
    private void OnEnable()
    {
        // 자기 자신을 static 리스트에 추가
        if (!AllTethers.Contains(this))
        {
            AllTethers.Add(this);
        }
    }

    // 이 테더가 비활성화되거나 파괴될 때 호출됩니다.
    private void OnDisable()
    {
        // 리스트에서 자기 자신을 제거
        if (AllTethers.Contains(this))
        {
            AllTethers.Remove(this);
        }
    }
}