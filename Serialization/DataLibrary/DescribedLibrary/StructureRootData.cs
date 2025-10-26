using System;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.Serialization.DataLibrary.DescribedLibrary
{
    [Serializable, Preserve]
    public abstract class StructureRootDataDescriptor
    {
        public string Id;
    }
}