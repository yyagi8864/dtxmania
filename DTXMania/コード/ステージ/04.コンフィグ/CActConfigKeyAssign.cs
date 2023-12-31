﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using FDK;
using SharpDX;

using Rectangle = System.Drawing.Rectangle;
using Color = System.Drawing.Color;
using SlimDXKey = SlimDX.DirectInput.Key;

namespace DTXMania
{
	internal class CActConfigKeyAssign : CActivity
	{
		public bool bキー入力待ちの最中である
		{
			get
			{
				return this.bキー入力待ち;
			}
		}

		public void t開始(EPad pad, string strパッド名)
		{
			this.pad = pad;
			this.strパッド名 = strパッド名;
			for (int i = 0; i < CConfigXml.AssignableCodes - 2; i++)
			{
				this.structReset用KeyAssign[i].InputDevice = CDTXMania.Instance.ConfigIni.KeyAssign[pad][i].入力デバイス;
				this.structReset用KeyAssign[i].ID = CDTXMania.Instance.ConfigIni.KeyAssign[pad][i].ID;
				this.structReset用KeyAssign[i].Code = CDTXMania.Instance.ConfigIni.KeyAssign[pad][i].コード;
			}
		}

		public void tEnter押下()
		{
			if (!this.bキー入力待ち)
			{
				CDTXMania.Instance.Skin.sound決定音.t再生する();
				if (ptr == CConfigXml.AssignableCodes - 2)
				{
					for (int i = 0; i < CConfigXml.AssignableCodes - 2; i++)
					{
						CDTXMania.Instance.ConfigIni.KeyAssign[pad][i].CopyFrom(this.structReset用KeyAssign[i]);
					}
					return;
				}
				else if (ptr == CConfigXml.AssignableCodes - 1)
				{

					CDTXMania.Instance.stageコンフィグ.tアサイン完了通知();
					return;
				}
				this.bキー入力待ち = true;
			}
		}

		public void OnNext()
		{
			if (!this.bキー入力待ち)
			{
				CDTXMania.Instance.Skin.soundカーソル移動音.t再生する();
				ptr++;
				ptr %= CConfigXml.AssignableCodes;
			}
		}

		public void OnPrevious()
		{
			if (!this.bキー入力待ち)
			{
				CDTXMania.Instance.Skin.soundカーソル移動音.t再生する();
				--ptr;
				ptr += CConfigXml.AssignableCodes;
				ptr %= CConfigXml.AssignableCodes;
			}
		}

		public override void On活性化()
		{
			if (base.b活性化してない)
			{
				this.pad = EPad.Unknown;
				this.strパッド名 = "";
				this.ptr = 0;
				this.bキー入力待ち = false;
				this.structReset用KeyAssign = new CKeyAssign[CConfigXml.AssignableCodes - 2];
				for (int i = 0; i < this.structReset用KeyAssign.Length; ++i)
				{
					structReset用KeyAssign[i] = new CKeyAssign(EInputDevice.Unknown, 0, 0);
				}

				base.On活性化();
			}
		}

		public override void On非活性化()
		{
			if (base.b活性化してる)
			{
				TextureFactory.tテクスチャの解放(ref this.txカーソル);
				TextureFactory.tテクスチャの解放(ref this.txHitKeyダイアログ);
				base.On非活性化();
			}
		}

