using Cysharp.Threading.Tasks;
using System.Threading;

public interface IAsyncCommand : ICommand
{
    UniTask ExecuteAsync(CancellationToken cancellationToken = default);
}