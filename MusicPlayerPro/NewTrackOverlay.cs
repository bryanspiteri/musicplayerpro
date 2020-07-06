using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerPro
{
	public partial class NewTrackOverlay : Form
	{
		public Timer timer = new Timer();
		public int SliderTimer = 50;
		public int SlideSpeed = 15;

		public int TimerMoveRequirement = 0;

		public int IterationCount = 0;
		public Point Offset;

		#region Disable Focusing
		protected override bool ShowWithoutActivation => true;

		private const int WS_EX_NOACTIVATE = 0x08000000;
		private const int WS_EX_TRANSPARENT = 0x00000020;
		protected override CreateParams CreateParams
		{
			get
			{
				var createParams = base.CreateParams;

				createParams.ExStyle |= WS_EX_NOACTIVATE;
				createParams.ExStyle |= WS_EX_TRANSPARENT;
				return createParams;
			}
		}

		private const int WM_MOUSEACTIVATE = 0x0021, MA_NOACTIVATE = 0x0003;

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_MOUSEACTIVATE)
			{
				m.Result = (IntPtr)MA_NOACTIVATE;
				return;
			}
			base.WndProc(ref m);
		}
		#endregion

		public NewTrackOverlay(Image art, string track, string artist, string album)
		{
			InitializeComponent();
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.BackColor = Color.Transparent;
			this.TopMost = true;

			TimerMoveRequirement = (int) Math.Ceiling((double)(Size.Width / SlideSpeed));

			timer.Tick += OnTimeFinished;
			timer.Interval = SliderTimer;
			timer.Start();

			Screen screen = Screen.FromControl(this);
			Offset = new Point(screen.WorkingArea.Width - Size.Width, 35);
			Location = new Point(screen.WorkingArea.Width - Size.Width, 35);

			newTrackPreviewControl1.BackColor = Color.Transparent;
			newTrackPreviewControl1.AlbumArt = art;
			newTrackPreviewControl1.TrackName = track;
			newTrackPreviewControl1.ArtistName = artist;
			newTrackPreviewControl1.AlbumName = album;

		}

		private void OnTimeFinished(object sender, EventArgs e)
		{
			IterationCount++;
			newTrackPreviewControl1.Size = Size;

			if (IterationCount < TimerMoveRequirement)
			{
				Location = new Point(Offset.X + Utils.Clamp(Size.Width - IterationCount * SlideSpeed, 0, Size.Width), Offset.Y);
			}
			else if (IterationCount == TimerMoveRequirement)
			{
				timer.Stop();
				timer.Interval = 10000;
				timer.Start();
			}
			else if (IterationCount == TimerMoveRequirement + 1)
			{
				timer.Stop();
				timer.Interval = SliderTimer;
				timer.Start();
				Location = new Point(Offset.X + Utils.Clamp((IterationCount - TimerMoveRequirement) * SlideSpeed, 0, Size.Width), Offset.Y);
			}
			else if (IterationCount < TimerMoveRequirement * 2)
			{
				Location = new Point(Offset.X + Utils.Clamp((IterationCount - TimerMoveRequirement) * SlideSpeed, 0, Size.Width), Offset.Y);
			}
			else
			{
				timer.Stop();
				Close();
			}
		}

		//transparent background

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// Don't paint background
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Image image = this.BackgroundImage;

			if (image == null)
			{
				return;
			}

			Graphics graphics = e.Graphics;

			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			graphics.DrawImage(image, Point.Empty);
		}
	}
}
