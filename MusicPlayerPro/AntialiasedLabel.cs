using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicPlayerPro
{
	public class AntialiasedLabel : Label
	{
		protected override void OnPaint(PaintEventArgs e)
		{
			Rectangle face = DeflateRect(ClientRectangle, Padding);

			if (UseCompatibleTextRendering)
			{
				base.OnPaint(e);
			}
			else
			{
				if (Enabled)
				{
					e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
					//Size = e.Graphics.MeasureString(Text, Font);
					e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(face.X, face.Y));
				}
				else
				{
					base.OnPaint(e);
				}
			}
		}


		public static Rectangle DeflateRect(Rectangle rect, Padding padding)
		{
			rect.X += padding.Left;
			rect.Y += padding.Top;
			rect.Width -= padding.Horizontal;
			rect.Height -= padding.Vertical;
			return rect;
		}
	}
}
