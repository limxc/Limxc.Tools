using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text.Json;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.Utils
{
    public sealed class MemoryMapper : IDisposable
    {
        private MemoryMappedFile _memoryMappedFile;

        public void Dispose()
        {
            _memoryMappedFile?.Dispose();
        }

        /// <summary>
        ///     默认1MB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapName"></param>
        /// <param name="entity"></param>
        /// <param name="capacitySize">bytes</param>
        public void Create<T>(string mapName, T entity, long capacitySize = 1024 * 1024)
        {
            try
            {
                _memoryMappedFile = MemoryMappedFile.CreateOrOpen(mapName, capacitySize);
                using (var mmvStream = _memoryMappedFile.CreateViewStream(0, 0))
                using (var jsonWriter = new Utf8JsonWriter(mmvStream))
                {
                    JsonSerializer.Serialize(jsonWriter, entity, new JsonSerializerOptions().Init(false));
                }
            }
            catch (Exception e)
            {
                _memoryMappedFile.Dispose();
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        public static T Read<T>(string mapName)
        {
            try
            {
                using (var mmf = MemoryMappedFile.OpenExisting(mapName))
                using (var mmvStream = mmf.CreateViewStream(0, 0))
                using (var sr = new StreamReader(mmvStream))
                {
                    var json = sr.ReadToEnd().Trim('\0');
                    return mmvStream.CanRead
                        ? JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions().Init(false))
                        : default;
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Memory-mapped file not found.");
                throw;
            }
        }
    }
}