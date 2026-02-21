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
        // Program útvonalak szótárban - Központi adatforrás
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

        /// <summary>
        /// Végigmegy a szótáron és letiltja azokat a gombokat, amikhez nem található fájl.
        /// Feltételezi, hogy a gombok neve "btn" + a szótár kulcsa (pl. btnStartAllBack).
        /// </summary>
        private void CheckButtonAvailability()
        {
            foreach (var item in programPaths)
            {
                if (this.FindName("btn" + item.Key) is Button button)
                {
                    button.IsEnabled = File.Exists(item.Value);
                }
            }
        }

        /// <summary>
        /// Közös eseménykezelő az összes gombhoz.
        /// A XAML-ben mindegyik gombra ezt állítsd be: Click="AppButton_Click"
        /// </summary>
        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // Kivesszük a gomb nevéből a "btn" előtagot, hogy megkapjuk a szótár kulcsát
                string key = btn.Name.Replace("btn", "");
                LaunchProgram(key);
            }
        }

        private void LaunchProgram(string key)
        {
            try
            {
                if (programPaths.TryGetValue(key, out string path) && File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
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