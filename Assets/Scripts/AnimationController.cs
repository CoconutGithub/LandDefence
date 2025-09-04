//AnimationController.cs
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    // Start 대신 Awake에서 Animator를 찾아 초기화합니다.
    // 다른 스크립트(예: TowerController)가 Start에서 이 컴포넌트를 참조할 수 있도록 합니다.
    void Awake()
    {
        // 이 스크립트가 붙은 게임 오브젝트 자체 또는 그 자식에서 Animator를 찾습니다.
        // 궁수 스프라이트가 타워의 자식 오브젝트에 있다면 GetComponentInChildren를 사용해야 합니다.
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning($"AnimationController: Animator 컴포넌트를 찾을 수 없습니다. 오브젝트: {gameObject.name}");
        }
    }

    // "DoAttack" Trigger를 발동시키는 공용 함수
    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("DoAttack");
        }
    }

    // 다른 애니메이션 Trigger를 재생해야 할 경우를 대비한 일반적인 함수
    public void SetAnimationTrigger(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    // 특정 Boolean 파라미터를 설정하는 함수 (예: IsMoving, IsCasting 등)
    public void SetAnimationBool(string boolName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(boolName, value);
        }
    }

    // 특정 Float 파라미터를 설정하는 함수 (예: MovementSpeed)
    public void SetAnimationFloat(string floatName, float value)
    {
        if (animator != null)
        {
            animator.SetFloat(floatName, value);
        }
    }
}