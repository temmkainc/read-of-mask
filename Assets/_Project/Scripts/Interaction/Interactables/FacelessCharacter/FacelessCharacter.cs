using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using TMPro;

public class FacelessCharacter : LookCloserInteractableBase
{
    public enum InteractionType { Default, RockPaperScissors }

    [Header("Interaction mode")]
    [SerializeField] private InteractionType _interactionType = InteractionType.Default;

    [Header("Animator")]
    [SerializeField] private Animator _animator;

    [Header("Rock Paper Scissors")]
    [SerializeField] private RockPaperScissorsMenuController _rpsController;
    [SerializeField] private float _facelessThinkDuration = 1.2f;
    [SerializeField] private float _resultDisplayDuration = 1.5f;
    [SerializeField] private TMP_Text _displayInfoText;

    private readonly int DoShowMiddleFingerHash = Animator.StringToHash("DoShowMiddleFinger");
    private readonly int DoHideMiddleFingerHash = Animator.StringToHash("DoHideMiddleFinger");

    private readonly int DoRockHash = Animator.StringToHash("DoRock");
    private readonly int DoPaperHash = Animator.StringToHash("DoPaper");
    private readonly int DoScissorsHash = Animator.StringToHash("DoScissors");
    private readonly int RPSLayerIndex = 2;

    [Inject] private InputManager _inputManager;
    [Inject] private LevelVariables _levelVariables;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        _inputManager = inputManager;
        _inputManager.ShowMiddleFingerAction.performed += On_MiddleFingerPressed;
        _inputManager.ShowMiddleFingerAction.canceled += On_MiddleFingerReleased;
        _displayInfoText.gameObject.SetActive(false);
    }

    public override void Interact(Player player = null)
    {
        if (player.Grabbing.IsHolding) return;

        base.Interact(player);
    }

    protected override void On_PlayerStateChanged(PlayerStateType type)
    {
        bool wasLookCloser = _previousPlayerStateType == PlayerStateType.LookCloser;

        base.On_PlayerStateChanged(type);

        if (type == PlayerStateType.LookCloser)
        {
            OnEnterLookCloser();
        }
        else if (wasLookCloser)
        {
            OnExitLookCloser();
        }
    }

    private void OnEnterLookCloser()
    {
        if (_interactionType == InteractionType.RockPaperScissors)
        {
            _animator.Play("Empty State", RPSLayerIndex, 0f);
            _animator.SetLayerWeight(RPSLayerIndex, 1f);
            _rpsController.Activate();
            _rpsController.OnPlayerSubmitted += OnPlayerSubmitted;
        }
    }

    private Coroutine _layerFadeCoroutine;

    private void OnExitLookCloser()
    {
        if (_interactionType == InteractionType.RockPaperScissors)
        {
            _rpsController.Deactivate();
            _rpsController.OnPlayerSubmitted -= OnPlayerSubmitted;

            if (_layerFadeCoroutine != null)
                StopCoroutine(_layerFadeCoroutine);
            _layerFadeCoroutine = StartCoroutine(FadeLayerWeight(RPSLayerIndex, 0f, 0.3f));
        }
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

        RockPaperScissorsChoice facelessChoice = (RockPaperScissorsChoice)UnityEngine.Random.Range(0, 3);
        PlayFacelessAnimation(facelessChoice);

        yield return new WaitForSeconds(_resultDisplayDuration);

        RPSResult result = Evaluate(playerChoice, facelessChoice);
        if (result == RPSResult.Win)
        {
            _displayInfoText.gameObject.SetActive(true);
            _displayInfoText.text = _levelVariables.CorrectPassword;
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

    private void On_MiddleFingerPressed(InputAction.CallbackContext ctx)
    {
        _animator.ResetTrigger(DoHideMiddleFingerHash);
        _animator.SetTrigger(DoShowMiddleFingerHash);
    }

    private void On_MiddleFingerReleased(InputAction.CallbackContext ctx)
    {
        _animator.ResetTrigger(DoShowMiddleFingerHash);
        _animator.SetTrigger(DoHideMiddleFingerHash);
    }
}