using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Vlc.DotNet.Core.Interops;
using System.Windows.Forms.Integration;


using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vlc.DotNet.Core;
using System.IO;
using System.Windows.Threading;

namespace WpfVlc
{
    public class VlcControl : WindowsFormsHost, INotifyPropertyChanged
    {

        #region interface INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Vlc.DotNet.Forms.VlcControl MediaPlayer;

        public VlcControl()
        {
            MediaPlayer = new Vlc.DotNet.Forms.VlcControl();
            this.Child = MediaPlayer;

            RegistCallback();



            RaisePropertyChanged("Rate");
            RaisePropertyChanged("Volume");

            /*
             * set a timer
             */
            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(17);
            timer.Tick += timer_Tick;
            old_time = DateTime.UtcNow;
        }

        #region timer callback
        DispatcherTimer timer;

        DateTime old_time;
        DateTime now;
        long delta;
        void timer_Tick(object sender, EventArgs e)
        {
            // calc delta
            now = DateTime.UtcNow;
            delta = (long)((now - old_time).Milliseconds * MediaPlayer.Rate);
            old_time = now;
            
            this.time.time +=delta;
            RaisePropertyChanged("Time");


            //timeFPS = (int)((1000 / delta) + timeFPS) / 2;
            //RaisePropertyChanged("TimeFPS");

        }
        #endregion


        #region vlc callback

        void RegistCallback()
        {
            //MediaPlayer.BackColor = System.Drawing.Color.FromString(BackColor);

            
            MediaPlayer.VlcLibDirectoryNeeded += VlcLibDirectoryNeeded;
            
            MediaPlayer.PositionChanged += PlayerPositionChanged;

            MediaPlayer.TimeChanged += TimeChanged;

            MediaPlayer.LengthChanged += LengthChanged;

            MediaPlayer.EndReached += MediaPlayer_EndReached;

           

        }

        void VlcLibDirectoryNeeded(object sender, Vlc.DotNet.Forms.VlcLibDirectoryNeededEventArgs e)
        {
            if (LibVlcPath != null)
            {
                e.VlcLibDirectory = new DirectoryInfo(LibVlcPath);
                vlc_ok = true;
                return;
            }

            // set the vlc path to current direction
            var currentAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                throw new Exception("Can't get Current Direction.");

            // test is libvlc.dll is exist
            if (!File.Exists(Path.Combine(currentDirectory,"libvlc.dll")))
                throw new FileNotFoundException("LibVlc not found.");

            // lib vlc is ok
            vlc_ok = true;

            e.VlcLibDirectory = new DirectoryInfo(currentDirectory);
        }

        
        
        public delegate void VideoReachEndDemo();
        public event VideoReachEndDemo EndReached;

        void MediaPlayer_EndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                is_end = true;

                // set to start
                this.position = 0;
                RaisePropertyChanged("Position");

                this.isPlay = false;

                if (EndReached != null) EndReached();
            }));
        }


        
        void TimeChanged(object sender, VlcMediaPlayerTimeChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                time = e.NewTime;
                second = e.NewTime;

                RaisePropertyChanged("Second");
            }));

        }

        void PlayerPositionChanged(object sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                position = e.NewPosition;

                RaisePropertyChanged("Position");
                RaisePropertyChanged("FPS");

            }));

        }

        void LengthChanged(object sender, VlcMediaPlayerLengthChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                //totalTime = (long)e.NewLength; // this lenght is error
                totalTime = MediaPlayer.Length;
                RaisePropertyChanged("TotalTime");

            }));
        }

        #endregion


        #region property


        #region time 
        /* 
         * current time for media
         * as 1ms
         * // read only
         */
        private Time totalTime = 0;
        public Time TotalTime
        {
            get { return totalTime; }
        }

        /* 
         * current time for media
         * as 1ms
         */
        private Time time = 0;
        public Time Time
        {
            get { return time; }
            set
            {
                if (!MediaPlayer.IsPlaying)
                    return;
                if (value == time) return;
                time = value;
                MediaPlayer.Time = value;
            }
        }

        public Time second;
        public Time Second { get { return second;} }


        /*  
         * the media current position
         * between 0 and 1
         */
        private double position;
        public double Position
        {
            get { return position; }
            set
            {
                // this only for control video position out side of class
                if (!is_open)
                    return;
                if (value < 0) return;
                if (value == position) return;
                position = value;
                MediaPlayer.Position = (float)value;
            }
        }

        #endregion

        public string LibVlcPath { get; set; }

        private string source;
        public string Source
        {
            get { return source; }
            set
            {
                if (value == null) return;
                source = value;
                this.Open(source);
                RaisePropertyChanged();
            }
        }


        #region vidoe or audio control

        public int Volume
        {
            get { return MediaPlayer.Audio.Volume; }
            set
            {
                if (value < 0) return;

                MediaPlayer.Audio.Volume = value;

                RaisePropertyChanged();
            }
        }

        public float Rate
        {
            get { return MediaPlayer.Rate; }
            set
            {
                if (value < 0) return;

                MediaPlayer.Rate = value;
                RaisePropertyChanged();
            }
        }

        private string subFile;
        public string SubFile
        {
            get { return subFile; }
            set
            {
                subFile = value;

                MediaPlayer.SetSubTitle(value);
            }
        }

        private bool mutex;
        public bool Mutex
        {
            get { return mutex; }
            set
            {
                mutex = value;

                if(mutex && ! MediaPlayer.Audio.IsMute)
                    MediaPlayer.Audio.ToggleMute();
                
                if(!mutex && MediaPlayer.Audio.IsMute)
                    MediaPlayer.Audio.ToggleMute();

            }
        }

        private bool isPlay;
        public bool IsPlay
        {
            get { return isPlay; }
            set
            {
                if (!vlc_ok || !is_open)
                {
                    isPlay = false; return;
                }

                isPlay = value;
                if (value)
                    Play();
                else
                    Pause();

                RaisePropertyChanged();
            }

        }

        #endregion

        #region other
        public string BackColor { get; set; }


        public float FPS { get { return MediaPlayer.FPS; } }

        private int timeFPS;
        public int TimeFPS { get{return timeFPS;} }

        
        #endregion

        #endregion

        #region local var
        bool is_end = false;
        bool vlc_ok = false;
        bool is_open = false;

        #endregion


        #region function
        public void Play()
        {
            if (!vlc_ok) return;

            
            if (is_end)
            {
                MediaPlayer.Play(new FileInfo(source));

                is_end = false;
            }

            old_time = DateTime.UtcNow;
            timer.Start();
            MediaPlayer.Play();

            // set position if need
            MediaPlayer.Position = (float)this.position;
        }

        public void Pause()
        {
            if (!vlc_ok || !is_open) return;

            timer.Stop();
            MediaPlayer.Pause();
        }

        public void Open(string path)
        {
            this.source = path;
            is_open = true;

            timer.Stop();
            MediaPlayer.Pause();
            
            //detect path format
            if (path.Contains("http") || path.Contains("https"))
                MediaPlayer.SetMedia(new Uri(source));
            else
                MediaPlayer.SetMedia(new FileInfo(source), null);

            // is already open or play a media
            IsPlay = false;
            // go to start
            position = 0;
            RaisePropertyChanged("Position");


            // call function if need
            if (EndReached != null) EndReached();
        }

        public void EndInit()
        {
            MediaPlayer.EndInit();

        }

        #endregion


    }
}