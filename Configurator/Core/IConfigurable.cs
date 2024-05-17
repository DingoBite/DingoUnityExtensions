namespace DingoUnityExtensions.Configurator.Core
{
    /// <summary>
    /// Mark class for improve performance while find configurables in scene
    /// </summary>
    public interface IConfigurable
    {
        public interface Appliable : IConfigurable
        {
            public void ApplyConfig();
        }
    }
}