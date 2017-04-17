using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Vlc.DotNet.Wpf.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            setpath();
            myControl.EndInit();
            this.DataContext = myControl;

            //myControl.Source = @"r:\[UHA-Wing] [Nyanbo!][26][1080p][BIG5].mp4";
            myControl.Source = @"r:\1.mp4";
        }

        WpfVlc.VlcControl myControl = new WpfVlc.VlcControl();

        private void setpath()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;
            if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
                myControl.LibVlcPath = Path.Combine(currentDirectory, @"..\..\..\lib\x86\");
            else
                myControl.LibVlcPath = Path.Combine(currentDirectory, @"..\..\..\lib\x64\");
        }


        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            //myControl.MediaPlayer.Play(new Uri("http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_surround-fix.avi"));

            myControl.IsPlay = !myControl.IsPlay;
        }

        private void OnForwardButtonClick(object sender, RoutedEventArgs e)
        {
            myControl.Rate = 2;
        }

        private void GetLength_Click(object sender, RoutedEventArgs e)
        {
            myControl.SubFile= null;
        }

        private void GetCurrentTime_Click(object sender, RoutedEventArgs e)
        {

            myControl.SubFile = @"r:\[UHA-Wing] [Nyanbo!][26][1080p][BIG5].ass";
        }

    }
}
