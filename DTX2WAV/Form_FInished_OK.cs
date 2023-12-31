﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DTX2WAV
{
	public partial class Form_Finished_OK : Form
	{
		public Form_Finished_OK()
		{
			InitializeComponent();
		}

		private void button_OK_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Form_Finished_OK_Shown(object sender, EventArgs e)
		{
			Bitmap canvas = new Bitmap(pictureBox_OKIcon.Width, pictureBox_OKIcon.Height);
			Graphics g = Graphics.FromImage(canvas);

			g.DrawIcon(SystemIcons.Information, 0, 0);
			g.Dispose();
			pictureBox_OKIcon.Image = canvas;
		}
	}
}