		public override void OnManagedリソースの作成()
		{
			if (base.b活性化してる)
			{
				string fontname = CDTXMania.Instance.Resources.Explanation("strCfgConfigurationKeyAssignFontFileName");
				string path = Path.Combine(@"Graphics\fonts", fontname);
				this.prvFont = new CPrivateFastFont(CSkin.Path(path), (int)(18 * Scale.Y));
				//this.prvFont = new CPrivateFastFont(CSkin.Path(@"Graphics\fonts\mplus-1p-heavy.ttf"), (int)(18 * Scale.Y)); // t項目リストの設定 の前に必要
				this.txカーソル = TextureFactory.tテクスチャの生成(CSkin.Path(@"Graphics\ScreenConfig menu cursor.png"), false);

				#region [ Hit key to assign ダイアログイメージ作成 ]
				string fontPath = CDTXMania.Instance.Resources.Explanation("strCfgConfigurationKeyAssignFontFileName");
				var prvFont = new CPrivateFastFont(CSkin.Path(Path.Combine(@"Graphics\fonts\", fontPath)), 30, FontStyle.Bold);
				var bmp = new Bitmap(CSkin.Path(@"Graphics\ScreenConfig hit key to assign dialog.png"));

				string strHitKey = CDTXMania.Instance.Resources.Explanation("strCfgHitKeyToAssign");
				var strComments = strHitKey.Split(new string[] { "\n" }, StringSplitOptions.None);

				Graphics g = Graphics.FromImage(bmp);

				int y = 20;
				foreach (var s in strComments)
				{
					string ss = s.Trim();
					var b = prvFont.DrawPrivateFont(ss, Color.White);
					int x = (bmp.Width - b.Width) / 2;
					g.DrawImage(b, x, y);
					b.Dispose();

					y += prvFont.RectStrings.Height;
				}
				g.Dispose();
				prvFont.Dispose();
				prvFont = null;

				this.txHitKeyダイアログ = TextureFactory.tテクスチャの生成(bmp, false);

				bmp.Dispose();
				bmp = null;
				#endregion

				base.OnManagedリソースの作成();
			}
		}

		public override void OnManagedリソースの解放()
		{
			if (base.b活性化してる)
			{
				TextureFactory.t安全にDisposeする(ref prvFont);
			}
		}

		public override int On進行描画()
		{
			if (base.b活性化してる)
			{
				if (this.bキー入力待ち)
				{
					if (CDTXMania.Instance.Input管理.Keyboard.bキーが押された((int)SlimDXKey.Escape))
					{
						CDTXMania.Instance.Skin.sound取消音.t再生する();
						this.bキー入力待ち = false;
						CDTXMania.Instance.Input管理.tポーリング(CDTXMania.Instance.bApplicationActive, false);
					}
					else if ((this.tキーチェックとアサイン_Keyboard() || this.tキーチェックとアサイン_MidiIn()) || (this.tキーチェックとアサイン_Joypad() || this.tキーチェックとアサイン_Mouse()))
					{
						this.bキー入力待ち = false;
						CDTXMania.Instance.Input管理.tポーリング(CDTXMania.Instance.bApplicationActive, false);
					}
				}
				else if (CDTXMania.Instance.Input管理.Keyboard.bキーが押された((int)SlimDXKey.Delete))
				{
					CDTXMania.Instance.Skin.sound決定音.t再生する();
					CDTXMania.Instance.ConfigIni.KeyAssign[this.pad][ptr].Reset();
				}

				if (this.txカーソル != null)
				{
					int stepX = 0x144;
					int stepY = 0x3e + (20 * (ptr + 1));
					this.txカーソル.vc拡大縮小倍率 = new Vector3(1f, 0.75f, 1f);
					this.txカーソル.t2D描画(CDTXMania.Instance.Device, stepX * Scale.X, stepY * Scale.Y - 14, new Rectangle(0, 0, (int)(0x10 * Scale.X), (int)(0x20 * Scale.Y)));
					stepX += 0x10;
					Rectangle rectangle = new Rectangle((int)(8 * Scale.X), 0, (int)(0x10 * Scale.X), (int)(0x20 * Scale.Y));
					for (int j = 0; j < 14; j++)
					{
						this.txカーソル.t2D描画(CDTXMania.Instance.Device, stepX * Scale.X, stepY * Scale.Y - 14, rectangle);
						stepX += 0x10;
					}
					this.txカーソル.t2D描画(CDTXMania.Instance.Device, stepX * Scale.X, stepY * Scale.Y - 14, new Rectangle((int)(0x10 * Scale.X), 0, (int)(0x10 * Scale.X), (int)(0x20 * Scale.Y)));
				}

				int num5 = 20;
				int x = 0x134;
				int y = 0x40;
				using (Bitmap bmpStr = prvFont.DrawPrivateFont(this.strパッド名, Color.White, Color.Black))
				{
					CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr);
					txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
					txStr.t2D描画(CDTXMania.Instance.Device, x * Scale.X, y * Scale.Y - 20);
					TextureFactory.tテクスチャの解放(ref txStr);
				}

				y += num5;
				string strParam;
				bool b強調;
				for (int i = 0; i < CConfigXml.AssignableCodes - 2; i++)
				{
					COptionKeyAssign stkeyassignArray = CDTXMania.Instance.ConfigIni.KeyAssign[this.pad][i];
					switch (stkeyassignArray.入力デバイス)
					{
						case EInputDevice.Keyboard:
							this.tアサインコードの描画_Keyboard(i + 1, x + 20, y, stkeyassignArray.ID, stkeyassignArray.コード, ptr == i);
							break;

						case EInputDevice.MIDIIn:
							this.tアサインコードの描画_MidiIn(i + 1, x + 20, y, stkeyassignArray.ID, stkeyassignArray.コード, ptr == i);
							break;

						case EInputDevice.JoyPad:
							this.tアサインコードの描画_Joypad(i + 1, x + 20, y, stkeyassignArray.ID, stkeyassignArray.コード, ptr == i);
							break;

						case EInputDevice.Mouse:
							this.tアサインコードの描画_Mouse(i + 1, x + 20, y, stkeyassignArray.ID, stkeyassignArray.コード, ptr == i);
							break;

						default:
							strParam = string.Format("{0,2}.", i + 1);
							b強調 = (ptr == i);
							using (Bitmap bmpStr = b強調 ?
									prvFont.DrawPrivateFont(strParam, Color.White, Color.Black, Color.Yellow, Color.OrangeRed) :
									prvFont.DrawPrivateFont(strParam, Color.White, Color.Black))
							{
								CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr, false);
								txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
								txStr.t2D描画(CDTXMania.Instance.Device, (x + 20) * Scale.X, y * Scale.Y - 20);
								TextureFactory.tテクスチャの解放(ref txStr);
							}
							break;
					}
					y += num5;
				}

				strParam = "Reset All Assign";
				b強調 = (ptr == CConfigXml.AssignableCodes - 2);
				using (Bitmap bmpStr = b強調 ?
						prvFont.DrawPrivateFont(strParam, Color.White, Color.Black, Color.Yellow, Color.OrangeRed) :
						prvFont.DrawPrivateFont(strParam, Color.White, Color.Black))
				{
					CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr, false);
					txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
					txStr.t2D描画(CDTXMania.Instance.Device, (x + 20) * Scale.X, y * Scale.Y - 20);
					TextureFactory.tテクスチャの解放(ref txStr);
				}

				y += num5;
				strParam = "<< Returnto List";
				b強調 = (ptr == CConfigXml.AssignableCodes - 1);
				using (Bitmap bmpStr = b強調 ?
						prvFont.DrawPrivateFont(strParam, Color.White, Color.Black, Color.Yellow, Color.OrangeRed) :
						prvFont.DrawPrivateFont(strParam, Color.White, Color.Black))
				{
					CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr, false);
					txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
					txStr.t2D描画(CDTXMania.Instance.Device, (x + 20) * Scale.X, y * Scale.Y - 20);
					TextureFactory.tテクスチャの解放(ref txStr);
				}

				if (this.bキー入力待ち && (this.txHitKeyダイアログ != null))
				{
					this.txHitKeyダイアログ.t2D描画(CDTXMania.Instance.Device, 0x185 * Scale.X, 0xd7 * Scale.Y);
				}
			}
			return 0;
		}


