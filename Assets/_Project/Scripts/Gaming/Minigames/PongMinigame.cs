using UnityEngine;

public class PongMinigame : MinigameBase
{
    [SerializeField] private PongPlayerPaddle _playerPaddle;
    [SerializeField] private PongPlayerPaddle _computerPaddle;
    [SerializeField] private float _minY = -4.5f;
    [SerializeField] private float _maxY = 4.5f;

    [SerializeField] private PongBall _ball;
    [SerializeField] private float _leftBoundary = -9f;
    [SerializeField] private float _rightBoundary = 9f;
    [SerializeField] private float _topBoundary = 5f;
    [SerializeField] private float _bottomBoundary = -5f;

    [SerializeField] private PongInputHandler _inputHandler;

    public override void Initialize()
    {
        base.Initialize();
        _inputHandler.Initialize(_inputManager.GamingDirectionAction, _playerPaddle);
        _playerPaddle.SetBounds(_minY, _maxY);
        _computerPaddle.SetBounds(_minY, _maxY);
    }

    protected override void OnMenuButtonSelected(int index)
    {
        switch (index)
        {
            case 0: StartGame(); break;
            case 1: ExitGameInternally(); break;
        }
    }

    public override void StartGame()
    {
        base.StartGame();
        _ball.ResetBall();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (!_isPaused)
        {
            _playerPaddle.Move(dt);

            // Simple AI
            float targetY = _ball.transform.localPosition.y;
            _computerPaddle.SetInput(Mathf.Sign(targetY - _computerPaddle.Y));
            _computerPaddle.Move(dt);

            // Move ball
            _ball.Move(dt);

            // Check paddle collisions
            _ball.CheckPaddleCollision(_playerPaddle);
            _ball.CheckPaddleCollision(_computerPaddle);

            // Check scoring
            _ball.CheckScore(_leftBoundary, _rightBoundary);
        }
    }

    public override void ExitGame()
    {
        _inputHandler.Dispose();
        base.ExitGame();
    }

    public override void EnterGame()
    {
        base.EnterGame();
    }

}