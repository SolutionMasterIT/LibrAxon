using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression; //Zip kicsomagoláshoz
using System.Net.Http;       //Letöltéshez
using System.Threading.Tasks;
using System.Windows;
using AxionControl.Views;

namespace AxionControl
{
    public partial class MainWindow : Window
    {
        private const string RootFolder = @"C:\LibrAxon";
        private const string DownloadUrl = "https://villamgyorspc.hu/downloads/LibrAxon_latest.zip";
        private const string TempZipPath = @"C:\LibrAxon\temp_update.zip";
        private const string Version = "26.02.21";

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
				// Könyvtár ellenőrzése
				bool isMissing = !Directory.Exists(RootFolder) || Directory.GetFileSystemEntries(RootFolder).Length == 0;

				if (isMissing)
				{
					// Ha a könyvtár nem létezik
					if (!Directory.Exists(RootFolder))
					{
						StatusTextBlock.Text = "Könyvtár létrehozása...";
						Directory.CreateDirectory(RootFolder);
					}
					// Ha az alapcsomag hiányzik, AKKOR megjelenítjük a gombot a menüben
					DownloadMenuItem.Visibility = Visibility.Visible;
					StatusTextBlock.Text = "Teljes funkcionalitáshoz letöltés szükséges!";
				}
				else
				{
					// Ha minden megvan, a gomb rejtve marad
					DownloadMenuItem.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hiba az ellenőrzéskor: " + ex.Message);
			}
		}

        private async Task DownloadAndExtractAsync()
		{
			DownloadProgressBar.Visibility = Visibility.Visible;
			DownloadProgressBar.IsIndeterminate = false; // Kikapcsoljuk a végtelen csíkot
			DownloadProgressBar.Value = 0;

			try
			{
				using (var client = new HttpClient())
				{
					// 1. LETÖLTÉS SZÁZALÉKKAL
					StatusTextBlock.Text = "Letöltés: 0%";
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

				// 2. KICSOMAGOLÁS SZÁZALÉKKAL
				StatusTextBlock.Text = "Kicsomagolás: 0%";
				DownloadProgressBar.Value = 0;

				await Task.Run(() =>
				{
					using (ZipArchive archive = ZipFile.OpenRead(TempZipPath))
					{
						int totalEntries = archive.Entries.Count;
						int extractedEntries = 0;

						foreach (ZipArchiveEntry entry in archive.Entries)
						{
							// Teljes elérési út meghatározása
							string destinationPath = Path.GetFullPath(Path.Combine(RootFolder, entry.FullName));

							// Mappa esetén létrehozzuk
							if (Path.GetFileName(destinationPath).Length == 0)
							{
								Directory.CreateDirectory(destinationPath);
								extractedEntries++;
								continue;
							}

							// Fájl kiírása (felülírással)
							Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
							entry.ExtractToFile(destinationPath, true);

							extractedEntries++;

							// UI frissítése
							double progress = (double)extractedEntries / totalEntries * 100;
							Dispatcher.Invoke(() =>
							{
								DownloadProgressBar.Value = progress;
								StatusTextBlock.Text = $"Kicsomagolás: {Math.Round(progress)}%";
							});
						}
					}
					File.Delete(TempZipPath);
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
		
		private async void ManualDownload_Click(object sender, RoutedEventArgs e)
		{
			// Ellenőrizzük, hogy nem fut-e már egy letöltés (opcionális, de ajánlott)
			if (DownloadProgressBar.Visibility == Visibility.Visible)
			{
				MessageBox.Show("Egy letöltés már folyamatban van!", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			var result = MessageBox.Show("Szeretnéd elindítani a LibrAxon összetevők letöltését/frissítését?", 
										 "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				await DownloadAndExtractAsync();
			}
		}

		void ShowInfo(object s, RoutedEventArgs e) => MainContent.Content = new InfoView();
        void ShowApps(object s, RoutedEventArgs e) => MainContent.Content = new AppsView();
        void ShowDrivers(object s, RoutedEventArgs e) => MainContent.Content = new DriversView();
        void ShowAbout(object s, RoutedEventArgs e) => MainContent.Content = new AboutView();
		
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
