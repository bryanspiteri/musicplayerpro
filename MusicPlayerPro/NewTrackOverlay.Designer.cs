namespace MusicPlayerPro
{
	partial class NewTrackOverlay
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.newTrackPreviewControl1 = new MusicPlayerPro.NewTrackPreviewControl();
			this.SuspendLayout();
			// 
			// newTrackPreviewControl1
			// 
			this.newTrackPreviewControl1.AlbumArt = null;
			this.newTrackPreviewControl1.AlbumBackgroundColor = System.Drawing.Color.Black;
			this.newTrackPreviewControl1.AlbumLocation = new System.Drawing.Point(20, 20);
			this.newTrackPreviewControl1.AlbumName = "oooooooo";
			this.newTrackPreviewControl1.AlbumSize = new System.Drawing.Size(96, 96);
			this.newTrackPreviewControl1.ArtistName = "aaaaaaaa";
			this.newTrackPreviewControl1.BackColor = System.Drawing.Color.Transparent;
			this.newTrackPreviewControl1.BorderRadius = 5;
			this.newTrackPreviewControl1.ControlColor = System.Drawing.Color.LightCoral;
			this.newTrackPreviewControl1.Font = new System.Drawing.Font("Advantage", 11F);
			this.newTrackPreviewControl1.Location = new System.Drawing.Point(0, 0);
			this.newTrackPreviewControl1.Name = "newTrackPreviewControl1";
			this.newTrackPreviewControl1.Size = new System.Drawing.Size(378, 136);
			this.newTrackPreviewControl1.TabIndex = 0;
			this.newTrackPreviewControl1.Text = "newTrackPreviewControl1";
			this.newTrackPreviewControl1.TextColor = System.Drawing.Color.Black;
			this.newTrackPreviewControl1.TrackName = "eeeeee";
			// 
			// NewTrackOverlay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(378, 136);
			this.Controls.Add(this.newTrackPreviewControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "NewTrackOverlay";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "NewTrackOverlay";
			this.ResumeLayout(false);

		}

		#endregion

		private NewTrackPreviewControl newTrackPreviewControl1;
	}
}