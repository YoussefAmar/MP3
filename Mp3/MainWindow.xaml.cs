using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.IO;
using WMPLib;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Controls;

namespace Mp3
{
    public partial class MainWindow : Window
    {
        private WindowsMediaPlayer player = new WindowsMediaPlayer();
        private IWMPPlaylist playlist;
        private IWMPMedia media;
        private FolderBrowserDialog read = null;
        private string path;
        private int CacheSon,CacheMusique;
        private Timer timer = new Timer();
        private bool loop = false;
        private bool shuffle = false;
        private Etat save = new Etat();
        private IFormatter formatter = new BinaryFormatter();
        private Stream stream;
        private List <Display> Data = new List<Display>();
        private string ficnom = "Etat", jsondata;
        private StreamWriter wr;
        private StreamReader sr;
        private ICollectionView Collection;
        private TaskbarManager progress = TaskbarManager.Instance;

        public WindowsMediaPlayer Player { get => player; set => player = value; }
        public IWMPPlaylist Playlist { get => playlist; set => playlist = value; }
        public IWMPMedia Media { get => media; set => media = value; }
        public FolderBrowserDialog Read { get => read; set => read = value; }
        public int CacheSon1 { get => CacheSon; set => CacheSon = value; }
        public Timer Timer { get => timer; set => timer = value; }
        public bool Loop { get => loop; set => loop = value; }
        public bool Shuffle { get => shuffle; set => shuffle = value; }
        public IFormatter Formatter { get => formatter; set => formatter = value; }
        public Stream Stream { get => stream; set => stream = value; }
        public Etat Save { get => save; set => save = value; }
        public string Path { get => path; set => path = value; }
        public List<Display> Data1 { get => Data; set => Data = value; }
        public string Ficnom { get => ficnom; set => ficnom = value; }
        public string Jsondata { get => jsondata; set => jsondata = value; }
        public StreamWriter Wr { get => wr; set => wr = value; }
        public StreamReader Sr { get => sr; set => sr = value; }
        public ICollectionView Collection1 { get => Collection; set => Collection = value; }
        public TaskbarManager Progress { get => progress; set => progress = value; }
        public int CacheMusique1 { get => CacheMusique; set => CacheMusique = value; }

        public MainWindow()
        {
            InitializeComponent();
            player.MediaChange += Player_MediaChange;
            player.settings.setMode("loop", true);
            SliderSon.Value = player.settings.volume;
            CacheSon = player.settings.volume;
            LbSon.Content = player.settings.volume.ToString();
            timer.Interval = 100;
            timer.Tick += timer_Tick;
            Activer(false);
            Load();
        }

        private void Player_MediaChange(object Item)
        {
            String Track = player.currentMedia.sourceURL.Substring(media.sourceURL.LastIndexOf("\\") + 1);
            tbNom.Text = Track.Remove(Track.Length - 4);
            SliderMusique.Maximum = player.currentMedia.duration;
            SliderMusique.TickFrequency = player.currentMedia.duration / 200;

            int index;

            index = Getmedia();

                if (CacheMusique != index)
                {       
                    try
                    {
                        DgPlaylist.SelectedIndex = index;
                        DgPlaylist.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        DgPlaylist.ScrollIntoView(DgPlaylist.SelectedItem);
                    }
                    catch { }
                  
                   CacheMusique = index;
                }
        }

        private void Load()
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                sr = new StreamReader(ficnom);

                jsondata = sr.ReadToEnd();

                save = JsonConvert.DeserializeObject<Etat>(jsondata);

                sr.Close();

                Playlist_Remplir(save.PlaylistSave);

                path = save.PlaylistSave;

                SliderSon.Value = save.SonSave;

                if (save.ShuffleSave)
                {
                    shuffle = true;
                    player.settings.setMode("shuffle", shuffle);
                    BtnRandom.Content = FindResource("Randomb");
                }

                if (save.LoopSave)
                {
                    loop = true;
                    BtnLoop.Content = FindResource("Loopb");
                }

                Recherche(save.PlayerSave);

                player.controls.currentPosition = save.PositionSave;
                CacheSon =(int)save.SonSave;
                CacheMusique = save.PlayerSave;

                SliderMusique.Value = player.controls.currentPosition;
                Player_MediaChange(null);
                LbDuration.Content = "??:??" + " / " + player.currentMedia.durationString;

                player.controls.stop();
                BtnPlay.Content = FindResource("Play");
                timer.Stop();
                Activer(true);

                player.controls.currentPosition = save.PositionSave;

                try { PlaylistFocus();}
                catch { }

                Mouse.OverrideCursor = null;
            }

