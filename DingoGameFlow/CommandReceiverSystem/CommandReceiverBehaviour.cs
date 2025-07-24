using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DingoUnityExtensions.DingoGameFlow.CommandReceiverSystem
{
    public abstract class CommandReceiverBehaviour<T> : MonoBehaviour, ICommandReceiver<T>
    {
        
        protected T LastCommand { get; private set; }

        public async UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, CancellationTokenSource cts = null)
        {
            try
            {
                return await ReceiveWrapAsync(command, cts);
            }
            catch (Exception e)
            {
                return new CommandReceiveResponse(CommandReceiveState.Exception, receiver: this, exception: e);
            }
        }

        private async UniTask<CommandReceiveResponse> ReceiveWrapAsync(T command, CancellationTokenSource cts)
        {
            try
            {
                LastCommand = command;
                return await ReceiveAsync(LastCommand, cts);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    return CommandReceiveResponse.CanceledFrom(this);
                throw;
            }
        }
        
        protected abstract UniTask<CommandReceiveResponse> ReceiveAsync(T command, CancellationTokenSource cts);
    }
}