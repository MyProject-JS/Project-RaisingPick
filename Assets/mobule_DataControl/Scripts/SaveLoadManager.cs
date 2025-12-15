using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This class is a plain C# Singleton and uses JsonDataManager for all file operations.
public class SaveLoadManager : Singleton<SaveLoadManager>, IInitializer
{
    public void SaveGame(int checkpoint, GameData gameData)
    {
        string fileName = GetSaveFileName(checkpoint);
        JsonDataManager.SaveToJson(gameData, fileName);
        Debug.Log($"Game data saved for checkpoint {checkpoint} to {Path.Combine(JsonDataManager.SaveFolder, fileName)}");
    }

    public GameData LoadGame(int checkpoint)
    {
        string fileName = GetSaveFileName(checkpoint);
        string path = Path.Combine(JsonDataManager.SaveFolder, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"체크포인트 {checkpoint}의 저장 파일을 찾을 수 없습니다!");
            return null;
        }
        
        GameData gameData = JsonDataManager.LoadFromJson<GameData>(fileName);
        Debug.Log($"체크포인트 {checkpoint}에서 게임 데이터를 불러왔습니다.");
        return gameData;
    }

    public void DeleteGame(int checkpoint)
    {
        string fileName = GetSaveFileName(checkpoint);
        string filePath = Path.Combine(JsonDataManager.SaveFolder, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"체크포인트 {checkpoint}의 게임 데이터가 삭제되었습니다.");
        }
        else
        {
            Debug.LogWarning($"체크포인트 {checkpoint}에서 삭제할 저장 파일을 찾을 수 없습니다!");
        }
    }

    public List<int> GetAllCheckpoints()
    {
        List<int> checkpoints = new List<int>();
        
        if (Directory.Exists(JsonDataManager.SaveFolder))
        {
            string searchPattern = $"{nameof(GameData)}_checkpoint_*.json";
            string[] files = Directory.GetFiles(JsonDataManager.SaveFolder, searchPattern);

            string prefix = $"{nameof(GameData)}_checkpoint_";

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string checkpointString = fileName.Substring(prefix.Length);

                if (int.TryParse(checkpointString, out int checkpoint))
                {
                    checkpoints.Add(checkpoint);
                }
            }
        }

        return checkpoints;
    }

    private string GetSaveFileName(int checkpoint)
    {
        return $"{nameof(GameData)}_checkpoint_{checkpoint}.json";
    }
}
