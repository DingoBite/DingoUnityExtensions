#if NEWTONSOFT_EXISTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bind;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.Serialization.DataLibrary
{
    [Serializable, Preserve]
    public class DescribedDataDirectoriesConfig
    {
        public string DataFileName;
        public string DescriptorFileName;
        public string RootFolderPath;
        
        public DescribedDataDirectoriesConfig(string rootFolderPath, string descriptorFileName, string dataFileName)
        {
            RootFolderPath = rootFolderPath;
            DescriptorFileName = descriptorFileName;
            DataFileName = dataFileName;
        }
    }

    public class DescribedDataByDirectories<TDescriptor, TData>
    {
        private readonly DescribedDataDirectoriesConfig _config;
        
        private readonly DirectoryInfo _rootDirectory;
        private readonly ISerializer _serializer;
        private readonly Func<TDescriptor, string> _descriptorToId;

        private readonly BindList<TDescriptor> _descriptors = new();

        public IReadonlyBind<IReadOnlyList<TDescriptor>> Descriptors => _descriptors;

        public DescribedDataByDirectories(DescribedDataDirectoriesConfig config, Func<TDescriptor, string> descriptorToId, ISerializer serializer)
        {
            _config = config;
            _serializer = serializer;
            _descriptorToId = descriptorToId;
            _rootDirectory = new DirectoryInfo(config.RootFolderPath);
            if (!_rootDirectory.Exists)
                Directory.CreateDirectory(_rootDirectory.FullName);
        }

        public string GetDirectoryFullPath(TDescriptor descriptor)
        {
            var subDirectory = _descriptorToId(descriptor);
            var directory = $"{_rootDirectory.FullName}/{subDirectory}";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }

        public TDescriptor GetDescriptor(int index) => _descriptors.V.ElementAtOrDefault(index);
        public TDescriptor GetDescriptor(Func<TDescriptor, bool> predicate) => _descriptors.V.FirstOrDefault(predicate);

        public async Task<TData> GetDataAsync(TDescriptor descriptor)
        {
            var dataPath = GetDataPath(GetDirectoryFullPath(descriptor));
            try
            {
                var data = await _serializer.LoadDeserializeAsync<TData>(dataPath, false);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return default;
        }

        public async Task RefreshDescriptorsAsync()
        {
            var directories = _rootDirectory.EnumerateDirectories().ToList();
            var descriptors = _descriptors.V;
            descriptors.Clear();

            foreach (var directory in directories)
            {
                var descriptorPath = GetDescriptorPath(directory.FullName);
                try
                {
                    var descriptor = await _serializer.LoadDeserializeAsync<TDescriptor>(descriptorPath, false);
                    descriptors.Add(descriptor);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            
            _descriptors.V = descriptors;
        }

        public async Task CreatePairAsync(TDescriptor descriptor, TData data)
        {
            var folderPath = GetDirectoryFullPath(descriptor);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            var descriptors = _descriptors.V;
            try 
            { 
                await _serializer.SaveSerializeAsync(GetDescriptorPath(descriptor), descriptor, false);
                await _serializer.SaveSerializeAsync(GetDataPath(descriptor), data);
                descriptors.Add(descriptor);
                _descriptors.V = descriptors;
            }
            catch (Exception)
            {
                Directory.Delete(folderPath);
                throw;
            }
        }

        public async Task SaveAsync(TDescriptor descriptor, TData data)
        {
            if (!_descriptors.V.Contains(descriptor))
            {
                Debug.LogError($"Descriptor: {_descriptorToId(descriptor)} is not registered");
                return;
            }
            
            try 
            { 
                await _serializer.SaveSerializeAsync(GetDescriptorPath(descriptor), descriptor, false);
                await _serializer.SaveSerializeAsync(GetDataPath(descriptor), data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async Task DeleteAsync(TDescriptor descriptor)
        {
            var list = _descriptors.V;
            await DeleteDescriptorsAsync(new List<TDescriptor>{ descriptor }, list);
            _descriptors.V = list;
        }

        public async Task DeleteAsync(IEnumerable<TDescriptor> descriptors)
        {
            var list = _descriptors.V;
            await DeleteDescriptorsAsync(descriptors, list);
            _descriptors.V = list;
        }
        
        public async Task DeleteAllAsync()
        {
            var list = _descriptors.V;
            await DeleteDescriptorsAsync(list, list);
            _descriptors.V = list;
        }
        
        protected string GetDescriptorPath(TDescriptor descriptor) => $"{GetDirectoryFullPath(descriptor)}/{_config.DescriptorFileName}";
        protected string GetDescriptorPath(string directoryFullPath) => $"{directoryFullPath}/{_config.DescriptorFileName}";
        protected string GetDataPath(TDescriptor descriptor) => $"{GetDirectoryFullPath(descriptor)}/{_config.DataFileName}";
        protected string GetDataPath(string directoryFullPath) => $"{directoryFullPath}/{_config.DataFileName}";
        
        private async Task DeleteDescriptorsAsync(IEnumerable<TDescriptor> descriptors, List<TDescriptor> list)
        {
            foreach (var descriptor in descriptors.ToList())
            {
                try
                {
                    var directory = GetDirectoryFullPath(descriptor);
                    if (Directory.Exists(directory))
                        await Task.Run(() => Directory.Delete(directory, true));
                    var index = list.FindIndex(d => _descriptorToId(d) == _descriptorToId(descriptor));
                    if (index < 0)
                        return;
                    list.RemoveAt(index);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
#endif