using UnityEngine;

// 게임 데이터를 영구적으로 저장하고 불러오는 역할을 하는 클래스입니다.
// static으로 선언하여 게임 내 어디서든 쉽게 접근할 수 있습니다.
public static class DataManager
{
    // 데이터를 저장할 때 사용할 키(Key)입니다. 오타를 방지하기 위해 상수로 만들어 둡니다.
    private const string ArcherExpKey = "ArcherExperience";
    // 나중에 다른 타워 경험치가 추가되면 여기에 키를 추가하면 됩니다.
    // private const string MageExpKey = "MageExperience";

    // 궁수 경험치를 저장하는 함수입니다.
    public static void SaveArcherExperience(int totalExp)
    {
        // PlayerPrefs는 간단한 데이터를 사용자의 컴퓨터에 저장하는 Unity의 기능입니다.
        PlayerPrefs.SetInt(ArcherExpKey, totalExp);
        PlayerPrefs.Save(); // 변경사항을 디스크에 즉시 저장합니다.
        Debug.Log("궁수 경험치 저장 완료: " + totalExp);
    }

    // 저장된 궁수 경험치를 불러오는 함수입니다.
    public static int LoadArcherExperience()
    {
        // 저장된 값이 없으면 기본값으로 0을 반환합니다.
        return PlayerPrefs.GetInt(ArcherExpKey, 0);
    }
}
