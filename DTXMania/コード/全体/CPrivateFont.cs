﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using SharpDX;
using FDK;

using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using GraphicPath = System.Drawing.Drawing2D.GraphicsPath;

namespace DTXMania
{
	/// <summary>
	/// プライベートフォントでの描画を扱うクラス。
	/// </summary>
	/// <exception cref="FileNotFoundException">フォントファイルが見つからない時に例外発生</exception>
	/// <exception cref="ArgumentException">スタイル指定不正時に例外発生</exception>
	/// <remarks>
	/// 簡単な使い方
	/// CPrivateFont prvFont = new CPrivateFont( CSkin.Path( @"Graphics\fonts\mplus-1p-bold.ttf" ), 36 );	// プライベートフォント
	/// とか
	/// CPrivateFont prvFont = new CPrivateFont( new FontFamily("MS UI Gothic"), 36, FontStyle.Bold );		// システムフォント
	/// とかした上で、
	/// Bitmap bmp = prvFont.DrawPrivateFont( "ABCDE", Color.White, Color.Black );							// フォント色＝白、縁の色＝黒の例。縁の色は省略可能
	/// とか
	/// Bitmap bmp = prvFont.DrawPrivateFont( "ABCDE", Color.White, Color.Black, Color.Yellow, Color.OrangeRed ); // 上下グラデーション(Yellow→OrangeRed)
	/// とかして、
	/// CTexture ctBmp = TextureFactory.tテクスチャの生成( bmp, false );
	/// ctBMP.t2D描画( ～～～ );
	/// で表示してください。
	///  
	/// 注意点
	/// 任意のフォントでのレンダリングは結構負荷が大きいので、なるべくなら描画フレーム毎にフォントを再レンダリングするようなことはせず、
	/// 一旦レンダリングしたものを描画に使い回すようにしてください。
	/// また、長い文字列を与えると、返されるBitmapも横長になります。この横長画像をそのままテクスチャとして使うと、
	/// 古いPCで問題を発生させやすいです。これを回避するには、一旦Bitmapとして取得したのち、256pixや512pixで分割して
	/// テクスチャに定義するようにしてください。FDKをお使いの場合は、CTexture()の代わりにCTextureAf()を使うと、
	/// このような縦長/横長の画像をクラス内部で2^n平方の正方形に近いテクスチャに折りたたんで登録する一方で、
	/// 表示時は縦長/横長のままのテクスチャとして扱うことができて便利です。
	/// </remarks>
	public class CPrivateFont : IDisposable
	{
		/// <summary>
		/// プライベートフォントのFontクラス。CPrivateFont()の初期化後に使用可能となる。
		/// プライベートフォントでDrawString()したい場合にご利用ください。
		/// </summary>
		public Font font
		{
			get => _font;
		}

		/// <summary>
		/// フォント登録失敗時に代替使用するフォント名。システムフォントのみ設定可能。
		/// 後日外部指定できるようにします。(＝コンストラクタで指定できるようにします)
		/// </summary>
		private string strAlternativeFont = "MS PGothic";


		#region [ コンストラクタ ]
		public CPrivateFont(FontFamily fontfamily, int pt, FontStyle style)
		{
			Initialize(null, null, fontfamily, pt, style);
		}
		public CPrivateFont(FontFamily fontfamily, int pt)
		{
			Initialize(null, null, fontfamily, pt, FontStyle.Regular);
		}
		public CPrivateFont(string fontpath, FontFamily fontfamily, int pt, FontStyle style)
		{
			Initialize(fontpath, null, fontfamily, pt, style);
		}
		public CPrivateFont(string fontpath, int pt, FontStyle style)
		{
			Initialize(fontpath, null, null, pt, style);
		}
		public CPrivateFont(string fontpath, int pt)
		{
			Initialize(fontpath, null, null, pt, FontStyle.Regular);
		}
		public CPrivateFont()
		{
			//throw new ArgumentException("CPrivateFont: 引数があるコンストラクタを使用してください。");
		}
		#endregion

