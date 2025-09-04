//SoundManager.cs
using UnityEngine;

// 게임의 모든 사운드 재생을 중앙에서 관리하는 스크립트입니다.
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    // Inspector 창에서 연결할 오디오 클립들입니다.
    [Header("사운드 클립")]
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip buildSound;

    // 실제 소리를 재생할 오디오 소스 컴포넌트입니다.
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // 이 오브젝트가 씬이 바뀌어도 파괴되지 않게 합니다. (나중에 다른 씬을 추가할 경우를 대비)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 이 오브젝트에 붙어있는 AudioSource 컴포넌트를 가져옵니다.
        audioSource = GetComponent<AudioSource>();
    }

    // 외부에서 사운드 재생을 요청할 때 사용할 함수들입니다.
    public void PlayAttackSound()
    {
        // PlayOneShot은 기존에 재생 중인 소리를 멈추지 않고 새로운 소리를 겹쳐서 재생합니다.
        audioSource.PlayOneShot(attackSound);
    }

    public void PlayHitSound()
    {
        audioSource.PlayOneShot(hitSound);
    }

    public void PlayDeathSound()
    {
        audioSource.PlayOneShot(deathSound);
    }

    public void PlayBuildSound()
    {
        audioSource.PlayOneShot(buildSound);
    }
}
