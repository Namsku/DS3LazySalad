using System.Diagnostics;
using System.IO;
using System.Windows; 
using System.Windows.Controls; 
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32; 

namespace DS3LazySalad
{
    public partial class MainWindow : Window
    {
        private string? _modEngineFolder = null;
        private string? _ds3Folder = null;
        private readonly string _configPath = "config.txt";
        private DispatcherTimer _watcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            ModEnginePathBox.TextChanged += PathBox_TextChanged;
            DS3PathBox.TextChanged += PathBox_TextChanged;

            _watcherTimer = new DispatcherTimer();
            _watcherTimer.Interval = System.TimeSpan.FromMilliseconds(100);
            _watcherTimer.Tick += WatcherTimer_Tick;
            _watcherTimer.Start();
        }

        private void ModEngineBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select ModEngine Folder",
            };
            if (dialog.ShowDialog() == true)
            {
                ModEnginePathBox.Text = dialog.FolderName;
            }
        }

        private void DS3Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select DS3 Steam Folder",
            };
            if (dialog.ShowDialog() == true)
            {
                DS3PathBox.Text = dialog.FolderName;
            }
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateModEngineStatus();
            UpdateDS3RandomizerStatus();
            SaveConfig();
        }

        private void UpdateModEngineStatus()
        {
            var path = ModEnginePathBox.Text;
            if (File.Exists(path))
                path = Path.GetDirectoryName(path);

            var batPath = Path.Combine(path ?? "", "launchmod_darksouls3.bat");
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(batPath))
            {
                ModEngineStatus.Text = "Found";
                ModEngineStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
                _modEngineFolder = path;
            }
            else
            {
                ModEngineStatus.Text = "Not Found";
                ModEngineStatus.Foreground = new SolidColorBrush(Colors.Red);
                _modEngineFolder = null;
            }
        }

        private void UpdateDS3RandomizerStatus()
        {
            var ds3Path = DS3PathBox.Text;
            var randomizerDir = Path.Combine(ds3Path ?? "", "randomizer");
            if (!string.IsNullOrWhiteSpace(ds3Path) && Directory.Exists(randomizerDir))
            {
                DS3RandomizerStatus.Text = "Found";
                DS3RandomizerStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
                _ds3Folder = ds3Path;
            }
            else
            {
                DS3RandomizerStatus.Text = "Not Found";
                DS3RandomizerStatus.Foreground = new SolidColorBrush(Colors.Red);
                _ds3Folder = null;
            }
        }

        private void SaveConfig()
        {
            if (!string.IsNullOrEmpty(_modEngineFolder) && !string.IsNullOrEmpty(_ds3Folder))
            {
                File.WriteAllLines(_configPath, new[]
                {
                        $"ModEngine={_modEngineFolder}",
                        $"DS3={_ds3Folder}"
                    });
            }
        }

        private void LoadConfig()
        {
            if (File.Exists(_configPath))
            {
                var lines = File.ReadAllLines(_configPath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("ModEngine="))
                        ModEnginePathBox.Text = line.Substring("ModEngine=".Length);
                    if (line.StartsWith("DS3="))
                        DS3PathBox.Text = line.Substring("DS3=".Length);
                }

                UpdateModEngineStatus();
                UpdateDS3RandomizerStatus();
            }
        }

        private async void DS3NoCopy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_modEngineFolder) || string.IsNullOrEmpty(_ds3Folder))
            {
                MessageBox.Show("Both ModEngine and DS3 Randomizer folders must be found before proceeding.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _watcherTimer.Stop();

            string modEngineSource = Path.Combine(_ds3Folder, "randomizer", "ModEngine");
            if (Directory.Exists(modEngineSource))
            {
                foreach (var srcFile in Directory.GetFiles(modEngineSource))
                {
                    string destFile = Path.Combine(_ds3Folder, Path.GetFileName(srcFile));
                    if (File.Exists(destFile))
                    {
                        try { File.Delete(destFile); } catch { /* ignore errors */ }
                    }
                }
            }

            string batFile = Path.Combine(_modEngineFolder, "launchmod_darksouls3.bat");

            if (File.Exists(batFile))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = batFile,
                    WorkingDirectory = _modEngineFolder,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("launchmod_darksouls3.bat not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _watcherTimer.Start();
                return;
            }

            await System.Threading.Tasks.Task.Delay(15000);
            _watcherTimer.Start();
        }

        private async void SaladIt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_modEngineFolder) || string.IsNullOrEmpty(_ds3Folder))
            {
                MessageBox.Show("Both ModEngine and DS3 Randomizer folders must be found before proceeding.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _watcherTimer.Stop();

            string modEngineSource = Path.Combine(_ds3Folder, "randomizer", "ModEngine");
            if (Directory.Exists(modEngineSource))
            {
                foreach (var srcFile in Directory.GetFiles(modEngineSource))
                {
                    string destFile = Path.Combine(_ds3Folder, Path.GetFileName(srcFile));
                    if (File.Exists(destFile))
                    {
                        try { File.Delete(destFile); } catch { /* ignore errors */ }
                    }
                }
            }

            string modFolder = Path.Combine(_modEngineFolder, "mod");
            string randomizerFolder = Path.Combine(_ds3Folder, "randomizer");
            string batFile = Path.Combine(_modEngineFolder, "launchmod_darksouls3.bat");

            if (Directory.Exists(modFolder))
            {
                foreach (var dir in Directory.GetDirectories(modFolder))
                    Directory.Delete(dir, true);
                foreach (var file in Directory.GetFiles(modFolder))
                    File.Delete(file);
            }
            else
            {
                Directory.CreateDirectory(modFolder);
            }

            if (Directory.Exists(randomizerFolder))
            {
                CopyDirectory(randomizerFolder, modFolder);
            }
            else
            {
                MessageBox.Show("Randomizer folder not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _watcherTimer.Start();
                return;
            }

            if (File.Exists(batFile))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = batFile,
                    WorkingDirectory = _modEngineFolder,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("launchmod_darksouls3.bat not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _watcherTimer.Start();
                return;
            }

            await System.Threading.Tasks.Task.Delay(15000);
            _watcherTimer.Start();
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dir.Replace(sourceDir, destDir));
            }
            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, file.Replace(sourceDir, destDir), true);
            }
        }

        private void WatcherTimer_Tick(object? sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(_ds3Folder))
                return;

            bool isRunning = Process.GetProcessesByName("DarkSoulsIII").Length > 0;
            if (!isRunning)
            {
                string modEngineSource = Path.Combine(_ds3Folder, "randomizer", "ModEngine");
                string targetFolder = _ds3Folder;

                if (Directory.Exists(modEngineSource))
                {
                    if (NeedsCopy(modEngineSource, targetFolder))
                    {
                        CopyFilesFlat(modEngineSource, targetFolder);
                    }
                }
            }
        }

        private bool NeedsCopy(string sourceDir, string destDir)
        {
            foreach (var srcFile in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(srcFile));
                if (!File.Exists(destFile))
                    return true;
                var srcInfo = new FileInfo(srcFile);
                var destInfo = new FileInfo(destFile);
                if (srcInfo.Length != destInfo.Length ||
                    srcInfo.LastWriteTimeUtc != destInfo.LastWriteTimeUtc)
                    return true;
            }
            return false;
        }

        private void CopyFilesFlat(string sourceDir, string destDir)
        {
            foreach (var srcFile in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(srcFile));
                File.Copy(srcFile, destFile, true);
            }
        }
    }
}