		protected void Initialize(string fontpath, string baseFontPath, FontFamily fontfamily, int pt, FontStyle style)
		{
			this._pfc = null;
			this._fontfamily = null;
			this._font = null;
			this._pt = pt;
			this._rectStrings = new Rectangle(0, 0, 0, 0);
			this._ptOrigin = new Point(0, 0);
			this.bDispose完了済み = false;
			this._baseFontname = baseFontPath;

			if (fontfamily != null)
			{
				this._fontfamily = fontfamily;
			}
			else
			{
				if (Path.GetFileName(fontpath) == "")
				{
					Trace.TraceWarning($"No font filename is specified (only path is specified, etc). Trying to use MS PGothic as alternative. ({fontpath}, {baseFontPath}, {fontfamily}, {pt}, {style})");
					_fontfamily = null;
				}
				else
				{
					try
					{
						if (Path.GetExtension(fontpath) != "")
						{
							// ttfなどを指定した場合
							this._pfc = new System.Drawing.Text.PrivateFontCollection();    //PrivateFontCollectionオブジェクトを作成する
							this._pfc.AddFontFile(fontpath);                                //PrivateFontCollectionにフォントを追加する
							_fontfamily = _pfc.Families[0];
						}
						else
						{
							// "MS Gothic"などを指定した場合
							this._fontfamily = new FontFamily(Path.GetFileNameWithoutExtension(fontpath));
						}
					}
					catch (Exception e) when (e is System.IO.FileNotFoundException || e is System.Runtime.InteropServices.ExternalException)
					{
						Trace.TraceWarning(e.Message);
						Trace.TraceWarning("プライベートフォントの追加に失敗しました({0})。代わりにMS PGothicの使用を試みます。", fontpath);
						//throw new FileNotFoundException( "プライベートフォントの追加に失敗しました。({0})", Path.GetFileName( fontpath ) );
						//return;

						_fontfamily = null;
					}
					catch (IndexOutOfRangeException e)
					{
						Trace.TraceWarning(e.Message);
						Trace.TraceWarning($"AddFontFile() succeeded, but not reflected to the Array of Families. Failed to add PrivateFont({fontpath}).");
						_fontfamily = null;
					}

					//foreach ( FontFamily ff in _pfc.Families )
					//{
					//	Debug.WriteLine( "fontname=" + ff.Name );
					//	if ( ff.Name == Path.GetFileNameWithoutExtension( fontpath ) )
					//	{
					//		_fontfamily = ff;
					//		break;
					//	}
					//}
					//if ( _fontfamily == null )
					//{
					//	Trace.TraceError( "プライベートフォントの追加後、検索に失敗しました。({0})", fontpath );
					//	return;
					//}
				}

			}

			// 指定されたフォントスタイルが適用できない場合は、フォント内で定義されているスタイルから候補を選んで使用する
			// 何もスタイルが使えないようなフォントなら、例外を出す。
			if (_fontfamily != null)
			{
				if (!_fontfamily.IsStyleAvailable(style))
				{
					FontStyle[] FS = { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Underline, FontStyle.Strikeout };
					style = FontStyle.Regular | FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout;  // null非許容型なので、代わりに全盛をNGワードに設定
					foreach (FontStyle ff in FS)
					{
						if (this._fontfamily.IsStyleAvailable(ff))
						{
							style = ff;
							Trace.TraceWarning("フォント{0}へのスタイル指定を、{1}に変更しました。", Path.GetFileName(fontpath), style.ToString());
							break;
						}
					}
					if (style == (FontStyle.Regular | FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout))
					{
						Trace.TraceWarning("フォント{0}は適切なスタイル{1}を選択できませんでした。", Path.GetFileName(fontpath), style.ToString());
					}
				}
				//this._font = new Font(this._fontfamily, pt, style);			//PrivateFontCollectionの先頭のフォントのFontオブジェクトを作成する
				float emSize = pt * 96.0f / 72.0f;
				this._font = new Font(this._fontfamily, emSize, style, GraphicsUnit.Pixel); //PrivateFontCollectionの先頭のフォントのFontオブジェクトを作成する
																																										//HighDPI対応のため、pxサイズで指定
			}
			else
			// フォントファイルが見つからなかった場合 (MS PGothicを代わりに指定する)
			{
				float emSize = pt * 96.0f / 72.0f;
				this._font = new Font(strAlternativeFont, emSize, style, GraphicsUnit.Pixel); //MS PGothicのFontオブジェクトを作成する
				FontFamily[] ffs = new System.Drawing.Text.InstalledFontCollection().Families;
				int lcid = System.Globalization.CultureInfo.GetCultureInfo("en-us").LCID;
				foreach (FontFamily ff in ffs)
				{
					// Trace.WriteLine( lcid ) );
					if (ff.GetName(lcid) == strAlternativeFont)
					{
						this._fontfamily = ff;
						Trace.TraceInformation($"{strAlternativeFont}を代わりに指定しました。");
						return;
					}
				}
				throw new FileNotFoundException($"プライベートフォントの追加に失敗し、{strAlternativeFont}での代替処理にも失敗しました。({Path.GetFileName(fontpath)})");
			}
		}

