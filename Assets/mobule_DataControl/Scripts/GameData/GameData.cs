[System.Serializable]
public class GameData
{
    public string playerName;
    public int playerLevel;
    public int playerScore;

    // 역직렬화를 위해 매개변수 없는 생성자가 필요합니다.
    public GameData()
    {
        // 필요한 경우 기본값으로 초기화합니다.
        this.playerName = "New Player";
        this.playerLevel = 1;
        this.playerScore = 0;
    }

    // 모든 필드를 초기화하는 생성자입니다.
    public GameData(string playerName, int playerLevel, int playerScore)
    {
        this.playerName = playerName;
        this.playerLevel = playerLevel;
        this.playerScore = playerScore;
    }
}