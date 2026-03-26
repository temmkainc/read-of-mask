using TMPro;
using UnityEngine;

public class SnakeScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private SnakeScoreSystem _score;

    public void Initialize(SnakeScoreSystem score)
    {
        _score = score;
        UpdateUI();
    }

    public void UpdateUI()
    {
        _text.text = "Score: " + _score.Score.ToString("D8");
    }
}