using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

public sealed class CommandBus : ICommandBus
{
    private readonly ICommandResolver _commandResolver;
    private readonly Dictionary<Type, Stack<IUndoCommand>> _undoable = new();
    private readonly object _lock = new();

    private CancellationTokenSource _cancellationSource = new();

    public CommandBus(ICommandResolver commandResolver)
    {
        _commandResolver = commandResolver;
    }

    public CommandHandler<T> Register<T>(Func<T> commandFactory) where T : ICommand
    {
        return new CommandHandler<T>(commandFactory, this);
    }

    internal void ExecuteInternal(ISyncCommand command)
    {
        _commandResolver.Resolve(command);

        try
        {
            TryRegisterUndo(command);
            command.Execute();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred while executing the command of type {command.GetType()}.", ex);
        }
    }

    internal UniTask ExecuteInternalAsync(IAsyncCommand command)
    {
        _commandResolver.Resolve(command);

        try
        {
            CancellationToken token;
            lock (_lock)
            {
                token = _cancellationSource.Token;
            }

            TryRegisterUndo(command);

            token.ThrowIfCancellationRequested();
            return command.ExecuteAsync(token);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred while executing the async command of type {command.GetType()}.", ex);
        }
    }

    private void TryRegisterUndo<T>(T command)
    {
        if (command is not IUndoCommand undoCommand)
            return;

        var type = command.GetType();

        lock (_lock)
        {
            if (_undoable.TryGetValue(type, out var undoCommands))
            {
                undoCommands.Push(undoCommand);
            }
            else
            {
                var stack = new Stack<IUndoCommand>();
                stack.Push(undoCommand);
                _undoable[type] = stack;
            }
        }
    }

    public void Undo<T>()
    {
        var type = typeof(T);

        if (!_undoable.TryGetValue(type, out var undoCommands))
            return;

        if (undoCommands.Count <= 0)
        {
            _undoable.Remove(type);
            return;
        }

        var undo = undoCommands.Pop();

        undo.Undo();
    }

    public void ClearUndo<T>()
    {
        var type = typeof(T);

        if (!_undoable.TryGetValue(type, out var undoCommands))
            return;

        undoCommands.Clear();
    }

    public void AbortAll()
    {
        lock (_lock)
        {
            _cancellationSource.Cancel();
            _cancellationSource.Dispose();
            _cancellationSource = new CancellationTokenSource();
        }
    }

    public void Dispose()
    {
        AbortAll();
    }
}
