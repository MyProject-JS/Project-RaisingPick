using System;

/// <summary>
/// 현재 활성화된 게임 데이터를 보유하고 관리하는 싱글톤 매니저입니다.
/// 게임 상태에 대한 중앙 접근점 역할을 합니다.
/// </summary>
public class GameDataManager : Singleton<GameDataManager>, IInitializer
{
    /// <summary>
    /// 활성 게임 데이터가 변경되었을 때 리스너에게 알립니다.
    /// 새로운 GameData 인스턴스가 인자로 전달됩니다.
    /// </summary>
    public event Action<GameData> OnDataChanged;

    /// <summary>
    /// 게임이 저장되었을 때 리스너에게 알립니다.
    /// 체크포인트 번호가 인자로 전달됩니다.
    /// </summary>
    public event Action<int> OnGameSaved;

    /// <summary>
    /// 게임을 불러왔을 때 리스너에게 알립니다.
    /// 체크포인트 번호가 인자로 전달됩니다.
    /// </summary>
    public event Action<int> OnGameLoaded;

    private GameData _data;

    /// <summary>
    /// 현재 활성화된 게임 데이터입니다.
    /// OnDataChanged 이벤트를 수신하는 것이 데이터 업데이트를 받는 가장 좋은 방법입니다.
    /// Setter는 private으로 설정되어 메서드를 통해서만 데이터가 예측 가능하게 변경되도록 보장합니다.
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
    /// GameDataManager를 초기화하고 새로운 GameData 인스턴스를 생성합니다.
    /// </summary>
    public override void Init()
    {
        // 매니저가 처음 초기화될 때 새로운 빈 게임 데이터 객체를 생성합니다.
        // 이 작업은 OnDataChanged 이벤트를 발생시킵니다.
        Data = new GameData();
    }

    /// <summary>
    /// 새로운 게임을 생성하고, 현재 데이터를 새로운 인스턴스로 리셋합니다.
    /// </summary>
    public void CreateNewGame()
    {
        // 이 작업은 OnDataChanged 이벤트를 발생시킵니다.
        Data = new GameData();
    }

    /// <summary>
    /// 특정 체크포인트의 게임 데이터를 불러와 활성 Data 프로퍼티에 저장합니다.
    /// </summary>
    /// <param name="checkpoint">불러올 체크포인트 번호입니다.</param>
    public void LoadGame(int checkpoint)
    {
        // SaveLoadManager를 사용하여 파일에서 데이터를 가져옵니다.
        var loadedData = SaveLoadManager.Instance.LoadGame(checkpoint);
        if (loadedData != null)
        {
            // 로드에 성공하면, 현재 활성 데이터를 교체합니다.
            // 이 작업은 OnDataChanged 이벤트를 발생시킵니다.
            this.Data = loadedData;

            // OnGameLoaded 이벤트를 발생시킵니다.
            OnGameLoaded?.Invoke(checkpoint);
        }
    }

    /// <summary>
    /// 현재 활성 게임 데이터를 특정 체크포인트에 저장합니다.
    /// </summary>
    /// <param name="checkpoint">저장할 체크포인트 번호입니다.</param>
    public void SaveGame(int checkpoint)
    {
        // SaveLoadManager를 사용하여 활성 데이터를 파일에 씁니다.
        SaveLoadManager.Instance.SaveGame(checkpoint, this.Data);

        // OnGameSaved 이벤트를 발생시킵니다.
        OnGameSaved?.Invoke(checkpoint);
    }
    
    /// <summary>
    /// 제공된 액션을 사용하여 현재 게임 데이터를 수정하고 리스너에게 알립니다.
    /// 게임 플레이 중 데이터 필드를 변경하는 데 권장되는 방법입니다.
    /// </summary>
    /// <param name="modification">GameData 인스턴스를 수정하는 액션입니다.</param>
    public void ModifyData(Action<GameData> modification)
    {
        modification?.Invoke(_data);
        OnDataChanged?.Invoke(_data);
    }
}
