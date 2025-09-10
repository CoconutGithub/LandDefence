    using UnityEngine;

    // 이 스크립트는 애니메이션이 재생된 후 자동으로 게임 오브젝트를 파괴하는 역할을 합니다.
    public class EffectAutoDestroy : MonoBehaviour
    {
        void Start()
        {
            // 이 오브젝트에 붙어있는 Animator 컴포넌트를 찾습니다.
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                // 현재 재생 중인 애니메이션 상태의 길이를 가져옵니다.
                float animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
                // 애니메이션 길이만큼의 시간이 지난 후에 이 게임 오브젝트를 파괴하도록 예약합니다.
                Destroy(gameObject, animationLength);
            }
            else
            {
                // 만약 Animator를 찾지 못하는 예외 상황이 발생하면, 1초 뒤에 파괴합니다.
                Destroy(gameObject, 1f);
            }
        }
    }
    