		// その他
		[StructLayout(LayoutKind.Sequential)]
		private struct STKEYLABEL
		{
			public int nCode;
			public string strLabel;
			public STKEYLABEL(int nCode, string strLabel)
			{
				this.nCode = nCode;
				this.strLabel = strLabel;
			}
		}

		private bool bキー入力待ち;

		private STKEYLABEL[] KeyLabel = new STKEYLABEL[] {
			#region [ *** ]
			new STKEYLABEL((int)SlimDXKey.Escape, "[ESC]"),
			new STKEYLABEL((int)SlimDXKey.D1, "[ 1 ]"),
			new STKEYLABEL((int)SlimDXKey.D2, "[ 2 ]"),
			new STKEYLABEL((int)SlimDXKey.D3, "[ 3 ]"),
			new STKEYLABEL((int)SlimDXKey.D4, "[ 4 ]"),
			new STKEYLABEL((int)SlimDXKey.D5, "[ 5 ]"),
			new STKEYLABEL((int)SlimDXKey.D6, "[ 6 ]"),
			new STKEYLABEL((int)SlimDXKey.D7, "[ 7 ]"),
			new STKEYLABEL((int)SlimDXKey.D8, "[ 8 ]"),
			new STKEYLABEL((int)SlimDXKey.D9, "[ 9 ]"),
			new STKEYLABEL((int)SlimDXKey.D0, "[ 0 ]"),
			new STKEYLABEL((int)SlimDXKey.Minus, "[ - ]"),
			new STKEYLABEL((int)SlimDXKey.Equals, "[ = ]"),
			new STKEYLABEL((int)SlimDXKey.Backspace, "[BSC]"),
			new STKEYLABEL((int)SlimDXKey.Tab, "[TAB]"),
			new STKEYLABEL((int)SlimDXKey.Q, "[ Q ]"),
			new STKEYLABEL((int)SlimDXKey.W, "[ W ]"),
			new STKEYLABEL((int)SlimDXKey.E, "[ E ]"),
			new STKEYLABEL((int)SlimDXKey.R, "[ R ]"),
			new STKEYLABEL((int)SlimDXKey.T, "[ T ]"),
			new STKEYLABEL((int)SlimDXKey.Y, "[ Y ]"),
			new STKEYLABEL((int)SlimDXKey.U, "[ U ]"),
			new STKEYLABEL((int)SlimDXKey.I, "[ I ]"),
			new STKEYLABEL((int)SlimDXKey.O, "[ O ]"),
			new STKEYLABEL((int)SlimDXKey.P, "[ P ]"),
			new STKEYLABEL((int)SlimDXKey.LeftBracket, "[ [ ]"),
			new STKEYLABEL((int)SlimDXKey.RightBracket, "[ ] ]"),
			new STKEYLABEL((int)SlimDXKey.Return, "[Enter]"),
			new STKEYLABEL((int)SlimDXKey.LeftControl, "[L-Ctrl]"),
			new STKEYLABEL((int)SlimDXKey.A, "[ A ]"),
			new STKEYLABEL((int)SlimDXKey.S, "[ S ]"),
			new STKEYLABEL((int)SlimDXKey.D, "[ D ]"),
			new STKEYLABEL((int)SlimDXKey.F, "[ F ]"),
			new STKEYLABEL((int)SlimDXKey.G, "[ G ]"),
			new STKEYLABEL((int)SlimDXKey.H, "[ H ]"),
			new STKEYLABEL((int)SlimDXKey.J, "[ J ]"),
			new STKEYLABEL((int)SlimDXKey.K, "[ K ]"),
			new STKEYLABEL((int)SlimDXKey.L, "[ L ]"),
			new STKEYLABEL((int)SlimDXKey.Semicolon, "[ ; ]"),
			new STKEYLABEL((int)SlimDXKey.Apostrophe, "[ ' ]"),
			new STKEYLABEL((int)SlimDXKey.Grave, "[ ` ]"),
			new STKEYLABEL((int)SlimDXKey.LeftShift, "[L-Shift]"),
			new STKEYLABEL((int)SlimDXKey.Backslash, @"[ \]"),
			new STKEYLABEL((int)SlimDXKey.Z, "[ Z ]"),
			new STKEYLABEL((int)SlimDXKey.X, "[ X ]"),
			new STKEYLABEL((int)SlimDXKey.C, "[ C ]"),
			new STKEYLABEL((int)SlimDXKey.V, "[ V ]"),
			new STKEYLABEL((int)SlimDXKey.B, "[ B ]"),
			new STKEYLABEL((int)SlimDXKey.N, "[ N ]"),
			new STKEYLABEL((int)SlimDXKey.M, "[ M ]"),
			new STKEYLABEL((int)SlimDXKey.Comma, "[ , ]"),
			new STKEYLABEL((int)SlimDXKey.Period, "[ . ]"),
			new STKEYLABEL((int)SlimDXKey.Slash, "[ / ]"),
			new STKEYLABEL((int)SlimDXKey.RightShift, "[R-Shift]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadStar, "[ * ]"),
			new STKEYLABEL((int)SlimDXKey.LeftAlt, "[L-Alt]"),
			new STKEYLABEL((int)SlimDXKey.Space, "[Space]"),
			new STKEYLABEL((int)SlimDXKey.CapsLock, "[CAPS]"),
			new STKEYLABEL((int)SlimDXKey.F1, "[F1]"),
			new STKEYLABEL((int)SlimDXKey.F2, "[F2]"),
			new STKEYLABEL((int)SlimDXKey.F3, "[F3]"),
			new STKEYLABEL((int)SlimDXKey.F4, "[F4]"),
			new STKEYLABEL((int)SlimDXKey.F5, "[F5]"),
			new STKEYLABEL((int)SlimDXKey.F6, "[F6]"),
			new STKEYLABEL((int)SlimDXKey.F7, "[F7]"),
			new STKEYLABEL((int)SlimDXKey.F8, "[F8]"),
			new STKEYLABEL((int)SlimDXKey.F9, "[F9]"),
			new STKEYLABEL((int)SlimDXKey.F10, "[F10]"),
			new STKEYLABEL((int)SlimDXKey.NumberLock, "[NumLock]"),
			new STKEYLABEL((int)SlimDXKey.ScrollLock, "[Scroll]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad7, "[NPad7]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad8, "[NPad8]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad9, "[NPad9]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadMinus, "[NPad-]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad4, "[NPad4]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad5, "[NPad5]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad6, "[NPad6]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadPlus, "[NPad+]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad1, "[NPad1]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad2, "[NPad2]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad3, "[NPad3]"),
			new STKEYLABEL((int)SlimDXKey.NumberPad0, "[NPad0]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadPeriod, "[NPad.]"),
			new STKEYLABEL((int)SlimDXKey.F11, "[F11]"),
			new STKEYLABEL((int)SlimDXKey.F12, "[F12]"),
			new STKEYLABEL((int)SlimDXKey.F13, "[F13]"),
			new STKEYLABEL((int)SlimDXKey.F14, "[F14]"),
			new STKEYLABEL((int)SlimDXKey.F15, "[F15]"),
			new STKEYLABEL((int)SlimDXKey.Kana, "[Kana]"),
			new STKEYLABEL((int)SlimDXKey.AbntC1, "[ ? ]"),
			new STKEYLABEL((int)SlimDXKey.Convert, "[Henkan]"),
			new STKEYLABEL((int)SlimDXKey.NoConvert, "[MuHenkan]"),
			new STKEYLABEL((int)SlimDXKey.Backslash, @"[ \ ]"),
			new STKEYLABEL((int)SlimDXKey.AbntC2, "[NPad.]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadEquals, "[NPad=]"),
			new STKEYLABEL((int)SlimDXKey.PreviousTrack, "[ ^ ]"),	// DIK_CIRCUMFLEX = 0x90
			new STKEYLABEL((int)SlimDXKey.AT, "[ @ ]"),
			new STKEYLABEL((int)SlimDXKey.Colon, "[ : ]"),
			new STKEYLABEL((int)SlimDXKey.Underline, "[ _ ]"),
			new STKEYLABEL((int)SlimDXKey.Kanji, "[Kanji]"),
			new STKEYLABEL((int)SlimDXKey.Stop, "[Stop]"),
			new STKEYLABEL((int)SlimDXKey.AX, "[AX]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadEnter, "[NPEnter]"),
			new STKEYLABEL((int)SlimDXKey.RightControl, "[R-Ctrl]"),
			new STKEYLABEL((int)SlimDXKey.Mute, "[Mute]"),
			new STKEYLABEL((int)SlimDXKey.Calculator, "[Calc]"),
			new STKEYLABEL((int)SlimDXKey.PlayPause, "[PlayPause]"),
			new STKEYLABEL((int)SlimDXKey.MediaStop, "[MediaStop]"),
			new STKEYLABEL((int)SlimDXKey.VolumeDown, "[Volume-]"),
			new STKEYLABEL((int)SlimDXKey.VolumeUp, "[Volume+]"),
			new STKEYLABEL((int)SlimDXKey.WebHome, "[WebHome]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadComma, "[NPad,]"),
			new STKEYLABEL((int)SlimDXKey.NumberPadSlash, "[ / ]"),
			new STKEYLABEL((int)SlimDXKey.PrintScreen, "[PrtScn]"),
			new STKEYLABEL((int)SlimDXKey.RightAlt, "[R-Alt]"),
			new STKEYLABEL((int)SlimDXKey.Pause, "[Pause]"),
			new STKEYLABEL((int)SlimDXKey.Home, "[Home]"),
			new STKEYLABEL((int)SlimDXKey.UpArrow, "[Up]"),
			new STKEYLABEL((int)SlimDXKey.PageUp, "[PageUp]"),
			new STKEYLABEL((int)SlimDXKey.LeftArrow, "[Left]"),
			new STKEYLABEL((int)SlimDXKey.RightArrow, "[Right]"),
			new STKEYLABEL((int)SlimDXKey.End, "[End]"),
			new STKEYLABEL((int)SlimDXKey.DownArrow, "[Down]"),
			new STKEYLABEL((int)SlimDXKey.PageDown, "[PageDown]"),
			new STKEYLABEL((int)SlimDXKey.Insert, "[Insert]"),
			new STKEYLABEL((int)SlimDXKey.Delete, "[Delete]"),
			new STKEYLABEL((int)SlimDXKey.LeftWindowsKey, "[L-Win]"),
			new STKEYLABEL((int)SlimDXKey.RightWindowsKey, "[R-Win]"),
			new STKEYLABEL((int)SlimDXKey.Applications, "[APP]"),
			new STKEYLABEL((int)SlimDXKey.Power, "[Power]"),
			new STKEYLABEL((int)SlimDXKey.Sleep, "[Sleep]"),
			new STKEYLABEL((int)SlimDXKey.Wake, "[Wake]"),
			#endregion
		};

		private EPad pad;
		int ptr;
		private CKeyAssign[] structReset用KeyAssign;
		private string strパッド名;
		private CTexture txHitKeyダイアログ;
		private CTexture txカーソル;
		private CPrivateFastFont prvFont;

		private void tアサインコードの描画_Joypad(int line, int x, int y, int nID, int nCode, bool b強調)
		{
			string str = string.Format("{0,2}. ", line);
			switch (nCode)
			{
				case 0:
					str += "Left";
					break;

				case 1:
					str += "Right";
					break;

				case 2:
					str += "Up";
					break;

				case 3:
					str += "Down";
					break;

				case 4:
					str += "Forward";
					break;

				case 5:
					str += "Back";
					break;

				default:
					if ((6 <= nCode) && (nCode < 6 + 128))              // other buttons (128 types)
					{
						str += string.Format("Button{0}", nCode - 5);
					}
					else if ((6 + 128 <= nCode) && (nCode < 6 + 128 + 8))       // POV HAT ( 8 types; 45 degrees per HATs)
					{
						str += string.Format("POV {0}", (nCode - 6 - 128) * 45);
					}
					else
					{
						str += string.Format("Code{0}", nCode);
					}
					break;
			}
			using (Bitmap bmpStr = b強調 ?
					prvFont.DrawPrivateFont(str, Color.White, Color.Black, Color.Yellow, Color.OrangeRed) :
					prvFont.DrawPrivateFont(str, Color.White, Color.Black))
			{
				CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr, false);
				txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
				txStr.t2D描画(CDTXMania.Instance.Device, x * Scale.X, y * Scale.Y - 20);
				TextureFactory.tテクスチャの解放(ref txStr);
			}
		}

		private void tアサインコードの描画_Keyboard(int line, int x, int y, int nID, int nCode, bool b強調)
		{
			string str = null;
			foreach (STKEYLABEL stkeylabel in this.KeyLabel)
			{
				if (stkeylabel.nCode == nCode)
				{
					str = string.Format("{0,2}. Key {1}", line, stkeylabel.strLabel);
					break;
				}
			}
			if (str == null)
			{
				str = string.Format("{0,2}. Key 0x{1:X2}", line, nCode);
			}

			using (Bitmap bmpStr = b強調 ?
					prvFont.DrawPrivateFont(str, Color.White, Color.Black, Color.Yellow, Color.OrangeRed) :
					prvFont.DrawPrivateFont(str, Color.White, Color.Black))
			{
				CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr, false);
				txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
				txStr.t2D描画(CDTXMania.Instance.Device, x * Scale.X, y * Scale.Y - 20);
				TextureFactory.tテクスチャの解放(ref txStr);
			}
		}

		private void tアサインコードの描画_MidiIn(int line, int x, int y, int nID, int nCode, bool b強調)
		{
			string str = string.Format("{0,2}. MidiIn #{1} code.{2}", line, nID, nCode);
			using (Bitmap bmpStr = b強調 ?
					prvFont.DrawPrivateFont(str, Color.White, Color.Black, Color.Yellow, Color.OrangeRed) :
					prvFont.DrawPrivateFont(str, Color.White, Color.Black))
			{
				CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr, false);
				txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
				txStr.t2D描画(CDTXMania.Instance.Device, x * Scale.X, y * Scale.Y - 20);
				TextureFactory.tテクスチャの解放(ref txStr);
			}
		}

		private void tアサインコードの描画_Mouse(int line, int x, int y, int nID, int nCode, bool b強調)
		{
			string str = string.Format("{0,2}. Mouse Button{1}", line, nCode);
			using (Bitmap bmpStr = b強調 ?
					prvFont.DrawPrivateFont(str, Color.White, Color.Black, Color.Yellow, Color.OrangeRed) :
					prvFont.DrawPrivateFont(str, Color.White, Color.Black))
			{
				CTexture txStr = TextureFactory.tテクスチャの生成(bmpStr, false);
				txStr.vc拡大縮小倍率 = new Vector3(0.75f, 0.75f, 1f);
				txStr.t2D描画(CDTXMania.Instance.Device, x * Scale.X, y * Scale.Y - 20);
				TextureFactory.tテクスチャの解放(ref txStr);
			}
		}

		private bool tキーチェックとアサイン_Joypad()
		{
			foreach (IInputDevice device in CDTXMania.Instance.Input管理.list入力デバイス)
			{
				if (device.e入力デバイス種別 == E入力デバイス種別.Joystick)
				{
					for (int i = 0; i < 6 + 0x80 + 8; i++)      // +6 for Axis, +8 for HAT
					{
						if (device.bキーが押された(i))
						{
							CDTXMania.Instance.Skin.sound決定音.t再生する();

							// #xxxxx: 2017.5.7 from: アサイン済みのキーと今回割り当てるキーが同じである場合は、削除されないようコードを未使用値(ここでは-1)にする。
							if (i == CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].コード)
								CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].コード = -1;

							CDTXMania.Instance.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice.JoyPad, device.ID, i);
							CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].入力デバイス = EInputDevice.JoyPad;
							CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].ID = device.ID;
							CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].コード = i;
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool tキーチェックとアサイン_Keyboard()
		{
			if (CDTXMania.Instance.Input管理.Keyboard == null)        // #38848 2019.1.7 yyagi; need to null check because it become null in case you've never connected keyboard (maybe so)
			{
				return false;
			}
			for ( int i = 0; i < 256; i++ )
			{
				if( i != (int) SlimDXKey.Escape &&
					i != (int) SlimDXKey.UpArrow &&
					i != (int) SlimDXKey.DownArrow &&
					i != (int) SlimDXKey.LeftArrow &&
					i != (int) SlimDXKey.RightArrow &&
					i != (int) SlimDXKey.Delete &&
					 CDTXMania.Instance.Input管理.Keyboard.bキーが押された( i ) )
				{
					CDTXMania.Instance.Skin.sound決定音.t再生する();

					// #xxxxx: 2017.5.7 from: アサイン済みのキーと今回割り当てるキーが同じである場合は、削除されないようコードを未使用値(ここでは-1)にする。
					if( i == CDTXMania.Instance.ConfigIni.KeyAssign[ pad ][ ptr ].コード )
						CDTXMania.Instance.ConfigIni.KeyAssign[ pad ][ ptr ].コード = -1;

					CDTXMania.Instance.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する( EInputDevice.Keyboard, 0, i );

					CDTXMania.Instance.ConfigIni.KeyAssign[ pad ][ ptr ].入力デバイス = EInputDevice.Keyboard;
					CDTXMania.Instance.ConfigIni.KeyAssign[ pad ][ ptr ].ID = 0;
					CDTXMania.Instance.ConfigIni.KeyAssign[ pad ][ ptr ].コード = i;
					return true;
				}
			}
			return false;
		}

		private bool tキーチェックとアサイン_MidiIn()
		{
			foreach (IInputDevice device in CDTXMania.Instance.Input管理.list入力デバイス)
			{
				if (device.e入力デバイス種別 == E入力デバイス種別.MidiIn)
				{
					for (int i = 0; i < 0x100; i++)
					{
						if (device.bキーが押された(i))
						{
							CDTXMania.Instance.Skin.sound決定音.t再生する();
							CDTXMania.Instance.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice.MIDIIn, device.ID, i);
							CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].入力デバイス = EInputDevice.MIDIIn;
							CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].ID = device.ID;
							CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].コード = i;
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool tキーチェックとアサイン_Mouse()
		{
			if (CDTXMania.Instance.Input管理.Mouse == null)		// #38848 2019.1.7 yyagi; need to null check because it become null in case you've never connected mouse (possibly. reported.)
			{
				return false;
			}
			for (int i = 0; i < 8; i++)
			{
				if (CDTXMania.Instance.Input管理.Mouse.bキーが押された(i))
				{
					CDTXMania.Instance.Skin.sound決定音.t再生する();
					CDTXMania.Instance.ConfigIni.t指定した入力が既にアサイン済みである場合はそれを全削除する(EInputDevice.Mouse, 0, i);
					CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].入力デバイス = EInputDevice.Mouse;
					CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].ID = 0;
					CDTXMania.Instance.ConfigIni.KeyAssign[pad][ptr].コード = i;
					return true;
				}
			}
			return false;
		}
	}
}
