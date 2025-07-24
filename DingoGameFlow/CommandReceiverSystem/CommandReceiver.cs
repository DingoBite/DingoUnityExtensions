using System.Threading;
using Cysharp.Threading.Tasks;

namespace DingoUnityExtensions.DingoGameFlow.CommandReceiverSystem
{
    public interface ICommandReceiver<in T>
    {
        UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, CancellationTokenSource cts = null);
    }
}