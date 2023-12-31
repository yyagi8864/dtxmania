﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using FDK;

namespace DTXMania
{
	internal class CSkin : IDisposable
	{
		public class Cシステムサウンド : IDisposable
		{
			public static CSkin.Cシステムサウンド r最後に再生した排他システムサウンド;

			public bool bCompact対象;
			public bool bループ;
			public bool b読み込み未試行;
			public bool b読み込み成功;
			public bool b排他;
			public string strファイル名 = "";

			public bool b再生中
			{
				get
				{
					if (this.rSound[1 - this.n次に鳴るサウンド番号] == null)
						return false;

					return this.rSound[1 - this.n次に鳴るサウンド番号].b再生中;
				}
			}

			public int n位置_現在のサウンド
			{
				get
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.n位置;
				}
				set
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound != null)
						sound.n位置 = value;
				}
			}

			public int n位置_次に鳴るサウンド
			{
				get
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.n位置;
				}
				set
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound != null)
						sound.n位置 = value;
				}
			}

			public int n音量
			{
				set
				{
					for ( int i = 0; i < this.rSound.GetLength( 0 ); i++ )
					{
						CSound sound = this.rSound[ i ];
						if ( sound != null )
							sound.n音量 = value;
					}
				}
			}

			public int n音量_現在のサウンド
			{
				get
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound == null)
						return 0;

					return sound.n音量;
				}
				set
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound != null)
						sound.n音量 = value;
				}
			}

			public int n音量_次に鳴るサウンド
			{
				get
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound == null)
					{
						return 0;
					}
					return sound.n音量;
				}
				set
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound != null)
					{
						sound.n音量 = value;
					}
				}
			}

			public int n長さ_現在のサウンド
			{
				get
				{
					CSound sound = this.rSound[1 - this.n次に鳴るサウンド番号];
					if (sound == null)
					{
						return 0;
					}
					return sound.n総演奏時間ms;
				}
			}

			public int n長さ_次に鳴るサウンド
			{
				get
				{
					CSound sound = this.rSound[this.n次に鳴るサウンド番号];
					if (sound == null)
					{
						return 0;
					}
					return sound.n総演奏時間ms;
				}
			}


			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="strファイル名"></param>
			/// <param name="bループ"></param>
			/// <param name="b排他"></param>
			/// <param name="bCompact対象"></param>
			public Cシステムサウンド(string strファイル名, bool bループ, bool b排他, bool bCompact対象)
			{
				this.strファイル名 = strファイル名;
				this.bループ = bループ;
				this.b排他 = b排他;
				this.bCompact対象 = bCompact対象;
				this.b読み込み未試行 = true;
			}

			public Cシステムサウンド()
			{
				this.b読み込み未試行 = true;
			}

			public void t読み込み()
			{
				this.b読み込み未試行 = false;
				this.b読み込み成功 = false;
				if (string.IsNullOrEmpty(this.strファイル名))
					throw new InvalidOperationException("ファイル名が無効です。");

				if (!File.Exists(CSkin.Path(this.strファイル名)))
				{
					throw new FileNotFoundException(this.strファイル名);
				}

				for (int i = 0; i < 2; i++)
				{
					try
					{
						if (i > 0 && CDTXMania.Instance.Sound管理.CurrentSoundDeviceType == ESoundDeviceType.DirectSound )
						{
							this.rSound[i] = (CSound)this.rSound[0].Clone();
							if (this.rSound[i] == null)
							{
								Trace.TraceWarning("Clone failed: " + System.IO.Path.GetFileName(this.strファイル名));
							}
						}
						else
						{
							this.rSound[i] = CDTXMania.Instance.Sound管理.tサウンドを生成する(CSkin.Path(this.strファイル名));
						}
					}
					catch
					{
						this.rSound[i] = null;
						throw;
					}
				}
				this.b読み込み成功 = true;
			}

			public void t再生する()
			{
				if (this.b読み込み未試行)
				{
					try
					{
						t読み込み();
					}
					catch
					{
						this.b読み込み未試行 = false;
					}
				}
				if (this.b排他)
				{
					if (r最後に再生した排他システムサウンド != null)
						r最後に再生した排他システムサウンド.t停止する();

					r最後に再生した排他システムサウンド = this;
				}
				CSound sound = this.rSound[this.n次に鳴るサウンド番号];
				if (sound != null)
					sound.t再生を開始する(this.bループ);

				this.n次に鳴るサウンド番号 = 1 - this.n次に鳴るサウンド番号;
			}

			public void t停止する()
			{
				if (this.rSound[0] != null)
					this.rSound[0].t再生を停止する();

				if (this.rSound[1] != null)
					this.rSound[1].t再生を停止する();

				if (r最後に再生した排他システムサウンド == this)
					r最後に再生した排他システムサウンド = null;
			}

			public void tRemoveMixer()
			{
				if (CDTXMania.Instance.Sound管理.CurrentSoundDeviceType != ESoundDeviceType.DirectSound )
				{
					for (int i = 0; i < 2; i++)
					{
						if (this.rSound[i] != null)
						{
							CDTXMania.Instance.Sound管理.RemoveMixer(this.rSound[i]);
						}
					}
				}
			}

			#region [ IDisposable 実装 ]
			//-----------------
			public void Dispose()
			{
				if (!this.bDisposed済み)
				{
					for (int i = 0; i < 2; i++)
					{
						if (this.rSound[i] != null)
						{
							CDTXMania.Instance.Sound管理.tサウンドを破棄する(this.rSound[i]);
							this.rSound[i] = null;
						}
					}
					this.b読み込み成功 = false;
					this.bDisposed済み = true;
				}
			}
			//-----------------
			#endregion

			#region [ private ]
			//-----------------
			private bool bDisposed済み;
			private int n次に鳴るサウンド番号;
			private CSound[] rSound = new CSound[2];
			//-----------------
			#endregion
		}


		// プロパティ

		public Cシステムサウンド bgmオプション画面 = null;
		public Cシステムサウンド bgmコンフィグ画面 = null;
		public Cシステムサウンド bgm起動画面 = null;
		public Cシステムサウンド bgm選曲画面 = null;
		public Cシステムサウンド soundSTAGEFAILED音 = null;
		public Cシステムサウンド soundカーソル移動音 = null;
		public Cシステムサウンド soundゲーム開始音 = null;
		public Cシステムサウンド soundゲーム終了音 = null;
		public Cシステムサウンド soundステージクリア音 = null;
		public Cシステムサウンド soundタイトル音 = null;
		public Cシステムサウンド soundフルコンボ音 = null;
		public Cシステムサウンド sound歓声音 = null;
		public Cシステムサウンド sound曲読込開始音 = null;
		public Cシステムサウンド sound決定音 = null;
		public Cシステムサウンド sound取消音 = null;
		public Cシステムサウンド sound変更音 = null;
		public Cシステムサウンド soundClickHigh = null;
		public Cシステムサウンド soundClickLow  = null;
		public Cシステムサウンド soundClickBottom = null;
		public readonly int nシステムサウンド数 = (int) Eシステムサウンド.Count;
		public Cシステムサウンド this[Eシステムサウンド sound]
		{
			get
			{
				switch (sound)
				{
					case Eシステムサウンド.SOUNDカーソル移動音:
						return this.soundカーソル移動音;

					case Eシステムサウンド.SOUND決定音:
						return this.sound決定音;

					case Eシステムサウンド.SOUND変更音:
						return this.sound変更音;

					case Eシステムサウンド.SOUND取消音:
						return this.sound取消音;

					case Eシステムサウンド.SOUND歓声音:
						return this.sound歓声音;

					case Eシステムサウンド.SOUNDステージ失敗音:
						return this.soundSTAGEFAILED音;

					case Eシステムサウンド.SOUNDゲーム開始音:
						return this.soundゲーム開始音;

					case Eシステムサウンド.SOUNDゲーム終了音:
						return this.soundゲーム終了音;

					case Eシステムサウンド.SOUNDステージクリア音:
						return this.soundステージクリア音;

					case Eシステムサウンド.SOUNDフルコンボ音:
						return this.soundフルコンボ音;

					case Eシステムサウンド.SOUND曲読込開始音:
						return this.sound曲読込開始音;

					case Eシステムサウンド.SOUNDタイトル音:
						return this.soundタイトル音;

					case Eシステムサウンド.SOUNDClickHigh:
						return this.soundClickHigh;

					case Eシステムサウンド.SOUNDClickLow:
						return this.soundClickLow;

					case Eシステムサウンド.SOUNDClickBottom:
						return this.soundClickBottom;

					case Eシステムサウンド.BGM起動画面:
						return this.bgm起動画面;

					case Eシステムサウンド.BGMオプション画面:
						return this.bgmオプション画面;

					case Eシステムサウンド.BGMコンフィグ画面:
						return this.bgmコンフィグ画面;

					case Eシステムサウンド.BGM選曲画面:
						return this.bgm選曲画面;
				}
				throw new IndexOutOfRangeException();
			}
		}
		public Cシステムサウンド this[int index]
		{
			get
			{
				switch (index)
				{
					case (int) Eシステムサウンド.SOUNDカーソル移動音:
						return this.soundカーソル移動音;

					case (int) Eシステムサウンド.SOUND決定音:
						return this.sound決定音;

					case (int) Eシステムサウンド.SOUND変更音:
						return this.sound変更音;

					case (int) Eシステムサウンド.SOUND取消音:
						return this.sound取消音;

					case (int) Eシステムサウンド.SOUND歓声音:
						return this.sound歓声音;

					case (int) Eシステムサウンド.SOUNDステージ失敗音:
						return this.soundSTAGEFAILED音;

					case (int) Eシステムサウンド.SOUNDゲーム開始音:
						return this.soundゲーム開始音;

					case (int) Eシステムサウンド.SOUNDゲーム終了音:
						return this.soundゲーム終了音;

					case (int) Eシステムサウンド.SOUNDステージクリア音:
						return this.soundステージクリア音;

					case (int) Eシステムサウンド.SOUNDフルコンボ音:
						return this.soundフルコンボ音;

					case (int) Eシステムサウンド.SOUND曲読込開始音:
						return this.sound曲読込開始音;

					case (int) Eシステムサウンド.SOUNDタイトル音:
						return this.soundタイトル音;

					case (int) Eシステムサウンド.BGM起動画面:
						return this.bgm起動画面;

					case (int) Eシステムサウンド.BGMオプション画面:
						return this.bgmオプション画面;

					case (int) Eシステムサウンド.BGMコンフィグ画面:
						return this.bgmコンフィグ画面;

					case (int) Eシステムサウンド.BGM選曲画面:
						return this.bgm選曲画面;

					case (int) Eシステムサウンド.SOUNDClickHigh:
						return this.soundClickHigh;

					case (int) Eシステムサウンド.SOUNDClickLow:
						return this.soundClickLow;

					case (int)Eシステムサウンド.SOUNDClickBottom:
						return this.soundClickBottom;
				}
				throw new IndexOutOfRangeException();
			}
		}


		// スキンの切り替えについて・・・
		//
		// ・スキンの種類は大きく分けて2種類。Systemスキンとboxdefスキン。
		// 　前者はSystem/フォルダにユーザーが自らインストールしておくスキン。
		// 　後者はbox.defで指定する、曲データ制作者が提示するスキン。
		//
		// ・Config画面で、2種のスキンを区別無く常時使用するよう設定することができる。
		// ・box.defの#SKINPATH100 設定により、boxdefスキンを一時的に使用するよう設定する。
		// 　(box.defの効果の及ばない他のmuxic boxでは、当該boxdefスキンの有効性が無くなる)
		//
		// これを実現するために・・・
		// ・Systemスキンの設定情報と、boxdefスキンの設定情報は、分離して持つ。
		// 　(strSystem～～ と、strBoxDef～～～)
		// ・Config画面からは前者のみ書き換えできるようにし、
		// 　選曲画面からは後者のみ書き換えできるようにする。(SetCurrent...())
		// ・読み出しは両者から行えるようにすると共に
		// 　選曲画面用に二種の情報を区別しない読み出し方法も提供する(GetCurrent...)

		private object lockBoxDefSkin;
		public static bool bUseBoxDefSkin = true;           // box.defからのスキン変更を許容するか否か

		public string strSystemSkinRoot = null;
		public string[] strSystemSkinSubfolders = null;   // List<string>だとignoreCaseな検索が面倒なので、配列に逃げる :-)
		private string[] _strBoxDefSkinSubfolders = null;
		public string[] strBoxDefSkinSubfolders
		{
			get
			{
				lock (lockBoxDefSkin)
				{
					return _strBoxDefSkinSubfolders;
				}
			}
			set
			{
				lock (lockBoxDefSkin)
				{
					_strBoxDefSkinSubfolders = value;
				}
			}
		}     // 別スレッドからも書き込みアクセスされるため、スレッドセーフなアクセス法を提供

		private static string strSystemSkinSubfolderFullName;     // Config画面で設定されたスキン
		private static string strBoxDefSkinSubfolderFullName = "";    // box.defで指定されているスキン

		/// <summary>
		/// スキンパス名をフルパスで取得する
		/// </summary>
		/// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
		/// <returns></returns>
		public string GetCurrentSkinSubfolderFullName(bool bFromUserConfig)
		{
			if (!bUseBoxDefSkin || bFromUserConfig == true || strBoxDefSkinSubfolderFullName == "")
			{
				return strSystemSkinSubfolderFullName;
			}
			else
			{
				return strBoxDefSkinSubfolderFullName;
			}
		}

		/// <summary>
		/// スキンパス名をフルパスで設定する
		/// </summary>
		/// <param name="value">スキンパス名</param>
		/// <param name="bFromUserConfig">ユーザー設定用ならtrue, box.defからの設定ならfalse</param>
		public void SetCurrentSkinSubfolderFullName(string value, bool bFromUserConfig)
		{
			if (bFromUserConfig)
			{
				strSystemSkinSubfolderFullName = value;
			}
			else
			{
				strBoxDefSkinSubfolderFullName = value;
			}
		}

		public CSkin(string _strSkinSubfolderFullName, bool _bUseBoxDefSkin)
		{
			lockBoxDefSkin = new object();
			strSystemSkinSubfolderFullName = _strSkinSubfolderFullName;
			bUseBoxDefSkin = _bUseBoxDefSkin;
			InitializeSkinPathRoot();
			ReloadSkinPaths();
			PrepareReloadSkin();
		}

		public CSkin()
		{
			lockBoxDefSkin = new object();
			InitializeSkinPathRoot();
			bUseBoxDefSkin = true;
			ReloadSkinPaths();
			PrepareReloadSkin();
		}

		private string InitializeSkinPathRoot()
		{
			strSystemSkinRoot = System.IO.Path.Combine(CDTXMania.Instance.strEXEのあるフォルダ, "System" + System.IO.Path.DirectorySeparatorChar);
			return strSystemSkinRoot;
		}

		/// <summary>
		/// Skin(Sounds)を再読込する準備をする(再生停止,Dispose,ファイル名再設定)。
		/// あらかじめstrSkinSubfolderを適切に設定しておくこと。
		/// その後、ReloadSkinPaths()を実行し、strSkinSubfolderの正当性を確認した上で、本メソッドを呼び出すこと。
		/// 本メソッド呼び出し後に、ReloadSkin()を実行することで、システムサウンドを読み込み直す。
		/// ReloadSkin()の内容は本メソッド内に含めないこと。起動時はReloadSkin()相当の処理をCEnumSongsで行っているため。
		/// </summary>
		public void PrepareReloadSkin()
		{
			Trace.TraceInformation("SkinPath設定: {0}",
				(strBoxDefSkinSubfolderFullName == "") ?
				strSystemSkinSubfolderFullName :
				strBoxDefSkinSubfolderFullName
			);

			for (int i = 0; i < nシステムサウンド数; i++)
			{
				if (this[i] != null && this[i].b読み込み成功)
				{
					this[i].t停止する();
					this[i].Dispose();
				}
			}
			this.soundカーソル移動音 = new Cシステムサウンド(@"Sounds\Move.ogg", false, false, false);
			this.sound決定音 = new Cシステムサウンド(@"Sounds\Decide.ogg", false, false, false);
			this.sound変更音 = new Cシステムサウンド(@"Sounds\Change.ogg", false, false, false);
			this.sound取消音 = new Cシステムサウンド(@"Sounds\Cancel.ogg", false, false, true);
			this.sound歓声音 = new Cシステムサウンド(@"Sounds\Audience.ogg", false, false, true);
			this.soundSTAGEFAILED音 = new Cシステムサウンド(@"Sounds\Stage failed.ogg", false, true, true);
			this.soundゲーム開始音 = new Cシステムサウンド(@"Sounds\Game start.ogg", false, false, false);
			this.soundゲーム終了音 = new Cシステムサウンド(@"Sounds\Game end.ogg", false, true, false);
			this.soundステージクリア音 = new Cシステムサウンド(@"Sounds\Stage clear.ogg", false, true, true);
			this.soundフルコンボ音 = new Cシステムサウンド(@"Sounds\Full combo.ogg", false, false, true);
			this.sound曲読込開始音 = new Cシステムサウンド(@"Sounds\Now loading.ogg", false, true, true);
			this.soundタイトル音 = new Cシステムサウンド(@"Sounds\Title.ogg", false, true, false);
			this.soundClickHigh = new Cシステムサウンド( @"Sounds\Click_High.ogg", false, false, false );
			this.soundClickLow = new Cシステムサウンド( @"Sounds\Click_Low.ogg", false, false, false );
			this.soundClickBottom = new Cシステムサウンド(@"Sounds\Click_Bottom.ogg", false, false, false);
			this.bgm起動画面 = new Cシステムサウンド( @"Sounds\Setup BGM.ogg", true, true, false );
			this.bgmオプション画面 = new Cシステムサウンド(@"Sounds\Option BGM.ogg", true, true, false);
			this.bgmコンフィグ画面 = new Cシステムサウンド(@"Sounds\Config BGM.ogg", true, true, false);
			this.bgm選曲画面 = new Cシステムサウンド(@"Sounds\Select BGM.ogg", true, true, false);
		}

		public void ReloadSkin()
		{
			for (int i = 0; i < nシステムサウンド数; i++)
			{
				if (!this[i].b排他) // BGM系以外のみ読み込む。(BGM系は必要になったときに読み込む)
				{
					Cシステムサウンド cシステムサウンド = this[i];
					if (!CDTXMania.Instance.bコンパクトモード || cシステムサウンド.bCompact対象)
					{
						try
						{
							cシステムサウンド.t読み込み();
							Trace.TraceInformation("システムサウンドを読み込みました。({0})", cシステムサウンド.strファイル名);
						}
						catch (FileNotFoundException)
						{
							Trace.TraceWarning("システムサウンドが存在しません。({0})", cシステムサウンド.strファイル名);
						}
						catch (Exception e)
						{
							Trace.TraceError(e.Message);
							Trace.TraceWarning("システムサウンドの読み込みに失敗しました。({0})", cシステムサウンド.strファイル名);
						}
					}
				}
			}
		}


		/// <summary>
		/// Skinの一覧を再取得する。
		/// System/*****/Graphics (やSounds/) というフォルダ構成を想定している。
		/// もし再取得の結果、現在使用中のSkinのパス(strSystemSkinSubfloderFullName)が消えていた場合は、
		/// 以下の優先順位で存在確認の上strSystemSkinSubfolderFullNameを再設定する。
		/// 1. System/Default/
		/// 2. System/*****/ で最初にenumerateされたもの
		/// 3. System/ (従来互換)
		/// </summary>
		public void ReloadSkinPaths()
		{
			#region [ まず System/*** をenumerateする ]
			string[] tempSkinSubfolders = System.IO.Directory.GetDirectories(strSystemSkinRoot, "*");
			strSystemSkinSubfolders = new string[tempSkinSubfolders.Length];
			int size = 0;
			for (int i = 0; i < tempSkinSubfolders.Length; i++)
			{
				#region [ 検出したフォルダがスキンフォルダかどうか確認する]
				if (!bIsValid(tempSkinSubfolders[i]))
					continue;
				#endregion
				#region [ スキンフォルダと確認できたものを、strSkinSubfoldersに入れる ]
				// フォルダ名末尾に必ず\をつけておくこと。さもないとConfig読み出し側(必ず\をつける)とマッチできない
				if (tempSkinSubfolders[i][tempSkinSubfolders[i].Length - 1] != System.IO.Path.DirectorySeparatorChar)
				{
					tempSkinSubfolders[i] += System.IO.Path.DirectorySeparatorChar;
				}
				strSystemSkinSubfolders[size] = tempSkinSubfolders[i];
				Trace.TraceInformation("SkinPath検出: {0}", strSystemSkinSubfolders[size]);
				size++;
				#endregion
			}
			Trace.TraceInformation("SkinPath入力: {0}", strSystemSkinSubfolderFullName);
			Array.Resize(ref strSystemSkinSubfolders, size);
			Array.Sort(strSystemSkinSubfolders);  // BinarySearch実行前にSortが必要
			#endregion

			#region [ 現在のSkinパスがbox.defスキンをCONFIG指定していた場合のために、最初にこれが有効かチェックする。有効ならこれを使う。 ]
			if (bIsValid(strSystemSkinSubfolderFullName) &&
				Array.BinarySearch(strSystemSkinSubfolders, strSystemSkinSubfolderFullName,
				StringComparer.InvariantCultureIgnoreCase) < 0)
			{
				strBoxDefSkinSubfolders = new string[1] { strSystemSkinSubfolderFullName };
				return;
			}
			#endregion

			#region [ 次に、現在のSkinパスが存在するか調べる。あれば終了。]
			if (Array.BinarySearch(strSystemSkinSubfolders, strSystemSkinSubfolderFullName,
				StringComparer.InvariantCultureIgnoreCase) >= 0)
				return;
			#endregion
			#region [ カレントのSkinパスが消滅しているので、以下で再設定する。]
			/// 以下の優先順位で現在使用中のSkinパスを再設定する。
			/// 1. System/Default/
			/// 2. System/*****/ で最初にenumerateされたもの
			/// 3. System/ (従来互換)
			#region [ System/Default/ があるなら、そこにカレントSkinパスを設定する]
			string tempSkinPath_default = System.IO.Path.Combine(strSystemSkinRoot, "Default" + System.IO.Path.DirectorySeparatorChar);
			if (Array.BinarySearch(strSystemSkinSubfolders, tempSkinPath_default,
				StringComparer.InvariantCultureIgnoreCase) >= 0)
			{
				strSystemSkinSubfolderFullName = tempSkinPath_default;
				return;
			}
			#endregion
			#region [ System/SkinFiles.*****/ で最初にenumerateされたものを、カレントSkinパスに再設定する ]
			if (strSystemSkinSubfolders.Length > 0)
			{
				strSystemSkinSubfolderFullName = strSystemSkinSubfolders[0];
				return;
			}
			#endregion
			#region [ System/ に、カレントSkinパスを再設定する。]
			strSystemSkinSubfolderFullName = strSystemSkinRoot;
			strSystemSkinSubfolders = new string[1] { strSystemSkinSubfolderFullName };
			#endregion
			#endregion
		}

		public static string Path(string strファイルの相対パス)
		{
			if (strBoxDefSkinSubfolderFullName == "" || !bUseBoxDefSkin)
			{
				return System.IO.Path.Combine(strSystemSkinSubfolderFullName, strファイルの相対パス);
			}
			else
			{
				return System.IO.Path.Combine(strBoxDefSkinSubfolderFullName, strファイルの相対パス);
			}
		}

		/// <summary>
		/// フルパス名を与えると、スキン名として、ディレクトリ名末尾の要素を返す
		/// 例: C:\foo\bar\ なら、barを返す
		/// </summary>
		/// <param name="skinpath">スキンが格納されたパス名(フルパス)</param>
		/// <returns>スキン名</returns>
		public static string GetSkinName(string skinPathFullName)
		{
			if (skinPathFullName != null)
			{
				if (skinPathFullName == "")   // 「box.defで未定義」用
					skinPathFullName = strSystemSkinSubfolderFullName;
				string[] tmp = skinPathFullName.Split(System.IO.Path.DirectorySeparatorChar);
				return tmp[tmp.Length - 2];   // ディレクトリ名の最後から2番目の要素がスキン名(最後の要素はnull。元stringの末尾が\なので。)
			}
			return null;
		}

		public static string[] GetSkinName(string[] skinPathFullNames)
		{
			string[] ret = new string[skinPathFullNames.Length];
			for (int i = 0; i < skinPathFullNames.Length; i++)
			{
				ret[i] = GetSkinName(skinPathFullNames[i]);
			}
			return ret;
		}


		public string GetSkinSubfolderFullNameFromSkinName(string skinName)
		{
			foreach (string s in strSystemSkinSubfolders)
			{
				if (GetSkinName(s) == skinName)
					return s;
			}
			foreach (string b in strBoxDefSkinSubfolders)
			{
				if (GetSkinName(b) == skinName)
					return b;
			}
			return null;
		}

		/// <summary>
		/// スキンパス名が妥当かどうか
		/// (FHDのタイトル画像にアクセスできるかどうかで判定する)
		/// </summary>
		/// <param name="skinPathFullName">妥当性を確認するスキンパス(フルパス)</param>
		/// <returns>妥当ならtrue</returns>
		public bool bIsValid(string skinPathFullName)
		{
			string filePathTitle;
			filePathTitle = System.IO.Path.Combine(skinPathFullName, @"Graphics\ScreenTitle background.jpg");
			if (File.Exists(filePathTitle))
			{
				Bitmap bmp = Bitmap.FromFile(filePathTitle) as Bitmap;
				int width = bmp.Width;
				bmp.Dispose();
				if (width >= 1920)
				{
					return true;
				}
			}
			return false;
		}

		public void tRemoveMixerAll()
		{
			for (int i = 0; i < nシステムサウンド数; i++)
			{
				if (this[i] != null && this[i].b読み込み成功)
				{
					this[i].t停止する();
					this[i].tRemoveMixer();
				}
			}

		}
		#region [ IDisposable 実装 ]
		//-----------------
		public void Dispose()
		{
			if (!this.bDisposed済み)
			{
				for (int i = 0; i < this.nシステムサウンド数; i++)
					this[i].Dispose();

				this.bDisposed済み = true;
			}
		}
		//-----------------
		#endregion


		// その他

		#region [ private ]
		//-----------------
		private bool bDisposed済み;
		//-----------------
		#endregion

	}
}
