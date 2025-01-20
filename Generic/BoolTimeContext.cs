namespace DingoUnityExtensions.Generic
{
    public enum BoolTimeContext
    {
        None = 0,
        True = 1,
        False = 2,
        TrueImmediately = 3,
        FalseImmediately = 4
    }

    public static class BoolTimeContextExtensions
    {
        public static bool Bool(this BoolTimeContext boolTimeContext) => boolTimeContext is BoolTimeContext.True or BoolTimeContext.TrueImmediately;
        public static bool Immediately(this BoolTimeContext boolTimeContext) => boolTimeContext is BoolTimeContext.TrueImmediately or BoolTimeContext.FalseImmediately;
    }
}