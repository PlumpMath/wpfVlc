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
            myControl.MediaPlayer.VlcLibDirectoryNeeded += OnVlcControlNeedsLibDirectory;
            myControl.MediaPlayer.EndInit();
        }

        private void OnVlcControlNeedsLibDirectory(object sender, Forms.VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;
            if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x86\"));
            else
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"..\..\..\lib\x64\"));
        }

        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            //myControl.MediaPlayer.Play(new Uri("http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_surround-fix.avi"));
            myControl.MediaPlayer.Play(new FileInfo(@"r:\[UHA-Wing] [Nyanbo!][26][1080p][BIG5].mp4"));
        }

        private void OnForwardButtonClick(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Rate = 2;
        }

        private void GetLength_Click(object sender, RoutedEventArgs e)
        {
            GetLength.Content = myControl.MediaPlayer.Length + " ms";
            myControl.MediaPlayer.SetSubTitle(null);
        }

        private void GetCurrentTime_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTime.Content = myControl.MediaPlayer.Time + " ms";
            myControl.MediaPlayer.SetSubTitle(@"r:\[UHA-Wing] [Nyanbo!][26][1080p][BIG5].ass");
        }

        private void SetCurrentTime_Click(object sender, RoutedEventArgs e)
        {
            myControl.MediaPlayer.Time = 5000;
            SetCurrentTime.Content = myControl.MediaPlayer.Time + " ms";
        }
    }
}
