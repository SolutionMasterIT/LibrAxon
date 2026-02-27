using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AxionControl.Views
{
    public partial class SelfView : UserControl
    {
        private readonly string basePath = @"C:\LibrAxon\SELF";

        public SelfView()
        {
            InitializeComponent();
            Loaded += SelfView_Loaded;
        }

        private void SelfView_Loaded(object sender, RoutedEventArgs e)
        {
            EnsureDirectoryStructure();
            RefreshButtons();
        }

        private void EnsureDirectoryStructure()
        {
            try
            {
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    
                    // Example könyvtár létrehozása
                    string examplePath = Path.Combine(basePath, "ExampleApp");
                    Directory.CreateDirectory(examplePath);
                    
                    File.WriteAllText(Path.Combine(examplePath, "leiras.txt"), "Példa Alkalmazás");
                    File.WriteAllText(Path.Combine(examplePath, "start.cmd"), "@echo off\necho Ez egy pelda indito fajl!\npause");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a könyvtárstruktúra létrehozásakor: {ex.Message}");
            }
        }

        private void RefreshButtons()
        {
            // Kitakarítjuk a meglévő gombokat (a XAML-ben adj nevet a StackPanelnek!)
            DynamicButtonPanel.Children.Clear();

            try
            {
                var directories = Directory.GetDirectories(basePath);

                foreach (var dir in directories)
                {
                    string leirasPath = Path.Combine(dir, "leiras.txt");
                    string cmdPath = Path.Combine(dir, "start.cmd");

                    // Csak akkor hozunk létre gombot, ha mindkét fájl létezik
                    if (File.Exists(leirasPath) && File.Exists(cmdPath))
                    {
                        string buttonText = File.ReadAllText(leirasPath).Trim();
                        
                        Button btn = new Button
                        {
                            Content = buttonText,
                            Tag = cmdPath, // Itt tároljuk az indítandó fájl útvonalát
                            Style = (Style)FindResource("PurpleButtonStyle")
                        };

                        btn.Click += DynamicButton_Click;
                        DynamicButtonPanel.Children.Add(btn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a gombok betöltésekor: {ex.Message}");
            }
        }

        private void DynamicButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string cmdPath)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = cmdPath,
                        WorkingDirectory = Path.GetDirectoryName(cmdPath), // Fontos a relatív utak miatt
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a start.cmd indításakor: {ex.Message}");
                }
            }
        }
    }
}