using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
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

        public string CreateItemDirectory(NoteType type, string itemId)
        {
            EnsureDirectories();
            string typePath = GetTypePath(type);
            string itemPath = Path.Combine(typePath, itemId);
            if (!Directory.Exists(itemPath)) Directory.CreateDirectory(itemPath);
            return itemPath;
        }

        public void DeleteItemDirectory(NoteType type, string itemId)
        {
            string typePath = GetTypePath(type);
            string itemPath = Path.Combine(typePath, itemId);
            if (Directory.Exists(itemPath))
            {
                Directory.Delete(itemPath, true);
            }
        }

        public async Task<string> SaveMarkdownAsync(string itemFolderPath, string content)
        {
            // File name can be generic or specific, let's use a standard name for the main content
            string fileName = "content.md"; 
            string fullPath = Path.Combine(itemFolderPath, fileName);
            
            await File.WriteAllTextAsync(fullPath, content, Encoding.UTF8);
            return fullPath; 
        }

        public async Task<string> LoadMarkdownAsync(NoteType type, string itemId)
        {
            string typePath = GetTypePath(type);
            string itemPath = Path.Combine(typePath, itemId, "content.md");
            
            if (File.Exists(itemPath))
            {
                return await File.ReadAllTextAsync(itemPath, Encoding.UTF8);
            }
            return string.Empty;
        }

        public async Task UpdateMarkdownAsync(NoteType type, string itemId, string content)
        {
            string typePath = GetTypePath(type);
            string itemPath = Path.Combine(typePath, itemId, "content.md");
            
            await File.WriteAllTextAsync(itemPath, content, Encoding.UTF8);
        }

         public async Task<string> BundleResourcesAsync(string targetFolder, List<DroppedContent> contents)
        {
            var textParts = new List<string>();
            var fileParts = new List<string>();
            var linkParts = new List<string>();
            string[] imgExts = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            foreach (var content in contents)
            {
                if (content.ContentType == DropContentType.Text)
                {
                    textParts.Add(content.TextContent);
                }
                else if (content.ContentType == DropContentType.Url)
                {
                    linkParts.Add($"[{content.Url}]({content.Url})");
                }
                else if (content.ContentType == DropContentType.Files)
                {
                    var mdLines = new List<string>();
                    foreach (var path in content.FilePaths)
                    {
                        await CreateShortcutAsync(targetFolder, path);
                        string fileName = Path.GetFileName(path);
                        string lnkName = fileName + ".lnk";
                        string ext = Path.GetExtension(path).ToLower();

                        if (imgExts.Contains(ext)) mdLines.Add($"![{fileName}]({lnkName})");
                        else mdLines.Add($"[{fileName}]({lnkName})");
                    }
                    fileParts.Add(string.Join(Environment.NewLine + Environment.NewLine, mdLines));
                }
            }

            var finalParts = new List<string>();
            if (textParts.Count > 0) finalParts.Add(string.Join(Environment.NewLine + Environment.NewLine, textParts));
            if (fileParts.Count > 0) finalParts.Add(string.Join(Environment.NewLine + Environment.NewLine, fileParts));
            if (linkParts.Count > 0) finalParts.Add(string.Join(Environment.NewLine + Environment.NewLine, linkParts));

            return string.Join(Environment.NewLine + "---" + Environment.NewLine, finalParts);
        }

        public async Task SyncTodoStatusToFileAsync(string filePath, bool isCompleted)
        {
            if (!File.Exists(filePath)) return;
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length > 0)
            {
                string firstLine = lines[0];
                string expectedPrefix = isCompleted ? "- [x]" : "- [ ]";
                string currentPrefix = isCompleted ? "- [ ]" : "- [x]";
                
                if (firstLine.StartsWith(currentPrefix))
                {
                    lines[0] = expectedPrefix + firstLine.Substring(currentPrefix.Length);
                    await File.WriteAllLinesAsync(filePath, lines);
                }
                else if (!firstLine.StartsWith(expectedPrefix))
                {
                    // If no prefix at all, prepend it
                    var newLines = new List<string>(lines);
                    newLines.Insert(0, expectedPrefix);
                    await File.WriteAllLinesAsync(filePath, newLines);
                }
            }
        }

        public async Task AppendToMarkdownAsync(string filePath, string newContent)
        {
            if (!File.Exists(filePath)) return;
            string separator = Environment.NewLine + "---" + Environment.NewLine;
            await File.AppendAllTextAsync(filePath, separator + newContent);
        }

         public async Task CreateShortcutAsync(string itemFolderPath, string sourcePath)
         {
             string shortcutName = Path.GetFileName(sourcePath) + ".lnk";
             string destinationPath = Path.Combine(itemFolderPath, shortcutName);

             string script = $"$s=(New-Object -COM WScript.Shell).CreateShortcut('{destinationPath}');$s.TargetPath='{sourcePath}';$s.Save()";
             
             await RunPowerShellAsync(script);
         }

         public async Task<string> ResolveShortcutAsync(string shortcutPath)
         {
             if (!File.Exists(shortcutPath)) return string.Empty;

             // Escape single quotes for PowerShell
             string escapedPath = shortcutPath.Replace("'", "''");
             string script = $"(New-Object -COM WScript.Shell).CreateShortcut('{escapedPath}').TargetPath";
             return await RunPowerShellOutputAsync(script);
         }

         private async Task RunPowerShellAsync(string script)
         {
             await Task.Run(() =>
             {
                 var psi = new ProcessStartInfo
                 {
                     FileName = "powershell",
                     Arguments = $"-NoProfile -Command \"{script}\"",
                     UseShellExecute = false,
                     CreateNoWindow = true
                 };
                 try { Process.Start(psi)?.WaitForExit(); } catch { }
             });
         }

         private async Task<string> RunPowerShellOutputAsync(string script)
         {
             return await Task.Run(() =>
             {
                 var psi = new ProcessStartInfo
                 {
                     FileName = "powershell",
                     Arguments = $"-NoProfile -Command \"{script}\"",
                     UseShellExecute = false,
                     CreateNoWindow = true,
                     RedirectStandardOutput = true
                 };
                 try 
                 { 
                     using var process = Process.Start(psi);
                     string output = process?.StandardOutput.ReadToEnd() ?? string.Empty;
                     process?.WaitForExit();
                     return output.Trim();
                 } 
                  catch { return string.Empty; }
             });
         }

         public string GeneratePreviewText(string content)
         {
             if (string.IsNullOrEmpty(content)) return string.Empty;

             // Remove markdown links but keep text [text](link) -> text
             var clean = Regex.Replace(content, @"\[(.*?)\]\(.*?\)", "$1");
             // Remove image tags ![]()
             clean = Regex.Replace(clean, @"!\[.*?\]\(.*?\)", "");
             // Remove separators
             clean = clean.Replace("---", "");
             // Remove todo checkboxes
             clean = clean.Replace("- [ ]", "").Replace("- [x]", "").Replace("- [X]", "");

             // Get multiple lines and join them to avoid preview being just one short line or having too many newlines
             var lines = clean.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(l => l.Trim())
                              .Where(l => !string.IsNullOrWhiteSpace(l));
             
             clean = string.Join(" ", lines);

             // Trim and take first N characters
             int maxLength = 200; 
             if (clean.Length > maxLength)
                 clean = clean.Substring(0, maxLength - 3) + "...";

             return clean;
         }
     }
}
