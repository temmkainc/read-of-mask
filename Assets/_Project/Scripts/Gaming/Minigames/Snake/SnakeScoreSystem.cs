public class SnakeScoreSystem
{
    public int Score { get; private set; }

    private const int SCORE_PER_FOOD = 10;

    public void AddScore()
    {
        Score += SCORE_PER_FOOD;
    }

    public void Reset()
    {
        Score = 0;
    }
}