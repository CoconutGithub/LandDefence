//DataManager.cs
using UnityEngine;

// 게임 데이터를 영구적으로 저장하고 불러오는 역할을 하는 클래스입니다.
public static class DataManager
{
    // (수정) 각 데이터 종류별로 키를 생성하는 헬퍼 함수들을 만듭니다.
    private static string GetExpKey(TowerType type)
    {
        return type.ToString() + "Experience"; // 예: "ArcherExperience", "MageExperience"
    }

    private static string GetDamageLevelKey(TowerType type)
    {
        return type.ToString() + "DamageLevel"; // 예: "ArcherDamageLevel"
    }

    // --- 경험치 저장 및 불러오기 (타워 타입별) ---
    public static void SaveExperience(TowerType type, int totalExp)
    {
        PlayerPrefs.SetInt(GetExpKey(type), totalExp);
        PlayerPrefs.Save();
        Debug.Log($"{type} 경험치 저장 완료: {totalExp}");
    }

    public static int LoadExperience(TowerType type)
    {
        return PlayerPrefs.GetInt(GetExpKey(type), 0);
    }

    // --- 공격력 레벨 저장 및 불러오기 (타워 타입별) ---
    public static void SaveDamageLevel(TowerType type, int level)
    {
        PlayerPrefs.SetInt(GetDamageLevelKey(type), level);
        PlayerPrefs.Save();
        Debug.Log($"{type} 공격력 레벨 저장 완료: {level}");
    }

    public static int LoadDamageLevel(TowerType type)
    {
        return PlayerPrefs.GetInt(GetDamageLevelKey(type), 0);
    }
}
