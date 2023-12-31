﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using SharpDX;
using SharpDX.Direct3D9;
using FDK;
using SampleFramework;

namespace DTXMania
{
	internal class CActEnumSongs : CActivity
	{
		public bool bコマンドでの曲データ取得;


		/// <summary>
		/// Constructor
		/// </summary>
		public CActEnumSongs()
		{
			Init(false);
		}

		public CActEnumSongs(bool _bコマンドでの曲データ取得)
		{
			Init(_bコマンドでの曲データ取得);
		}
		private void Init(bool _bコマンドでの曲データ取得)
		{
			base.b活性化してない = true;
			bコマンドでの曲データ取得 = _bコマンドでの曲データ取得;
		}

		// CActivity 実装

		public override void On活性化()
		{
			if (this.b活性化してる)
				return;
			base.On活性化();

			try
			{
				this.ctNowEnumeratingSongs = new CCounter();  // 0, 1000, 17, CDTXMania.Instance.Timer );
				this.ctNowEnumeratingSongs.t開始(0, 100, 17, CDTXMania.Instance.Timer);
			}
			finally
			{
			}
		}
		public override void On非活性化()
		{
			if (this.b活性化してない)
				return;
			base.On非活性化();
			this.ctNowEnumeratingSongs = null;
		}
		public override void OnManagedリソースの作成()
		{
			if (this.b活性化してない)
				return;
			string pathNowEnumeratingSongs = CSkin.Path(@"Graphics\ScreenTitle NowEnumeratingSongs.png");
			if (File.Exists(pathNowEnumeratingSongs))
			{
				this.txNowEnumeratingSongs = TextureFactory.tテクスチャの生成(pathNowEnumeratingSongs, false);
			}
			else
			{
				this.txNowEnumeratingSongs = null;
			}
			string pathDialogNowEnumeratingSongs = CSkin.Path(@"Graphics\ScreenConfig NowEnumeratingSongs.png");
			if (File.Exists(pathDialogNowEnumeratingSongs))
			{
				this.txDialogNowEnumeratingSongs = TextureFactory.tテクスチャの生成(pathDialogNowEnumeratingSongs, false);
			}
			else
			{
				this.txDialogNowEnumeratingSongs = null;
			}
			try
			{
				CPrivateFastFont pfMessage;
				//System.Drawing.Font ftMessage = new System.Drawing.Font( @"MS PGothic", 40.0f, FontStyle.Bold, GraphicsUnit.Pixel );

				string fontname = CDTXMania.Instance.Resources.Explanation("strCfgPopupFontFileName");
				string path = Path.Combine(@"Graphics\fonts", fontname);
				pfMessage = new CPrivateFastFont(CSkin.Path(path), 40);

				string strMessage = CDTXMania.Instance.Resources.Explanation("strEnumeratingSongs");
				if ( ( strMessage != null ) && ( strMessage.Length > 0 ) )
				{
					Bitmap image = pfMessage.DrawPrivateFont(
						strMessage, System.Drawing.Color.White, System.Drawing.Color.Black,
						new Size(SampleFramework.GameWindowSize.Width, (int)(128 * Scale.Y))
					);
					System.Drawing.Rectangle rect = pfMessage.RectStrings;

					Bitmap image_trim = image.Clone(rect, image.PixelFormat);

					this.txMessage = new CTexture(CDTXMania.Instance.Device, image_trim, CDTXMania.Instance.TextureFormat);


					//Bitmap image = new Bitmap(1, 1);
					//Graphics graphics = Graphics.FromImage(image);
					//SizeF ef = graphics.MeasureString(strMessage, ftMessage);
					//Size size = new Size( (int) Math.Ceiling( (double) ef.Width ), (int) Math.Ceiling( (double) ef.Height ) );
					//graphics.Dispose();
					//image.Dispose();
					//image = new Bitmap(size.Width, size.Height);
					//graphics = Graphics.FromImage(image);
					//graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					//graphics.DrawString(strMessage, ftMessage, Brushes.White, (float) 0f, (float) 0f);
					//graphics.Dispose();
					//this.txMessage = new CTexture(CDTXMania.Instance.Device, image, CDTXMania.Instance.TextureFormat);
					//image.Dispose();
					TextureFactory.t安全にDisposeする(ref image_trim);
					TextureFactory.t安全にDisposeする(ref image);
					TextureFactory.t安全にDisposeする(ref pfMessage);
				}
				else
				{
					this.txMessage = null;
				}
			}
			catch (CTextureCreateFailedException)
			{
				Trace.TraceError("テクスチャの生成に失敗しました。(txMessage)");
				this.txMessage = null;
			}

			base.OnManagedリソースの作成();
		}
		public override void OnManagedリソースの解放()
		{
			if (this.b活性化してない)
				return;

			TextureFactory.t安全にDisposeする(ref this.txDialogNowEnumeratingSongs);
			TextureFactory.t安全にDisposeする(ref this.txNowEnumeratingSongs);
			TextureFactory.t安全にDisposeする(ref this.txMessage);
			base.OnManagedリソースの解放();
		}

		public override int On進行描画()
		{
			if (this.b活性化してない)
			{
				return 0;
			}
			this.ctNowEnumeratingSongs.t進行Loop();
			if (this.txNowEnumeratingSongs != null)
			{
				this.txNowEnumeratingSongs.n透明度 = (int)(176.0 + 80.0 * Math.Sin((double)(2 * Math.PI * this.ctNowEnumeratingSongs.n現在の値 * 2 / 100.0)));
				this.txNowEnumeratingSongs.t2D描画(CDTXMania.Instance.Device, 18, 7);
			}
			if (bコマンドでの曲データ取得 && this.txDialogNowEnumeratingSongs != null)
			{
				this.txDialogNowEnumeratingSongs.t2D描画(CDTXMania.Instance.Device, 500, 300);
				this.txMessage.t2D描画(CDTXMania.Instance.Device, 540, 320);
			}

			return 0;
		}


		private CCounter ctNowEnumeratingSongs;
		private CTexture txNowEnumeratingSongs = null;
		private CTexture txDialogNowEnumeratingSongs = null;
		private CTexture txMessage;
	}
}