            catch {Mouse.OverrideCursor = null; }
        }

        private void Recherche(int info)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            do
            {
                media = player.currentPlaylist.Item[info];
                player.controls.playItem(media);
            }
            while (player.currentMedia.duration == 0);

            Mouse.OverrideCursor = null;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            Play_Pause();
        }

        private void BtnPlaylist_Click(object sender, RoutedEventArgs e)
        {

            read = new FolderBrowserDialog();

            if (read.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                player.currentPlaylist.clear();
                LbPlaylist.Content = "";

                path = read.SelectedPath;

                Playlist_Remplir(path);
            }

            else
            {
                return;
            }
        }

        private void Playlist_Remplir(string path)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            int i = 0;

            Data.Clear();
            DgPlaylist.ItemsSource = null;
            DgPlaylist.Items.Clear();
            DgPlaylist.Items.Refresh();

            playlist = player.playlistCollection.newPlaylist(path);

            foreach (string file in Directory.GetFiles(path, "*.mp3"))
            {
                media = player.newMedia(file);
                playlist.appendItem(media);
                String Track = media.sourceURL.Substring(media.sourceURL.LastIndexOf("\\") + 1);
                Data.Add(new Display(Track.Remove(Track.Length - 4), media.durationString, i));
                    
                i++;
            }

            foreach (string file in Directory.GetFiles(path, "*.wav"))
            {
                media = player.newMedia(file);
                playlist.appendItem(media);
                String Track = media.sourceURL.Substring(media.sourceURL.LastIndexOf("\\") + 1);
                Data.Add(new Display(Track.Remove(Track.Length - 4), media.durationString, i));

                i++;
            }

            Mouse.OverrideCursor = null;

            try
            {
                DgPlaylist.ItemsSource = Data;
                Collection = CollectionViewSource.GetDefaultView(Data);
                player.currentPlaylist = playlist;
                String Track = player.currentMedia.sourceURL.Substring(media.sourceURL.LastIndexOf("\\") + 1);
                tbNom.Text = Track.Remove(Track.Length - 4);
                LbPlaylist.Content = "Dossier : " + playlist.name.Substring(playlist.name.LastIndexOf("\\") + 1);
                LbNombre.Content = "Musiques : " + player.currentPlaylist.count;
                Start();
            }

            catch
            {
                System.Windows.Forms.MessageBox.Show("Veuillez ouvrir un dossier contenant des fichiers .mp3 ou .wav", "Erreur de lecture", MessageBoxButtons.OK, MessageBoxIcon.Error);
                player.currentPlaylist.clear();
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

                try { PlaylistFocus(); }
                catch { }

                try
                {
                    progress.SetProgressState(TaskbarProgressBarState.Normal);
                }
                catch{ }
            }
            else if (BtnPlay.Content == FindResource("Pause"))
            {
                BtnPlay.Content = FindResource("Play");
                timer.Stop();
                player.controls.pause();
                try { PlaylistFocus(); }
                catch { }
                progress.SetProgressState(TaskbarProgressBarState.Paused);
            }
        }

        private void SliderSon_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.settings.volume = (int)SliderSon.Value;
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
            if (player.controls.currentPosition != player.currentMedia.duration)
            {
                SliderMusique.Value = player.controls.currentPosition;
                LbDuration.Content = player.controls.currentPositionString + " / " + player.currentMedia.durationString;
                progress.SetProgressValue((int)SliderMusique.Value, (int)SliderMusique.Maximum);
            }
        }

        public void Start()
        {
            BtnPlay.Content = FindResource("Play");
            SliderMusique.Value = 0;
            Activer(true);
            BtnPlay_Click(null, null);
        }

        private void SliderMusique_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SliderMusique.Value >= player.controls.currentPosition + 0.1 || SliderMusique.Value <= player.controls.currentPosition - 0.1)
            {
                if (SliderMusique.Value + 0.19 < player.currentMedia.duration)
                {
                    player.controls.currentPosition = SliderMusique.Value;
                    LbDuration.Content = player.controls.currentPositionString + " / " + player.currentMedia.durationString;
                }
            }

            if (SliderMusique.Value + 0.19 >= player.currentMedia.duration && loop)
            {
                BtnPrevious_Click(null, null);
            }

        }

        private void BtnLoop_Click(object sender, RoutedEventArgs e)
        {
            loop = !loop;

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

            if (SliderMusique.Value <= 1)
            {
                SliderMusique.Value = 0;
                player.controls.previous();
                String Track = player.currentMedia.sourceURL.Substring(media.sourceURL.LastIndexOf("\\") + 1);
                tbNom.Text = Track.Remove(Track.Length - 4);
                LbDuration.Content = "00:00" + " / " + player.currentMedia.durationString;
            }

            else
            {
                SliderMusique.Value = 0;
                player.controls.currentPosition = 0;
                try { PlaylistFocus(); }
                catch { }
            }

        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            SliderMusique.Value = 0;
            player.controls.next();
            String Track = player.currentMedia.sourceURL.Substring(media.sourceURL.LastIndexOf("\\") + 1);
            tbNom.Text = Track.Remove(Track.Length - 4);
            LbDuration.Content = "00:00" + " / " + player.currentMedia.durationString;
        }

        private int Getmedia()
        {
            int index = 0;

            for(int i = 0; i < player.currentPlaylist.count; i++)
            {
                if(player.currentMedia.isIdentical[player.currentPlaylist.Item[i]])
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                int index = Getmedia();

                save = new Etat(index, player.controls.currentPosition, (double)player.settings.volume, path, loop, shuffle);

                wr = new StreamWriter(ficnom);

                jsondata = JsonConvert.SerializeObject(save);

                wr.Write(jsondata);

                wr.Close();

                BtnPlay.Content = FindResource("Play");
                player.controls.pause();
                timer.Stop();
            }

            catch { }

            e.Cancel = false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            tbRecherche.Text = "";
            DgPlaylist.ItemsSource = Data;

            try { PlaylistFocus(); }
            catch { }
        }

        private void tbRecherche_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Collection.Filter = w => ((Display) w).Musique.Contains(tbRecherche.Text);

            if (tbRecherche.Text != "")
            {
                DgPlaylist.ItemsSource = Collection;
            }

            else
                DgPlaylist.ItemsSource = Data;

        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (path != "")
            {
                Playlist_Remplir(path);

                BtnCancel_Click(null, null);

                player.controls.stop();
                BtnPlay.Content = FindResource("Play");

                timer.Stop();
                LbDuration.Content = "00:00" + " / " + player.currentMedia.durationString;

                PlaylistFocus();
            }

        }

        private void PlaylistFocus()
        {
            DgPlaylist.SelectedIndex = Getmedia();
            DgPlaylist.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            DgPlaylist.ScrollIntoView(DgPlaylist.SelectedItem);
        }

        private void DgPlaylist_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
           if(path !="")
            {
                if (DgPlaylist.SelectedIndex > -1)
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                    int item = (DgPlaylist.SelectedItem as Display).ID;

                    string argument = "/select, \"" + player.currentPlaylist.Item[item].sourceURL + "\"";

                    System.Diagnostics.Process.Start("explorer.exe", argument);

                    Mouse.OverrideCursor = null;

                }                   
            }

        }    

        private void DgPlaylist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgPlaylist.SelectedIndex > -1)
            {
                DependencyObject src = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);

                if ((!(src is System.Windows.Controls.Primitives.ScrollBar) && (!(src is System.Windows.Controls.Primitives.RepeatButton)) && (!(src is System.Windows.Controls.Grid)) && src.GetType() != typeof(System.Windows.Controls.Primitives.Thumb)))
                {
                    var item = (DgPlaylist.SelectedItem as Display).ID;
                    Recherche(item);
                    BtnPlay.Content = FindResource("Pause");
                    PlaylistFocus();
                    player.controls.currentPosition = 0;
                    timer.Start();
                    player.controls.play();
                }
            }
        }     
    }

    [Serializable]
    public class Etat : ISerializable
    {
        private string playlistSave;
        private int playerSave;
        private double sonSave;
        private double positionSave;
        private bool loopSave;
        private bool shuffleSave;

        public Etat()
        {
            playlistSave = null;
            playerSave = 0;
            sonSave = 0;
            positionSave = 0;
            loopSave = false;
            shuffleSave = false;
        }

        public Etat(int p, double po, double so, string pl, bool l, bool s)
        {
            playlistSave = pl;
            playerSave = p;
            positionSave = po;
            sonSave = so;
            loopSave = l;
            shuffleSave = s;
        }

        public Etat(SerializationInfo info, StreamingContext context)
        {
            this.playlistSave = (string)info.GetValue("playlist", typeof(string));
            this.playerSave = (int)info.GetValue("media", typeof(int));
            this.positionSave = (double)info.GetValue("position", typeof(double));
            this.sonSave = (double)info.GetValue("son", typeof(double));
            this.loopSave = (bool)info.GetValue("loop", typeof(bool));
            this.shuffleSave = (bool)info.GetValue("shuffle", typeof(bool));
        }

        public int PlayerSave { get => playerSave; set => playerSave = value; }
        public bool LoopSave { get => loopSave; set => loopSave = value; }
        public bool ShuffleSave { get => shuffleSave; set => shuffleSave = value; }
        public string PlaylistSave { get => playlistSave; set => playlistSave = value; }
        public double PositionSave { get => positionSave; set => positionSave = value; }
        public double SonSave { get => sonSave; set => sonSave = value; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("playlist", playlistSave, typeof(string));
            info.AddValue("media", playerSave, typeof(int));
            info.AddValue("position", positionSave, typeof(double));
            info.AddValue("son", sonSave, typeof(double));
            info.AddValue("loop", loopSave, typeof(bool));
            info.AddValue("shuffle", shuffleSave, typeof(bool));
        }
    }

    public class Display
    {
        private string Nom;
        private string Duration;
        private int Index;

        public Display(string nom, string duration, int index)
        {
            this.Nom = nom;
            this.Duration = duration;
            this.Index = index;
        }
        
        public string Musique { get => Nom; set => Nom = value; }
        public string Temps { get => Duration; set => Duration = value; }
        public int ID { get => Index; set => Index = value; }
    }

}