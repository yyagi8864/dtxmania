﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct3D9;
using FDK;

using Rectangle = System.Drawing.Rectangle;

namespace DTXMania
{
	internal class CActSelectPreimageパネル : CActivity
	{
		// メソッド

		public CActSelectPreimageパネル()
		{
			base.b活性化してない = true;
		}
		public void t選択曲が変更された()
		{
			this.ct遅延表示 = new CCounter(-CDTXMania.Instance.ConfigIni.nPreImageWeightMs, 100, 1, CDTXMania.Instance.Timer);
			this.b新しいプレビューファイルを読み込んだ = false;
		}

		public bool bIsPlayingPremovie    // #27060
		{
			get
			{
				return (this.rAVI != null);
			}
		}
		public CAct演奏AVI actAVI
		{
			get;
			set;
		}

		// CActivity 実装

		public override void On活性化()
		{
			this.n本体X = (int)(8 * Scale.X);
			this.n本体Y = (int)(0x39 * Scale.Y);
			this.r表示するプレビュー画像 = this.txプレビュー画像がないときの画像;
			this.str現在のファイル名 = "";
			this.b新しいプレビューファイルを読み込んだ = false;
			base.On活性化();

			this.actAVI.bIsPreviewMovie = true;
			this.actAVI.On活性化();
		}
		public override void On非活性化()
		{
			this.ct登場アニメ用 = null;
			this.ct遅延表示 = null;
			if (this.rAVI != null)
			{
				this.rAVI.Dispose();
				this.rAVI = null;
			}
			base.On非活性化();
			this.actAVI.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if (!base.b活性化してない)
			{
				this.txパネル本体 = TextureFactory.tテクスチャの生成(CSkin.Path(@"Graphics\ScreenSelect preimage panel.png"), false);
				this.txセンサ = TextureFactory.tテクスチャの生成(CSkin.Path(@"Graphics\ScreenSelect sensor.png"), false);
				this.txセンサ光 = TextureFactory.tテクスチャの生成(CSkin.Path(@"Graphics\ScreenSelect sensor light.png"), false);
				this.txプレビュー画像 = null;
				this.txプレビュー画像がないときの画像 = TextureFactory.tテクスチャの生成(CSkin.Path(@"Graphics\ScreenSelect preimage default.png"), false);
				//this.sfAVI画像 = Surface.CreateOffscreenPlain( CDTXMania.Instance.app.Device, 0xcc, 0x10d, CDTXMania.Instance.app.GraphicsDeviceManager.CurrentSettings.BackBufferFormat, Pool.SystemMemory );
				//this.sfAVI画像 = Surface.CreateOffscreenPlain( CDTXMania.Instance.app.Device, 192, 269, CDTXMania.Instance.app.GraphicsDeviceManager.CurrentSettings.BackBufferFormat, Pool.Default );
				//this.nAVI再生開始時刻 = -1;
				//this.n前回描画したフレーム番号 = -1;
				//this.b動画フレームを作成した = false;
				//this.pAVIBmp = IntPtr.Zero;
				this.tプレビュー画像_動画の変更();
				base.OnManagedリソースの作成();

				this.actAVI.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				TextureFactory.tテクスチャの解放(ref this.txパネル本体);
				TextureFactory.tテクスチャの解放(ref this.txセンサ);
				TextureFactory.tテクスチャの解放(ref this.txセンサ光);
				TextureFactory.tテクスチャの解放(ref this.txプレビュー画像);
				TextureFactory.tテクスチャの解放(ref this.txプレビュー画像がないときの画像);
				//if( this.sfAVI画像 != null )
				//{
				//    this.sfAVI画像.Dispose();
				//    this.sfAVI画像 = null;
				//}
				base.OnManagedリソースの解放();
				this.actAVI.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if (!base.b活性化してない)
			{
				if (base.b初めての進行描画)
				{
					this.ct登場アニメ用 = new CCounter(0, 100, 5, CDTXMania.Instance.Timer);
					this.ctセンサ光 = new CCounter(0, 100, 30, CDTXMania.Instance.Timer);
					this.ctセンサ光.n現在の値 = 70;
					base.b初めての進行描画 = false;
				}
				this.ct登場アニメ用.t進行();
				this.ctセンサ光.t進行Loop();
				if ((!CDTXMania.Instance.stage選曲.bスクロール中 && (this.ct遅延表示 != null)) && this.ct遅延表示.b進行中)
				{
					this.ct遅延表示.t進行();
					if ((this.ct遅延表示.n現在の値 >= 0) && this.b新しいプレビューファイルをまだ読み込んでいない)
					{
						this.tプレビュー画像_動画の変更();
						CDTXMania.Instance.Timer.t更新();
						this.ct遅延表示.n現在の経過時間ms = CDTXMania.Instance.Timer.n現在時刻;
						this.b新しいプレビューファイルを読み込んだ = true;
					}
					else if (this.ct遅延表示.b終了値に達した && this.ct遅延表示.b進行中)
					{
						this.ct遅延表示.t停止();
					}
				}
				//else if( ( ( this.avi != null ) && ( this.sfAVI画像 != null ) ) && ( this.nAVI再生開始時刻 != -1 ) )
				//{
				//    int time = (int) ( ( CDTXMania.Instance.Timer.n現在時刻 - this.nAVI再生開始時刻 ) * ( ( (double) CDTXMania.Instance.ConfigIni.n演奏速度 ) / 20.0 ) );
				//    int frameNoFromTime = this.avi.GetFrameNoFromTime( time );
				//    if( frameNoFromTime >= this.avi.GetMaxFrameCount() )
				//    {
				//        this.nAVI再生開始時刻 = CDTXMania.Instance.Timer.n現在時刻;
				//    }
				//    else if( ( this.n前回描画したフレーム番号 != frameNoFromTime ) && !this.b動画フレームを作成した )
				//    {
				//        this.b動画フレームを作成した = true;
				//        this.n前回描画したフレーム番号 = frameNoFromTime;
				//        this.pAVIBmp = this.avi.GetFramePtr( frameNoFromTime );
				//    }
				//}
				this.t描画処理_パネル本体();
				this.t描画処理_ジャンル文字列();
				this.t描画処理_プレビュー画像();
				this.t描画処理_センサ光();
				this.t描画処理_センサ本体();

			}
			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		//private CAvi avi;
		private CDTX.CAVI rAVI;

		//private bool b動画フレームを作成した;
		private CCounter ctセンサ光;
		private CCounter ct遅延表示;
		private CCounter ct登場アニメ用;
		//private long nAVI再生開始時刻;
		//private int n前回描画したフレーム番号;
		private int n本体X;
		private int n本体Y;
		//private IntPtr pAVIBmp;
		private readonly Rectangle rcセンサ光 = new Rectangle((int)(0 * Scale.X), (int)(0xc0 * Scale.Y), (int)(0x40 * Scale.X), (int)(0x40 * Scale.Y));
		private readonly Rectangle rcセンサ本体下半分 = new Rectangle((int)(0x40 * Scale.X), (int)(0 * Scale.Y), (int)(0x40 * Scale.X), (int)(0x80 * Scale.Y));
		private readonly Rectangle rcセンサ本体上半分 = new Rectangle((int)(0 * Scale.X), (int)(0 * Scale.Y), (int)(0x40 * Scale.X), (int)(0x80 * Scale.Y));
		private CTexture r表示するプレビュー画像;
		//private Surface sfAVI画像;
		private string str現在のファイル名;
		private CTexture txセンサ;
		private CTexture txセンサ光;
		private CTexture txパネル本体;
		private CTexture txプレビュー画像;
		private CTexture txプレビュー画像がないときの画像;
		private bool b新しいプレビューファイルを読み込んだ;
		private bool b新しいプレビューファイルをまだ読み込んでいない
		{
			get
			{
				return !this.b新しいプレビューファイルを読み込んだ;
			}
			set
			{
				this.b新しいプレビューファイルを読み込んだ = !value;
			}
		}

		//private unsafe void tサーフェイスをクリアする( Surface sf )
		//{
		//    DataRectangle rectangle = sf.LockRectangle( LockFlags.None );
		//    DataStream data = rectangle.Data;
		//    switch( ( rectangle.Pitch / sf.Description.Width ) )
		//    {
		//        case 4:
		//            {
		//                uint* numPtr = (uint*) data.DataPointer.ToPointer();
		//                for( int i = 0; i < sf.Description.Height; i++ )
		//                {
		//                    for( int j = 0; j < sf.Description.Width; j++ )
		//                    {
		//                        ( numPtr + ( i * sf.Description.Width ) )[ j ] = 0;
		//                    }
		//                }
		//                break;
		//            }
		//        case 2:
		//            {
		//                ushort* numPtr2 = (ushort*) data.DataPointer.ToPointer();
		//                for( int k = 0; k < sf.Description.Height; k++ )
		//                {
		//                    for( int m = 0; m < sf.Description.Width; m++ )
		//                    {
		//                        ( numPtr2 + ( k * sf.Description.Width ) )[ m ] = 0;
		//                    }
		//                }
		//                break;
		//            }
		//    }
		//    sf.UnlockRectangle();
		//}
		private void tプレビュー画像_動画の変更()
		{
			this.actAVI.Stop();
			if (this.rAVI != null)
			{
				this.rAVI.Dispose();
				this.rAVI = null;
			}
			//this.pAVIBmp = IntPtr.Zero;
			//this.nAVI再生開始時刻 = -1;
			if (!CDTXMania.Instance.ConfigIni.bStoicMode)
			{
				if (this.tプレビュー動画の指定があれば構築する())
				{
					return;
				}
				if (this.tプレビュー画像の指定があれば構築する())
				{
					return;
				}
				if (this.t背景画像があればその一部からプレビュー画像を構築する())
				{
					return;
				}
			}
			this.r表示するプレビュー画像 = this.txプレビュー画像がないときの画像;
			this.str現在のファイル名 = "";
		}
		private bool tプレビュー画像の指定があれば構築する()
		{
			Cスコア cスコア = CDTXMania.Instance.stage選曲.r現在選択中のスコア;
			if ((cスコア == null) || string.IsNullOrEmpty(cスコア.譜面情報.Preimage))
			{
				return false;
			}
			string str = cスコア.ファイル情報.フォルダの絶対パス + cスコア.譜面情報.Preimage;
			if (!str.Equals(this.str現在のファイル名))
			{
				TextureFactory.tテクスチャの解放(ref this.txプレビュー画像);
				this.str現在のファイル名 = str;
				if (!File.Exists(this.str現在のファイル名))
				{
					Trace.TraceWarning("ファイルが存在しません。({0})", new object[] { this.str現在のファイル名 });
					return false;
				}
				this.txプレビュー画像 = TextureFactory.tテクスチャの生成(this.str現在のファイル名, false);
				if (this.txプレビュー画像 != null)
				{
					this.r表示するプレビュー画像 = this.txプレビュー画像;
				}
				else
				{
					this.r表示するプレビュー画像 = this.txプレビュー画像がないときの画像;
				}
			}
			return true;
		}
		private bool tプレビュー動画の指定があれば構築する()
		{
			Cスコア cスコア = CDTXMania.Instance.stage選曲.r現在選択中のスコア;
			if ((CDTXMania.Instance.ConfigIni.bAVI && (cスコア != null)) && !string.IsNullOrEmpty(cスコア.譜面情報.Premovie))
			{
				string filename = cスコア.ファイル情報.フォルダの絶対パス + cスコア.譜面情報.Premovie;
				if (filename.Equals(this.str現在のファイル名))
				{
					return true;
				}
				if (this.rAVI != null)
				{
					this.rAVI.Dispose();
					this.rAVI = null;
				}
				this.str現在のファイル名 = filename;
				if (!File.Exists(this.str現在のファイル名))
				{
					Trace.TraceWarning("ファイルが存在しません。({0})", Path.GetFileName(this.str現在のファイル名));
					return false;
				}
				try
				{
					this.rAVI = new CDTX.CAVI(00, this.str現在のファイル名, "", CDTXMania.Instance.ConfigIni.nPlaySpeed);
					this.rAVI.OnDeviceCreated();
					this.actAVI.Start(EChannel.Movie, rAVI, 204, 269, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1);
				}
				catch (Exception e)
				{
					Trace.TraceError("動画の生成に失敗しました。({0})", Path.GetFileName(filename));
					Trace.TraceError("例外メッセージ:{0}", e.Message);
					Trace.TraceError("　　スタックトレース:{0}", e.StackTrace);
					this.rAVI = null;
				}
			}
			return false;
		}
		private bool t背景画像があればその一部からプレビュー画像を構築する()
		{
			Cスコア cスコア = CDTXMania.Instance.stage選曲.r現在選択中のスコア;
			if ((cスコア == null) || string.IsNullOrEmpty(cスコア.譜面情報.Backgound))
			{
				return false;
			}
			string path = cスコア.ファイル情報.フォルダの絶対パス + cスコア.譜面情報.Backgound;
			if (!path.Equals(this.str現在のファイル名))
			{
				if (!File.Exists(path))
				{
					Trace.TraceWarning("ファイルが存在しません。({0})", new object[] { path });
					return false;
				}
				TextureFactory.tテクスチャの解放(ref this.txプレビュー画像);
				this.str現在のファイル名 = path;
				Bitmap image = null;
				Bitmap bitmap2 = null;
				Bitmap bitmap3 = null;
				try
				{
					image = new Bitmap(this.str現在のファイル名);
					bitmap2 = new Bitmap(SampleFramework.GameWindowSize.Width, SampleFramework.GameWindowSize.Height);
					Graphics graphics = Graphics.FromImage(bitmap2);
					int x = 0;
					for (int i = 0; i < SampleFramework.GameWindowSize.Height; i += image.Height)
					{
						for (x = 0; x < SampleFramework.GameWindowSize.Width; x += image.Width)
						{
							graphics.DrawImage(image, x, i, image.Width, image.Height);
						}
					}
					graphics.Dispose();
					bitmap3 = new Bitmap(0xcc, 0x10d);
					graphics = Graphics.FromImage(bitmap3);
					graphics.DrawImage(bitmap2, 5, 5, new Rectangle(0x157, 0x6d, 0xcc, 0x10d), GraphicsUnit.Pixel);
					graphics.Dispose();
					this.txプレビュー画像 = new CTexture(CDTXMania.Instance.Device, bitmap3, CDTXMania.Instance.TextureFormat);
					this.r表示するプレビュー画像 = this.txプレビュー画像;
				}
				catch
				{
					Trace.TraceError("背景画像の読み込みに失敗しました。({0})", new object[] { this.str現在のファイル名 });
					this.r表示するプレビュー画像 = this.txプレビュー画像がないときの画像;
					return false;
				}
				finally
				{
					if (image != null)
					{
						image.Dispose();
					}
					if (bitmap2 != null)
					{
						bitmap2.Dispose();
					}
					if (bitmap3 != null)
					{
						bitmap3.Dispose();
					}
				}
			}
			return true;
		}
		private void t描画処理_ジャンル文字列()
		{
			C曲リストノード c曲リストノード = CDTXMania.Instance.stage選曲.r現在選択中の曲;
			Cスコア cスコア = CDTXMania.Instance.stage選曲.r現在選択中のスコア;
			if ((c曲リストノード != null) && (cスコア != null))
			{
				string str = "";
				switch (c曲リストノード.eノード種別)
				{
					case C曲リストノード.Eノード種別.SCORE:
						if ((c曲リストノード.strジャンル == null) || (c曲リストノード.strジャンル.Length <= 0))
						{
							if ((cスコア.譜面情報.ジャンル != null) && (cスコア.譜面情報.ジャンル.Length > 0))
							{
								str = cスコア.譜面情報.ジャンル;
							}
#if false  // #32644 2013.12.21 yyagi "Unknown"なジャンル表示を削除。DTX/BMSなどの種別表示もしない。
							else
							{
								switch( cスコア.譜面情報.曲種別 )
								{
									case CDTX.E種別.DTX:
										str = "DTX";
										break;

									case CDTX.E種別.GDA:
										str = "GDA";
										break;

									case CDTX.E種別.G2D:
										str = "G2D";
										break;

									case CDTX.E種別.BMS:
										str = "BMS";
										break;

									case CDTX.E種別.BME:
										str = "BME";
										break;
								}
								str = "Unknown";
							}
#endif
							break;
						}
						str = c曲リストノード.strジャンル;
						break;

					case C曲リストノード.Eノード種別.SCORE_MIDI:
						str = "MIDI";
						break;

					case C曲リストノード.Eノード種別.BOX:
						str = "MusicBox";
						break;

					case C曲リストノード.Eノード種別.BACKBOX:
						str = "BackBox";
						break;

					case C曲リストノード.Eノード種別.RANDOM:
						str = "Random";
						break;

					default:
						str = "Unknown";
						break;
				}
				CDTXMania.Instance.act文字コンソール.tPrint(
					this.n本体X + (int)(0x12 * Scale.X),
					this.n本体Y - (int)(1 * Scale.Y),
					C文字コンソール.Eフォント種別.赤細,
					str
				);
			}
		}
		private void t描画処理_センサ光()
		{
			int num = this.ctセンサ光.n現在の値;
			if (num < 12)
			{
				int x = this.n本体X + (int)(0xcc * Scale.X);
				int y = this.n本体Y + (int)(0x7b * Scale.Y);
				if (this.txセンサ光 != null)
				{
					this.txセンサ光.vc拡大縮小倍率 = new Vector3(1f, 1f, 1f);
					this.txセンサ光.n透明度 = 0xff;
					this.txセンサ光.t2D描画(
						CDTXMania.Instance.Device,
						x,
						y,
						new Rectangle(
							(int)((num % 4) * 0x40 * Scale.X),
							(int)((num / 4) * 0x40 * Scale.Y),
							(int)(0x40 * Scale.X),
							(int)(0x40 * Scale.Y)
						)
					);
				}
			}
			else if (num < 0x18)
			{
				int num4 = num - 11;
				double num5 = ((double)num4) / 11.0;
				double num6 = 1.0 + (num5 * 0.5);
				int num7 = (int)(64.0 * num6);
				int num8 = (int)(64.0 * num6);
				int x = ((this.n本体X + (int)(0xcc * Scale.X)) + (int)(0x20 * Scale.X)) - ((int)(num7 * Scale.X) / 2);
				int y = ((this.n本体Y + (int)(0x7b * Scale.Y)) + (int)(0x20 * Scale.Y)) - ((int)(num8 * Scale.Y) / 2);
				if (this.txセンサ光 != null)
				{
					this.txセンサ光.vc拡大縮小倍率 = new Vector3((float)num6, (float)num6, 1f);
					this.txセンサ光.n透明度 = (int)(255.0 * (1.0 - num5));
					this.txセンサ光.t2D描画(
						CDTXMania.Instance.Device,
						x,
						y,
						this.rcセンサ光
					);
				}
			}
		}
		private void t描画処理_センサ本体()
		{
			int x = this.n本体X + (int)(0xcd * Scale.X);
			int y = this.n本体Y - (int)(4 * Scale.Y);
			if (this.txセンサ != null)
			{
				this.txセンサ.t2D描画(CDTXMania.Instance.Device, x, y, this.rcセンサ本体上半分);
				y += (int)(0x80 * Scale.Y);
				this.txセンサ.t2D描画(CDTXMania.Instance.Device, x, y, this.rcセンサ本体下半分);
			}
		}
		private void t描画処理_パネル本体()
		{
			if (this.ct登場アニメ用.b終了値に達した || (this.txパネル本体 != null))
			{
				this.n本体X = (int)(8 * Scale.X);
				this.n本体Y = (int)(0x39 * Scale.Y);
			}
			else
			{
				double num = ((double)this.ct登場アニメ用.n現在の値) / 100.0;
				double num2 = Math.Cos((1.5 + (0.5 * num)) * Math.PI);
				this.n本体X = (int)(8 * Scale.X);
				this.n本体Y = (int)(0x39 * Scale.Y) - ((int)(this.txパネル本体.sz画像サイズ.Height * (1.0 - (num2 * num2))));
			}
			if (this.txパネル本体 != null)
			{
				this.txパネル本体.t2D描画(CDTXMania.Instance.Device, this.n本体X, this.n本体Y);
			}
		}
		private unsafe void t描画処理_プレビュー画像()
		{
			if (!CDTXMania.Instance.stage選曲.bスクロール中 && (((this.ct遅延表示 != null) && (this.ct遅延表示.n現在の値 > 0)) && !this.b新しいプレビューファイルをまだ読み込んでいない))
			{
				int x = this.n本体X + (int)(18 * Scale.X);
				int y = this.n本体Y + (int)(16 * Scale.Y);

				if (this.rAVI != null)
				{
					actAVI.t進行描画(x, y, 612, 605);
					return;
				}

				float f = ((float)this.ct遅延表示.n現在の値) / 100f;
				float mag = 0.9f + (0.1f * f);

				#region [ プレビュー画像表示 ]
				if (this.r表示するプレビュー画像 != null)
				{
					CPreviewMagnifier cmg = new CPreviewMagnifier(CPreviewMagnifier.EPreviewType.MusicSelect);
					cmg.GetMagnifier(this.r表示するプレビュー画像.sz画像サイズ.Width, this.r表示するプレビュー画像.sz画像サイズ.Height, mag, mag);

					int width = cmg.width;
					int height = cmg.height;
					this.r表示するプレビュー画像.vc拡大縮小倍率.X = cmg.magX;
					this.r表示するプレビュー画像.vc拡大縮小倍率.Y = cmg.magY;

					x += (int)((612 - width * cmg.magX) / 2);
					y += (int)((605 - height * cmg.magY) / 2);
					this.r表示するプレビュー画像.n透明度 = (int)(255f * f);
					this.r表示するプレビュー画像.t2D描画(CDTXMania.Instance.Device, x, y);
				}
				#endregion
			}
		}
		//-----------------
		#endregion
	}
}
