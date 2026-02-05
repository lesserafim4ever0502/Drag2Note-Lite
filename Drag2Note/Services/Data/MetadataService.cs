using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drag2Note.Models;

namespace Drag2Note.Services.Data
{
    public class MetadataService
    {
        private static MetadataService? _instance;
        public static MetadataService Instance => _instance ??= new MetadataService();

        public event EventHandler? DataChanged;

        private readonly string _metadataFilePath;
        private AppData _cachedData = new AppData();

        private MetadataService()
        {
            _metadataFilePath = Path.Combine(StorageService.Instance.UserDataPath, "metadata.json");
            LoadData(); // Load synchronously on init or make Init async
        }

        public void LoadData()
        {
            if (File.Exists(_metadataFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_metadataFilePath);
                    _cachedData = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
                }
                catch
                {
                    _cachedData = new AppData();
                }
            }
            else
            {
                _cachedData = new AppData();
            }
        }

        public void Reload()
        {
            LoadData();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task SaveDataAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_cachedData, options);
            await File.WriteAllTextAsync(_metadataFilePath, json);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public AppData GetData()
        {
            return _cachedData;
        }

        public async Task AddItemAsync(MetadataItem item)
        {
            _cachedData.Items.Add(item);
            await SaveDataAsync();
        }

        public async Task RemoveItemAsync(string itemId)
        {
            var item = _cachedData.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                _cachedData.Items.Remove(item);
                await SaveDataAsync();
            }
        }
    }
}
