using UnityEngine;

public class LaunchTrapPlant : DangerousPlant
{
    [Header("함정 설정")]
    private float launchForce = 200f; // 플레이어를 띄우는 힘
    public Animator animator;

    private bool isTriggered = false; // 한 번만 발동되도록 설정
    private bool hasEmerged = false; // 처음 튀어나왔는지 여부
    protected override void Attack()
    {
        if (!hasEmerged)
        {
            hasEmerged = true;

            if (animator != null)
            {
                animator.SetTrigger("Surprise");
            }
        }

        // 플레이어 발사 요청
        if (player != null)
        {
            if (pc != null)
            {
                pc.RequestTrapLaunch(launchForce);
            }
        }
    }
}
