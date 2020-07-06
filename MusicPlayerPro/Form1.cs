using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MusicPlayerPro
{
	public partial class Form1 : Form
	{
		public List<string> Queue = new List<string>();
		public static bool IsTrackOverlayVisible = false;

		public WaveOut currentSong;
		public Mp3FileReader reader;
		public Timer seekbarHandler;
		public bool Active = false;

		public ThumbnailButton prevBtn, nextBtn, playBtn;

		public float Volume = 1.0f;
		/// <summary>
		/// 0 = No shuffle; 1 = Random Shuffle; 2 = Smart Shuffle
		/// </summary>
		public int ShuffleType = 0;
		/// <summary>
		/// 0 = No loop; 1 = Loop (End of queue); 2 = Loop Once
		/// </summary>
		public int LoopType = 0;
		public int QueueIndex = 0;

		CommonOpenFileDialog dialog;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//TODO: Load previous queue
			//TODO: Load preferences
			ReAlignControls();

			miniSongName.Text = "";
			TrackName.Text = "";
			Album.Text = "";
			Artist.Text = "";
			antialiasedLabel1.Text = "";

			//Get icons
			Icon Play, Prev, Next;
			Play = Properties.Resources.playIcon;
			Prev = Properties.Resources.previousTrackIcon;
			Next = Properties.Resources.nextTrackIcon;

			//Create the buttons
			prevBtn = new ThumbnailButton(Prev, "Previous Track");
			nextBtn = new ThumbnailButton(Next, "Next Track");
			playBtn = new ThumbnailButton(Play, "Play");

			//Disable buttons
			prevBtn.Enabled = false;
			nextBtn.Enabled = false;
			playBtn.Enabled = false;

			//Add handlers to the buttons
			prevBtn.Click += previousTrack_Click;
			nextBtn.Click += nextTrack_Click;
			playBtn.Click += play_Click;

			dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = true;
			dialog.RestoreDirectory = true;

			TaskbarHelper.Instance.AddThumbnailButtons(this.Handle, prevBtn, playBtn, nextBtn);
		}

		#region Form Handling

		private void Form1_Activated(object sender, EventArgs e)
		{
			Active = true;
		}

		private void Form1_Deactivate(object sender, EventArgs e)
		{
			Active = false;
		}

		private void loadMP3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (mp3Finder.ShowDialog() == DialogResult.OK)
			{
				string[] selected = mp3Finder.FileNames;
				Queue.AddRange(selected);
				QueueIndex = Queue.Count - 1;
				Shuffle();
				PlaySong(true);
			}
		}

		private void loadFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				string folder = dialog.FileName;
				string[] selected = Directory.GetFiles(folder, "*.mp3");
				Queue.AddRange(selected);
				if (Queue.Count > 0)
				{
					QueueIndex = Utils.Clamp(Queue.Count - 1, 0, int.MaxValue);
					Shuffle();
					PlaySong(true);
				}
			}
		}

		private void homeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tablessControl1.SelectedIndex = 0;
		}

		private void playerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tablessControl1.SelectedIndex = 1;
		}

		private void queueToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tablessControl1.SelectedIndex = 2;
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tablessControl1.SelectedIndex = 3;
		}

		public void ReAlignControls()
		{
			Point TopLeftMostPosition = new Point(Math.Max(splitContainer2.Size.Width / 2 - 104, TrackName.Size.Width + 10), 2);

			loop.Location = TopLeftMostPosition;
			shuffle.Location = new Point(TopLeftMostPosition.X + 38, TopLeftMostPosition.Y);

			separator.Location = new Point(TopLeftMostPosition.X + 86, TopLeftMostPosition.Y);

			previousTrack.Location = new Point(TopLeftMostPosition.X + 101, TopLeftMostPosition.Y);
			play.Location = new Point(TopLeftMostPosition.X + 139, TopLeftMostPosition.Y);
			nextTrack.Location = new Point(TopLeftMostPosition.X + 177, TopLeftMostPosition.Y);

			TopLeftMostPosition = new Point(Utils.Clamp(Math.Max(splitContainer1.Size.Width / 2 - 104, miniSongName.Size.Width + 10), 0, splitContainer2.Size.Width - volumeSlider1.Size.Width), 2);

			pictureBox4.Location = TopLeftMostPosition;
			pictureBox3.Location = new Point(TopLeftMostPosition.X + 38, TopLeftMostPosition.Y);

			pictureBox2.Location = new Point(TopLeftMostPosition.X + 86, TopLeftMostPosition.Y);

			pictureBox7.Location = new Point(TopLeftMostPosition.X + 101, TopLeftMostPosition.Y);
			pictureBox6.Location = new Point(TopLeftMostPosition.X + 139, TopLeftMostPosition.Y);
			pictureBox5.Location = new Point(TopLeftMostPosition.X + 177, TopLeftMostPosition.Y);

			TopLeftMostPosition = new Point(Utils.Clamp(Math.Max(splitContainer3.Size.Width / 2 - 104, miniSongName.Size.Width + 10), 0, splitContainer2.Size.Width - volumeSlider1.Size.Width), 2);

			pictureBox9.Location = TopLeftMostPosition;
			pictureBox8.Location = new Point(TopLeftMostPosition.X + 38, TopLeftMostPosition.Y);

			pictureBox1.Location = new Point(TopLeftMostPosition.X + 86, TopLeftMostPosition.Y);

			pictureBox12.Location = new Point(TopLeftMostPosition.X + 101, TopLeftMostPosition.Y);
			pictureBox11.Location = new Point(TopLeftMostPosition.X + 139, TopLeftMostPosition.Y);
			pictureBox10.Location = new Point(TopLeftMostPosition.X + 177, TopLeftMostPosition.Y);

			Artist.Location = new Point(Album.Location.X + Album.Size.Width + 5, Album.Location.Y);
		}

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			ReAlignControls();
		}
		#endregion

		#region Playback
		public void PlaySong(bool changeView)
		{
			//Get the file from queue index, and raise a play mp3 event
			PlayMP3(Queue[QueueIndex], changeView);
		}

		/// <summary>
		/// This method handles the backend (updating values, playing the audio, etc.)
		/// </summary>
		/// <param name="file"></param>
		/// <param name="changeView"></param>
		public void PlayMP3(string file, bool changeView)
		{
			if (changeView)
			{
				//Set the view to the player
				playerToolStripMenuItem_Click(null, null);
			}

			//Set the details
			TagLib.File mp3File = TagLibSharpUtils.OpenMp3File(file);

			playerArt.Image = TagLibSharpUtils.GetCoverArt(mp3File);

			TrackName.Text = mp3File.Tag.Title;
			Album.Text = mp3File.Tag.Album;
			Artist.Text = mp3File.Tag.FirstPerformer;

			miniSongName.Text = mp3File.Tag.Title;
			miniAlbumArt.Image = playerArt.Image;

			antialiasedLabel1.Text = mp3File.Tag.Title;
			QminiAlbumArt.Image = playerArt.Image;

			//Set the title
			Text = mp3File.Tag.FirstPerformer + " - " + mp3File.Tag.Title + " - Music Player Pro";

			//Play the mp3
			if (reader != null)
			{
				reader.Dispose();
			}

			//MP3 Reader
			reader = new Mp3FileReader(file);
			if (currentSong == null || currentSong.PlaybackState == PlaybackState.Playing || currentSong.PlaybackState == PlaybackState.Paused)
			{
				if (currentSong != null)
				{
					//Reset looping temporarily, we don't want to initial a loop if we were told to load a song
					int tmp = LoopType;
					LoopType = 0;

					currentSong.PlaybackStopped -= OnPlaybackStopped;

					currentSong.Stop();
					currentSong.Dispose();

					LoopType = tmp;
				}
				currentSong = new WaveOut();
			}

			//Seekbar handler
			if (seekbarHandler != null)
			{
				seekbarHandler.Stop();
			}
			else
			{
				seekbarHandler = new Timer();
				seekbarHandler.Interval = 100;
				seekbarHandler.Tick += Timer_Elapsed;
			}

			currentSong.Init(reader);
			currentSong.Play();
			currentSong.Volume = Volume;

			//Seeker
			metroTrackBar1.Value = 0;
			metroTrackBar1.Maximum = (int)(reader.TotalTime.TotalSeconds * 10);
			metroTrackBar1.LeftText = "00:00";
			metroTrackBar1.RightText = reader.TotalTime.ToString(@"mm\:ss");

			metroTrackBar2.Value = 0;
			metroTrackBar2.Maximum = (int)(reader.TotalTime.TotalSeconds * 10);
			metroTrackBar2.LeftText = "00:00";
			metroTrackBar2.RightText = reader.TotalTime.ToString(@"mm\:ss");

			metroTrackBar3.Value = 0;
			metroTrackBar3.Maximum = (int)(reader.TotalTime.TotalSeconds * 10);
			metroTrackBar3.LeftText = "00:00";
			metroTrackBar3.RightText = reader.TotalTime.ToString(@"mm\:ss");

			currentSong.PlaybackStopped += OnPlaybackStopped;
			seekbarHandler.Start();

			ReAlignControls();

			playBtn.Enabled = true;
			play.Image = Properties.Resources.Pause;
			playBtn.Icon = Properties.Resources.pauseIcon;
			playBtn.Tip = "Pause";
			tooltip.SetToolTip(play, "Pause");

			pictureBox6.Image = play.Image;
			pictureBox11.Image = play.Image;
			tooltip.SetToolTip(pictureBox6, tooltip.GetToolTip(play));
			tooltip.SetToolTip(pictureBox11, tooltip.GetToolTip(play));

			if (WindowState == FormWindowState.Minimized || !Active)
			{
				if (IsTrackOverlayVisible == false)
				{
					IsTrackOverlayVisible = true;
					new NewTrackOverlay(playerArt.Image, TrackName.Text, Artist.Text, Album.Text).Show();
				}
			}
		}

		/// <summary>
		/// Called when MP3 playback ends. We handle looping here
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPlaybackStopped(object sender, StoppedEventArgs e)
		{
			//Increase the queue index by 1 if we aren't looping once
			if (LoopType == 1)
			{
				//End of queue
				QueueIndex++;
				if (QueueIndex == Queue.Count)
				{
					//End of queue. Reshuffle
					Shuffle();
					QueueIndex = 0;
				}

				//Enable the buttons
				prevBtn.Enabled = true;
				nextBtn.Enabled = true;
				playBtn.Enabled = true;

				PlaySong(false);
			}
			else if (LoopType == 2)
			{
				//Enable the buttons
				prevBtn.Enabled = true;
				nextBtn.Enabled = true;
				playBtn.Enabled = true;

				//Loop once
				PlaySong(false);
			}
			else
			{
				//Reset state
				playerArt.Image = null;
				miniAlbumArt.Image = null;
				QminiAlbumArt.Image = null;

				TrackName.Text = "";
				Album.Text = "";
				Artist.Text = "";
				miniSongName.Text = "";
				antialiasedLabel1.Text = "";

				Text = "Music Player Pro";

				//End playback
				if (seekbarHandler != null)
				{
					seekbarHandler.Stop();
					seekbarHandler.Dispose();
					seekbarHandler = null;
				}

				//Disable the buttons
				prevBtn.Enabled = Queue.Count != 0;
				nextBtn.Enabled = Queue.Count != 0;
				playBtn.Enabled = false;

				//Reset trackbars
				metroTrackBar1.LeftText = "--:--";
				metroTrackBar1.RightText = "--:--";
				metroTrackBar2.LeftText = "--:--";
				metroTrackBar2.RightText = "--:--";
				metroTrackBar3.LeftText = "--:--";
				metroTrackBar3.RightText = "--:--";

				metroTrackBar1.Value = 0;
				metroTrackBar2.Value = 0;
				metroTrackBar3.Value = 0;

				//Pause the button
				playBtn.Icon = Properties.Resources.pauseIcon;
				playBtn.Tip = "Pause";
				tooltip.SetToolTip(play, "Pause");

				play.Image = Properties.Resources.Pause;
				playBtn.Icon = Properties.Resources.pauseIcon;
				playBtn.Tip = "Pause";
				tooltip.SetToolTip(play, "Pause");

				pictureBox6.Image = play.Image;
				pictureBox11.Image = play.Image;
				tooltip.SetToolTip(pictureBox6, tooltip.GetToolTip(play));
				tooltip.SetToolTip(pictureBox11, tooltip.GetToolTip(play));
			}
		}
		#endregion

		#region Interface Interaction
		private void volumeSlider3_Scroll(object sender, ScrollEventArgs e)
		{
			Volume = volumeSlider3.Value / 100f;
			volumeSlider3.LeftText = (Volume * 100).ToString();
			volumeSlider3.RightText = "100";

			volumeSlider1.Value = volumeSlider3.Value;
			volumeSlider1.LeftText = volumeSlider3.LeftText;
			volumeSlider1.RightText = volumeSlider3.RightText;

			volumeSlider2.Value = volumeSlider3.Value;
			volumeSlider2.LeftText = volumeSlider3.LeftText;
			volumeSlider2.RightText = volumeSlider3.RightText;

			if (currentSong != null && (currentSong.PlaybackState == PlaybackState.Playing || currentSong.PlaybackState == PlaybackState.Paused))
			{
				currentSong.Volume = Volume;
			}
		}

		private void volumeSlider2_Scroll(object sender, ScrollEventArgs e)
		{
			Volume = volumeSlider2.Value / 100f;
			volumeSlider2.LeftText = (Volume * 100).ToString();
			volumeSlider2.RightText = "100";

			volumeSlider1.Value = volumeSlider2.Value;
			volumeSlider1.LeftText = volumeSlider2.LeftText;
			volumeSlider1.RightText = volumeSlider2.RightText;

			volumeSlider3.Value = volumeSlider2.Value;
			volumeSlider3.LeftText = volumeSlider2.LeftText;
			volumeSlider3.RightText = volumeSlider2.RightText;

			if (currentSong != null && (currentSong.PlaybackState == PlaybackState.Playing || currentSong.PlaybackState == PlaybackState.Paused))
			{
				currentSong.Volume = Volume;
			}
		}

		private void volumeSlider1_Scroll(object sender, ScrollEventArgs e)
		{
			Volume = volumeSlider1.Value / 100f;
			volumeSlider1.LeftText = (Volume * 100).ToString();
			volumeSlider1.RightText = "100";

			volumeSlider2.Value = volumeSlider1.Value;
			volumeSlider2.LeftText = volumeSlider1.LeftText;
			volumeSlider2.RightText = volumeSlider1.RightText;

			volumeSlider3.Value = volumeSlider1.Value;
			volumeSlider3.LeftText = volumeSlider1.LeftText;
			volumeSlider3.RightText = volumeSlider1.RightText;

			if (currentSong != null && (currentSong.PlaybackState == PlaybackState.Playing || currentSong.PlaybackState == PlaybackState.Paused))
			{
				currentSong.Volume = Volume;
			}
		}

		private void play_Click(object sender, EventArgs e)
		{
			if (currentSong != null)
			{
				if (currentSong.PlaybackState == PlaybackState.Playing)
				{
					currentSong.Pause();
					play.Image = Properties.Resources.Play;
					playBtn.Icon = Properties.Resources.playIcon;
					playBtn.Tip = "Play";
					tooltip.SetToolTip(play, "Play");
				}
				else if (currentSong.PlaybackState == PlaybackState.Paused)
				{
					currentSong.Play();
					play.Image = Properties.Resources.Pause;
					playBtn.Icon = Properties.Resources.pauseIcon;
					playBtn.Tip = "Pause";
					tooltip.SetToolTip(play, "Pause");
				}
			}
			pictureBox6.Image = play.Image;
			pictureBox11.Image = play.Image;
			tooltip.SetToolTip(pictureBox6, tooltip.GetToolTip(play));
			tooltip.SetToolTip(pictureBox11, tooltip.GetToolTip(play));
		}

		private void loop_Click(object sender, EventArgs e)
		{
			LoopType++;
			if (LoopType > 2)
			{
				LoopType = 0;
			}

			//Update image
			switch (LoopType)
			{
				case 0:
					loop.Image = Properties.Resources.LoopOff;
					tooltip.SetToolTip(loop, "Loop");

					//Enable the buttons
					prevBtn.Enabled = true;
					nextBtn.Enabled = true;

					break;
				case 1:
					loop.Image = Properties.Resources.LoopOn;
					tooltip.SetToolTip(loop, "Loop Once");

					//Enable the buttons
					prevBtn.Enabled = false;
					nextBtn.Enabled = false;

					break;
				case 2:
					loop.Image = Properties.Resources.LoopOnce;
					tooltip.SetToolTip(loop, "Disable Looping");

					//Enable the buttons
					prevBtn.Enabled = false;
					nextBtn.Enabled = false;

					break;
			}
			pictureBox4.Image = loop.Image;
			pictureBox9.Image = loop.Image;
			tooltip.SetToolTip(pictureBox4, tooltip.GetToolTip(loop));
			tooltip.SetToolTip(pictureBox9, tooltip.GetToolTip(loop));
		}

		private void shuffle_Click(object sender, EventArgs e)
		{
			ShuffleType++;
			if (ShuffleType > 2)
			{
				ShuffleType = 0;
			}

			//Update image
			switch (ShuffleType)
			{
				case 0:
					shuffle.Image = Properties.Resources.shuffleOff;
					tooltip.SetToolTip(shuffle, "Basic Shuffle");
					break;
				case 1:
					shuffle.Image = Properties.Resources.ShuffleOn;
					tooltip.SetToolTip(shuffle, "Smart Shuffle");
					break;
				case 2:
					shuffle.Image = Properties.Resources.SmartShuffle;
					tooltip.SetToolTip(shuffle, "Disable Shuffle");
					break;
			}
			pictureBox3.Image = shuffle.Image;
			pictureBox8.Image = shuffle.Image;
			tooltip.SetToolTip(pictureBox3, tooltip.GetToolTip(shuffle));
			tooltip.SetToolTip(pictureBox8, tooltip.GetToolTip(shuffle));
		}
		private void metroTrackBar1_Scroll(object sender, ScrollEventArgs e)
		{
			if (currentSong != null && reader != null)
			{
				TimeSpan toSeek = TimeSpan.FromSeconds(metroTrackBar1.Value / 10D);
				reader.CurrentTime = toSeek;

				metroTrackBar1.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar1.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar2.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar2.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar3.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar3.RightText = reader.TotalTime.ToString(@"mm\:ss");
			}
		}

		private void nextTrack_Click(object sender, EventArgs e)
		{
			QueueIndex++;
			if (QueueIndex == Queue.Count)
			{
				//End of queue. Reshuffle
				Shuffle();
				QueueIndex = 0;
			}
			PlaySong(false);
			pictureBox5.Image = nextTrack.Image;
			pictureBox10.Image = nextTrack.Image;
		}

		private void previousTrack_Click(object sender, EventArgs e)
		{
			QueueIndex--;
			if (QueueIndex < 0)
			{
				//End of queue. Reshuffle
				Shuffle();
				QueueIndex = Queue.Count - 1;
			}
			PlaySong(false);
			pictureBox7.Image = previousTrack.Image;
			pictureBox12.Image = previousTrack.Image;
		}

		private void pictureBox5_Click(object sender, EventArgs e)
		{
			nextTrack_Click(null, null);
		}

		private void pictureBox7_Click(object sender, EventArgs e)
		{
			previousTrack_Click(null, null);
		}

		private void pictureBox6_Click(object sender, EventArgs e)
		{
			play_Click(null, null);
		}

		private void pictureBox3_Click(object sender, EventArgs e)
		{
			shuffle_Click(null, null);
		}

		private void pictureBox4_Click(object sender, EventArgs e)
		{
			loop_Click(null, null);
		}

		private void pictureBox9_Click(object sender, EventArgs e)
		{
			loop_Click(null, null);
		}

		private void pictureBox8_Click(object sender, EventArgs e)
		{
			shuffle_Click(null, null);
		}

		private void pictureBox10_Click(object sender, EventArgs e)
		{
			nextTrack_Click(null, null);
		}

		private void pictureBox11_Click(object sender, EventArgs e)
		{
			play_Click(null, null);
		}

		private void pictureBox12_Click(object sender, EventArgs e)
		{
			previousTrack_Click(null, null);
		}

		private void metroTrackBar2_Scroll(object sender, ScrollEventArgs e)
		{
			if (currentSong != null && reader != null)
			{
				TimeSpan toSeek = TimeSpan.FromSeconds(metroTrackBar2.Value / 10D);
				reader.CurrentTime = toSeek;

				metroTrackBar1.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar1.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar2.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar2.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar3.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar3.RightText = reader.TotalTime.ToString(@"mm\:ss");
			}
		}

		private void metroTrackBar3_Scroll(object sender, ScrollEventArgs e)
		{
			if (currentSong != null && reader != null)
			{
				TimeSpan toSeek = TimeSpan.FromSeconds(metroTrackBar3.Value / 10D);
				reader.CurrentTime = toSeek;

				metroTrackBar1.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar1.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar2.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar2.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar3.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar3.RightText = reader.TotalTime.ToString(@"mm\:ss");
			}
		}

		private void UpdateSeekBar()
		{
			if (currentSong.PlaybackState == PlaybackState.Playing)
			{
				metroTrackBar1.Value = Utils.Clamp((int)(reader.CurrentTime.TotalSeconds * 10), 0, metroTrackBar1.Maximum);
				metroTrackBar1.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar1.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar2.Value = Utils.Clamp((int)(reader.CurrentTime.TotalSeconds * 10), 0, metroTrackBar1.Maximum);
				metroTrackBar2.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar2.RightText = reader.TotalTime.ToString(@"mm\:ss");

				metroTrackBar3.Value = Utils.Clamp((int)(reader.CurrentTime.TotalSeconds * 10), 0, metroTrackBar1.Maximum);
				metroTrackBar3.LeftText = reader.CurrentTime.ToString(@"mm\:ss");
				metroTrackBar3.RightText = reader.TotalTime.ToString(@"mm\:ss");
			}
		}

		private void Timer_Elapsed(object sender, EventArgs e)
		{
			UpdateSeekBar();
		}
		#endregion

		public void Shuffle()
		{
			if (ShuffleType == 1)
			{
				Queue.Shuffle();
			}
			else if (ShuffleType == 2)
			{
				//TODO: Smart Shuffle Algorithm
			}
		}

		#region Keyboard Shortcuts
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			//Handle keyboard shortcuts
			if (e != null)
			{
				//Alt + F4
				if (ModifierKeys == (Keys.Alt) && e.KeyCode == Keys.F4)
				{
					Close();
				}

				//File Menustrip
				if (ModifierKeys == (Keys.Control) && e.KeyCode == Keys.O)
				{
					loadMP3ToolStripMenuItem_Click(null, null);
				}
				if (ModifierKeys == (Keys.Control) && e.KeyCode == Keys.F)
				{
					loadFolderToolStripMenuItem_Click(null, null);
				}

				//View Menustrip
				if (ModifierKeys == (Keys.Control) && e.KeyCode == Keys.H)
				{
					homeToolStripMenuItem_Click(null, null);
				}
				if (ModifierKeys == (Keys.Control) && e.KeyCode == Keys.Q)
				{
					queueToolStripMenuItem_Click(null, null);
				}
				if (ModifierKeys == (Keys.Control) && e.KeyCode == Keys.P)
				{
					playerToolStripMenuItem_Click(null, null);
				}
				if (ModifierKeys == (Keys.Control) && e.KeyCode == Keys.S)
				{
					settingsToolStripMenuItem_Click(null, null);
				}

				//Player controls
				if (e.KeyCode == Keys.Space)
				{
					play_Click(null, null);
				}

				//VOLUME CONTROL
				if (e.KeyCode == Keys.Up)
				{
					volumeSlider2.Value = Utils.Clamp(volumeSlider2.Value / 5 + 1, 0, 20) * 5;
					volumeSlider1.Value = volumeSlider2.Value;
					volumeSlider3.Value = volumeSlider2.Value;
					volumeSlider2_Scroll(null, null);
				}
				if (e.KeyCode == Keys.Down)
				{
					volumeSlider2.Value = Utils.Clamp(volumeSlider2.Value / 5, 0, 20) * 5;
					if (volumeSlider2.Value == volumeSlider1.Value)
					{
						//no change, try subtracting 5
						volumeSlider2.Value = Utils.Clamp(volumeSlider2.Value -5, 0, 100);
					}
					volumeSlider1.Value = volumeSlider2.Value;
					volumeSlider3.Value = volumeSlider2.Value;
					volumeSlider2_Scroll(null, null);
				}

				if (e.KeyCode == Keys.PageUp)
				{
					volumeSlider2.Value = Utils.Clamp(volumeSlider2.Value / 5 + 2, 0, 20) * 5;
					volumeSlider1.Value = volumeSlider2.Value;
					volumeSlider3.Value = volumeSlider2.Value;
					volumeSlider2_Scroll(null, null);
				}
				if (e.KeyCode == Keys.PageDown)
				{
					volumeSlider2.Value = Utils.Clamp(volumeSlider2.Value / 5 - 1, 0, 20) * 5;
					if (volumeSlider2.Value == volumeSlider1.Value)
					{
						//no change, try subtracting 5
						volumeSlider2.Value = Utils.Clamp(volumeSlider2.Value - 10, 0, 100);
					}
					volumeSlider1.Value = volumeSlider2.Value;
					volumeSlider3.Value = volumeSlider2.Value;
					volumeSlider2_Scroll(null, null);
				}

				//SEEKBAR
				if (e.KeyCode == Keys.Left)
				{
					metroTrackBar1.Value = Utils.Clamp(metroTrackBar1.Value - 5, metroTrackBar1.Minimum, metroTrackBar1.Maximum);
					metroTrackBar2.Value = Utils.Clamp(metroTrackBar2.Value - 5, metroTrackBar2.Minimum, metroTrackBar2.Maximum);
					metroTrackBar3.Value = Utils.Clamp(metroTrackBar3.Value - 5, metroTrackBar3.Minimum, metroTrackBar3.Maximum);
					metroTrackBar1_Scroll(null, null);
				}
				if (e.KeyCode == Keys.Right)
				{
					metroTrackBar1.Value = Utils.Clamp(metroTrackBar1.Value + 5, metroTrackBar1.Minimum, metroTrackBar1.Maximum);
					metroTrackBar2.Value = Utils.Clamp(metroTrackBar2.Value + 5, metroTrackBar2.Minimum, metroTrackBar2.Maximum);
					metroTrackBar3.Value = Utils.Clamp(metroTrackBar3.Value + 5, metroTrackBar3.Minimum, metroTrackBar3.Maximum);
					metroTrackBar1_Scroll(null, null);
				}

				if (e.KeyCode == Keys.J)
				{
					metroTrackBar1.Value = Utils.Clamp(metroTrackBar1.Value - 10, metroTrackBar1.Minimum, metroTrackBar1.Maximum);
					metroTrackBar2.Value = Utils.Clamp(metroTrackBar2.Value - 10, metroTrackBar2.Minimum, metroTrackBar2.Maximum);
					metroTrackBar3.Value = Utils.Clamp(metroTrackBar3.Value - 10, metroTrackBar3.Minimum, metroTrackBar3.Maximum);
					metroTrackBar1_Scroll(null, null);
				}
				if (e.KeyCode == Keys.L)
				{
					metroTrackBar1.Value = Utils.Clamp(metroTrackBar1.Value + 10, metroTrackBar1.Minimum, metroTrackBar1.Maximum);
					metroTrackBar2.Value = Utils.Clamp(metroTrackBar2.Value + 10, metroTrackBar2.Minimum, metroTrackBar2.Maximum);
					metroTrackBar3.Value = Utils.Clamp(metroTrackBar3.Value + 10, metroTrackBar3.Minimum, metroTrackBar3.Maximum);
					metroTrackBar1_Scroll(null, null);
				}
			}
		}

		private void splitContainer2_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void splitContainer1_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void metroTrackBar2_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void tablessControl1_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void volumeSlider1_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void volumeSlider2_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void splitContainer3_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void metroTrackBar3_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void volumeSlider3_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		private void metroTrackBar1_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
			Form1_KeyDown(sender, e);
		}

		#endregion

		#region Settings
		public void SaveSettings()
		{
			//Saves settings
			string settingsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Music Player Pro");

			string SettingsMisc = Path.Combine(settingsRoot, "settings.txt");
			string[] settings = new string[10];


		}
		#endregion
	}
}
