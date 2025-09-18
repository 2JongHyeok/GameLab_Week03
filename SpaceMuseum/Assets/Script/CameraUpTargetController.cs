using UnityEngine;

public class CameraUpTargetController : MonoBehaviour
{
    public Transform player;
    public Transform planet;

    void Update()
    {
        if (player == null || planet == null) return;

        // 1. 플레이어 위치를 따라다닌다.
        transform.position = player.position;

        // 2. 이 오브젝트의 '위쪽(up)' 방향이 항상 행성 중심에서 플레이어를 향하도록 설정한다.
        Vector3 gravityUp = (player.position - planet.position).normalized;
        transform.up = gravityUp;
    }
}