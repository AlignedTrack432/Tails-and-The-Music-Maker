using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;

namespace ARE_YOU_READY_TO_ROCK_THE_KEYBOARD
{
    public partial class Form1 : Form
    {
        int isStarted = 0;
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private WaveOutEvent? waveOut;
        private Mp3FileReader? mp3Reader;
        private System.Windows.Forms.Timer positionTimer;
        string time = "";
        bool isCold = true;
        string mp3FilePath = "";
        bool isTextBox1Empty = true;
        int timestexthasbeenchanged = 0;
        public Form1()
        {
            InitializeComponent();
            this.Resize += (s, e) => CenterElements();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            positionTimer = new System.Windows.Forms.Timer();
            positionTimer.Interval = 1000; // Update every second
            positionTimer.Tick += PositionTimer_Tick!;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Restart();
            if (textBox1.Text != "")
            {
                isTextBox1Empty = false;
                button1.Text = "Start";
            }
            else if (textBox1.Text == "")
            {
                isTextBox1Empty = true;
                button1.Text = "Browse";
            }
            if (File.Exists(mp3FilePath))
            {
                timestexthasbeenchanged++;
            }
            if (timestexthasbeenchanged == 2)
            {
                timestexthasbeenchanged = 0;
                isTextBox1Empty = true;
                button1.Text = "Start";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // If the text box is empty, open the file dialog
            if (isTextBox1Empty)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                    Title = "Select a File"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    mp3FilePath = openFileDialog.FileName;
                    textBox1.Text = mp3FilePath;
                    PlayMusic(1); // Play music after selecting a file
                }
            }
            else
            {
                switch (isStarted)
                {
                    case 0:
                        PlayMusic();
                        break;
                    case 1:
                        MusicPaused();
                        break;
                    case 2:
                        MusicResumed();
                        break;
                }
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
            trackBar1.Left = (this.ClientSize.Width - trackBar1.Width) / 2;
            textBox1.Left = (this.ClientSize.Width - textBox1.Width) / 2;
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

        void PlayMusic(int isGUIPicker = 0)
        {
            try
            {
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                }
                if (isGUIPicker == 0)
                {
                    mp3Reader = new Mp3FileReader(textBox1.Text);
                }
                else if (isGUIPicker == 1)
                {
                    mp3Reader = new Mp3FileReader(mp3FilePath);
                }
                waveOut = new WaveOutEvent();
                waveOut.Init(mp3Reader);
                waveOut.Play();

                positionTimer.Start();

                if (isGUIPicker == 0)
                {
                    mp3FilePath = TrimQuotes(textBox1.Text);
                }
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
                if (File.Exists(filePath))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox1.Image = Image.FromStream(fs);
                    }
                    File.Delete(filePath);
                }
                else
                {
                    using (System.IO.Stream? stream = this.GetType().Assembly.GetManifestResourceStream("ARE_YOU_READY_TO_ROCK_THE_KEYBOARD.Untitled.png"))
                    {
                        Bitmap? bitmap = new Bitmap(stream!);
                        pictureBox1.Image = bitmap;
                    }
                }
                Mp3FileReader reader = new Mp3FileReader(textBox1.Text);
                waveOut!.Volume = 0.5F;
                TimeSpan duration = reader.TotalTime;
                time = duration.ToString(@"mm\:ss");
                int secondsInSong = duration.Seconds;
                var TagLibStuff = TagLib.File.Create(mp3FilePath);
                string Album = TagLibStuff.Tag.Album;
                string Title = TagLibStuff.Tag.Title;
                if (Album.Length > 60)
                {
                    int maxLength = 60;

                    string truncatedString = Album.Length <= maxLength ? Album : Album.Substring(0, maxLength);

                    if (Album.Length > maxLength)
                    {
                        truncatedString += "...";
                    }

                    label3.Text = truncatedString;
                }
                else if (Album.Length < 60)
                {
                    label3.Text = TagLibStuff.Tag.Album;
                }

