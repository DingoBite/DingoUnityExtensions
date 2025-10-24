using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace DingoUnityExtensions.Serialization.DataLibrary
{
    public class DataByPathLibrary
    {
        private readonly DirectoryInfo _rootDirectory;
        private readonly ISerializer _serializer;
        
        public DataByPathLibrary(string rooFolderPath, ISerializer serializer)
        {
            _serializer = serializer;
            _rootDirectory = new DirectoryInfo(rooFolderPath);
            if (!_rootDirectory.Exists)
                Directory.CreateDirectory(_rootDirectory.FullName);
        }

        public async Task<T> GetOrCreateAsync<T>(string path, Func<T> factory)
        {
            var dataPath = GetDataPath(path);
            try
            {
                if (!File.Exists(dataPath))
                {
                    var data = factory();
                    await _serializer.SaveSerializeAsync(dataPath, data, false);
                    return data;
                }
                
                return await _serializer.LoadDeserializeAsync<T>(dataPath, false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return default;
        }
        
        public async Task<T> GetAsync<T>(string path)
        {
            var dataPath = GetDataPath(path);
            try
            {
                return await _serializer.LoadDeserializeAsync<T>(dataPath, false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return default;
        }
        
        public async Task SaveAsync(string path, object data)
        {
            var dataPath = GetDataPath(path);
            var folder = Path.GetDirectoryName(dataPath);
            if (folder == null)
                return;
            
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            try
            {
                await _serializer.SaveSerializeAsync(dataPath, data, false);
            }
            catch (Exception e)
            {
                Directory.Delete(folder);
                Debug.LogException(e);
            }
        }
        
        protected string GetDataPath(string subPath) => $"{_rootDirectory.FullName}/{subPath}";
    }
}