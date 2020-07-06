using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace MusicPlayerPro.Trackbar
{
	[DefaultEvent("Scroll")]
	public class MetroTrackBar : Control
	{
		#region Events

		public event EventHandler ValueChanged;
		private void OnValueChanged()
		{
			if (ValueChanged != null)
				ValueChanged(this, EventArgs.Empty);
		}

		public event ScrollEventHandler Scroll;
		private void OnScroll(ScrollEventType scrollType, int newValue)
		{
			if (Scroll != null)
				Scroll(this, new ScrollEventArgs(scrollType, newValue));
		}


		#endregion

		#region Fields

		private int trackerValue = 50;
		public int Value
		{
			get { return trackerValue; }
			set
			{
				if (value >= barMinimum & value <= barMaximum)
				{
					trackerValue = value;
					OnValueChanged();
					Invalidate();
				}
				else throw new ArgumentOutOfRangeException("Value is outside appropriate range (min, max)");
			}
		}

		private int barMinimum = 0;
		public int Minimum
		{
			get { return barMinimum; }
			set
			{
				if (value < barMaximum)
				{
					barMinimum = value;
					if (trackerValue < barMinimum)
					{
						trackerValue = barMinimum;
						if (ValueChanged != null) ValueChanged(this, new EventArgs());
					}
					Invalidate();
				}
				else throw new ArgumentOutOfRangeException("Minimal value is greather than maximal one");
			}
		}

		private int padding = 5;
		public int TextPadding
		{
			get { return padding; }
			set
			{
				if (value > 0)
				{
					padding = value;
					if (ValueChanged != null) ValueChanged(this, new EventArgs());
					Invalidate();
				}
			}
		}


		private int barMaximum = 100;
		public int Maximum
		{
			get { return barMaximum; }
			set
			{
				if (value > barMinimum)
				{
					barMaximum = value;
					if (trackerValue > barMaximum)
					{
						trackerValue = barMaximum;
						if (ValueChanged != null) ValueChanged(this, new EventArgs());
					}
					Invalidate();
				}
				else throw new ArgumentOutOfRangeException("Maximal value is lower than minimal one");
			}
		}

		private uint smallChange = 1;
		public uint SmallChange
		{
			get { return smallChange; }
			set { smallChange = value; }
		}

		private uint largeChange = 5;
		public uint LargeChange
		{
			get { return largeChange; }
			set { largeChange = value; }
		}

		private int mouseWheelBarPartitions = 10;
		public int MouseWheelBarPartitions
		{
			get { return mouseWheelBarPartitions; }
			set
			{
				if (value > 0)
					mouseWheelBarPartitions = value;
				else throw new ArgumentOutOfRangeException("MouseWheelBarPartitions has to be greather than zero");
			}
		}

		private string leftText = "Minimum";
		[Category("Appearance")]
		public string LeftText
		{
			get { return leftText; }
			set
			{
				leftText = value;
				if (ValueChanged != null) ValueChanged(this, new EventArgs());

				Invalidate();
			}
		}

		private string rightText = "Maximum";
		[Category("Appearance")]
		public string RightText
		{
			get { return rightText; }
			set
			{
				rightText = value;
				if (ValueChanged != null) ValueChanged(this, new EventArgs());

				Invalidate();
			}
		}

		private bool acceptKeys = true;
		[Category("Behavior")]
		public bool AllowKeyboardInput
		{
			get { return acceptKeys; }
			set
			{
				acceptKeys = value;
				if (ValueChanged != null) ValueChanged(this, new EventArgs());
				Invalidate();
			}
		}
		private bool isHovered = false;
		private bool isPressed = false;
		private bool isFocused = false;

		#endregion

		#region Constructor

		public MetroTrackBar(int min, int max, int value)
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
					 ControlStyles.OptimizedDoubleBuffer |
					 ControlStyles.ResizeRedraw |
					 ControlStyles.Selectable |
					 ControlStyles.SupportsTransparentBackColor |
					 ControlStyles.UserMouse |
					 ControlStyles.UserPaint, true);

			BackColor = Color.Transparent;

			Minimum = min;
			Maximum = max;
			Value = value;
		}

		public MetroTrackBar() : this(0, 100, 50) { }

		#endregion

		#region Paint Methods

		protected override void OnPaint(PaintEventArgs e)
		{
			Color backColor, thumbColor, barColor;

			backColor = BackColor;

			if (isHovered && !isPressed && Enabled)
			{
				thumbColor = Color.FromArgb(127, 127, 127);
				barColor = Color.FromArgb(64, 64, 64);
			}
			else if (isHovered && isPressed && Enabled)
			{
				thumbColor = Color.FromArgb(127, 127, 127);
				barColor = Color.FromArgb(64, 64, 64);
			}
			else if (!Enabled)
			{
				thumbColor = Color.FromArgb(127, 127, 127);
				barColor = Color.FromArgb(64, 64, 64);
			}
			else
			{
				thumbColor = Color.FromArgb(127, 127, 127);
				barColor = Color.FromArgb(64, 64, 64);
			}

			e.Graphics.Clear(BackColor);
			DrawTrackBar(e.Graphics, thumbColor, barColor, Color.FromArgb(214, 214, 214), Color.FromArgb(0, 120, 215));

			if (false && isFocused)
				ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
		}

		private void DrawTrackBar(Graphics g, Color thumbColor, Color thumbColorBorder, Color barColor, Color progressFilledColor)
		{
			int TrackX = Utils.Clamp((trackerValue - barMinimum) * Width / (barMaximum - barMinimum), 6, Width - 18);

			TextRenderer.DrawText(g, leftText, Font, new Point(TextPadding, 16 + TextPadding), ForeColor, BackColor, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
			TextRenderer.DrawText(g, rightText, Font, new Point(Size.Width - TextRenderer.MeasureText(rightText, Font).Width - TextPadding, 16 + TextPadding), ForeColor, BackColor, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

			using (SolidBrush b = new SolidBrush(progressFilledColor))
			{
				Rectangle barRect = new Rectangle(0, 6, TrackX, 4);
				g.FillRectangle(b, barRect);

				//Tracker
				Rectangle thumbRect = new Rectangle(TrackX - 5, 0, 16, 16);
				Rectangle barRectBorder = new Rectangle(thumbRect.X - 1, thumbRect.Y - 1, thumbRect.Width + 2, thumbRect.Height + 2);
				g.FillRectangle(new SolidBrush(thumbColorBorder), barRectBorder);
				g.FillRectangle(new SolidBrush(thumbColor), thumbRect);
			}

			using (SolidBrush b = new SolidBrush(barColor))
			{
				Rectangle barRect = new Rectangle(TrackX + 12, 6, Width - TrackX + 7, 4);
				g.FillRectangle(b, barRect);
			}

		}

		#endregion

		#region Focus Methods

		protected override void OnGotFocus(EventArgs e)
		{
			isFocused = true;
			Invalidate();

			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			isFocused = false;
			isHovered = false;
			isPressed = false;
			Invalidate();

			base.OnLostFocus(e);
		}

		protected override void OnEnter(EventArgs e)
		{
			isFocused = true;
			Invalidate();

			base.OnEnter(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			isFocused = false;
			isHovered = false;
			isPressed = false;
			Invalidate();

			base.OnLeave(e);
		}

		#endregion

		#region Keyboard Methods

		protected override void OnKeyDown(KeyEventArgs e)
		{
			isHovered = true;
			isPressed = true;
			Invalidate();

			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (AllowKeyboardInput)
			{
				isHovered = false;
				isPressed = false;
				Invalidate();

				base.OnKeyUp(e);

				switch (e.KeyCode)
				{
					case Keys.Down:
					case Keys.Left:
						SetProperValue(Value - (int)smallChange);
						OnScroll(ScrollEventType.SmallDecrement, Value);
						break;
					case Keys.Up:
					case Keys.Right:
						SetProperValue(Value + (int)smallChange);
						OnScroll(ScrollEventType.SmallIncrement, Value);
						break;
					case Keys.Home:
						Value = barMinimum;
						break;
					case Keys.End:
						Value = barMaximum;
						break;
					case Keys.PageDown:
						SetProperValue(Value - (int)largeChange);
						OnScroll(ScrollEventType.LargeDecrement, Value);
						break;
					case Keys.PageUp:
						SetProperValue(Value + (int)largeChange);
						OnScroll(ScrollEventType.LargeIncrement, Value);
						break;
				}

				if (Value == barMinimum)
					OnScroll(ScrollEventType.First, Value);

				if (Value == barMaximum)
					OnScroll(ScrollEventType.Last, Value);

				Point pt = PointToClient(Cursor.Position);
				OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, pt.X, pt.Y, 0));
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Tab | ModifierKeys == Keys.Shift)
				return base.ProcessDialogKey(keyData);
			else
			{
				OnKeyDown(new KeyEventArgs(keyData));
				return true;
			}
		}

		#endregion

		#region Mouse Methods

		protected override void OnMouseEnter(EventArgs e)
		{
			isHovered = true;
			Invalidate();

			base.OnMouseEnter(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isPressed = true;
				Invalidate();
			}

			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left)
			{
				Capture = true;
				OnScroll(ScrollEventType.ThumbTrack, trackerValue);
				OnValueChanged();
				OnMouseMove(e);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (Capture & e.Button == MouseButtons.Left)
			{
				ScrollEventType set = ScrollEventType.ThumbPosition;
				Point pt = e.Location;
				int p = pt.X;

				float coef = (float)(barMaximum - barMinimum) / (float)(ClientSize.Width - 3);
				trackerValue = (int)(p * coef + barMinimum);

				if (trackerValue <= barMinimum)
				{
					trackerValue = barMinimum;
					set = ScrollEventType.First;
				}
				else if (trackerValue >= barMaximum)
				{
					trackerValue = barMaximum;
					set = ScrollEventType.Last;
				}

				OnScroll(set, trackerValue);
				OnValueChanged();

				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			isPressed = false;
			Invalidate();

			base.OnMouseUp(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			isHovered = false;
			Invalidate();

			base.OnMouseLeave(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			int v = e.Delta / 120 * (barMaximum - barMinimum) / mouseWheelBarPartitions;
			SetProperValue(Value + v);
		}

		#endregion

		#region Overridden Methods

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			Invalidate();
		}

		#endregion

		#region Helper Methods

		private void SetProperValue(int val)
		{
			if (val < barMinimum) Value = barMinimum;
			else if (val > barMaximum) Value = barMaximum;
			else Value = val;
		}

		#endregion
	}
}
