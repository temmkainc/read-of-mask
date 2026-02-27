using UnityEngine;
using Zenject;

public sealed class CommandFactory : ICommandResolver
{
    private readonly DiContainer _container;

    public CommandFactory(DiContainer container)
    {
        _container = container;
    }

    public void Resolve(ICommand command)
    {
        _container.Inject(command);
    }
}