		[Flags]
		protected enum DrawMode
		{
			Normal,
			Edge,
			Gradation
		}

		#region [ DrawPrivateFontのオーバーロード群 ]
		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <returns>描画済テクスチャ</returns>
		public Bitmap DrawPrivateFont(string drawstr, Color fontColor, Size? sz = null)
		{
			return DrawPrivateFont(drawstr, DrawMode.Normal, fontColor, Color.White, Color.White, Color.White, sz);
		}

		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="edgeColor">縁取色</param>
		/// <returns>描画済テクスチャ</returns>
		public Bitmap DrawPrivateFont(string drawstr, Color fontColor, Color edgeColor, Size? sz = null)
		{
			return DrawPrivateFont(drawstr, DrawMode.Edge, fontColor, edgeColor, Color.White, Color.White, sz);
		}

		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="gradationTopColor">グラデーション 上側の色</param>
		/// <param name="gradationBottomColor">グラデーション 下側の色</param>
		/// <returns>描画済テクスチャ</returns>
		//public Bitmap DrawPrivateFont( string drawstr, Color fontColor, Color gradationTopColor, Color gradataionBottomColor )
		//{
		//    return DrawPrivateFont( drawstr, DrawMode.Gradation, fontColor, Color.White, gradationTopColor, gradataionBottomColor );
		//}

		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="edgeColor">縁取色</param>
		/// <param name="gradationTopColor">グラデーション 上側の色</param>
		/// <param name="gradationBottomColor">グラデーション 下側の色</param>
		/// <returns>描画済テクスチャ</returns>
		public Bitmap DrawPrivateFont(string drawstr, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradataionBottomColor, Size? sz = null)
		{
			return DrawPrivateFont(drawstr, DrawMode.Edge | DrawMode.Gradation, fontColor, edgeColor, gradationTopColor, gradataionBottomColor,sz);
		}

#if こちらは使わない // (Bitmapではなく、CTextureを返す版)
		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <returns>描画済テクスチャ</returns>
		public CTexture DrawPrivateFont( string drawstr, Color fontColor )
		{
			Bitmap bmp = DrawPrivateFont( drawstr, DrawMode.Normal, fontColor, Color.White, Color.White, Color.White );
			return TextureFactory.tテクスチャの生成( bmp, false );
		}

		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="edgeColor">縁取色</param>
		/// <returns>描画済テクスチャ</returns>
		public CTexture DrawPrivateFont( string drawstr, Color fontColor, Color edgeColor )
		{
			Bitmap bmp = DrawPrivateFont( drawstr, DrawMode.Edge, fontColor, edgeColor, Color.White, Color.White );
			return TextureFactory.tテクスチャの生成( bmp, false );
		}

		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="gradationTopColor">グラデーション 上側の色</param>
		/// <param name="gradationBottomColor">グラデーション 下側の色</param>
		/// <returns>描画済テクスチャ</returns>
		//public CTexture DrawPrivateFont( string drawstr, Color fontColor, Color gradationTopColor, Color gradataionBottomColor )
		//{
		//    Bitmap bmp = DrawPrivateFont( drawstr, DrawMode.Gradation, fontColor, Color.White, gradationTopColor, gradataionBottomColor );
		//	  return TextureFactory.tテクスチャの生成( bmp, false );
		//}

		/// <summary>
		/// 文字列を描画したテクスチャを返す
		/// </summary>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="edgeColor">縁取色</param>
		/// <param name="gradationTopColor">グラデーション 上側の色</param>
		/// <param name="gradationBottomColor">グラデーション 下側の色</param>
		/// <returns>描画済テクスチャ</returns>
		public CTexture DrawPrivateFont( string drawstr, Color fontColor, Color edgeColor,  Color gradationTopColor, Color gradataionBottomColor )
		{
			Bitmap bmp = DrawPrivateFont( drawstr, DrawMode.Edge | DrawMode.Gradation, fontColor, edgeColor, gradationTopColor, gradataionBottomColor );
			return TextureFactory.tテクスチャの生成( bmp, false );
		}
#endif
		#endregion

