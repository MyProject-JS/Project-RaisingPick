using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위해 추가
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Inspector에서 버튼을 할당할 수 있도록 private 변수로 선언합니다.
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        // 버튼이 할당되었는지 확인하고, 각 버튼에 클릭 리스너를 코드로 추가합니다.
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    public void StartGame()
    {
        // "GameScene"이라는 이름의 씬을 불러옵니다.
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        // 에디터에서는 동작하지 않지만, 빌드된 게임에서는 어플리케이션을 종료합니다.
        Debug.Log("Game is exiting"); // 에디터 확인용 로그
        Application.Quit();
    }
}
