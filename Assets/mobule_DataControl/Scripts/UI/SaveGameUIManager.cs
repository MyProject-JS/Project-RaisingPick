using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 저장 슬롯 UI 목록을 동적으로 생성하고, 저장/로드/삭제 등 관련 UI 상호작용을 관리합니다.
/// '저장 모드'와 '로드 모드'를 전환하여 각 슬롯의 클릭 동작을 다르게 처리하는 로직을 포함합니다.
/// 최적화를 위해 오브젝트 풀링 기법을 사용하여 UI 요소의 파괴와 생성을 최소화합니다.
/// </summary>
public class SaveGameUIManager : MonoBehaviour
{
    [Header("프리팹 및 UI 패널")]
    [Tooltip("각 저장 슬롯을 나타내는 UI 프리팹")]
    public GameObject saveSlotButtonPrefab;
    [Tooltip("저장 슬롯 버튼들이 생성될 부모 Transform (ScrollView의 Content)")]
    public Transform contentPanel;
    [Tooltip("새로운 세이브 파일을 생성하는 버튼")]
    public Button createSaveButton;

    [Header("모드 전환 버튼")]
    [Tooltip("저장 모드로 전환하는 버튼")]
    [SerializeField] private Button saveButton;
    [Tooltip("로드 모드로 전환하는 버튼")]
    [SerializeField] private Button loadButton;

    // 현재 UI 모드를 결정합니다. true이면 저장 모드, false이면 로드 모드입니다.
    public bool saveMode = false;

    // 다음에 생성될 체크포인트 번호입니다.
    private int nextCheckpoint = 1;

    private void Start()
    {
        // UI를 초기화하고 버튼 리스너를 설정합니다.
        UpdateSaveGameUI();

        // 저장/로드 모드 전환 버튼에 리스너를 할당합니다.
        if (saveButton != null)
            saveButton.onClick.AddListener(() => { saveMode = true; });
        
        if (loadButton != null)
            loadButton.onClick.AddListener(() => { saveMode = false; });
        
        // '새로 만들기' 버튼에 리스너를 할당합니다.
        if (createSaveButton != null)
            createSaveButton.onClick.AddListener(CreateNewSaveData);
    }

    /// <summary>
    /// 저장된 파일 목록을 읽어와 전체 UI를 새로 고칩니다. (오브젝트 풀링 최적화 적용)
    /// </summary>
    public void UpdateSaveGameUI()
    {
        if (contentPanel == null) return;

        // 파일 시스템에서 모든 체크포인트를 가져와 정렬합니다.
        List<int> checkpoints = SaveLoadManager.Instance.GetAllCheckpoints();
        checkpoints.Sort(); 

        // 필요한 만큼 버튼을 활성화/생성하고 데이터를 설정합니다.
        for (int i = 0; i < checkpoints.Count; i++)
        {
            GameObject slotButtonGO;
            if (i < contentPanel.childCount)
            {
                // 기존에 있던 버튼을 재활용합니다.
                slotButtonGO = contentPanel.GetChild(i).gameObject;
            }
            else
            {
                // 버튼이 부족하면 새로 생성합니다.
                slotButtonGO = Instantiate(saveSlotButtonPrefab, contentPanel);
            }
            
            // 버튼을 설정하고 활성화합니다.
            SetupSlotButton(slotButtonGO, checkpoints[i]);
            slotButtonGO.SetActive(true);
        }

        // 사용하지 않는 나머지 버튼들은 파괴하는 대신 비활성화하여 풀에 남겨둡니다.
        for (int i = checkpoints.Count; i < contentPanel.childCount; i++)
        {
            contentPanel.GetChild(i).gameObject.SetActive(false);
        }

        // 다음에 저장될 체크포인트 번호를 업데이트합니다.
        nextCheckpoint = checkpoints.Count > 0 ? checkpoints[checkpoints.Count - 1] + 1 : 1;
    }

    /// <summary>
    /// 슬롯 버튼(신규 또는 재사용)의 내용과 이벤트를 설정합니다.
    /// </summary>
    private void SetupSlotButton(GameObject slotButtonGO, int checkpoint)
    {
        // 데이터 표시 설정
        GameData gameData = SaveLoadManager.Instance.LoadGame(checkpoint);
        TMP_Text buttonText = slotButtonGO.GetComponentInChildren<TMP_Text>();
        if (buttonText != null && gameData != null)
        {
            buttonText.text = $"체크포인트 {checkpoint}: {gameData.playerName}, 레벨: {gameData.playerLevel}, 점수: {gameData.playerScore}";
        }

        // 슬롯 클릭 리스너 설정 (재사용을 위해 기존 리스너 모두 제거 후 새로 추가)
        Button slotButton = slotButtonGO.GetComponent<Button>();
        slotButton.onClick.RemoveAllListeners(); 
        slotButton.onClick.AddListener(() => SaveOrLoad(checkpoint));

        // 삭제 버튼 리스너 설정 (재사용을 위해 기존 리스너 모두 제거 후 새로 추가)
        Button deleteButton = slotButtonGO.transform.Find("DeleteButton")?.GetComponent<Button>();
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => DeleteGame(checkpoint));
        }
    }

    /// <summary>
    /// 현재 모드(saveMode)에 따라 저장 또는 로드를 실행하는 분기 메서드입니다.
    /// </summary>
    private void SaveOrLoad(int checkpoint)
    {
        if (saveMode)
        {
            SaveChanges(checkpoint);
        }
        else
        {
            LoadGame(checkpoint);
        }
        // 어떤 동작이든 실행 후에는 UI를 새로 고쳐 최신 상태를 반영합니다.
        UpdateSaveGameUI(); 
    }
    
    /// <summary>
    /// 현재 게임 상태를 지정된 체크포인트에 덮어씁니다.
    /// </summary>
    private void SaveChanges(int checkpoint)
    {
        Debug.Log($"현재 게임 상태를 체크포인트 {checkpoint}에 덮어쓰기를 시도합니다.");
        GameDataManager.Instance.SaveGame(checkpoint);
    }
    
    /// <summary>
    /// 지정된 체크포인트의 데이터를 불러옵니다.
    /// </summary>
    private void LoadGame(int checkpoint)
    {
        Debug.Log($"체크포인트 {checkpoint}의 데이터를 불러오기를 시도합니다.");
        GameDataManager.Instance.LoadGame(checkpoint);
    }

    /// <summary>
    /// 지정된 체크포인트와 관련된 파일을 삭제합니다.
    /// </summary>
    private void DeleteGame(int checkpoint)
    {
        SaveLoadManager.Instance.DeleteGame(checkpoint);
        UpdateSaveGameUI(); // 삭제 후 UI를 새로 고칩니다.
    }

    /// <summary>
    /// 현재 게임 상태를 새로운 체크포인트 파일로 저장합니다.
    /// </summary>
    private void CreateNewSaveData()
    {
        GameDataManager.Instance.SaveGame(nextCheckpoint);
        UpdateSaveGameUI(); // 새로 저장 후 UI를 새로 고칩니다.
    }
}