		/// <summary>
		/// 文字列を描画したテクスチャを返す(メイン処理)
		/// </summary>
		/// <param name="rectDrawn">描画された領域</param>
		/// <param name="ptOrigin">描画文字列</param>
		/// <param name="drawstr">描画文字列</param>
		/// <param name="drawmode">描画モード</param>
		/// <param name="fontColor">描画色</param>
		/// <param name="edgeColor">縁取色</param>
		/// <param name="gradationTopColor">グラデーション 上側の色</param>
		/// <param name="gradationBottomColor">グラデーション 下側の色</param>
		/// <param name="sz">描画領域(省略可; 省略時は横長の改行なし・中央寄せで、指定時は描画領域内に改行あり・Near寄せでbitmapを生成)</param>
		/// <returns>描画済テクスチャ</returns>
		protected Bitmap DrawPrivateFont(string drawstr, DrawMode drawmode, Color fontColor, Color edgeColor, Color gradationTopColor, Color gradationBottomColor, Size? sz = null)
		{
			if (this._fontfamily == null || drawstr == null || drawstr == "")
			{
				// nullを返すと、その後bmp→texture処理や、textureのサイズを見て・・の処理で全部例外が発生することになる。
				// それは非常に面倒なので、最小限のbitmapを返してしまう。
				// まずはこの仕様で進めますが、問題有れば(上位側からエラー検出が必要であれば)例外を出したりエラー状態であるプロパティを定義するなり検討します。
				if (drawstr != "")
				{
					Trace.TraceWarning("DrawPrivateFont()の入力不正。最小値のbitmapを返します。");
				}
				_rectStrings = new Rectangle(0, 0, 0, 0);
				_ptOrigin = new Point(0, 0);
				return new Bitmap(1, 1);
			}
			bool bEdge = drawmode.HasFlag(DrawMode.Edge);
			bool bGradation = drawmode.HasFlag(DrawMode.Gradation);

			// 縁取りの縁のサイズは、とりあえずフォントの大きさの1/4とする
			int nEdgePt = (bEdge) ? _pt / 4 : _pt /8;

			// 描画サイズを測定する (外部から描画領域を指定した場合は、それを使う)
			Size stringSize;
			if (sz.HasValue)
			{
				stringSize = sz.Value;
			}
			else
			{
				using (Bitmap b = new Bitmap(1, 1))
				using (Graphics g = Graphics.FromImage(b))
				using (StringFormat sf = new StringFormat(StringFormat.GenericTypographic))
				{
					//g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					//sf.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

					//SizeF sizef = g.MeasureString(drawstr, this._font, int.MaxValue, sf) ;
					//stringSize = sizef.ToSize();
					stringSize = System.Windows.Forms.TextRenderer.MeasureText(drawstr, this._font, new Size(int.MaxValue, int.MaxValue),
						System.Windows.Forms.TextFormatFlags.NoPrefix |
						System.Windows.Forms.TextFormatFlags.NoPadding |
						System.Windows.Forms.TextFormatFlags.Bottom
					);

					//Rectangle rc = MeasureStringPrecisely(g, drawstr, this._font, new Size(stringSize.Width * 2, stringSize.Height * 2), sf);
					//stringSize = new Size(rc.Width, rc.Height);
				}
			}


			// 文字数をカウントする (横幅に文字数*2の縁取り幅を確保するために用いる)
			System.Globalization.StringInfo si = new System.Globalization.StringInfo(drawstr);
			int len = si.LengthInTextElements;

			Size stringSizeWithEdge = sz.HasValue ?
				//	sz.Value : new Size((int)(stringSize.Width + nEdgePt * len), stringSize.Height + nEdgePt * 2);
				//sz.Value : stringSize;
				sz.Value : new Size((int)(stringSize.Width * 1.05f), stringSize.Height);

			//取得した描画サイズを基に、描画先のbitmapを作成する
			//Bitmap bmp = new Bitmap(stringSize.Width + nEdgePt * 2, stringSize.Height + nEdgePt * 2);
			Bitmap bmp = new Bitmap(stringSizeWithEdge.Width, stringSizeWithEdge.Height);
			bmp.MakeTransparent();

			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (StringFormat sf = new StringFormat())
				{
					if (sz.HasValue)
					{
						// 画面上部（垂直方向位置）
						sf.LineAlignment = StringAlignment.Near;
						// 画面左（水平方向位置）
						sf.Alignment = StringAlignment.Near;
						sf.FormatFlags = StringFormatFlags.LineLimit;
					}
					else
					{
						// 画面上（垂直方向位置）
						sf.LineAlignment = StringAlignment.Near;
						// 画面中央（水平方向位置）
						//sf.Alignment = StringAlignment.Center;
						sf.Alignment = StringAlignment.Near;
						sf.FormatFlags = StringFormatFlags.NoWrap;
					}
					// レイアウト枠
					//Rectangle r = new Rectangle(0, 0, stringSize.Width + nEdgePt * 2, stringSize.Height + nEdgePt * 2);
					//Rectangle r = new Rectangle(0, 0, stringSizeWithEdge.Width, stringSizeWithEdge.Height);
					Rectangle r = (sz.HasValue)?
						new Rectangle(0, 0, (int)(stringSize.Width),        stringSize.Height) :
						new Rectangle(0, 0, (int)(stringSize.Width * 1.2f), stringSize.Height);

					// 縁取り有りの描画
					if (bEdge)
					{
						// DrawPathで、ポイントサイズを使って描画するために、DPIを使って単位変換する
						// (これをしないと、単位が違うために、小さめに描画されてしまう)
						float sizeInPixels = _font.SizeInPoints * g.DpiY / 72;  // 1 inch = 72 points

						using (GraphicsPath gp = new GraphicsPath())
						{
							gp.AddString(drawstr, this._fontfamily, (int)this._font.Style, sizeInPixels, r, sf);

							// 縁取りを描画する
							using (Pen p = new Pen(edgeColor, nEdgePt))
							{
								p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
								g.DrawPath(p, gp);

								// 塗りつぶす
								using (Brush br = bGradation ?
									new LinearGradientBrush(r, gradationTopColor, gradationBottomColor, LinearGradientMode.Vertical) as Brush :
									new SolidBrush(fontColor) as Brush)
								{
									g.FillPath(br, gp);
								}
							}
						}
					}
					else
					{
						// 縁取りなしの描画
						using (Brush br = new SolidBrush(fontColor))
						{
							g.DrawString(drawstr, _font, br, 0f, 0f);
						}
						// System.Windows.Forms.TextRenderer.DrawText(g, drawstr, _font, new Point(0, 0), fontColor);
					}
#if debug表示
			g.DrawRectangle( new Pen( Color.White, 1 ), new Rectangle( 1, 1, stringSizeWithEdge.Width-1, stringSizeWithEdge.Height-1 ) );
			g.DrawRectangle( new Pen( Color.Green, 1 ), new Rectangle( 0, 0, bmp.Width - 1, bmp.Height - 1 ) );
#endif
					_rectStrings = new Rectangle(0, 0, stringSize.Width, stringSize.Height);
					_ptOrigin = new Point(nEdgePt * 2, nEdgePt * 2);
				}
			}

