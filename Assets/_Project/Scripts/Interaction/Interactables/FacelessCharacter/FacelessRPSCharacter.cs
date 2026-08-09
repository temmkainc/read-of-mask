using System.Collections;
using UnityEngine;
using Zenject;
using TMPro;

public class FacelessRPSCharacter : FacelessCharacter
{
    [Header("Rock Paper Scissors")]
    [SerializeField] private RockPaperScissorsMenuController _rpsController;
    [SerializeField] private float _facelessThinkDuration = 1.2f;
    [SerializeField] private float _resultDisplayDuration = 1.5f;
    [SerializeField] private TMP_Text _displayInfoText;
    [SerializeField] private PlayerFirstPersonRPSController _playerFirstPersonRPSController;

    private readonly int DoRockHash = Animator.StringToHash("DoRock");
    private readonly int DoPaperHash = Animator.StringToHash("DoPaper");
    private readonly int DoScissorsHash = Animator.StringToHash("DoScissors");
    private const int RPSLayerIndex = 2;
    private bool _hasWon = false;
    [Inject] private LevelVariables _levelVariables;

    private Coroutine _layerFadeCoroutine;

    private void Start()
    {
        _displayInfoText.gameObject.SetActive(false);
    }

    protected override void On_PlayerStateChanged(PlayerStateType type)
    {
        bool wasLookCloser = _previousPlayerStateType == PlayerStateType.LookCloser;

        base.On_PlayerStateChanged(type);

        if (type == PlayerStateType.LookCloser)
            OnEnterLookCloser();
        else if (wasLookCloser)
            OnExitLookCloser();
    }

    private void OnEnterLookCloser()
    {
        _animator.Play("Empty State", RPSLayerIndex, 0f);
        _playerFirstPersonRPSController.gameObject.SetActive(true);
        _animator.SetLayerWeight(RPSLayerIndex, 1f);
        _rpsController.Activate();
        _rpsController.OnPlayerSubmitted += OnPlayerSubmitted;
    }

    private void OnExitLookCloser()
    {
        _rpsController.Deactivate();
        _rpsController.OnPlayerSubmitted -= OnPlayerSubmitted;

        if (_layerFadeCoroutine != null)
            StopCoroutine(_layerFadeCoroutine);
        _layerFadeCoroutine = StartCoroutine(FadeLayerWeight(RPSLayerIndex, 0f, 0.3f));
        _playerFirstPersonRPSController.gameObject.SetActive(false);
    }

    private IEnumerator FadeLayerWeight(int layerIndex, float targetWeight, float duration)
    {
        float startWeight = _animator.GetLayerWeight(layerIndex);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _animator.SetLayerWeight(layerIndex, Mathf.Lerp(startWeight, targetWeight, elapsed / duration));
            yield return null;
        }

        _animator.SetLayerWeight(layerIndex, targetWeight);
        _layerFadeCoroutine = null;
    }

    private void OnPlayerSubmitted(RockPaperScissorsChoice playerChoice)
    {
        StartCoroutine(PlayRound(playerChoice));
    }

    private IEnumerator PlayRound(RockPaperScissorsChoice playerChoice)
    {
        yield return new WaitForSeconds(_facelessThinkDuration);
        AudioManager.Instance.PlaySFX(SfxClips.RockPaperScissors, volumeScale: 1.5f);

        RockPaperScissorsChoice facelessChoice = (RockPaperScissorsChoice)UnityEngine.Random.Range(0, 3);
        PlayFacelessAnimation(facelessChoice);
        _playerFirstPersonRPSController.PlayAnimation(playerChoice);

        yield return new WaitForSeconds(_resultDisplayDuration);

        RPSResult result = Evaluate(playerChoice, facelessChoice);
        if (result == RPSResult.Win && !_hasWon)
        {
            _hasWon = true;
            _displayInfoText.gameObject.SetActive(true);
            _displayInfoText.text = _levelVariables.CorrectPassword;

            _commandBus.GoToPreviousPlayerState();
            yield break;
        }
        ResetRPSAnimations();
        _rpsController.Unlock();
    }

    private void PlayFacelessAnimation(RockPaperScissorsChoice choice)
    {
        ResetRPSAnimations();

        int hash = choice switch
        {
            RockPaperScissorsChoice.Rock => DoRockHash,
            RockPaperScissorsChoice.Paper => DoPaperHash,
            RockPaperScissorsChoice.Scissors => DoScissorsHash,
            _ => DoRockHash
        };

        _animator.SetTrigger(hash);
    }

    private void ResetRPSAnimations()
    {
        _animator.ResetTrigger(DoRockHash);
        _animator.ResetTrigger(DoPaperHash);
        _animator.ResetTrigger(DoScissorsHash);
    }

    private enum RPSResult { Win, Lose, Draw }

    private static RPSResult Evaluate(RockPaperScissorsChoice player, RockPaperScissorsChoice faceless)
    {
        if (player == faceless) return RPSResult.Draw;

        bool win = (player == RockPaperScissorsChoice.Rock && faceless == RockPaperScissorsChoice.Scissors) ||
                   (player == RockPaperScissorsChoice.Paper && faceless == RockPaperScissorsChoice.Rock) ||
                   (player == RockPaperScissorsChoice.Scissors && faceless == RockPaperScissorsChoice.Paper);

        return win ? RPSResult.Win : RPSResult.Lose;
    }
}