namespace DingoUnityExtensions.Serialization.DataLibrary.DescribedLibrary
{
    public abstract class DescribedSelectedAccessModel<TDescriptor> where TDescriptor : StructureRootDataDescriptor
    {
        public readonly TDescriptor Descriptor;
        
        protected readonly DataByPathLibrary DataAccess;
        
        public string Id => Descriptor.Id;

        protected DescribedSelectedAccessModel(DataByPathLibrary dataAccess, TDescriptor descriptor)
        {
            Descriptor = descriptor;
            DataAccess = dataAccess;
        }
    }
}