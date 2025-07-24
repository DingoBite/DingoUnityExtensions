using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.DingoGameFlow.CommandReceiverSystem
{
    public readonly struct CommandReceiveResponse
    {
        public readonly CommandReceiveState State;
        public readonly Object Receiver;
        public readonly string Message;
        public readonly object Data;
        public readonly Exception Exception;
        
        public CommandReceiveResponse(CommandReceiveState state, string message = "", object data = null, Object receiver = null, Exception exception = null)
        {
            State = state;
            Receiver = receiver;
            Message = message;
            Data = data;
            Exception = exception;
        }

        public void LogResponse(bool successIgnore = true, bool pendingIgnore = true, bool cancelIgnore = true, bool noneIgnore = true)
        {
            switch (State)
            {
                case CommandReceiveState.None:
                    if (!noneIgnore)
                        Debug.Log($"None|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.Pending:
                    if (!pendingIgnore)
                        Debug.Log($"Wait|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.Success:
                    if (!successIgnore)
                        Debug.Log($"Success|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.Canceled:
                    if (!cancelIgnore)
                        Debug.Log($"Cancel|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.Collision:
                    Debug.LogError($"Collision|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.Disabled:
                    Debug.LogWarning($"Disable|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.InvalidData:
                    Debug.LogError($"Data|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.InitializeError:
                    Debug.LogError($"Init|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                case CommandReceiveState.Exception:
                    Debug.LogError($"Ex|Command receive response {State}:\n{Message}\n{Data}", Receiver);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (Exception != null)
                Debug.LogException(Exception, Receiver);
        }

        public static CommandReceiveResponse Success => new(CommandReceiveState.Success);
        public static CommandReceiveResponse Pending => new(CommandReceiveState.Pending);
        public static CommandReceiveResponse Disabled => new(CommandReceiveState.Disabled);
        public static CommandReceiveResponse None => new(CommandReceiveState.None);
        public static CommandReceiveResponse Canceled => new(CommandReceiveState.Canceled);
        
        public static CommandReceiveResponse SuccessFrom(Object r) => new(CommandReceiveState.Success, receiver: r);
        public static CommandReceiveResponse PendingFrom(Object r) => new(CommandReceiveState.Pending, receiver: r);
        public static CommandReceiveResponse DisabledFrom(Object r) => new(CommandReceiveState.Disabled, receiver: r);
        public static CommandReceiveResponse CanceledFrom(Object r) => new(CommandReceiveState.Canceled, receiver: r);
        public static CommandReceiveResponse NoneFrom(Object r) => new(CommandReceiveState.None, receiver: r);
    }
}