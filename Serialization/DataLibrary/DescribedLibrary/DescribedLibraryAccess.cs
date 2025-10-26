using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bind;
using UnityEngine;

namespace DingoUnityExtensions.Serialization.DataLibrary.DescribedLibrary
{
    public class DescribedLibraryAccess<TDescriptor> where TDescriptor : StructureRootDataDescriptor
    {
        private readonly SemaphoreSerializer _semaphoreSerializer = new(JsonOptions.UnitySerializationOptions);

        private readonly BindDict<string, DataByPathLibrary> _managers = new();
        private readonly Bind<DataByPathLibrary> _opened = new();
        private readonly Bind<DataByPathLibrary> _closed = new();
        
        private readonly DescribedDirectories<TDescriptor> _describedDirectories;

        public IReadonlyBind<IReadOnlyList<TDescriptor>> Descriptors => _describedDirectories?.Descriptors;
        public IReadonlyBind<IReadOnlyDictionary<string, DataByPathLibrary>> Managers => _managers;
        public IReadonlyBind<DataByPathLibrary> Opened => _opened;
        public IReadonlyBind<DataByPathLibrary> Closed => _closed;

        public DescribedLibraryAccess(string rootFolderPath, string descriptorSubPath)
        {
            var describedDataDirectoriesConfig = new DescribedDirectoriesConfig(rootFolderPath, descriptorSubPath);
            _describedDirectories = new DescribedDirectories<TDescriptor>(describedDataDirectoriesConfig, d => d.Id, _semaphoreSerializer);
        }

        public async Task RefreshDescriptorsAsync() => await _describedDirectories.RefreshDescriptorsAsync();

        public async Task<StructureRootDataDescriptor> CreateAsync(Func<string, TDescriptor> guidFactory)
        {
            var id = Guid.NewGuid().ToString();
            var descriptor = guidFactory(id);
            try
            {
                await _describedDirectories.CreateAsync(descriptor);
                return descriptor;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }

        public async Task DeleteAsync(TDescriptor descriptor)
        {
            CloseAccess(descriptor);
            await _describedDirectories.DeleteAsync(descriptor);
        }

        public DataByPathLibrary OpenAccess(TDescriptor descriptor)
        {
            if (_managers.V.TryGetValue(descriptor.Id, out var manager))
            {
                Debug.LogError($"Profile {descriptor.Id} is already opened");
                return manager;
            }
            manager = new DataByPathLibrary(_describedDirectories.GetRootFolder(), _semaphoreSerializer);
            _managers.V[descriptor.Id] = manager;
            _managers.V = _managers.V;
            _opened.V = manager;
            return manager;
        }

        public void CloseAccess(TDescriptor descriptor)
        {
            if (_managers.V.Remove(descriptor.Id, out var profileManager))
            {
                _managers.V = _managers.V;
                _closed.V = profileManager;
            }
        }
    }
}