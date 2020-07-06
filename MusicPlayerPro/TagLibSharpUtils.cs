using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using System.IO;

namespace MusicPlayerPro
{
	public class TagLibSharpUtils
	{
		/// <summary>
		/// Returns a Bitmap containing the album cover art
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static Image GetCoverArt(TagLib.File file)
		{
			try
			{
				//pic contains the image data
				IPicture pic = file.Tag.Pictures[0];

				//Get the data stream from the image
				MemoryStream stream = new MemoryStream(pic.Data.Data);

				return Image.FromStream(stream);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Returns a TagLib.File object of the given MP3
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static TagLib.File OpenMp3File(string filePath)
		{
			try
			{
				if (System.IO.File.Exists(filePath))
				{
					TagLib.File file = TagLib.File.Create(filePath);
					return file;
				}
				else
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
		}
	}
}
