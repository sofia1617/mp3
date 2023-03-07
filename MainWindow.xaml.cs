using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace mp3pl
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeSpan position;
        private TimeSpan Time;
        private TimeSpan _remainingTime;
        static bool restart = false;
        static bool mix = false;
        static bool playOrStop;
        static string folderPath = "";
        static int songindex = 0;
        List<string> fileNames = new List<string>();
        List<string> listNames = new List<string>();
        List<string> copySongsListNames = new List<string>();
        List<string> copyFileNames = new List<string>();
        Random random = new Random();
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        string[] files;
        public TimeSpan time
        {
            get { return Time; }
            set { Time = value; OnPropertyChanged("time"); }
        }

        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; OnPropertyChanged("RemainingTime"); }
        }


        public MainWindow()
        {
            InitializeComponent();
            PlayOrPause.Content = "\u25B6";
            Next.Content = "\u23E9";
            Back.Content = "\u23EA";
            Mix.Content = "\u1F500";
            Restart.Content = "\u21BA";
            DataContext = this;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }


        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan) MediaSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.Ticks;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MediaSlider.Value = mediaPlayer.Position.Ticks;
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    RemainingTime = mediaPlayer.NaturalDuration.TimeSpan - mediaPlayer.Position;
                    remainingTimeText.Text = RemainingTime.ToString(@"mm\:ss");
                }
                time = mediaPlayer.Position;
                currentTimeText.Text = time.ToString(@"mm\:ss");
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    if (time == mediaPlayer.NaturalDuration.TimeSpan)
                    {
                        if (restart == false & mix == false)
                        {
                            NextSong();
                        }
                        else
                        {
                            RestartSong();
                        }
                    }
                }
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NextSong()
        {
            if (songindex != fileNames.Count - 1)
            {
                songindex++;
                MediaSlider.Value = 0;
                ListSong.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
            else
            {
                songindex = fileNames.Count - fileNames.Count;
                MediaSlider.Value = 0;
                ListSong.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
        }

        private void BackSong()
        {
            if (songindex != 0)
            {
                MediaSlider.Value = 0;
                songindex--;
                ListSong.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
            else
            {
                MediaSlider.Value = 0;
                songindex = fileNames.Count - 1;
                ListSong.SelectedIndex = songindex;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
        }
        private void RestartSong()
        {
            MediaSlider.Value = 0;
            mediaPlayer.Position = new TimeSpan(Convert.ToInt64(MediaSlider.Value));
        }

        private void MediaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            position = new TimeSpan(Convert.ToInt64(MediaSlider.Value));
            mediaPlayer.Position = new TimeSpan(Convert.ToInt64(MediaSlider.Value));
        }

        private void ListSong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListSong.Items.Count != 0)
            {
                songindex = ListSong.SelectedIndex;
                Play(songindex);
            }
        }

        private void PlayOrPause_Click(object sender, RoutedEventArgs e)
        {
            if (playOrStop == true)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void Pause()
        {
            PlayOrPause.Content = "\u23F8";
            playOrStop = false;
            position = mediaPlayer.Position;
            mediaPlayer.Stop();
            timer.Enabled = false;
        }

        private void Play()
        {
            PlayOrPause.Content = "\u25B6";
            playOrStop = true;
            mediaPlayer.Position = position;
            mediaPlayer.Play();
            timer.Enabled = true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackSong();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
        }

        private void Mix_Click(object sender, RoutedEventArgs e)
        {
            Mixer();
        }


        private void Mixer()
        {
            if (mix == false)
            {
                mix = true;
                foreach (var name in fileNames)
                {
                    copyFileNames.Add(name);
                }

                foreach (var name in listNames)
                {
                    copySongsListNames.Add(name);
                }

                int n = copyFileNames.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    (fileNames[n], fileNames[k]) = (fileNames[k], fileNames[n]);
                    (listNames[n], listNames[k]) = (listNames[k], listNames[n]);
                }
                ListSong.Items.Clear();
                foreach (var name in listNames)
                {
                    ListSong.Items.Add(name);
                }
            }
            else
            {
                mix = false;
                fileNames.Clear();
                foreach (var name in copyFileNames)
                {
                    fileNames.Add(name);
                }
                ListSong.Items.Clear();
                listNames.Clear();
                foreach (var name in copySongsListNames)
                {
                    listNames.Add(name);
                    ListSong.Items.Add(name);
                }
                copySongsListNames.Clear();
                copyFileNames.Clear();
            }
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            if (restart == false) { restart = true; } else { restart = false; }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string extensionFilter = "*.mp3|*.m4a|*.flac|*.wav";
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            infoClear();
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                folderPath = dialog.FileName;
                files = Directory.GetFiles(dialog.FileName).Where(f => extensionFilter.Contains(System.IO.Path.GetExtension(f).ToLower())).ToArray();
                foreach (var file in files)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(file);
                    string fullName = System.IO.Path.GetFileName(file);
                    listNames.Add(name);
                    fileNames.Add(fullName);
                    ListSong.Items.Add(name);
                }
                Play(0);
            }
        }
        private void Play(int index)
        {
            try
            {
                playOrStop = true;
                ListSong.SelectedIndex = index;
                mediaPlayer.Source = new Uri(folderPath + "/" + fileNames[songindex]);
                mediaPlayer.Play();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void infoClear()
        {
            folderPath = "";
            fileNames.Clear();
            listNames.Clear();
            ListSong.Items.Clear();
        }
    }
}
