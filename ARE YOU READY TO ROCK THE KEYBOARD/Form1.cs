using NAudio.Wave;

namespace ARE_YOU_READY_TO_ROCK_THE_KEYBOARD
{
    public partial class Form1 : Form
    {
        int isStarted = 0;
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private WaveOutEvent? waveOut;
        private Mp3FileReader? mp3Reader;

        public Form1()
        {
        InitializeComponent();
            this.Resize += (s, e) => CenterElements();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Restart();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isStarted == 0)
            {
                PlayMusic();
            }
            else if (isStarted == 1)
            {
                MusicPaused();
            }
            else if (isStarted == 2)
            {
                MusicResumed();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            AdjustVolume();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            trackBar1.Value = 50;
            this.Height = 110;
            CenterElements();
        }

        private void CenterElements()
        {
            pictureBox1.Left = (this.ClientSize.Width - pictureBox1.Width) / 2;
            button1.Left = (this.ClientSize.Width - button1.Width) / 2;
            label1.Left = (this.ClientSize.Width - label1.Width) / 2;
            label2.Left = (this.ClientSize.Width - label2.Width) / 2;
            label3.Left = (this.ClientSize.Width - label3.Width) / 2;
            textBox1.Left = (this.ClientSize.Width - textBox1.Width) / 2;
            trackBar1.Left = (this.ClientSize.Width - trackBar1.Width) / 2;
        }
        private void StartResizing()
        {
            int[] heights = { 160, 210, 260, 310, 340, 380, 395 };
            int[] delays = { 50, 30, 20, 15, 10, 5, 0 };

            for (int i = 0; i < heights.Length; i++)
            {
                this.Height = heights[i];

                if (i < heights.Length - 1)
                {
                    ResizingHelper(delays[i]);
                }
            }
        }

        void PlayMusic()
        {
            try
            {
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                }
                mp3Reader = new Mp3FileReader(textBox1.Text);
                waveOut = new WaveOutEvent();
                waveOut.Init(mp3Reader);
                waveOut.Play();

                string mp3FilePath = TrimQuotes(textBox1.Text);
                textBox1.Text = mp3FilePath;

                string filePath = Path.Combine(Path.GetTempPath(), "MP3PlayerAlbumArtTemp.png");
                var file = TagLib.File.Create(mp3FilePath);
                if (file.Tag.Pictures.Length > 0)
                {
                    var bin = (byte[])(file.Tag.Pictures[0].Data.Data);
                    using (var ms = new MemoryStream(bin))
                    {
                        var image = Image.FromStream(ms);
                        image.Save(filePath);
                    }
                }
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    pictureBox1.Image = Image.FromStream(fs);
                }
                Mp3FileReader reader = new Mp3FileReader(textBox1.Text);
                TimeSpan duration = reader.TotalTime;
                string time = duration.ToString(@"mm\:ss");
                label2.Text = time;
                int secondsInSong = duration.Seconds;
                var TagLibStuff = TagLib.File.Create(mp3FilePath);
                label1.Text = TagLibStuff.Tag.Title;
                label3.Text = TagLibStuff.Tag.Album;
                CenterElements();
                StartResizing();

                isStarted = 1;
                button1.Text = "Pause";
                waveOut.Volume = 0.5F;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured: " + ex);
            }
        }

        void Restart()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
            }

            if (mp3Reader != null)
            {
                mp3Reader.Dispose();
            }

            string mp3FilePath = TrimQuotes(textBox1.Text);
            textBox1.Text = mp3FilePath;
            isStarted = 0;
            button1.Text = "Start";
        }

        void MusicPaused()
        {
            if (waveOut != null)
                waveOut.Pause();
            isStarted = 2;
            button1.Text = "Play";
        }

        void MusicResumed()
        {
            if (waveOut != null)
                waveOut.Play();
            button1.Text = "Pause";
            isStarted = 1;
        }

        void AdjustVolume()
        {
            float volume = trackBar1.Value / 100f;
            waveOut!.Volume = volume;
        }

        private string TrimQuotes(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 2)
                return input;

            if (input[0] == '"' && input[input.Length - 1] == '"')
                return input.Substring(1, input.Length - 2);

            return input;
        }

        void ResizingHelper(int milliseconds)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < milliseconds)
            {
                Application.DoEvents();
                Thread.Sleep(5); // Very short sleep
            }
        }
    }
}