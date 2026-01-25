using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression; // ? Zip kicsomagoláshoz
using System.Net.Http;       // ? Letöltéshez
using System.Threading.Tasks;
using System.Windows;
using AxionControl.Views;

namespace AxionControl
{
    public partial class MainWindow : Window
    {
        private const string RootFolder = @"C:\LibrAxon";
        private const string DownloadUrl = "http://villamgyorspc.hu/downloads/LibrAxon_latest.zip";
        private const string TempZipPath = @"C:\LibrAxon\temp_update.zip";

        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new AxionControl.Views.InfoView();
            
            // Indítás utáni ellenõrzés
            Loaded += async (s, e) => await InitializeLibraryAsync();
        }

        private async Task InitializeLibraryAsync()
        {
            try
            {
                // 1. Könyvtár ellenõrzése
                if (!Directory.Exists(RootFolder))
                {
                    StatusTextBlock.Text = "Könyvtár létrehozása...";
                    Directory.CreateDirectory(RootFolder);
                }

                // 2. Tartalom ellenõrzése (ha üres a mappa, letöltünk)
                if (Directory.GetFileSystemEntries(RootFolder).Length == 0)
                {
                    var result = MessageBox.Show("A LibrAxon könyvtár üres. Szeretnéd letölteni az összetevõket?", 
                        "Frissítés", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await DownloadAndExtractAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Hiba az inicializáláskor.";
                MessageBox.Show("Hiba: " + ex.Message);
            }
        }

        private async Task DownloadAndExtractAsync()
        {
            StatusTextBlock.Text = "Letöltés folyamatban...";
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.IsIndeterminate = true;

            try
            {
                using (var client = new HttpClient())
                {
                    // Letöltés
                    var response = await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(TempZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }

                StatusTextBlock.Text = "Kicsomagolás...";
                DownloadProgressBar.IsIndeterminate = true;

                // Kicsomagolás (külön szálon, hogy ne akadjon az UI)
                await Task.Run(() => 
                {
                    ZipFile.ExtractToDirectory(TempZipPath, RootFolder, true);
                    File.Delete(TempZipPath); // Ideiglenes fájl törlése
                });

                StatusTextBlock.Text = "Rendszer kész.";
                MessageBox.Show("Sikeres letöltés és kicsomagolás!", "LibrAxon", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Hiba történt.";
                MessageBox.Show($"Hiba: {ex.Message}");
            }
            finally
            {
                DownloadProgressBar.Visibility = Visibility.Collapsed;
            }
        }

		void ShowInfo(object s, RoutedEventArgs e)
			=> MainContent.Content = new InfoView();

        void ShowApps(object s, RoutedEventArgs e)
            => MainContent.Content = new AppsView();

        void ShowDrivers(object s, RoutedEventArgs e)
            => MainContent.Content = new DriversView();

        void ShowAbout(object s, RoutedEventArgs e)
            => MainContent.Content = new AboutView();
		
		void Run(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("Fájl nem található:\n" + path);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "runas" // UAC
            });
        }
    }
}
