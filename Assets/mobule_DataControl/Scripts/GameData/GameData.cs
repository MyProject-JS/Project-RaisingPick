[System.Serializable]
public class GameData
{
    public string playerName;
    public int playerLevel;
    public int playerScore;
    public int currentLives;
    public int highScore;

    // 역직렬화를 위해 매개변수 없는 생성자가 필요합니다.
    public GameData()
    {
        // 필요한 경우 기본값으로 초기화합니다.
        // 실제 생명력 초기화는 GameManager에서 담당합니다.
        this.playerName = "New Player";
        this.playerLevel = 1;
        this.playerScore = 0;
        this.currentLives = 3;
        this.highScore = 0;
    }

    // 모든 필드를 초기화하는 생성자입니다.
    public GameData(string playerName, int playerLevel, int playerScore, int currentLives, int highScore)
    {
        this.playerName = playerName;
        this.playerLevel = playerLevel;
        this.playerScore = playerScore;
        this.currentLives = currentLives;
        this.highScore = highScore;
    }
}