using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using Drag2Note.Models;

namespace Drag2Note.Services.Data
{
    public class StorageService
    {
        private static StorageService _instance;
        public static StorageService Instance => _instance ??= new StorageService();

        private readonly string _userDataPath;

        private StorageService()
        {
            // Move UserData to Project Root (assuming Debug/Dev context mainly for now)
            // In a published app, this might need to be proper AppData.
            // Current: bin/Debug/net8.0-windows/
            // Goal: Project Root (up 3 levels)
#if DEBUG
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Go up 3 levels to reach Project Directory (Drag2Note/)
            // We want it in the solution root effectively or project root. 
            // Let's put it in the Project Root: Drag2Note/UserData
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            _userDataPath = Path.Combine(projectRoot, "UserData");
#else
             _userDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData");
#endif
            EnsureDirectories();
        }

        public string UserDataPath => _userDataPath;

        public string GetTypePath(NoteType type)
        {
            return type == NoteType.Note 
                ? Path.Combine(_userDataPath, "Notes") 
                : Path.Combine(_userDataPath, "To-do");
        }

        private void EnsureDirectories()
        {
            if (!Directory.Exists(_userDataPath)) Directory.CreateDirectory(_userDataPath);
            Directory.CreateDirectory(GetTypePath(NoteType.Note));
            Directory.CreateDirectory(GetTypePath(NoteType.Todo));
        }

        public async Task<string> SaveMarkdownAsync(string content, NoteType type)
        {
            EnsureDirectories();
            string folder = GetTypePath(type);
            string fileName = $"Note-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.md";
            string fullPath = Path.Combine(folder, fileName);
            
            // Fix encoding issue: Use UTF8 explicitly
            await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);
            return fullPath; 
        }

        public async Task CreateShortcutAsync(string sourcePath, string shortcutName, NoteType type)
         {
             EnsureDirectories();
             if (!shortcutName.EndsWith(".lnk")) shortcutName += ".lnk";
             
             string folder = GetTypePath(type);
             string destinationPath = Path.Combine(folder, shortcutName);

             string script = $"$s=(New-Object -COM WScript.Shell).CreateShortcut('{destinationPath}');$s.TargetPath='{sourcePath}';$s.Save()";
             
             await Task.Run(() =>
             {
                 var psi = new ProcessStartInfo
                 {
                     FileName = "powershell",
                     Arguments = $"-NoProfile -Command \"{script}\"",
                     UseShellExecute = false,
                     CreateNoWindow = true,
                     RedirectStandardOutput = true,
                     RedirectStandardError = true
                 };

                 try 
                 {
                    using var process = Process.Start(psi);
                    process?.WaitForExit();
                 }
                 catch (Exception ex)
                 {
                     Debug.WriteLine($"Failed to create shortcut: {ex.Message}");
                 }
             });
         }
    }
}
