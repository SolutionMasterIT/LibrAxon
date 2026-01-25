using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices; // ❗ Ez kell a háttérképhez
using System.Windows;
using System.Windows.Controls;

namespace AxionControl.Views
{
    public partial class DriversView : UserControl
    {
        // Windows API hívás a háttérkép beállításához
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        private readonly Dictionary<string, string> driverPaths = new Dictionary<string, string>
        {
            { "DirectX", @"C:\LibrAxon\DRV\DirectX\setup.exe" },
            { "Realtek", @"C:\LibrAxon\DRV\Realtek\setup.exe" },
            { "Dolby1", @"C:\LibrAxon\DRV\Dolby\DolbyDigitalPlusDecoderOEM.AppxBundle" },
            { "DolbyAC4", @"C:\LibrAxon\DRV\Dolby\DolbyAC4DecoderOEM.AppxBundle" },
            { "DefenderKiller", @"C:\LibrAxon\BIN\DefenderKiller\dControl.exe" },
            { "Wallpaper", @"C:\LibrAxon\MEDIA\IMAGE\BACKGROUND\01.jpg" }, // A kép útvonala
            { "RemoteDesktop", @"C:\LibrAxon\BIN\RDP\rdp_manager.exe" },
            { "Winbox", @"C:\LibrAxon\DRV\Mikrotik\winbox64.exe" },
            { "MedveIro", @"C:\LibrAxon\BIN\AdwCleaner\AdwCleaner.exe" },
            { "DDU", @"C:\LibrAxon\DRV\DDU\ddu.exe" }
        };

        public DriversView()
        {
            InitializeComponent();
            Loaded += DriversView_Loaded;
        }

        private void DriversView_Loaded(object sender, RoutedEventArgs e)
        {
            CheckButtonAvailability();
        }

        private void CheckButtonAvailability()
        {
            foreach (var item in driverPaths)
            {
                var button = this.FindName("btn" + item.Key) as Button;
                if (button != null)
                {
                    button.IsEnabled = File.Exists(item.Value);
                }
            }
        }

        private void DriverButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string key = btn.Name.Replace("btn", "");
                LaunchDriver(key);
            }
        }

		private void LaunchDriver(string key)
		{
			if (this.FindName("btn" + key) is Button btn)
			{
				if (key == "Wallpaper")
				{
					SetWallpaper(driverPaths[key]);
				}
				else if (key == "TestMode")
				{
					HandleTestMode(btn);
				}
				else
				{
					// Minden más program normál indítása
					try
					{
						if (driverPaths.TryGetValue(key, out string path) && File.Exists(path))
						{
							Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Hiba: {ex.Message}");
					}
				}
			}
		}

        // Metódus a háttérkép tényleges módosításához
        private void SetWallpaper(string path)
        {
            int result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            
            if (result != 0)
            {
                MessageBox.Show("Axion háttérkép sikeresen beállítva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Nem sikerült beállítani a háttérképet.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
		
		// Osztály szintű változó az állapot tárolásához
		private bool _isTestModeActive = false;

		// Segédmetódus a BCD parancsok futtatásához (rendszergazdai jog szükséges!)
		private void RunBCDCommand(string command)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = $"/c {command}",
					Verb = "runas", // Rendszergazdai jog kérése
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					UseShellExecute = true
				};
				Process.Start(psi);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Hiba a parancs végrehajtásakor: {ex.Message}");
			}
		}

		// A Test Mode gomb logikája
		private void HandleTestMode(Button btn)
		{
			if (_isTestModeActive)
			{
				RunBCDCommand("bcdedit /set nointegritychecks OFF");
				RunBCDCommand("bcdedit /set testsigning OFF");
				_isTestModeActive = false;
				btn.Content = "Test Mode: KIKAPCSOLVA";
				// Opcionálisan változtathatod a színét is, hogy jelezd a váltást
				btn.Opacity = 0.7; 
			}
			else
			{
				RunBCDCommand("bcdedit /set nointegritychecks ON");
				RunBCDCommand("bcdedit /set testsigning ON");
				_isTestModeActive = true;
				btn.Content = "Test Mode: BEKAPCSOLVA";
				btn.Opacity = 1.0;
			}

			MessageBox.Show("A módosítás érvénybe lépéséhez újraindítás szükséges!", "Rendszer értesítés", MessageBoxButton.OK, MessageBoxImage.Information);
		}
    }
}