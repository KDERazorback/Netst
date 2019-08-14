using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Netst
{
    /// <summary>
    /// Lógica de interacción para AnnouncerStartWindow.xaml
    /// </summary>
    public partial class AnnouncerStartWindow : Window
    {
        public AnnouncerStartWindow()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty AnnouncerTaskProperty = DependencyProperty.Register(
            "AnnouncerTask", typeof(Task), typeof(AnnouncerStartWindow), new PropertyMetadata(default(Task)));

        public Task AnnouncerTask
        {
            get { return (Task)GetValue(AnnouncerTaskProperty); }
            set { SetValue(AnnouncerTaskProperty, value); }
        }

        public static AnnouncerStartWindow ShowAndStart()
        {
            AnnouncerStartWindow wnd = new AnnouncerStartWindow();
            wnd.Show();
            wnd.Start();

            return wnd;
        }

        public void Start()
        {
            AnnouncerTask = new Task(() =>
            {
                AnnouncerStartWindow parent = this;

                Settings.Volatile.TryAttachAnnouncer();
            });
            AnnouncerTask.ContinueWith(task =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (task.IsFaulted)
                    {
                        MessageBox.Show(
                            "An error occured while trying to start the announcer.\n\n"
                                + "\n\n" + task.Exception?.ToString(), "Announcer error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    this.Close();
                });
            });
            AnnouncerTask.Start();
        }
    }
}