                if (Title.Length > 60)
                {
                    int maxLength = 60;

                    string truncatedString = Title.Length <= maxLength ? Title : Title.Substring(0, maxLength);

                    if (Title.Length > maxLength)
                    {
                        truncatedString += "...";
                    }

                    label1.Text = truncatedString;
                }
                else if (Album.Length < 60)
                {
                    label1.Text = TagLibStuff.Tag.Album;
                }

                if (label1.Text == "")
                {
                    label1.Text = "Unknown Title";
                }
                if (label3.Text == "")
                {
                    label3.Text = "Unknown Album";
                }
                CenterElements();

                if (isCold == true)
                {
                    StartResizing();
                    isCold = false;
                }

                isStarted = 1;
                button1.Text = "Pause";
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred: " + ex);
            }
        }

        void FilePickerPlayMusic()
        {
            try
            {
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                }
                mp3Reader = new Mp3FileReader(mp3FilePath);
                waveOut = new WaveOutEvent();
                waveOut.Init(mp3Reader);
                waveOut.Play();

                positionTimer.Start();

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
                if (File.Exists(filePath))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox1.Image = Image.FromStream(fs);
                    }
                    File.Delete(filePath);
                }
                else
                {
                    using (System.IO.Stream? stream = this.GetType().Assembly.GetManifestResourceStream("ARE_YOU_READY_TO_ROCK_THE_KEYBOARD.Untitled.png"))
                    {
                        Bitmap? bitmap = new Bitmap(stream!);
                        pictureBox1.Image = bitmap;
                    }
                }
                Mp3FileReader reader = new Mp3FileReader(mp3FilePath);
                waveOut!.Volume = 0.5F;
                TimeSpan duration = reader.TotalTime;
                time = duration.ToString(@"mm\:ss");
                int secondsInSong = duration.Seconds;
                var TagLibStuff = TagLib.File.Create(mp3FilePath);
                label1.Text = TagLibStuff.Tag.Title;
                string heellooo = TagLibStuff.Tag.Album;
                if (heellooo.Length > 60)
                {
                    int maxLength = 60;

                    string truncatedString = heellooo.Length <= maxLength ? heellooo : heellooo.Substring(0, maxLength);

                    if (heellooo.Length > maxLength)
                    {
                        truncatedString += "...";
                    }

                    label3.Text = truncatedString;
                }
                else if (heellooo.Length < 60)
                {
                    label3.Text = TagLibStuff.Tag.Album;
                }

                if (label1.Text == "")
                {
                    label1.Text = "Unknown Title";
                }
                if (label3.Text == "")
                {
                    label3.Text = "Unknown Album";
                }
                CenterElements();

                if (isCold == true)
                {
                    StartResizing();
                    isCold = false;
                }

                isStarted = 1;
                button1.Text = "Pause";
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred: " + ex);
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
            positionTimer.Stop();
        }

        void MusicPaused()
        {
            if (waveOut != null)
                waveOut.Pause();
            isStarted = 2;
            button1.Text = "Play";
            positionTimer.Stop();
        }

        void MusicResumed()
        {
            if (waveOut != null)
                waveOut.Play();
            button1.Text = "Pause";
            isStarted = 1;
            positionTimer.Start();
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

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (waveOut != null && mp3Reader != null)
            {
                // Get the current playback time directly from mp3Reader
                TimeSpan currentTime = mp3Reader.CurrentTime;

                // Update the label with the current time
                label2.Text = currentTime.ToString(@"mm\:ss") + "/" + time;

                label2.Left = (this.ClientSize.Width - label2.Width) / 2;

                if (label2.Text == time + "/" + time)
                {
                    Restart();
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            int x = (Screen.PrimaryScreen!.WorkingArea.Width - this.Width) / 2;
            int y = (Screen.PrimaryScreen!.WorkingArea.Height - this.Height) / 2;

            this.Location = new Point(x, y);
        }

        void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the filter for file types
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            openFileDialog.Title = "Select a File";

            // Show the dialog and check if the user clicked OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file path
                mp3FilePath = openFileDialog.FileName;

                textBox1.Text = openFileDialog.FileName;

                // You can now use the file path (e.g., display it, read the file, etc.)
                FilePickerPlayMusic();
            }
        }
    }
}