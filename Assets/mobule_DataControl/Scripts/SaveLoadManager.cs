using System.IO;
using UnityEngine;

/// <summary>
/// 자동 저장 시스템을 위한 파일 입출력을 담당하는 싱글톤 클래스입니다.
/// JsonDataManager를 사용하여 모든 파일 작업을 처리합니다.
/// </summary>
public class SaveLoadManager : Singleton<SaveLoadManager>, IInitializer
{
    // 단일 자동 저장 파일의 이름을 상수로 정의합니다.
    private const string SaveFileName = "GameData_AutoSave.json";

    /// <summary>
    /// 현재 게임 데이터를 자동 저장 파일에 저장합니다.
    /// </summary>
    /// <param name="gameData">저장할 게임 데이터입니다.</param>
    public void SaveGame(GameData gameData)
    {
        JsonDataManager.SaveToJson(gameData, SaveFileName);
        // 자동 저장은 빈번하게 일어나므로, 디버그 로그는 주석 처리하거나 필요 시에만 활성화하는 것이 좋습니다.
        // Debug.Log($"Game data auto-saved to {Path.Combine(JsonDataManager.SaveFolder, SaveFileName)}");
    }

    /// <summary>
    /// 자동 저장된 게임 데이터를 불러옵니다.
    /// </summary>
    /// <returns>불러온 게임 데이터, 파일이 없으면 null을 반환합니다.</returns>
    public GameData LoadGame()
    {
        string path = Path.Combine(JsonDataManager.SaveFolder, SaveFileName);

        if (!File.Exists(path))
        {
            Debug.Log("자동 저장 파일을 찾을 수 없습니다. 새로운 게임을 시작합니다.");
            return null;
        }
        
        GameData gameData = JsonDataManager.LoadFromJson<GameData>(SaveFileName);
        Debug.Log("자동 저장된 게임 데이터를 불러왔습니다.");
        return gameData;
    }

    /// <summary>
    /// 자동 저장된 게임 데이터를 삭제합니다. (예: 새 게임 시작 시)
    /// </summary>
    public void DeleteSavedGame()
    {
        string filePath = Path.Combine(JsonDataManager.SaveFolder, SaveFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("자동 저장된 게임 데이터가 삭제되었습니다.");
        }
    }
}
