using System;
using TMPro;
using UnityEngine;

public class BreakoutScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerScoreText;

    private BreakoutScoreSystem _scoreSystem;

    public void Initialize(BreakoutScoreSystem scoreSystem)
    {
        _scoreSystem = scoreSystem;
        _scoreSystem.OnScoreChanged += UpdateScore;
    }

    public void UpdateScore(int player)
    {
        _playerScoreText.text = player.ToString("D8");
    }
}