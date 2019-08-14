using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Netst
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ActiveFrameProperty = DependencyProperty.Register(
            "ActiveFrame", typeof(Uri), typeof(MainWindow), new PropertyMetadata(new Uri("Pages/Home.xaml",
                UriKind.RelativeOrAbsolute)));

        public Uri ActiveFrame
        {
            get { return (Uri) GetValue(ActiveFrameProperty); }
            set { SetValue(ActiveFrameProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();
            Settings.Volatile.MainWindow = this;
            Settings.Volatile.PrimaryDispatcher = Dispatcher;

            try
            {
                if (File.Exists(Settings.DefaultFilename))
                    Settings.Load();
                else
                    Settings.Save();
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to load app settings from disk.\n\n" + e.Message, "Settings load failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Version assyVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            if (assyVersion != null)
            {
                if (assyVersion.Major < 1)
                    Title += " - BETA " + assyVersion.ToString(4);
            }
        }

        private void HomeTab_OnClick(object sender, RoutedEventArgs e)
        {
            NavigateToTab("home");
        }

        private void NetworkTab_OnClick(object sender, RoutedEventArgs e)
        {
            NavigateToTab("network");
        }

        private void SettingsTab_OnClick(object sender, RoutedEventArgs e)
        {
            NavigateToTab("settings");
        }

        public void NavigateToTab(string xamlName)
        {
            ActiveFrame = new Uri("Pages/" + xamlName + ".xaml", UriKind.RelativeOrAbsolute);
            Settings.Volatile.MainWindowFrame = ActiveFrame;
        }
    }
}
