using System;
using BookCurlPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using System.Collections;

public sealed class DiaryState : PlayerState
{
    [Inject] private InputManager _inputsManager;
    [Inject] private Diary _diary;
    [Inject] private BookPro _book;
    private bool _flipInProgress = false;
    public DiaryState(PlayerStateData data) : base(data)
    {

    }
    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void Enter()
    {
        base.Enter();
        _flipInProgress = false; // always reset on enter
        _diary.gameObject.SetActive(true);
        _inputsManager.CloseDiaryAction.performed += On_CloseDiaryRequestedHandler;
        _inputsManager.DiaryDirectionAction.performed += On_DiaryDirectionHandler;
    }

    private void On_DiaryDirectionHandler(InputAction.CallbackContext context)
    {
        if (_flipInProgress) return;

        Vector2 direction = context.ReadValue<Vector2>();

        if (direction.x > 0.5f)
            NextPage();
        else if (direction.x < -0.5f)
            PreviousPage();
    }

    private void NextPage()
    {
        if (_book.currentPaper > _book.EndFlippingPaper) return;
        _flipInProgress = true;
        _book.DragRightPageToPoint(_book.EndBottomRight * 0.98f); // start from RIGHT corner
        _book.TweenForward(); // then animate to left
        _diary.StartCoroutine(ResetFlipFlag());
    }

    private void PreviousPage()
    {
        if (_book.currentPaper <= _book.StartFlippingPaper) return;
        _flipInProgress = true;
        _book.DragLeftPageToPoint(_book.EndBottomLeft * 0.98f); // start from LEFT corner
        _book.TweenForward();
        _diary.StartCoroutine(ResetFlipFlag());
    }
    private IEnumerator ResetFlipFlag()
    {
        yield return new WaitForSeconds(0.4f);
        _flipInProgress = false;
    }

    private void On_CloseDiaryRequestedHandler(InputAction.CallbackContext context)
    {
        CommandBus.GoToPreviousPlayerState();
    }

    public override void Exit()
    {
        base.Exit();
        _diary.StopAllCoroutines(); // prevent ResetFlipFlag from firing late
        _diary.gameObject.SetActive(false);
        _inputsManager.CloseDiaryAction.performed -= On_CloseDiaryRequestedHandler;
        _inputsManager.DiaryDirectionAction.performed -= On_DiaryDirectionHandler;
    }
}
