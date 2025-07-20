using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DingoUnityExtensions.DingoGameFlow.GameGlue
{
    public abstract class CommandReceiverBehaviour<T> : MonoBehaviour, ICommandReceiver<T>
    {
        [SerializeField] private MultiReceive _multiReceive = MultiReceive.Wait;
        
        private CommandReceiver<T> _commandReceiver;
        
        protected T LastCommand { get; private set; }

        public async UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, CancellationTokenSource cts = null)
            => await ReceiveCommandAsync(command, _multiReceive, cts);

        public async UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, MultiReceive multiReceive, CancellationTokenSource cts = null)
        {
            _commandReceiver ??= new CommandReceiver<T>(ReceiveWrapAsync, RollBackAsync, _multiReceive);
            try
            {
                return await _commandReceiver.ReceiveCommandAsync(command, multiReceive, cts);
            }
            catch (Exception e)
            {
                return new CommandReceiveResponse(CommandReceiveState.Exception, receiver: this, exception: e);
            }
        }

        public UniTask<CommandReceiveResponse> ReceiveCompleteCommandAsync(T command, Func<UniTask<bool>> completeAwait, CancellationTokenSource cts = null)
            => ReceiveCompleteCommandAsync(command, completeAwait, _multiReceive, cts);

        public async UniTask<CommandReceiveResponse> ReceiveCompleteCommandAsync(T command, Func<UniTask<bool>> completeAwait, MultiReceive multiReceive, CancellationTokenSource cts = null)
        {
            _commandReceiver ??= new CommandReceiver<T>(ReceiveWrapAsync, RollBackAsync, _multiReceive);
            try
            {
                return await _commandReceiver.ReceiveCompleteCommandAsync(command, completeAwait, multiReceive, cts);
            }
            catch (Exception e)
            {
                return new CommandReceiveResponse(CommandReceiveState.Exception, receiver: this, exception: e);
            }
        }

        public async Task<bool> CancelLastCommandAsync()
        {
            if (_commandReceiver == null)
                return false;
            try
            {
                return await _commandReceiver.CancelLastCommandAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                return false;
            }
        }
        
        private async UniTask<CommandReceiveResponse> ReceiveWrapAsync(T command, CancellationToken ct)
        {
            try
            {
                LastCommand = command;
                return await ReceiveAsync(LastCommand).AttachExternalCancellation(ct);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                    return CommandReceiveResponse.CanceledFrom(this);
                throw;
            }
        }
        
        protected abstract UniTask<CommandReceiveResponse> ReceiveAsync(T command);
        protected abstract UniTask RollBackAsync();
    }
}