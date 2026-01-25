using System;
using System.Diagnostics;
using System.Management;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AxionControl.Views
{
    public partial class InfoView : UserControl
    {
        private DispatcherTimer _timer;
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramAvailableCounter;
        private long _totalMemoryMB;

        public InfoView()
        {
            InitializeComponent();
            
            Console.WriteLine("=== InfoView inicializálás kezdés ===");
            
            Loaded += (s, e) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        InitializePerformanceCounters();
                        
                        Dispatcher.Invoke(() =>
                        {
                            LoadSystemInfo();
                            StartMonitoring();
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Inicializálási hiba: {ex.Message}");
                    }
                });
            };
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                Console.WriteLine("🔄 Performance counterek inicializálása...");
                
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                Console.WriteLine("✓ CPU counter létrehozva");
                
                _ramAvailableCounter = new PerformanceCounter("Memory", "Available MBytes", true);
                Console.WriteLine("✓ RAM counter létrehozva");
                
                GetTotalMemory();
                
                _cpuCounter.NextValue();
                _ramAvailableCounter.NextValue();
                Console.WriteLine("✓ Counterek inicializálva\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Performance counter hiba: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }
        }

		private void GetTotalMemory()
		{
			try
			{
				// 1. Megpróbáljuk a pontosabb módszert (WMI)
				using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
				{
					foreach (ManagementObject obj in searcher.Get())
					{
						var totalKB = obj["TotalVisibleMemorySize"];
						if (totalKB != null)
						{
							_totalMemoryMB = Convert.ToInt64(totalKB) / 1024;
							UpdateDebugInfo($"Teljes RAM: {_totalMemoryMB} MB");
						}
					}
				}
			}
			catch (Exception) { /* WMI hiba esetén megyünk tovább */ }

			// 2. Ha a WMI nem adott értéket (0 maradt), jöhet a biztonsági mentés
			if (_totalMemoryMB <= 0)
			{
				try
				{
					var memInfo = GC.GetGCMemoryInfo();
					_totalMemoryMB = memInfo.TotalAvailableMemoryBytes / (1024 * 1024);
				}
				catch { _totalMemoryMB = 8192; } // Végső eset: fix 8GB, hogy ne legyen osztás nullával
			}
		}

        private void UpdateDebugInfo(string message)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    if (DebugInfoTextBlock != null)
                    {
                        DebugInfoTextBlock.Text = message;
                    }
                });
            }
            catch (Exception) { }
        }

        private void LoadSystemInfo()
        {
            try
            {
                string cpuInfo = GetProcessorInfo();
                if (CpuInfoTextBlock != null)
                {
                    CpuInfoTextBlock.Text = cpuInfo;
                }

                string boardInfo = GetBoardInfo();
                if (BoardInfoTextBlock != null)
                {
                    BoardInfoTextBlock.Text = boardInfo;
                }

                string windowsVersion = GetWindowsVersion();
                if (WindowsVersionTextBlock != null)
                {
                    WindowsVersionTextBlock.Text = windowsVersion;
                }

                string gpuInfo = GetGPUInfo();
                if (GpuInfo1TextBlock != null && GpuInfo2TextBlock != null && GpuInfo3TextBlock != null)
                {
                    var gpuList = gpuInfo.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    GpuInfo1TextBlock.Text = gpuList.Length > 0 ? gpuList[0] : "N/A";
                    GpuInfo2TextBlock.Text = gpuList.Length > 1 ? gpuList[1] : "";
                    GpuInfo3TextBlock.Text = gpuList.Length > 2 ? gpuList[2] : "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Rendszerinformáció betöltési hiba: {ex.Message}");
            }
        }

        private void StartMonitoring()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _timer.Tick += (s, e) => { _ = UpdatePerformanceDataAsync(); };
            _timer.Start();

            Task.Delay(500).ContinueWith(_ => Dispatcher.Invoke(() => { _ = UpdatePerformanceDataAsync(); }));
        }

		private async Task UpdatePerformanceDataAsync()
		{
			try
			{
				var result = await Task.Run(() =>
				{
					float cpu = 0;
					float ram = 0;
					string debugMessage = "";

					try
					{
						// CPU mérés ellenőrzéssel
						if (_cpuCounter != null)
						{
							cpu = _cpuCounter.NextValue();
						}
					}
					catch (Exception) { debugMessage += "CPU hiba; "; }

					try
					{
						// RAM mérés - Itt a leggyakoribb a hiba
						if (_ramAvailableCounter != null && _totalMemoryMB > 0)
						{
							float availableMB = _ramAvailableCounter.NextValue();
							
							// Ha a számláló hülyeséget ad vissza (pl. több a szabad, mint az összes)
							if (availableMB > _totalMemoryMB) availableMB = _totalMemoryMB;

							float usedMB = _totalMemoryMB - availableMB;
							
							// Biztonságos osztás
							ram = (usedMB / (float)_totalMemoryMB) * 100f;
							
							debugMessage = $"RAM: {ram:F0}% ({usedMB:F0}/{_totalMemoryMB} MB)";
						}
						else
						{
							debugMessage += $"RAM inicializálatlan (Total: {_totalMemoryMB}); ";
						}
					}
					catch (Exception) { debugMessage += "RAM hiba; "; }

					return (cpu, ram, debugMessage);
				});

				// UI frissítés
				await Dispatcher.InvokeAsync(() =>
				{
					if (!string.IsNullOrEmpty(result.debugMessage))
						UpdateDebugInfo(result.debugMessage);

					if (CpuPercentageTextBlock != null)
					{
						CpuPercentageTextBlock.Text = $"{Math.Round(result.cpu)}%";
						UpdateCircleFill(CpuFillPath, result.cpu);
					}

					if (RamPercentageTextBlock != null)
					{
						RamPercentageTextBlock.Text = $"{Math.Round(result.ram)}%";
						UpdateCircleFill(RamFillPath, result.ram);
					}
				});
			}
			catch (Exception ex)
			{
				// Ha ide jutunk, akkor nem a mérés, hanem az UI frissítés halt el
				UpdateDebugInfo($"UI Update hiba: {ex.Message}");
			}
		}

        private void UpdateCircleFill(Path fillPath, float percentage)
        {
            if (fillPath == null) return;

            percentage = Math.Max(0, Math.Min(100, percentage));
            
            double radius = 86;
            double centerX = 90;
            double centerY = 90;
            
            double angle = (percentage / 100.0) * 360.0 - 90.0;
            double radians = angle * Math.PI / 180.0;
            
            double endX = centerX + radius * Math.Cos(radians);
            double endY = centerY + radius * Math.Sin(radians);
            
            bool isLargeArc = percentage > 50;
            
            if (percentage == 0)
            {
                fillPath.Data = Geometry.Parse("M 90,90");
            }
            else if (percentage >= 100)
            {
                fillPath.Data = new EllipseGeometry(new System.Windows.Point(centerX, centerY), radius, radius);
            }
            else
			{
				// A CultureInfo.InvariantCulture biztosítja, hogy a tizedesjel mindig pont (.) legyen
				string pathData = string.Create(System.Globalization.CultureInfo.InvariantCulture, 
					$"M {centerX},{centerY} L {centerX},{centerY - radius} A {radius},{radius} 0 {(isLargeArc ? 1 : 0)} 1 {endX},{endY} Z");
				
				fillPath.Data = Geometry.Parse(pathData);
			}
        }

        private string GetProcessorInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Name"]?.ToString() ?? "Ismeretlen CPU";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CPU info hiba: {ex.Message}");
            }
            return "CPU információ nem elérhető";
        }

        private string GetBoardInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Product, Manufacturer FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                        string product = obj["Product"]?.ToString() ?? "";
                        
                        if (!string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(product))
                        {
                            return $"{manufacturer} {product}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Board info hiba: {ex.Message}");
            }
            return "Alaplap információ nem elérhető";
        }

        private string GetWindowsVersion()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string caption = obj["Caption"]?.ToString() ?? "";
                        string buildNumber = obj["BuildNumber"]?.ToString() ?? "";
                        
                        if (int.TryParse(buildNumber, out int build) && build >= 22000)
                        {
                            return $"Windows 11 (Build {buildNumber})";
                        }
                        
                        return $"{caption} (Build {buildNumber})";
                    }
                }
            }
            catch { }
            
            return $"Windows (Build {Environment.OSVersion.Version.Build})";
        }

		private string GetGPUInfo()
		{
			try
			{
				var psi = new ProcessStartInfo
				{
					FileName = "nvidia-smi",
					Arguments = "--query-gpu=name,memory.total --format=csv,noheader",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using (var process = Process.Start(psi))
				{
					if (process == null)
						return "GPU információ nem elérhető";

					string output = process.StandardOutput.ReadToEnd();
					process.WaitForExit(1000);

					if (string.IsNullOrWhiteSpace(output))
						return "GPU információ nem elérhető";

					// Példa:
					// NVIDIA GeForce RTX 3070, 8192 MiB
					var parts = output.Split(',');

					string name = parts[0].Trim();
					string memory = parts.Length > 1 ? parts[1].Trim() : "";

					return $"{name}\nVRAM: {memory}";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ GPU info (nvidia-smi) hiba: {ex.Message}");
				return "GPU információ nem elérhető";
			}
		}

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _timer?.Stop();
            _cpuCounter?.Dispose();
            _ramAvailableCounter?.Dispose();
        }
    }
}