			return bmp;
		}

		/// <summary>
		/// 最後にDrawPrivateFont()した文字列の描画領域を取得します。
		/// </summary>
		public Rectangle RectStrings
		{
			get
			{
				return _rectStrings;
			}
			protected set
			{
				_rectStrings = value;
			}
		}
		public Point PtOrigin
		{
			get
			{
				return _ptOrigin;
			}
			protected set
			{
				_ptOrigin = value;
			}
		}

		public float Size
		{
			get
			{
				if (_font != null)
				{
					return _font.Size;
				}
				return 0f;
			}
		}

		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDispose完了済み)
			{
				if (this._font != null)
				{
					this._font.Dispose();
					this._font = null;
				}
				if (this._pfc != null)
				{
					this._pfc.Dispose();
					this._pfc = null;
				}
                if (this._fontfamily != null)
                {
                    this._fontfamily.Dispose();
                    this._fontfamily = null;
                }

                this.bDispose完了済み = true;
			}
		}
		//-----------------
		#endregion

		#region [ private ]
		//-----------------
		protected bool bDispose完了済み;
		protected Font _font = null;

		private System.Drawing.Text.PrivateFontCollection _pfc;
		private FontFamily _fontfamily = null;
		private int _pt;
		private Rectangle _rectStrings;
		private Point _ptOrigin;
		private string _baseFontname = null;
		//-----------------
		#endregion
	}
}
