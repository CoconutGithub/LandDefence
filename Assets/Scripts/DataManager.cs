using UnityEngine;

// 게임 데이터를 영구적으로 저장하고 불러오는 역할을 하는 클래스입니다.
public static class DataManager
{
    // 데이터를 저장할 때 사용할 키(Key)입니다.
    private const string ArcherExpKey = "ArcherExperience";
    private const string ArcherDamageLevelKey = "ArcherDamageLevel"; // (추가) 궁수 공격력 레벨 키

    // --- 궁수 경험치 관련 ---
    public static void SaveArcherExperience(int totalExp)
    {
        PlayerPrefs.SetInt(ArcherExpKey, totalExp);
        PlayerPrefs.Save();
        Debug.Log("궁수 경험치 저장 완료: " + totalExp);
    }

    public static int LoadArcherExperience()
    {
        return PlayerPrefs.GetInt(ArcherExpKey, 0);
    }

    // --- (추가) 궁수 공격력 레벨 관련 ---
    public static void SaveArcherDamageLevel(int level)
    {
        PlayerPrefs.SetInt(ArcherDamageLevelKey, level);
        PlayerPrefs.Save();
        Debug.Log("궁수 공격력 레벨 저장 완료: " + level);
    }

    public static int LoadArcherDamageLevel()
    {
        // 저장된 레벨이 없으면 기본값으로 0레벨을 반환합니다.
        return PlayerPrefs.GetInt(ArcherDamageLevelKey, 0);
    }
}
