using Cysharp.Threading.Tasks;
using System;

public sealed class CommandHandler<T> where T : ICommand
{
    private readonly Func<T> _factory;
    private readonly CommandBus _bus;

    public CommandHandler(Func<T> factory, CommandBus bus)
    {
        _factory = factory;
        _bus = bus;
    }

    public void Execute()
    {
        var command = _factory();

        switch (command)
        {
            case ISyncCommand syncCommand:
                _bus.ExecuteInternal(syncCommand);
                break;
            case IAsyncCommand asyncCommand:
                _bus.ExecuteInternalAsync(asyncCommand).GetAwaiter().GetResult();
                break;
            default:
                throw new InvalidOperationException("Registered command is not a valid command type.");
        }
    }

    public UniTask ExecuteAsync()
    {
        var command = _factory();

        switch (command)
        {
            case IAsyncCommand asyncCommand:
                return _bus.ExecuteInternalAsync(asyncCommand);
            case ISyncCommand syncCommand:
                _bus.ExecuteInternal(syncCommand);
                return UniTask.CompletedTask;
            default:
                throw new InvalidOperationException("Registered command is not a valid command type.");
        }
    }
}