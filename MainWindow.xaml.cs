using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using AxionControl.Views;

namespace AxionControl
{
    public partial class MainWindow : Window
    {
        private const string RootFolder = @"C:\LibrAxon";
        private const string XtremeShell = @"C:\LibrAxon\XtremeShell.exe";
        private const string XtremeShellUrl = "https://xtremeshell.neonity.hu/files/XtremeShell%205.2%20Portable.exe";
        private const string DownloadUrl = "https://villamgyorspc.hu/downloads/LibrAxon_latest.zip";
        private const string TempZipPath = @"C:\LibrAxon\temp_update.zip";
		public const string Version = "26.02.27";

        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new AxionControl.Views.InfoView();
            this.Title += " " + Version;

            // Indítás utáni ellenőrzés
            Loaded += async (s, e) => await InitializeLibraryAsync();
        }

        private async Task InitializeLibraryAsync()
        {
            try
            {
                // 1. Könyvtár ellenőrzése
                bool isFolderMissing = !Directory.Exists(RootFolder) || Directory.GetFileSystemEntries(RootFolder).Length == 0;
                // 2. XtremeShell fájl ellenőrzése
                bool isShellMissing = !File.Exists(XtremeShell);

                // Mappa és Főcsomag kezelése
                if (isFolderMissing)
                {
                    if (!Directory.Exists(RootFolder)) Directory.CreateDirectory(RootFolder);
                    DownloadMenuItem.Visibility = Visibility.Visible;
                    StatusTextBlock.Text = "Teljes funkcionalitáshoz letöltés szükséges!";
                }
                else
                {
                    DownloadMenuItem.Visibility = Visibility.Collapsed;
                    StatusTextBlock.Text = "Kész";
                }

                // XtremeShell gombok kezelése
                if (isShellMissing)
                {
                    XtremeShellDownloadMenuItem.Visibility = Visibility.Visible;
                    XtremeShellStartMenuItem.Visibility = Visibility.Collapsed;
                    if (!isFolderMissing) StatusTextBlock.Text = "XtremeShell hiányzik!";
                }
                else
                {
                    XtremeShellDownloadMenuItem.Visibility = Visibility.Collapsed;
                    XtremeShellStartMenuItem.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba az ellenőrzéskor: " + ex.Message);
            }
        }

        private async Task Download_XtremeShell()
        {
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(XtremeShellUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength;

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(XtremeShell, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int read;

                        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;
                            if (totalBytes.HasValue)
                            {
                                double progress = (double)totalRead / totalBytes.Value * 100;
                                DownloadProgressBar.Value = progress;
                                StatusTextBlock.Text = $"XtremeShell letöltés: {Math.Round(progress)}%";
                            }
                        }
                    }
                }
                // Frissítjük a gombokat
                await InitializeLibraryAsync();
                MessageBox.Show("XtremeShell 5.2 sikeresen letöltve!", "LibrAxon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a letöltés során: {ex.Message}");
            }
            finally
            {
                DownloadProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task DownloadAndExtractAsync()
        {
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;

            try
            {
                using (var client = new HttpClient())
                {
                    StatusTextBlock.Text = "Csomag letöltése...";
                    var response = await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength;

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(TempZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int read;

                        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;
                            if (totalBytes.HasValue)
                            {
                                double progress = (double)totalRead / totalBytes.Value * 100;
                                DownloadProgressBar.Value = progress;
                                StatusTextBlock.Text = $"Letöltés: {Math.Round(progress)}%";
                            }
                        }
                    }
                }

                StatusTextBlock.Text = "Kicsomagolás...";
                await Task.Run(() =>
                {
                    using (ZipArchive archive = ZipFile.OpenRead(TempZipPath))
                    {
                        int totalEntries = archive.Entries.Count;
                        int extractedEntries = 0;

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(RootFolder, entry.FullName));
                            if (Path.GetFileName(destinationPath).Length == 0)
                            {
                                Directory.CreateDirectory(destinationPath);
                                extractedEntries++;
                                continue;
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                            entry.ExtractToFile(destinationPath, true);
                            extractedEntries++;

                            double progress = (double)extractedEntries / totalEntries * 100;
                            Dispatcher.Invoke(() => {
                                DownloadProgressBar.Value = progress;
                                StatusTextBlock.Text = $"Kicsomagolás: {Math.Round(progress)}%";
                            });
                        }
                    }
                    if (File.Exists(TempZipPath)) File.Delete(TempZipPath);
                });

                await InitializeLibraryAsync();
                MessageBox.Show("Sikeres frissítés!", "LibrAxon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}");
            }
            finally
            {
                DownloadProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        // --- Eseménykezelők ---

        private void XtremeShellStart_Click(object sender, RoutedEventArgs e)
        {
            try {
                Process.Start(new ProcessStartInfo { FileName = XtremeShell, UseShellExecute = true });
            } catch (Exception ex) {
                MessageBox.Show("Nem sikerült elindítani: " + ex.Message);
            }
        }

        private async void XtremeShellDownload_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadProgressBar.Visibility == Visibility.Visible) return;
            if (MessageBox.Show("Letöltöd a XtremeShell-t?", "Megerősítés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                await Download_XtremeShell();
        }

        private async void ManualDownload_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadProgressBar.Visibility == Visibility.Visible) return;
            if (MessageBox.Show("Frissíted a LibrAxon összetevőket?", "Megerősítés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                await DownloadAndExtractAsync();
        }

        // Navigáció
        void ShowInfo(object s, RoutedEventArgs e) => MainContent.Content = new InfoView();
        void ShowApps(object s, RoutedEventArgs e) => MainContent.Content = new AppsView();
        void ShowDrivers(object s, RoutedEventArgs e) => MainContent.Content = new DriversView();
        void ShowSelf(object s, RoutedEventArgs e) => MainContent.Content = new SelfView();
        void ShowAbout(object s, RoutedEventArgs e) 
        {
            var aboutView = new AboutView();
            // Beállítjuk a DataContext-et, hogy a XAML tudja honnan vegye az adatot
            aboutView.DataContext = new { AppVersion = Version }; 
            MainContent.Content = aboutView;
        }
    }
}