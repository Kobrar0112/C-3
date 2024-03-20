using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private bool isRepeatEnabled = false;
        private bool isShuffleEnabled = false;
        private List<string> playlist;
        private int currentTrackIndex = 0;
        private Random random;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
        }

        private async void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaElement.Source != null) && (mediaElement.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                positionSlider.Minimum = 0;
                positionSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                positionSlider.Value = mediaElement.Position.TotalSeconds;

                durationTextBlock.Text = mediaElement.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                currentTimeTextBlock.Text = mediaElement.Position.ToString(@"mm\:ss");

                await Task.Delay(1); 
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg)|*.mp3;*.mpg;*.mpeg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                playlist = new List<string>(openFileDialog.FileNames);
                currentTrackIndex = 0;
                PlayCurrentTrack();
            }
        }

        private void PlayCurrentTrack()
        {
            if (playlist.Count > 0)
            {
                mediaElement.Source = new Uri(playlist[currentTrackIndex]);
                mediaElement.Play();
                mediaPlayerIsPlaying = true;
                timer.Start();
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                mediaElement.Pause();
                mediaPlayerIsPlaying = false;
                timer.Stop();
            }
            else
            {
                mediaElement.Play();
                mediaPlayerIsPlaying = true;
                timer.Start();
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrackIndex > 0)
            {
                currentTrackIndex--;
                PlayCurrentTrack();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void PlayNextTrack()
        {
            if (currentTrackIndex < playlist.Count - 1)
            {
                currentTrackIndex++;
                PlayCurrentTrack();
            }
            else if (isRepeatEnabled)
            {
                currentTrackIndex = 0;
                PlayCurrentTrack();
            }
            else
            {
                mediaPlayerIsPlaying = false;
                timer.Stop();
            }
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            isRepeatEnabled = !isRepeatEnabled;
            UpdateRepeatButtonVisual();
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            isShuffleEnabled = !isShuffleEnabled;
            UpdateShuffleButtonVisual();

            if (isShuffleEnabled)
            {
                random = new Random();
                playlist = playlist.OrderBy(x => random.Next()).ToList();
            }
            else
            {
                playlist = playlist.OrderBy(x => x).ToList();
            }
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!userIsDraggingSlider)
            {
                mediaElement.Position = TimeSpan.FromSeconds(positionSlider.Value);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
            volumeTextBlock.Text = $"{(int)(volumeSlider.Value * 100)}%";
        }

        private void UpdateRepeatButtonVisual()
        {
            RepeatButton.Content = isRepeatEnabled ? "Выключить повтор" : "Включить повтор";
        }

        private void UpdateShuffleButtonVisual()
        {
            ShuffleButton.Content = isShuffleEnabled ? "Выключить перемешивание" : "Включить перемешивание";
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }
    }
}