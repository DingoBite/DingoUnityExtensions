using System;

namespace DingoUnityExtensions.Configurator.Core
{
    [Serializable]
    public record Config
    {
        public string ConfigName;
        
        public virtual string FolderName => "default";
    }
}