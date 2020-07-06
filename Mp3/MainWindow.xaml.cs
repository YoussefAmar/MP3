using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using WMPLib;

namespace Mp3
{
    public partial class MainWindow : Window
    {
        private WindowsMediaPlayer player = new WindowsMediaPlayer();
        private OpenFileDialog read = null;
        private int CacheSon;
        private Timer timer = new Timer();
        private bool loop = false;
        private bool shuffle = false;

        public MainWindow()
        {
            InitializeComponent();
            SliderSon.Value = player.settings.volume;
            CacheSon = player.settings.volume;
            LbSon.Content = player.settings.volume.ToString();
            timer.Interval = 100;
            timer.Tick += timer_Tick;
            Activer(false);
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {           
                Play_Pause();           
        }

        private void BtnMusique_Click(object sender, RoutedEventArgs e)
        {         

        }

        private void BtnPlaylist_Click(object sender, RoutedEventArgs e)
        {
            read = new OpenFileDialog();
            read.Filter = "wav ou mp3 (*.wav,*.mp3)|*.wav;*.mp3;";

            if (read.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            else
            {
                player.URL = read.FileName;
                tbNom.Text = read.FileName.Substring(read.FileName.LastIndexOf("\\") + 1);
                tbNom.Text = tbNom.Text.Remove(tbNom.Text.Length - 4, 4);
                Start();
            }
        }

        private void Activer(bool actif)
        {
            SliderMusique.IsEnabled = BtnLoop.IsEnabled = BtnPlay.IsEnabled = BtnSound.IsEnabled = SliderSon.IsEnabled = BtnRandom.IsEnabled = 
            BtnNext.IsEnabled = BtnPrevious.IsEnabled = actif;
        }

        private void Play_Pause()
        {
            if (BtnPlay.Content == FindResource("Play"))
            {
                BtnPlay.Content = FindResource("Pause");
                timer.Start();
                player.controls.play();              
 
            }
            else if(BtnPlay.Content == FindResource("Pause"))
            {
                BtnPlay.Content = FindResource("Play");
                timer.Stop();
                player.controls.pause();
            }             
        }

        private void SliderSon_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.settings.volume =(int) SliderSon.Value;
            LbSon.Content = player.settings.volume.ToString();
            
            if (player.settings.volume == 0)
            {
                BtnSound.Content = FindResource("Mute");
            }

            else
            {
                BtnSound.Content = FindResource("Sound");
                CacheSon = player.settings.volume;
            }
        }

        private void BtnSound_Click(object sender, RoutedEventArgs e)
        {

            if (BtnSound.Content == FindResource("Sound"))
            {
                BtnSound.Content = FindResource("Mute");
                CacheSon = player.settings.volume;
                player.settings.volume = 0;
                SliderSon.Value = player.settings.volume;
                LbSon.Content = player.settings.volume.ToString();
            }
            else if (BtnSound.Content == FindResource("Mute"))
            {
                BtnSound.Content = FindResource("Sound");
                player.settings.volume = CacheSon;
                SliderSon.Value = player.settings.volume;
            }
        }

        private void timer_Tick(object sender, object e)
        {
            if(SliderMusique.Value == 0)
            {
                SliderMusique.Maximum = player.currentMedia.duration;
                SliderMusique.TickFrequency = player.currentMedia.duration / 200;
                LbDuration.Content = player.controls.currentPositionString + " / " + player.currentMedia.durationString;
            }

            if (player.controls.currentPosition != player.currentMedia.duration)
            {             
                SliderMusique.Value = player.controls.currentPosition;
                LbDuration.Content = player.controls.currentPositionString + " / " + player.currentMedia.durationString;
            }

            if(player.playState == WMPPlayState.wmppsStopped)
            {
                LbDuration.Content = player.currentMedia.durationString + " / " + player.currentMedia.durationString;
                timer.Stop();
                Stop();
            }
        }

        public void Start()
        {
            BtnPlay.Content = FindResource("Play");
            SliderMusique.Value = 0;
            Activer(true);
            BtnPlay_Click(null, null);                
        }

        public void Stop()
        {
            BtnPlay.Content = FindResource("Pause");
            BtnPlay_Click(null, null);
            LbDuration.Content = "00:00"+ " / " + player.currentMedia.durationString;
        }

        private void SliderMusique_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if(SliderMusique.Value >= player.controls.currentPosition + 0.1 || SliderMusique.Value <= player.controls.currentPosition - 0.1)
            {
                player.controls.currentPosition = SliderMusique.Value;
                LbDuration.Content = player.controls.currentPositionString + " / " + player.currentMedia.durationString;
            }           
        }

        private void BtnLoop_Click(object sender, RoutedEventArgs e)
        {
            loop = !loop;

            player.settings.setMode("loop",loop);

            if (loop)

                BtnLoop.Content = FindResource("Loopb");

            else
                BtnLoop.Content = FindResource("Loopa");

        }

        private void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            shuffle = !shuffle;

            player.settings.setMode("shuffle", shuffle);

            if (shuffle)

                BtnRandom.Content = FindResource("Randomb");

            else
                BtnRandom.Content = FindResource("Randoma");
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            player.controls.currentPosition = 0; //ajouter song avant
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            player.controls.currentPosition = player.currentMedia.duration - 0.1; //ajouter song après
        }
    }
}
