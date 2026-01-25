using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AxionControl.Views
{
    public partial class AppsView : UserControl
    {
        // Program útvonalak dictionary-ben
        private readonly Dictionary<string, string> programPaths = new Dictionary<string, string>
        {
            { "StartAllBack", @"C:\LibrAxon\ADD\StartAllBack\sab_setup.exe" },
            { "MediaPlayerClassic", @"C:\LibrAxon\ADD\MediaPlayerClassic\setup.exe" },
            { "WinampMagyar", @"C:\LibrAxon\ADD\Winamp\winamp_latest_full.exe" },
            { "Media2HB", @"C:\LibrAxon\ADD\2HBMediaPlayer\setup.exe" },
            { "VideoDownloader", @"C:\LibrAxon\BIN\yt-dlp\yt-dlp-gui.exe" },
            { "Iphone3Utools", @"C:\LibrAxon\ADD\Iphone3UTools\setup.exe" },
            { "FileUnlocker", @"C:\LibrAxon\ADD\FileUnlocker\setup.exe" },
            { "EdgeInstallProtected", @"C:\LibrAxon\ADD\Edge\edge_install_protected.msi" },
            { "EdgeInstallNative", @"C:\LibrAxon\ADD\Edge\edge_install_native.exe" },
            { "EdgeRemove", @"C:\LibrAxon\ADD\Edge\edge_remove.exe" },
            { "LibreOffice", @"C:\LibrAxon\ADD\LibreOffice\setup.msi" },
            { "MixxxDJ", @"C:\LibrAxon\BIN\MixxDJ\mixxx.exe" },
            { "PendriveRepair", @"C:\LibrAxon\BIN\KillDisk\Kill Disk.exe" },
            { "OneDriveEnable", @"C:\LibrAxon\ADD\OneDrive\setup.exe" }
        };

        public AppsView()
        {
            InitializeComponent();
            Loaded += AppsView_Loaded;
        }

        private void AppsView_Loaded(object sender, RoutedEventArgs e)
        {
            CheckButtonAvailability();
        }

        private void CheckButtonAvailability()
        {
            SetButtonState("btnStartAllBack", "StartAllBack");
            SetButtonState("btnMediaPlayerClassic", "MediaPlayerClassic");
            SetButtonState("btnWinampMagyar", "WinampMagyar");
            SetButtonState("btnMedia2HB", "Media2HB");
            SetButtonState("btnVideoDownloader", "VideoDownloader");
            SetButtonState("btnIphone3Utools", "Iphone3Utools");
            SetButtonState("btnFileUnlocker", "FileUnlocker");
            SetButtonState("btnEdgeInstallProtected", "EdgeInstallProtected");
            SetButtonState("btnEdgeInstallNative", "EdgeInstallNative");
            SetButtonState("btnEdgeRemove", "EdgeRemove");
            SetButtonState("btnLibreOffice", "LibreOffice");
            SetButtonState("btnMixxxDJ", "MixxxDJ");
            SetButtonState("btnPendriveRepair", "PendriveRepair");
            SetButtonState("btnOneDriveEnable", "OneDriveEnable");
        }

        private void SetButtonState(string buttonName, string programKey)
        {
            var button = this.FindName(buttonName) as Button;
            if (button != null && programPaths.ContainsKey(programKey))
            {
                button.IsEnabled = File.Exists(programPaths[programKey]);
            }
        }

        private void StartAllBack_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("StartAllBack");
        }

        private void MediaPlayerClassic_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("MediaPlayerClassic");
        }

        private void WinampMagyar_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("WinampMagyar");
        }

        private void Media2HB_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("Media2HB");
        }

        private void VideoDownloader_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("VideoDownloader");
        }

        private void Iphone3Utools_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("Iphone3Utools");
        }

        private void FileUnlocker_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("FileUnlocker");
        }

        private void EdgeInstallProtected_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("EdgeInstallProtected");
        }

        private void EdgeInstallNative_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("EdgeInstallNative");
        }

        private void EdgeRemove_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("EdgeRemove");
        }

        private void LibreOffice_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("LibreOffice");
        }

        private void MixxxDJ_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("MixxxDJ");
        }

        private void PendriveRepair_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("PendriveRepair");
        }

        private void OneDriveEnable_Click(object sender, RoutedEventArgs e)
        {
            LaunchProgram("OneDriveEnable");
        }

        private void LaunchProgram(string programKey)
        {
            try
            {
                if (programPaths.ContainsKey(programKey))
                {
                    string programPath = programPaths[programKey];
                    
                    if (File.Exists(programPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = programPath,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Hiba a program indításakor:\n{ex.Message}",
                    "Hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}