using System;
using TMPro;
using UnityEngine;

public class PongScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerScoreText;
    [SerializeField] private TMP_Text _computerScoreText;

    private PongScoreSystem _scoreSystem;

    public void Initialize(PongScoreSystem scoreSystem)
    {
        _scoreSystem = scoreSystem;
        _scoreSystem.OnScoreChanged += UpdateScore;
    }

    public void UpdateScore(int player, int computer)
    {
        _playerScoreText.text = player.ToString();
        _computerScoreText.text = computer.ToString();
    }
}