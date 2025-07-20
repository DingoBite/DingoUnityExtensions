using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace DingoUnityExtensions.DingoGameFlow.GameGlue
{
    public enum MultiReceive
    {
        Wait,
        Interrupt
    }

    public interface ICommandReceiver<in T>
    {
        UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, CancellationTokenSource cts = null);
        UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, MultiReceive multiReceive, CancellationTokenSource cts = null);
        
        UniTask<CommandReceiveResponse> ReceiveCompleteCommandAsync(T command, Func<UniTask<bool>> completeAwait, CancellationTokenSource cts = null);
        UniTask<CommandReceiveResponse> ReceiveCompleteCommandAsync(T command, Func<UniTask<bool>> completeAwait, MultiReceive multiReceive, CancellationTokenSource cts = null);
        
        Task<bool> CancelLastCommandAsync();
    }
    
    public class CommandReceiver<T> : ICommandReceiver<T>
    {
        public delegate UniTask<CommandReceiveResponse> CommandReceiveFunc(T command, CancellationToken ct);
        
        private readonly CommandReceiveFunc _receiveFunc;
        private readonly Func<UniTask> _interruptFunc;
        private readonly CommandReceiveFunc _completeFunc;
        private readonly MultiReceive _defaultMultiReceive;

        private UniTask _interruptingTask;
        private UniTask<CommandReceiveResponse> _pendingTask;
        private CancellationTokenSource _cts;
        private T _prevCommand;

        public CommandReceiver(CommandReceiveFunc receiveFunc, Func<UniTask> interruptFunc = null, MultiReceive defaultMultiReceive = MultiReceive.Wait, CommandReceiveFunc completeFunc = null)
        {
            _completeFunc = completeFunc;
            _defaultMultiReceive = defaultMultiReceive;
            _receiveFunc = receiveFunc;
            _interruptFunc = interruptFunc;
        }

        public UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, CancellationTokenSource cts = null) => ReceiveCommandAsync(command, _defaultMultiReceive, cts);
        
        public async UniTask<CommandReceiveResponse> ReceiveCommandAsync(T command, MultiReceive multiReceive, CancellationTokenSource cts = null)
        {
            if (_interruptingTask.Status is UniTaskStatus.Pending)
                await _interruptingTask;
            
            if (_pendingTask.Status is UniTaskStatus.Pending)
            {
                if (multiReceive is MultiReceive.Wait)
                    await _pendingTask;
                else if (multiReceive is MultiReceive.Interrupt)
                    await CancelLastCommandAsync();
            }

            _cts = cts ?? new CancellationTokenSource();
            _pendingTask = _receiveFunc(command, _cts.Token);
            return await _pendingTask;
        }

        public UniTask<CommandReceiveResponse> ReceiveCompleteCommandAsync(T command, Func<UniTask<bool>> completeAwait, CancellationTokenSource cts = null) => ReceiveCompleteCommandAsync(command, completeAwait, _defaultMultiReceive, cts);

        public async UniTask<CommandReceiveResponse> ReceiveCompleteCommandAsync(T command, Func<UniTask<bool>> completeAwait, MultiReceive multiReceive, CancellationTokenSource cts = null)
        {
            if (_completeFunc == null)
                return await ReceiveCommandAsync(command, multiReceive, cts);

            var canCompleteTask = completeAwait();
            
            if (_interruptingTask.Status is UniTaskStatus.Pending)
                await _interruptingTask;
            
            bool canBeCompleted;
            if (_pendingTask.Status is UniTaskStatus.Pending)
            {
                if (multiReceive is MultiReceive.Wait)
                {
                    canBeCompleted = await canCompleteTask;
                    if (!canBeCompleted)
                    {
                        await CancelLastCommandAsync();
                        return CommandReceiveResponse.Canceled;
                    }
                    await _pendingTask;
                    await _completeFunc(_prevCommand, _cts.Token);
                }
                await CancelLastCommandAsync();
            }
            
            _cts = cts ?? new CancellationTokenSource();
            _prevCommand = command;
            _pendingTask = _receiveFunc(_prevCommand, _cts.Token);
            canBeCompleted = await canCompleteTask;
            if (!canBeCompleted)
            {
                await CancelLastCommandAsync();
                return CommandReceiveResponse.Canceled;
            }

            return await _pendingTask;
        }

        public async Task<bool> CancelLastCommandAsync()
        {
            if (_interruptFunc == null || _pendingTask.Status is not UniTaskStatus.Pending)
                return false;

            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts = null;
            }
            _interruptingTask = _interruptFunc();
            await _interruptingTask;
            return true;
        }
    }
}