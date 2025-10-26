using System;
using System.Threading.Tasks;
using Bind;

namespace DingoUnityExtensions.Serialization.DataLibrary.DescribedLibrary
{
    public class BindPathSaveLoad<T>
    {
        private readonly DataByPathLibrary _dataAccess;
        private readonly string _subPath;
        private readonly string _id;
        
        private readonly Bind<T> _value = new ();

        public IReadonlyBind<T> Value => _value;

        public BindPathSaveLoad(DataByPathLibrary dataAccess, string subPath, string id)
        {
            _dataAccess = dataAccess;
            _subPath = subPath;
            _id = id;
        }

        public async Task LoadProfileAsync(Func<string, T> factory) => _value.V = await _dataAccess.GetOrCreateAsync(_subPath, () => factory(_id));
        public Task SaveProfileAsync() => _dataAccess.SaveAsync(_subPath, _value.V);
    }
}