using System;
using System.IO;
using System.Text.Json;
using Drag2Note.Models;
using Microsoft.Win32;

namespace Drag2Note.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private readonly string _settingsPath;
        private AppSettings _settings;

        public event EventHandler? SettingsChanged;

        private SettingsService()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Drag2Note");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            _settingsPath = Path.Combine(appData, "settings.json");
            _settings = LoadSettings();
        }

        public AppSettings GetSettings() => _settings;

        public void SaveSettings(AppSettings settings)
        {
            _settings = settings;
            string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
            
            UpdateStartupStatus(_settings.LaunchAtStartup);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private AppSettings LoadSettings()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch { }
            }
            return new AppSettings();
        }

        private void UpdateStartupStatus(bool enable)
        {
            try
            {
                string path = Environment.ProcessPath ?? "";
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    if (enable) key.SetValue("Drag2Note", path);
                    else key.DeleteValue("Drag2Note", false);
                }
            }
            catch { }
        }
    }
}
