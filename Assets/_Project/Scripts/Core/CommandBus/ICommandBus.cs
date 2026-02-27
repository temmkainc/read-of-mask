using System;

public interface ICommandBus : IDisposable
{
    public CommandHandler<T> Register<T>(Func<T> commandFactory) where T : ICommand;
    void Undo<T>();
    void ClearUndo<T>();
}