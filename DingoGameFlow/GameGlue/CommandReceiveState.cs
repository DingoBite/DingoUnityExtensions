namespace DingoUnityExtensions.DingoGameFlow.GameGlue
{
    public enum CommandReceiveState
    {
        None,
        Success,
        Pending,
        Collision,
        Canceled,
        
        Disabled,
        
        InvalidData,
        InitializeError,
        Exception,
    }
}