using System;
using UnityEngine; // For Debug.Log

/// <summary>
/// 현재 활성화된 게임 데이터를 보유하고 관리하는 싱글톤 매니저입니다.
/// 게임 상태에 대한 중앙 접근점 역할을 하며, 자동 저장/로드를 관리합니다.
/// </summary>
public class GameDataManager : Singleton<GameDataManager>, IInitializer
{
    /// <summary>
    /// 활성 게임 데이터가 변경되었을 때 리스너에게 알립니다.
    /// </summary>
    public event Action<GameData> OnDataChanged;

    private GameData _data;

    /// <summary>
    /// 현재 활성화된 게임 데이터입니다.
    /// 데이터를 변경하면 OnDataChanged 이벤트가 발생하여 자동으로 저장이 트리거됩니다.
    /// </summary>
    public GameData Data
    {
        get => _data;
        private set
        {
            _data = value;
            // 모든 구독자에게 데이터가 변경되었음을 알립니다.
            OnDataChanged?.Invoke(_data);
        }
    }

    /// <summary>
    /// GameDataManager를 초기화하고, 자동 저장을 설정하며, 기존 데이터를 불러옵니다.
    /// </summary>
    public override void Init()
    {
        // 데이터가 변경될 때마다 자동으로 SaveGame 메서드를 호출하도록 이벤트를 구독합니다.
        OnDataChanged += (newData) => SaveLoadManager.Instance.SaveGame(newData);
        
        // SaveLoadManager를 통해 저장된 데이터를 불러옵니다.
        var loadedData = SaveLoadManager.Instance.LoadGame();
        
        // 불러온 데이터가 있으면 사용하고, 없으면 새로운 게임 데이터를 생성합니다.
        // Data 프로퍼티를 사용함으로써, 새 데이터 생성 시 첫 저장이 자동으로 트리거됩니다.
        Data = loadedData ?? new GameData();
    }

    /// <summary>
    /// 새로운 게임을 생성하고, 현재 데이터를 리셋하며 기존 자동 저장 파일을 삭제합니다.
    /// </summary>
    public void CreateNewGame()
    {
        // 기존 자동 저장 파일을 삭제합니다.
        SaveLoadManager.Instance.DeleteSavedGame();
        
        // 새로운 GameData 인스턴스를 생성합니다.
        // 이 작업은 OnDataChanged 이벤트를 발생시켜 비어있는 새 파일로 자동 저장됩니다.
        Data = new GameData();
    }
    
    /// <summary>
    /// 제공된 액션을 사용하여 현재 게임 데이터를 수정하고 리스너에게 알립니다.
    /// OnDataChanged 이벤트가 발생하여 변경 사항이 자동으로 저장됩니다.
    /// </summary>
    /// <param name="modification">GameData 인스턴스를 수정하는 액션입니다.</param>
    public void ModifyData(Action<GameData> modification)
    {
        if (_data != null)
        {
            modification?.Invoke(_data);
            OnDataChanged?.Invoke(_data);
        }
        else
        {
            Debug.LogError("데이터가 초기화되지 않았습니다. ModifyData를 호출할 수 없습니다.");
        }
    }

    /// <summary>
    /// 현재 게임 데이터를 강제로 파일에 저장합니다. (예: 게임 종료 시)
    /// </summary>
    public void ForceSave()
    {
        if (_data != null)
        {
            SaveLoadManager.Instance.SaveGame(_data);
            Debug.Log("게임 데이터가 강제로 저장되었습니다.");
        }
    }
}
