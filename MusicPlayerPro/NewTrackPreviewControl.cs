using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerPro
{
	public class NewTrackPreviewControl : Control
	{
		#region Variables

		private Image _albumArt;
		private string _albumName;
		private string _trackName;
		private string _artistName;

		private Color _controlColor = Color.LightCoral;
		private Color _textColor = Color.Black;
		private Color _albumBackgroundColor = Color.Black;

		private int _borderRadius = 5;

		private Point _albumLocation = new Point(0, 0);
		private Size _albumSize = new Size(0, 0);

		private Point _trackLocation = new Point(0, 0);
		private Point _albumNameLocation = new Point(0, 0);
		private Point _artistLocation = new Point(0, 0);

		[Category("Appearance")]
		public Image AlbumArt
		{
			get { return _albumArt; }
			set
			{
				_albumArt = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public string AlbumName
		{
			get { return _albumName; }
			set
			{
				_albumName = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public string TrackName
		{
			get { return _trackName; }
			set
			{
				_trackName = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public string ArtistName
		{
			get { return _artistName; }
			set
			{
				_artistName = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public Color ControlColor
		{
			get { return _controlColor; }
			set
			{
				_controlColor = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public Color TextColor
		{
			get { return _textColor; }
			set
			{
				_textColor = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public Color AlbumBackgroundColor
		{
			get { return _albumBackgroundColor; }
			set
			{
				_albumBackgroundColor = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public int BorderRadius
		{
			get { return _borderRadius; }
			set
			{
				_borderRadius = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public Point AlbumLocation
		{
			get { return _albumLocation; }
			set
			{
				_albumLocation = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		public Size AlbumSize
		{
			get { return _albumSize; }
			set
			{
				_albumSize = value;
				Invalidate();
			}
		}

		#endregion

		private const int WS_EX_TRANSPARENT = 0x00000020;
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= WS_EX_TRANSPARENT;

				return cp;
			}
		}

		public NewTrackPreviewControl()
		{
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// Don't paint background
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics graphics = e.Graphics;

			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			//Draw bg
			graphics.FillEllipse(new SolidBrush(ControlColor), new Rectangle(0, 0, BorderRadius * 2, BorderRadius * 2));
			graphics.FillEllipse(new SolidBrush(ControlColor), new Rectangle(0, Size.Height - BorderRadius * 2, BorderRadius * 2, BorderRadius * 2));
			graphics.FillRectangle(new SolidBrush(ControlColor), new Rectangle(0, BorderRadius, Size.Width, Size.Height - BorderRadius * 2));
			graphics.FillRectangle(new SolidBrush(ControlColor), new Rectangle(BorderRadius, 0, Size.Width - BorderRadius, Size.Height));

			//Draw album art
			graphics.FillRectangle(new SolidBrush(AlbumBackgroundColor), new Rectangle(AlbumLocation, AlbumSize));
			if (AlbumArt != null)
			{
				graphics.DrawImage(AlbumArt, new Rectangle(AlbumLocation, AlbumSize));
			}

			//Draw the artist
			graphics.DrawString(TrackName, Font, new SolidBrush(TextColor), new Point(AlbumLocation.X + AlbumSize.Width + 10, AlbumLocation.Y));
			graphics.DrawString(AlbumName, Font, new SolidBrush(TextColor), new Point(AlbumLocation.X + AlbumSize.Width + 10, AlbumLocation.Y + 20));
			graphics.DrawString(ArtistName, Font, new SolidBrush(TextColor), new Point(AlbumLocation.X + AlbumSize.Width + 10, AlbumLocation.Y + 35));
		}
	}
}
