namespace DingoUnityExtensions.UnityViewProviders.Core.Data
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
        public static BoolTimeContext TimeContext(this bool boolTimeContext, bool immediately = true)
        {
            if (boolTimeContext)
                return immediately ? BoolTimeContext.TrueImmediately : BoolTimeContext.True;
            return immediately ? BoolTimeContext.FalseImmediately : BoolTimeContext.False;
        }
        
        public static bool Bool(this BoolTimeContext boolTimeContext) => boolTimeContext is BoolTimeContext.True or BoolTimeContext.TrueImmediately;
        public static bool Immediately(this BoolTimeContext boolTimeContext) => boolTimeContext is BoolTimeContext.TrueImmediately or BoolTimeContext.FalseImmediately;
    }
}