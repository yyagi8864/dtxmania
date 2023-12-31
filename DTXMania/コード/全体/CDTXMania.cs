﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Management;
using SharpDX;
using SharpDX.Direct3D9;
using FDK;
using SampleFramework;
using System.Runtime.Serialization;
using System.Runtime;
using System.Xml;

using Point = System.Drawing.Point;
using Color = System.Drawing.Color;

namespace DTXMania
{
	internal class CDTXMania : Game
	{
		// プロパティ
		#region [ properties ]
		public static readonly string VERSION = "119(211020)";
		//public static readonly string SLIMDXDLL = "c_net20x86_Jun2010";
		public static readonly string D3DXDLL = "d3dx9_43.dll";     // June 2010
		//public static readonly string D3DXDLL = "d3dx9_42.dll";	// February 2010
		//public static readonly string D3DXDLL = "d3dx9_41.dll";	// March 2009
		private static CDTXMania instance = new CDTXMania();

		public static CDTXMania Instance
		{
			get
			{
				return instance;
			}
		}
		public C文字コンソール act文字コンソール { get; private set; }
		public bool bコンパクトモード { get; private set; }
		public CConfigXml ConfigIni;
		public CResources Resources;

		public CDTX DTX
		{
			get
			{
				return dtx;
			}
			set
			{
				if ((dtx != null) && (Instance != null))
				{
					dtx.On非活性化();
					Instance.listトップレベルActivities.Remove(dtx);
				}
				dtx = value;
				if ((dtx != null) && (Instance != null))
				{
					Instance.listトップレベルActivities.Add(dtx);
				}
			}
		}
		public CFPS FPS { get; private set; }
		public CInput管理 Input管理 { get; private set; }
		#region [ 入力範囲ms ]
		public int nPerfect範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nPerfect範囲ms >= 0))
					{
						return c曲リストノード.nPerfect範囲ms;
					}
				}
				return ConfigIni.nHitRange.Perfect;
			}
		}
		public int nGreat範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nGreat範囲ms >= 0))
					{
						return c曲リストノード.nGreat範囲ms;
					}
				}
				return ConfigIni.nHitRange.Great;
			}
		}
		public int nGood範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nGood範囲ms >= 0))
					{
						return c曲リストノード.nGood範囲ms;
					}
				}
				return ConfigIni.nHitRange.Good;
			}
		}
		public int nPoor範囲ms
		{
			get
			{
				if (stage選曲.r確定された曲 != null)
				{
					C曲リストノード c曲リストノード = stage選曲.r確定された曲.r親ノード;
					if (((c曲リストノード != null) && (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)) && (c曲リストノード.nPoor範囲ms >= 0))
					{
						return c曲リストノード.nPoor範囲ms;
					}
				}
				return ConfigIni.nHitRange.Poor;
			}
		}
		#endregion
		public CPad Pad { get; private set; }
		public Random Random { get; private set; }
		public CSkin Skin { get; private set; }
		public CSongs管理 Songs管理 { get; set; }// 2012.1.26 yyagi private解除 CStage起動でのdesirialize読み込みのため
		public CEnumSongs EnumSongs { get; private set; }
		public CActEnumSongs actEnumSongs { get; private set; }
		public CActFlushGPU actFlushGPU { get; private set; }

		public CSound管理 Sound管理 { get; private set; }
		public CStage起動 stage起動 { get; private set; }
		public CStageタイトル stageタイトル { get; private set; }
		public CStageコンフィグ stageコンフィグ { get; private set; }
		public CStage選曲 stage選曲 { get; private set; }
		public CStage曲読み込み stage曲読み込み { get; private set; }
		public CStage演奏画面共通 stage演奏画面 { get; private set; }
		public CStage結果 stage結果 { get; private set; }
		public CStageChangeSkin stageChangeSkin { get; private set; }
		public CStage終了 stage終了 { get; private set; }
		public CStage r現在のステージ = null;
		public CStage r直前のステージ = null;
		public CStage r1フレーム前のステージ = null;
		public string strEXEのあるフォルダ { get; private set; }
		public string strコンパクトモードファイル { get; private set; }
		public CTimer Timer { get; private set; }
		public Format TextureFormat = Format.A8R8G8B8;
		internal IPluginActivity act現在入力を占有中のプラグイン = null;
		public bool bApplicationActive { get; private set; }
		public bool b次のタイミングで垂直帰線同期切り替えを行う { get; set; }
		public bool b次のタイミングで全画面_ウィンドウ切り替えを行う { get; set; }
		public Coordinates.CCoordinates Coordinates;
		public Device Device
		{
			get
			{
				return base.GraphicsDeviceManager.Direct3D9.Device;
			}
		}
		public CPluginHost PluginHost { get; private set; }
		public List<STPlugin> listプラグイン = new List<STPlugin>();

		private Size currentClientSize { get; set; }    // #23510 2010.10.27 add yyagi to keep current window size
																										//		public static CTimer ct;
		public IntPtr WindowHandle                  // 2012.10.24 yyagi; to add ASIO support
		{
			get
			{
				return base.Window.Handle;
			}
		}
        public CDTXVmode DTXVmode;                       // #28821 2014.1.23 yyagi
        public CDTX2WAVmode DTX2WAVmode;
        public CCommandParse CommandParse;
        #endregion

        // コンストラクタ

        private CDTXMania()
		{
		}

		public void InitializeInstance()
		{
			#region [ strEXEのあるフォルダを決定する ]
			// BEGIN #23629 2010.11.13 from: デバッグ時は Application.ExecutablePath が ($SolutionDir)/bin/x86/Debug/ などになり System/ の読み込みに失敗するので、カレントディレクトリを採用する。（プロジェクトのプロパティ→デバッグ→作業ディレクトリが有効になる）
#if DEBUG
			strEXEのあるフォルダ = Environment.CurrentDirectory + @"\";
			//strEXEのあるフォルダ = Path.GetDirectoryName( Environment.GetCommandLineArgs()[ 0 ] ) + @"\";
#else
			strEXEのあるフォルダ = Path.GetDirectoryName(Application.ExecutablePath) + @"\";	// #23629 2010.11.9 yyagi: set correct pathname where DTXManiaGR.exe is.
#endif
			// END #23629 2010.11.13 from
			#endregion

			#region [ 言語リソースの初期化 ]
			Trace.TraceInformation("言語リソースの初期化を行います。");
			Trace.Indent();
			try
			{
				Resources = new CResources();
				Resources.LoadResources("");
				Trace.TraceInformation("言語リソースの初期化を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ Config.ini の読込み ]
			ConfigIni = new CConfigXml();
			CDTXMania.Instance.LoadConfig();
			// #28200 2011.5.1 yyagi
			this.Window.EnableSystemMenu = CDTXMania.Instance.ConfigIni.bIsEnabledSystemMenu;
			// 2012.8.22 Config.iniが無いときに初期値が適用されるよう、この設定行をifブロック外に移動
			#endregion


			#region[座標値読み込み]
			Coordinates = new Coordinates.CCoordinates();
			UpdateCoordinates();
			//Coordinates = (DTXMania.Coordinates.CCoordinates) CDTXMania.DeserializeXML( strEXEのあるフォルダ + "Coordinates.xml", typeof( DTXMania.Coordinates.CCoordinates ) );
			//if ( Coordinates == null )
			//{
			//	if ( File.Exists( strEXEのあるフォルダ + "Coordinates.xml" ) )
			//	{
			//		Trace.TraceInformation( "Coordinates.xmlファイルは存在します。" );
			//	}
			//	Trace.TraceInformation( "Coordiantes.xmlファイルの読み込みができませんでした。無視して進めます。" );
			//	Coordinates = new Coordinates.CCoordinates();
			//}
			#endregion

			#region [ ログ出力開始 ]
			Trace.AutoFlush = true;
			if (ConfigIni.bLog)
			{
				try
				{
					Trace.Listeners.Add(new CTraceLogListener(new StreamWriter(System.IO.Path.Combine(strEXEのあるフォルダ, "DTXManiaLog.txt"), false, Encoding.GetEncoding("utf-16"))));
				}
				catch (System.UnauthorizedAccessException)          // #24481 2011.2.20 yyagi
				{
					Resources.Language = instance.ConfigIni.strLanguage;
					string mes = CDTXMania.Instance.Resources.Explanation("strErrorLogWrite");
					MessageBox.Show(mes, "DTXMania boot error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}
			}
			Trace.WriteLine("");
			Trace.WriteLine("DTXMania powered by YAMAHA Silent Session Drums");
			Trace.WriteLine(string.Format("Release: {0} {1} mode.", VERSION, (Environment.Is64BitProcess)? "x64":"x86" ));
			Trace.WriteLine("");
			Trace.TraceInformation("----------------------");
			Trace.TraceInformation("■ アプリケーションの初期化");

			CPutSystemLog.PutSystemLog();
			#endregion

			#region [ 言語の設定 ]
			Trace.TraceInformation("言語情報の読み込みを開始します。");
			//Debug.WriteLine( "language=" + Resources.Language );
			//Debug.WriteLine( "settings=" + instance.ConfigIni.strLanguage );
			Resources.Language = instance.ConfigIni.strLanguage;
			Trace.TraceInformation("言語を{0}に設定しました。", Resources.Language);

			#endregion

			#region [ DTXVmodeクラス, DTX2WAVmodeクラス, CommandParseクラス の初期化 ]
			//Trace.TraceInformation( "DTXVモードの初期化を行います。" );
			//Trace.Indent();
			try
			{
				DTXVmode = new CDTXVmode();
				DTXVmode.Enabled = false;
                //Trace.TraceInformation( "DTXVモードの初期化を完了しました。" );

                DTX2WAVmode = new CDTX2WAVmode();
                //Trace.TraceInformation( "DTX2WAVモードの初期化を完了しました。" );

                CommandParse = new CCommandParse();
                //Trace.TraceInformation( "CommandParseの初期化を完了しました。" );
            }
            finally
			{
				//Trace.Unindent();
			}
			#endregion
			#region [ コンパクトモードスイッチの有無、もしくは、DTXViewer/DTX2WAVとしての起動 ]
			bコンパクトモード = false;
			strコンパクトモードファイル = "";
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if ((commandLineArgs != null) && (commandLineArgs.Length > 1))
			{
				bコンパクトモード = true;
				string arg = "";

				for (int i = 1; i < commandLineArgs.Length; i++)
				{
					if (i != 1)
					{
						arg += " " + "\"" + commandLineArgs[i] + "\"";
					}
					else
					{
						arg += commandLineArgs[i];
					}
				}
				CommandParse.ParseArguments(arg, ref DTXVmode, ref DTX2WAVmode);
				if (DTXVmode.Enabled)
				{
					DTXVmode.Refreshed = false;                             // 初回起動時は再読み込みに走らせない
					strコンパクトモードファイル = DTXVmode.filename;
					switch (DTXVmode.soundDeviceType)                       // サウンド再生方式の設定
					{
						case ESoundDeviceType.DirectSound:
							ConfigIni.nSoundDeviceType.Value = ESoundDeviceTypeForConfig.DSound;
							break;
						case ESoundDeviceType.ExclusiveWASAPI:
							ConfigIni.nSoundDeviceType.Value = ESoundDeviceTypeForConfig.WASAPI_Exclusive;
							break;
						case ESoundDeviceType.SharedWASAPI:
							ConfigIni.nSoundDeviceType.Value = ESoundDeviceTypeForConfig.WASAPI_Shared;
							break;
						case ESoundDeviceType.ASIO:
							ConfigIni.nSoundDeviceType.Value = ESoundDeviceTypeForConfig.ASIO;
							ConfigIni.strASIODevice.Index = DTXVmode.nASIOdevice;
							break;
					}

					CDTXMania.Instance.ConfigIni.bVSyncWait.Value = DTXVmode.VSyncWait;
					CDTXMania.Instance.ConfigIni.bTimeStretch.Value = DTXVmode.TimeStretch;
					if (DTXVmode.GRmode)
					{
						CDTXMania.Instance.ConfigIni.eActiveInst.Value = EActiveInstrument.GBOnly;
					}
					else
					{
						CDTXMania.Instance.ConfigIni.eActiveInst.Value = EActiveInstrument.Both;
					}

					CDTXMania.Instance.ConfigIni.bFullScreen.Value = false;
					CDTXMania.Instance.ConfigIni.rcWindow_backup = CDTXMania.Instance.ConfigIni.rcWindow;       // #36612 2016.9.12 yyagi
					CDTXMania.Instance.ConfigIni.rcWindow.W = CDTXMania.Instance.ConfigIni.rcViewerWindow.W;
					CDTXMania.Instance.ConfigIni.rcWindow.H = CDTXMania.Instance.ConfigIni.rcViewerWindow.H;
					CDTXMania.Instance.ConfigIni.rcWindow.X = CDTXMania.Instance.ConfigIni.rcViewerWindow.X;
					CDTXMania.Instance.ConfigIni.rcWindow.Y = CDTXMania.Instance.ConfigIni.rcViewerWindow.Y;
				}
				else if (DTX2WAVmode.Enabled)
				{
					strコンパクトモードファイル = DTX2WAVmode.dtxfilename;
					#region [ FDKへの録音設定 ]
					FDK.CSound管理.strRecordInputDTXfilename = DTX2WAVmode.dtxfilename;
					FDK.CSound管理.strRecordOutFilename = DTX2WAVmode.outfilename;
					FDK.CSound管理.strRecordFileType = DTX2WAVmode.Format.ToString();
					FDK.CSound管理.nBitrate = DTX2WAVmode.bitrate;
					for (int i = 0; i < (int)FDK.CSound.EInstType.Unknown; i++)
					{
						FDK.CSound管理.nMixerVolume[ i ] = DTX2WAVmode.nMixerVolume[ i ];
					}
					ConfigIni.nMasterVolume.Value = DTX2WAVmode.nMixerVolume[(int)FDK.CSound.EInstType.Unknown];    // [5](Unknown)のところにMasterVolumeが入ってくるので注意
																													// CSound管理.nMixerVolume[5]は、結局ここからは変更しないため、
																													// 事実上初期値=100で固定。
					#endregion
					#region [ 録音用の本体設定 ]

					// 本体プロセスの優先度を少し上げる (最小化状態で動作させると、処理性能が落ちるようなので
					// → ほとんど効果がなかったので止めます
					//Process thisProcess = System.Diagnostics.Process.GetCurrentProcess();
					//thisProcess.PriorityClass = ProcessPriorityClass.AboveNormal;

					// エンコーダーのパス設定 (=DLLフォルダ)
					FDK.CSound管理.strEncoderPath = Path.Combine(strEXEのあるフォルダ, "DLL");

					CDTXMania.instance.ConfigIni.nSoundDeviceType.Value = ESoundDeviceTypeForConfig.WASAPI_Exclusive;
					CDTXMania.instance.ConfigIni.bEventDrivenWASAPI.Value = false;

					CDTXMania.instance.ConfigIni.bVSyncWait.Value = false;
					CDTXMania.instance.ConfigIni.bTimeStretch.Value = false;
					CDTXMania.instance.ConfigIni.eActiveInst.Value = EActiveInstrument.Both;

					CDTXMania.instance.ConfigIni.bFullScreen.Value = false;
					CDTXMania.instance.ConfigIni.rcWindow_backup = CDTXMania.Instance.ConfigIni.rcWindow;
					CDTXMania.instance.ConfigIni.rcWindow.W = CDTXMania.Instance.ConfigIni.rcViewerWindow.W;
					CDTXMania.instance.ConfigIni.rcWindow.H = CDTXMania.Instance.ConfigIni.rcViewerWindow.H;
					CDTXMania.instance.ConfigIni.rcWindow.X = CDTXMania.Instance.ConfigIni.rcViewerWindow.X;
					CDTXMania.instance.ConfigIni.rcWindow.Y = CDTXMania.Instance.ConfigIni.rcViewerWindow.Y;

					//全オート
					CDTXMania.instance.ConfigIni.bAutoPlay.LC.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.HH.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.HHO.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.SD.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.BD.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.HT.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.LT.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.FT.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.CY.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.RD.Value = true;

					CDTXMania.instance.ConfigIni.bAutoPlay.GtR.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.GtG.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.GtB.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.GtPick.Value = true;
					//CDTXMania.instance.ConfigIni.bAutoPlay.GtWail.Value = true;  // 無くてもよい 処理不可削減のため、敢えてWailはAutoにしない
					CDTXMania.instance.ConfigIni.bAutoPlay.BsR.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.BsG.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.BsB.Value = true;
					CDTXMania.instance.ConfigIni.bAutoPlay.BsPick.Value = true;
					//CDTXMania.instance.ConfigIni.bAutoPlay.BsWail.Value = true;

					//FillInオフ, 歓声オフ
					CDTXMania.instance.ConfigIni.bFillin.Value = false;
					CDTXMania.instance.ConfigIni.bAudience.Value = false;
					//ストイックモード
					CDTXMania.instance.ConfigIni.bStoicMode.Value = false;
					//チップ非表示
					CDTXMania.instance.ConfigIni.eSudHidInv.Drums.Value = ESudHidInv.FullInv;
					CDTXMania.instance.ConfigIni.eSudHidInv.Guitar.Value = ESudHidInv.FullInv;
					CDTXMania.instance.ConfigIni.eSudHidInv.Bass.Value = ESudHidInv.FullInv;

					// Dark=Full
					CDTXMania.instance.ConfigIni.eDark.Value = EDark.Full;

					//多重再生数=4
					CDTXMania.instance.ConfigIni.nPolyphonicSounds.Value = 4;
					CDTXMania.instance.ConfigIni.nPolyphonicSoundsGB.Value = 2;

					//再生速度x1
					CDTXMania.instance.ConfigIni.nPlaySpeed.Value = 20;

					//メトロノーム音量0
					CDTXMania.instance.ConfigIni.eClickType.Value = EClickType.Off;
					CDTXMania.instance.ConfigIni.nClickHighVolume.Value = 0;
					CDTXMania.instance.ConfigIni.nClickLowVolume.Value = 0;

					//自動再生音量=100
					CDTXMania.instance.ConfigIni.nAutoVolume.Value = 100;
					CDTXMania.instance.ConfigIni.nChipVolume.Value = 100;

					//マスターボリューム100
					//CDTXMania.instance.ConfigIni.nMasterVolume.Value = 100;	// DTX2WAV側から設定するので、ここでは触らない

					//StageFailedオフ
					CDTXMania.instance.ConfigIni.bStageFailed.Value = false;

					//グラフ無効
					CDTXMania.instance.ConfigIni.bGraph.Drums.Value = false;
					CDTXMania.instance.ConfigIni.bGraph.Guitar.Value = false;
					CDTXMania.instance.ConfigIni.bGraph.Bass.Value = false;

					//コンボ非表示,判定非表示
					CDTXMania.instance.ConfigIni.bDisplayCombo.Drums.Value = false;
					CDTXMania.instance.ConfigIni.bDisplayCombo.Guitar.Value = false;
					CDTXMania.instance.ConfigIni.bDisplayCombo.Bass.Value = false;
					CDTXMania.instance.ConfigIni.bDisplayJudge.Drums.Value = false;
					CDTXMania.instance.ConfigIni.bDisplayJudge.Guitar.Value = false;
					CDTXMania.instance.ConfigIni.bDisplayJudge.Bass.Value = false;


					//デバッグ表示オフ
					//CDTXMania.instance.ConfigIni.b演奏情報を表示する = false;
					CDTXMania.instance.ConfigIni.bDebugInfo.Value = false;

					//BGAオフ, AVIオフ
					CDTXMania.instance.ConfigIni.bBGA.Value = false;
					CDTXMania.instance.ConfigIni.bAVI.Value = false;

					//BGMオン、チップ音オン
					CDTXMania.instance.ConfigIni.bBGMPlay.Value = true;
					CDTXMania.instance.ConfigIni.bDrumsHitSound.Value = true;

					//パート強調オフ
					CDTXMania.instance.ConfigIni.bEmphasizePlaySound.Drums.Value = false;
					CDTXMania.instance.ConfigIni.bEmphasizePlaySound.Guitar.Value = false;
					CDTXMania.instance.ConfigIni.bEmphasizePlaySound.Bass.Value = false;

					// パッド入力等、基本操作の無効化 (ESCを除く)
					//CDTXMania.Instance.ConfigIni.KeyAssign[][];
					
					#endregion
				}
				else                                                        // 通常のコンパクトモード
				{
					strコンパクトモードファイル = commandLineArgs[1];
				}

				if (!File.Exists(strコンパクトモードファイル))      // #32985 2014.1.23 yyagi 
				{
					Trace.TraceError("コンパクトモードで指定されたファイルが見つかりません。DTXManiaを終了します。[{0}]", strコンパクトモードファイル);
#if DEBUG
					Environment.Exit(-1);
#else
					if (strコンパクトモードファイル == "")	// DTXMania未起動状態で、DTXCで再生停止ボタンを押した場合は、何もせず終了
					{
						Environment.Exit(-1);
					}
					else
					{
						throw new FileNotFoundException("コンパクトモードで指定されたファイルが見つかりません。DTXManiaを終了します。", strコンパクトモードファイル);
					}
#endif
				}
				if (DTXVmode.Enabled)
				{
					Trace.TraceInformation("DTXVモードで起動します。[{0}]", strコンパクトモードファイル);
				}
				else if (DTX2WAVmode.Enabled)
				{
					Trace.TraceInformation("DTX2WAVモードで起動します。[{0}]", strコンパクトモードファイル);
					DTX2WAVmode.SendMessage2DTX2WAV("BOOT");
				}
				else
				{
					Trace.TraceInformation("コンパクトモードで起動します。[{0}]", strコンパクトモードファイル);
				}
			}
			else
			{
				Trace.TraceInformation("通常モードで起動します。");
			}
			#endregion

			#region [ 現在の電源プランをバックアップし、CONFIGのHighPower=ONの場合は HighPerformanceに変更 ]
			if (CDTXMania.Instance.ConfigIni.bForceHighPowerPlan)
			{
				CPowerPlan.BackupCurrentPowerPlan();
				CPowerPlan.ChangeHighPerformance();
			}
			#endregion

			#region [ Input管理 の初期化 ]
			Trace.TraceInformation("DirectInput, MIDI入力の初期化を行います。");
			Trace.Indent();
			try
			{
				bool bUseMIDIIn = !DTXVmode.Enabled;
				Input管理 = new CInput管理(base.Window.Handle, bUseMIDIIn);

				// If the users uses MIDI2.0-USB cable, then warn it
				if (bUseMIDIIn && CDTXMania.Instance.ConfigIni.bWarnMIDI20USB.Value)
				{
					foreach (IInputDevice device in Input管理.list入力デバイス)
					{
						if ((device.e入力デバイス種別 == E入力デバイス種別.MidiIn) && (device.strDeviceName == "USB2.0-MIDI"))
						{
							string strWarnMes = CDTXMania.Instance.Resources.Explanation("strWarnMIDI20USB");
							var ret = MessageBox.Show(strWarnMes, "DTXMania Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
							if (ret == DialogResult.OK)
							{
								CDTXMania.Instance.ConfigIni.bWarnMIDI20USB.Value = false;
							}
						}
					}
				}



				foreach (IInputDevice device in Input管理.list入力デバイス)
				{
					if ((device.e入力デバイス種別 == E入力デバイス種別.Joystick) && !ConfigIni.dicJoystick.Value.ContainsValue(device.GUID))
					{
						int key = 0;
						while (ConfigIni.dicJoystick.Value.ContainsKey(key))
						{
							key++;
						}
						ConfigIni.dicJoystick.Value.Add(key, device.GUID);
					}
				}
				foreach (IInputDevice device2 in Input管理.list入力デバイス)
				{
					if (device2.e入力デバイス種別 == E入力デバイス種別.Joystick)
					{
						foreach (KeyValuePair<int, string> pair in ConfigIni.dicJoystick.Value)
						{
							if (device2.GUID.Equals(pair.Value))
							{
								((CInputJoystick)device2).SetID(pair.Key);
								break;
							}
						}
						continue;
					}
				}
				Trace.TraceInformation("DirectInput の初期化を完了しました。");
			}
			catch (Exception exception2)
			{
				Trace.TraceError(exception2.Message);
				Trace.TraceError("DirectInput, MIDI入力の初期化に失敗しました。");

				string mes = CDTXMania.Instance.Resources.Explanation("strErrorLogWrite");
				MessageBox.Show(mes, "DTXMania boot error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(1);
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ ウィンドウ初期化 ]
			// #30675 2013.02.04 ikanick add
			base.Window.StartPosition = FormStartPosition.Manual;
			base.Window.Location = new Point(ConfigIni.rcWindow.X, ConfigIni.rcWindow.Y);
			// 事前にDTXVmodeの実体を作っておくこと
			base.Window.Text = this.strWindowTitle;
			//base.Window.StartPosition = FormStartPosition.Manual;
			//base.Window.Location = new Point(ConfigIni.rcWindow.X, ConfigIni.rcWindow.Y);

			// #34510 yyagi 2010.10.31 to change window size got from Config.ini
			base.Window.ClientSize = new Size(ConfigIni.rcWindow.W, ConfigIni.rcWindow.H);
#if !WindowedFullscreen
			if (!ConfigIni.bウィンドウモード)						// #23510 2010.11.02 yyagi: add; to recover window size in case bootup with fullscreen mode
			{														// #30666 2013.02.02 yyagi: currentClientSize should be always made
#endif
			currentClientSize = new Size(ConfigIni.rcWindow.W, ConfigIni.rcWindow.H);
#if !WindowedFullscreen
			}
#endif
			// #23510 2010.11.04 yyagi: to support maximizing window
			base.Window.MaximizeBox = true;
			// #23510 2010.10.27 yyagi: changed from FixedDialog to Sizable, to support window resize
			base.Window.FormBorderStyle = FormBorderStyle.Sizable;
			// #30666 2013.02.02 yyagi: moved the code to t全画面・ウインドウモード切り替え()
			base.Window.ShowIcon = true;
			base.Window.Icon = Properties.Resources.dtx;
			base.Window.KeyDown += new KeyEventHandler(this.Window_KeyDown);
			base.Window.MouseUp += new MouseEventHandler(this.Window_MouseUp);
			base.Window.MouseDown += new MouseEventHandler(this.Window_MouseDown);
			// #23510 2010.11.13 yyagi: to go fullscreen mode
			base.Window.MouseDoubleClick += new MouseEventHandler(this.Window_MouseDoubleClick);
			// #23510 2010.11.20 yyagi: to set resized window size in Config.ini
			base.Window.ResizeEnd += new EventHandler(this.Window_ResizeEnd);
			base.Window.ApplicationActivated += new EventHandler(this.Window_ApplicationActivated);
			base.Window.ApplicationDeactivated += new EventHandler(this.Window_ApplicationDeactivated);
			base.Window.MouseMove += new MouseEventHandler(this.Window_MouseMove);
			#endregion

			#region [ Direct3D9Exを使うかどうか判定 ]
			#endregion

			#region [ Direct3D9 デバイスの生成 ]
			DeviceSettings settings = new DeviceSettings();
#if WindowedFullscreen
			// #30666 2013.2.2 yyagi: Fullscreenmode is "Maximized window" mode
			settings.Windowed = true;
#else
			settings.Windowed = ConfigIni.bウィンドウモード;
#endif
			settings.BackBufferWidth = SampleFramework.GameWindowSize.Width;
			settings.BackBufferHeight = SampleFramework.GameWindowSize.Height;
			settings.EnableVSync = ConfigIni.bVSyncWait;
			//settings.MultisampleType = MultisampleType.FourSamples;
			//settings.MultisampleQuality = 3;
			//settings.MultisampleType = MultisampleType.NonMaskable;
			//settings.Multithreaded = true;


			try
			{
				base.GraphicsDeviceManager.ChangeDevice(settings);
			}
			catch (DeviceCreationException e)
			{
				Trace.TraceError(e.ToString());
				MessageBox.Show(e.Message + e.ToString(), "DTXMania failed to boot: DirectX9 Initialize Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(-1);
			}
			Trace.TraceInformation("DeviceCaps       = " + base.GraphicsDeviceManager.Direct3D9.Device.Capabilities.DeviceCaps.ToString());
			Trace.TraceInformation("DeviceCaps2      = " + base.GraphicsDeviceManager.Direct3D9.Device.Capabilities.DeviceCaps2.ToString());
			Trace.TraceInformation("MaxTextureWidth  = " + base.GraphicsDeviceManager.Direct3D9.Device.Capabilities.MaxTextureWidth);
			Trace.TraceInformation("MaxTextureHeight = " + base.GraphicsDeviceManager.Direct3D9.Device.Capabilities.MaxTextureHeight);
			Trace.TraceInformation("TextureCaps      = " + base.GraphicsDeviceManager.Direct3D9.Device.Capabilities.TextureCaps.ToString());
			Trace.TraceInformation("DeviceInformation= " + base.GraphicsDeviceManager.DeviceInformation.ToString());
			Trace.TraceInformation("DeviceStatics    = " + base.GraphicsDeviceManager.DeviceStatistics.ToString());

			base.IsFixedTimeStep = false;
			//			base.TargetElapsedTime = TimeSpan.FromTicks( 10000000 / 75 );
			// #23510 2010.10.31 yyagi: to recover window size. width and height are able to get from Config.ini.
			base.Window.ClientSize = new Size(ConfigIni.rcWindow.W, ConfigIni.rcWindow.H);
			// #23568 2010.11.3 yyagi: to support valiable sleep value when !IsActive
			base.InactiveSleepTime = TimeSpan.FromMilliseconds((float)(ConfigIni.nSleepUnfocusMs));
			// #23568 2010.11.4 ikanick changed ( 1 -> ConfigIni )
#if WindowedFullscreen
			// #30666 2013.2.2 yyagi: finalize settings for "Maximized window mode"
			this.t全画面_ウィンドウモード切り替え();
#endif
			actFlushGPU = new CActFlushGPU();
			#endregion

			if (DTX2WAVmode.Enabled)
			{
				this.Window.WindowState = FormWindowState.Minimized;		//DTX2WAVモード時は自動的に最小化
			}

			DTX = null;

			#region [ Skin の初期化 ]
			Trace.TraceInformation("スキンの初期化を行います。");
			Trace.Indent();
			try
			{
				Skin = new CSkin(
					CDTXMania.Instance.ConfigIni.strSystemSkinSubfolderPath,
					CDTXMania.Instance.ConfigIni.bUseBoxDefSkin);
				// 旧指定のSkinフォルダが消滅していた場合に備える
				CDTXMania.Instance.ConfigIni.strSystemSkinSubfolderPath.Value = CDTXMania.Instance.Skin.GetCurrentSkinSubfolderFullName(true);
				Trace.TraceInformation("スキンの初期化を完了しました。");
			}
			catch
			{
				Trace.TraceInformation("スキンの初期化に失敗しました。");
				throw;
			}
			finally
			{
				Trace.Unindent();
			}

			#region [ Skin配下にある言語リソースの確認と初期化 ]
			Trace.TraceInformation("スキンフォルダに言語リソースがないか確認します。");
			Trace.Indent();
			try
			{
				Resources.csvCurrentPath = CDTXMania.Instance.ConfigIni.strSystemSkinSubfolderPath.Value;
				Trace.TraceInformation("Skin Path:" + Resources.csvCurrentPath);

				bool ret = Resources.LoadResources(instance.ConfigIni.strLanguage);
				if (ret)
				{
					Trace.TraceInformation("スキンフォルダ内に言語リソースが見つかりました。この言語リソースを使用します。");
				}
				else
				{
					Trace.TraceInformation("スキンフォルダ内の言語リソースを使用できません。既定の言語リソースを使用します。");

				}
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#endregion

			#region [ Timer の初期化 ]
			Trace.TraceInformation("タイマの初期化を行います。");
			Trace.Indent();
			try
			{
				Timer = new CTimer(CTimer.E種別.MultiMedia);
				Trace.TraceInformation("タイマの初期化を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ マウス消去用のクラスを初期化 ]
			cMouseHideControl = new CMouseHideControl();
			#endregion

			#region [ FPS カウンタの初期化 ]
			Trace.TraceInformation("FPSカウンタの初期化を行います。");
			Trace.Indent();
			try
			{
				FPS = new CFPS();
				Trace.TraceInformation("FPSカウンタを生成しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ act文字コンソールの初期化 ]
			Trace.TraceInformation("文字コンソールの初期化を行います。");
			Trace.Indent();
			try
			{
				act文字コンソール = new C文字コンソール();
				Trace.TraceInformation("文字コンソールを生成しました。");
				act文字コンソール.On活性化();
				Trace.TraceInformation("文字コンソールを活性化しました。");
				Trace.TraceInformation("文字コンソールの初期化を完了しました。");
			}
			catch (Exception exception)
			{
				Trace.TraceError(exception.Message);
				Trace.TraceError("文字コンソールの初期化に失敗しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ Pad の初期化 ]
			Trace.TraceInformation("パッドの初期化を行います。");
			Trace.Indent();
			try
			{
				Pad = new CPad();
				Trace.TraceInformation("パッドの初期化を完了しました。");
			}
			catch (Exception exception3)
			{
				Trace.TraceError(exception3.Message);
				Trace.TraceError("パッドの初期化に失敗しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ Sound管理 の初期化 ]
			Trace.TraceInformation("サウンドデバイスの初期化を行います。");
			Trace.Indent();
			try
			{
				ESoundDeviceType soundDeviceType;
				switch (CDTXMania.Instance.ConfigIni.nSoundDeviceType.Value)
				{
					case ESoundDeviceTypeForConfig.DSound:
						soundDeviceType = ESoundDeviceType.DirectSound;
						break;
					case ESoundDeviceTypeForConfig.ASIO:
						soundDeviceType = ESoundDeviceType.ASIO;
						break;
					case ESoundDeviceTypeForConfig.WASAPI_Exclusive:
						soundDeviceType = ESoundDeviceType.ExclusiveWASAPI;
						break;
					case ESoundDeviceTypeForConfig.WASAPI_Shared:
						soundDeviceType = ESoundDeviceType.SharedWASAPI;
						break;
					default:
						soundDeviceType = ESoundDeviceType.Unknown;
						break;
				}
				Sound管理 = new CSound管理(base.Window.Handle,
											soundDeviceType,
											CDTXMania.Instance.ConfigIni.nWASAPIBufferSizeMs,
											CDTXMania.instance.ConfigIni.bEventDrivenWASAPI,
											0,
											CDTXMania.Instance.ConfigIni.strASIODevice.Index,
											CDTXMania.Instance.ConfigIni.bUseOSTimer
				);
				//Sound管理 = FDK.CSound管理.Instance;
				//Sound管理.t初期化( soundDeviceType, 0, 0, CDTXMania.Instance.ConfigIni.nASIODevice, base.Window.Handle );

				ShowWindowTitleWithSoundType();
				FDK.CSound管理.bIsTimeStretch = CDTXMania.Instance.ConfigIni.bTimeStretch;
				Sound管理.nMasterVolume = CDTXMania.Instance.ConfigIni.nMasterVolume;
				//FDK.CSound管理.bIsMP3DecodeByWindowsCodec = CDTXMania.Instance.ConfigIni.bNoMP3Streaming;


				string strDefaultSoundDeviceBusType = CSound管理.strDefaultDeviceBusType;
				Trace.TraceInformation($"Bus type of the default sound device = {strDefaultSoundDeviceBusType}");

				if (strDefaultSoundDeviceBusType.ToUpper().Equals("USB"))
				{
					if (CDTXMania.Instance.ConfigIni.bWarnSoundDeviceOnUSB.Value)
					{
						string strWarnMes = CDTXMania.Instance.Resources.Explanation("strWarnSoundDeviceOnUSB");
						var ret = MessageBox.Show(strWarnMes, "DTXMania Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
						if (ret == DialogResult.OK)
						{
							CDTXMania.Instance.ConfigIni.bWarnSoundDeviceOnUSB.Value = false;
						}
					}
				}

				Trace.TraceInformation("サウンドデバイスの初期化を完了しました。");
			}
			catch (NullReferenceException)  // No audio output found
			{
				Trace.TraceError("Error: No sound output devices are ready.");
				string strWarnMes = CDTXMania.Instance.Resources.Explanation("strErrorNoActiveSoundDevice");
				MessageBox.Show(strWarnMes, "DTXMania Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				Environment.Exit(-1);
			}
			catch (Exception e)
			{
				Trace.TraceError(e.Message);
				throw;
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ Songs管理 の初期化 ]
			//---------------------
			Trace.TraceInformation("曲リストの初期化を行います。");
			Trace.Indent();
			try
			{
				Songs管理 = new CSongs管理();
				//				Songs管理_裏読 = new CSongs管理();
				EnumSongs = new CEnumSongs();
				actEnumSongs = new CActEnumSongs();
				Trace.TraceInformation("曲リストの初期化を完了しました。");
			}
			catch (Exception e)
			{
				Trace.TraceError(e.Message);
				Trace.TraceError("曲リストの初期化に失敗しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			//---------------------
			#endregion

			#region [ CAvi の初期化 ]
			CAvi.t初期化();
			#endregion

			#region [ Random の初期化 ]
			Random = new Random((int)Timer.nシステム時刻);
			#endregion

			#region [ ステージの初期化 ]
			r現在のステージ = null;
			r直前のステージ = null;
			stage起動 = new CStage起動();
			stageタイトル = new CStageタイトル();
			stageコンフィグ = new CStageコンフィグ();
			stage選曲 = new CStage選曲();
			stage曲読み込み = new CStage曲読み込み();
			stage演奏画面 = new CStage演奏画面共通();
			stage結果 = new CStage結果();
			stageChangeSkin = new CStageChangeSkin();
			stage終了 = new CStage終了();

			this.listトップレベルActivities = new List<CActivity>();
			this.listトップレベルActivities.Add(actEnumSongs);
			this.listトップレベルActivities.Add(act文字コンソール);
			this.listトップレベルActivities.Add(stage起動);
			this.listトップレベルActivities.Add(stageタイトル);
			this.listトップレベルActivities.Add(stageコンフィグ);
			this.listトップレベルActivities.Add(stage選曲);
			this.listトップレベルActivities.Add(stage曲読み込み);
			this.listトップレベルActivities.Add(stage演奏画面);
			this.listトップレベルActivities.Add(stage結果);
			this.listトップレベルActivities.Add(stageChangeSkin);
			this.listトップレベルActivities.Add(stage終了);
			this.listトップレベルActivities.Add(actFlushGPU);
			#endregion

			#region [ プラグインの検索と生成 ]
			PluginHost = new CPluginHost();

			Trace.TraceInformation("プラグインの検索と生成を行います。");
			Trace.Indent();
			try
			{
				this.tプラグイン検索と生成();
				Trace.TraceInformation("プラグインの検索と生成を完了しました。");
			}
			finally
			{
				Trace.Unindent();
			}
			#endregion

			#region [ プラグインの初期化 ]
			if (this.listプラグイン != null && this.listプラグイン.Count > 0)
			{
				Trace.TraceInformation("プラグインの初期化を行います。");
				Trace.Indent();
				try
				{
					foreach (STPlugin st in this.listプラグイン)
					{
						Directory.SetCurrentDirectory(st.strプラグインフォルダ);
						st.plugin.On初期化(this.PluginHost);
						st.plugin.OnManagedリソースの作成();
						st.plugin.OnUnmanagedリソースの作成();
						Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
					}
					Trace.TraceInformation("すべてのプラグインの初期化を完了しました。");
				}
				catch
				{
					Trace.TraceError("プラグインのどれかの初期化に失敗しました。");
					throw;
				}
				finally
				{
					Trace.Unindent();
				}
			}
			#endregion


			Trace.TraceInformation("アプリケーションの初期化を完了しました。");

			#region [ 最初のステージの起動 ]
			Trace.TraceInformation("----------------------");
			Trace.TraceInformation("■ 起動");

			if (CDTXMania.Instance.bコンパクトモード)
			{
				r現在のステージ = stage曲読み込み;
			}
			else
			{
				r現在のステージ = stage起動;
			}
			r現在のステージ.On活性化();
			this.Window.Activate();     // #41300 workaround to avoid SharpDX exception
			#endregion
		}


		public void t全画面_ウィンドウモード切り替え()
		{
#if WindowedFullscreen
			if (ConfigIni != null)
#else
			DeviceSettings settings = base.GraphicsDeviceManager.CurrentSettings.Clone();
			if ( ( ConfigIni != null ) && ( ConfigIni.bウィンドウモード != settings.Windowed ) )
#endif
			{
#if !WindowedFullscreen
				settings.Windowed = ConfigIni.bウィンドウモード;
#endif
				if (ConfigIni.bウィンドウモード == false)   // #23510 2010.10.27 yyagi: backup current window size before going fullscreen mode
				{
					currentClientSize = this.Window.ClientSize;
					ConfigIni.rcWindow.W = this.Window.ClientSize.Width;
					ConfigIni.rcWindow.H = this.Window.ClientSize.Height;
					//					FDK.CTaskBar.ShowTaskBar( false );
				}
#if !WindowedFullscreen
				base.GraphicsDeviceManager.ChangeDevice( settings );
#endif
				if (ConfigIni.bウィンドウモード == true)    // #23510 2010.10.27 yyagi: to resume window size from backuped value
				{
#if WindowedFullscreen
					// #30666 2013.2.2 yyagi Don't use Fullscreen mode becasue NVIDIA GeForce is
					// tend to delay drawing on Fullscreen mode. So DTXMania uses Maximized window
					// instead of using fullscreen mode.
					Instance.Window.WindowState = FormWindowState.Normal;
					Instance.Window.FormBorderStyle = FormBorderStyle.Sizable;
					Instance.Window.WindowState = FormWindowState.Normal;
#endif
					base.Window.ClientSize =
							new Size(currentClientSize.Width, currentClientSize.Height);
					//					FDK.CTaskBar.ShowTaskBar( true );
				}
#if WindowedFullscreen
				else
				{
					Instance.Window.WindowState = FormWindowState.Normal;
					Instance.Window.FormBorderStyle = FormBorderStyle.None;
					Instance.Window.WindowState = FormWindowState.Maximized;
				}
				if (cMouseHideControl != null)
				{
					if (ConfigIni.bウィンドウモード)
					{
						cMouseHideControl.Show();
					}
					else
					{
						cMouseHideControl.Hide();
					}
				}
#endif
			}
		}

		#region [ #24609 リザルト画像をpngで保存する ]		// #24609 2011.3.14 yyagi; to save result screen in case BestRank or HiSkill.
		/// <summary>
		/// リザルト画像のキャプチャと保存。
		/// </summary>
		/// <param name="strFilename">保存するファイル名(フルパス)</param>
		public bool SaveResultScreen(string strFullPath)
		{
			string strSavePath = Path.GetDirectoryName(strFullPath);
			if (!Directory.Exists(strSavePath))
			{
				try
				{
					Directory.CreateDirectory(strSavePath);
				}
				catch
				{
					return false;
				}
			}

			// http://www.gamedev.net/topic/594369-dx9slimdxati-incorrect-saving-surface-to-file/
			using (Surface pSurface = CDTXMania.Instance.Device.GetRenderTarget(0))
			{
				Surface.ToFile(pSurface, strFullPath, ImageFileFormat.Png);
			}
			return true;
		}
		#endregion

		// Game 実装

		protected override void Initialize()
		{
			//			new GCBeep();
			//sw.Start();
			//swlist1 = new List<int>( 8192 );
			//swlist2 = new List<int>( 8192 );
			//swlist3 = new List<int>( 8192 );
			//swlist4 = new List<int>( 8192 );
			//swlist5 = new List<int>( 8192 );
			if (this.listトップレベルActivities != null)
			{
				foreach (CActivity activity in this.listトップレベルActivities)
					activity.OnManagedリソースの作成();
			}

			foreach (STPlugin st in this.listプラグイン)
			{
				Directory.SetCurrentDirectory(st.strプラグインフォルダ);
				st.plugin.OnManagedリソースの作成();
				Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
			}
#if GPUFlushAfterPresent
			FrameEnd += dtxmania_FrameEnd;
#endif
		}
#if GPUFlushAfterPresent
		void dtxmania_FrameEnd( object sender, EventArgs e )	// GraphicsDeviceManager.game_FrameEnd()後に実行される
		{														// → Present()直後にGPUをFlushする
																// → 画面のカクツキが頻発したため、ここでのFlushは行わない
			actFlushGPU.On進行描画();		// Flush GPU
		}
#endif
		protected override void LoadContent()
		{
			if (cMouseHideControl != null)
			{
				if (ConfigIni.bウィンドウモード)
				{
					cMouseHideControl.Show();
				}
				else
				{
					cMouseHideControl.Hide();
				}
			}
			this.Device.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0f, 0f, (float)(-SampleFramework.GameWindowSize.Height / 2 * Math.Sqrt(3.0))), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f)));
			this.Device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH(C変換.DegreeToRadian((float)60f), ((float)this.Device.Viewport.Width) / ((float)this.Device.Viewport.Height), -100f, 100f));
			this.Device.SetRenderState(RenderState.Lighting, false);
			this.Device.SetRenderState(RenderState.ZEnable, false);						// trueにすると、一部システムで画面表示できなくなる
			this.Device.SetRenderState(RenderState.AntialiasedLineEnable, false);       // trueにすると、一部システムで画面表示できなくなる 
			this.Device.SetRenderState(RenderState.AlphaTestEnable, true);
			this.Device.SetRenderState(RenderState.AlphaRef, 10);

			this.Device.SetRenderState(RenderState.MultisampleAntialias, true);
			this.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
			this.Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);

			this.Device.SetRenderState<Compare>(RenderState.AlphaFunc, Compare.Greater);
			this.Device.SetRenderState(RenderState.AlphaBlendEnable, true);
			this.Device.SetRenderState<Blend>(RenderState.SourceBlend, Blend.SourceAlpha);
			this.Device.SetRenderState<Blend>(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
			this.Device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
			this.Device.SetTextureStageState(0, TextureStage.AlphaArg1, 2);
			this.Device.SetTextureStageState(0, TextureStage.AlphaArg2, 1);

			if (this.listトップレベルActivities != null)
			{
				foreach (CActivity activity in this.listトップレベルActivities)
					activity.OnUnmanagedリソースの作成();
			}

			foreach (STPlugin st in this.listプラグイン)
			{
				Directory.SetCurrentDirectory(st.strプラグインフォルダ);
				st.plugin.OnUnmanagedリソースの作成();
				Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
			}
		}
		protected override void UnloadContent()
		{
			if (this.listトップレベルActivities != null)
			{
				foreach (CActivity activity in this.listトップレベルActivities)
					activity.OnUnmanagedリソースの解放();
			}

			foreach (STPlugin st in this.listプラグイン)
			{
				Directory.SetCurrentDirectory(st.strプラグインフォルダ);
				st.plugin.OnUnmanagedリソースの解放();
				Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
			}
		}
		protected override void OnExiting(EventArgs e)
		{
			CPowerManagement.tEnableMonitorSuspend();       // スリープ抑止状態を解除
			this.t終了処理();
			base.OnExiting(e);
		}
		protected override void Update(GameTime gameTime)
		{
		}
		protected override void Draw(GameTime gameTime)
		{
			if (Sound管理 == null)
			{
				return;
			}
			//Sound管理.t再生中の処理をする();

			if (Timer != null)
				Timer.t更新();
			if (CSound管理.rc演奏用タイマ != null)
				CSound管理.rc演奏用タイマ.t更新();

			if (Input管理 != null)
				Input管理.tポーリング(this.bApplicationActive, CDTXMania.Instance.ConfigIni.bBufferedInput);

			if (FPS != null)
				FPS.tカウンタ更新();

			//if( Pad != null )					ポーリング時にクリアしたらダメ！曲の開始時に1回だけクリアする。(2010.9.11)
			//	Pad.st検知したデバイス.Clear();

			if (this.Device == null)
				return;

			if (this.bApplicationActive)    // DTXMania本体起動中の本体/モニタの省電力モード移行を抑止
				CPowerManagement.tDisableMonitorSuspend();

			// #xxxxx 2013.4.8 yyagi; sleepの挿入位置を、EndScnene～Present間から、BeginScene前に移動。描画遅延を小さくするため。
			#region [ スリープ ]
			if (ConfigIni.nSleepPerFrameMs >= 0)            // #xxxxx 2011.11.27 yyagi
			{
				Thread.Sleep(ConfigIni.nSleepPerFrameMs);
			}
			#endregion

			#region [ DTXCreator/DTX2WAVからの指示 ]
			if (this.Window.IsReceivedMessage)  // ウインドウメッセージで、
			{
				string strMes = this.Window.strMessage;
				this.Window.IsReceivedMessage = false;
				if (strMes != null)
				{
					CommandParse.ParseArguments(strMes, ref DTXVmode, ref DTX2WAVmode);

					if (DTXVmode.Enabled)
					{
						bコンパクトモード = true;
						strコンパクトモードファイル = DTXVmode.filename;
						if (DTXVmode.Command == CDTXVmode.ECommand.Preview)
						{
							// preview soundの再生
							string strPreviewFilename = DTXVmode.previewFilename;
							//Trace.TraceInformation( "Preview Filename=" + DTXVmode.previewFilename );
							try
							{
								if (this.previewSound != null)
								{
									this.previewSound.tサウンドを停止する();
									this.previewSound.Dispose();
									this.previewSound = null;
								}
								this.previewSound = CDTXMania.Instance.Sound管理.tサウンドを生成する(strPreviewFilename);
								this.previewSound.n音量 = DTXVmode.previewVolume;
								this.previewSound.n位置 = DTXVmode.previewPan;
								this.previewSound.t再生を開始する();
								Trace.TraceInformation("DTXCからの指示で、サウンドを生成しました。({0})", strPreviewFilename);
							}
							catch
							{
								Trace.TraceError("DTXCからの指示での、サウンドの生成に失敗しました。({0})", strPreviewFilename);
								if (this.previewSound != null)
								{
									this.previewSound.Dispose();
								}
								this.previewSound = null;
							}
						}
					}
					if (DTX2WAVmode.Enabled)
					{
						if (DTX2WAVmode.Command == CDTX2WAVmode.ECommand.Cancel)
						{
							Trace.TraceInformation("録音のCancelコマンドをDTXMania本体が受信しました。");
							//Microsoft.VisualBasic.Interaction.AppActivate("メモ帳");
							//SendKeys.Send("{ESC}");
							//SendKeys.SendWait("%{F4}");
							//Application.Exit();
							if (DTX != null)	// 曲読み込みの前に録音Cancelされると、DTXがnullのままここにきてでGPFとなる→nullチェック追加
							{
								DTX.t全チップの再生停止();
								DTX.On非活性化();
							}
							r現在のステージ.On非活性化();

							//Environment.ExitCode = 10010;		// この組み合わせではダメ、返り値が反映されない
							//base.Window.Close();
							Environment.Exit(10010);			// このやり方ならばOK
						}
					}
				}
			}
			#endregion

			this.Device.BeginScene();
			this.Device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, SharpDX.Color.Black, 1f, 0);

			if (r現在のステージ != null)
			{
				this.n進行描画の戻り値 = (r現在のステージ != null) ? r現在のステージ.On進行描画() : 0;

				#region [ プラグインの進行描画 ]
				//---------------------
				foreach (STPlugin sp in this.listプラグイン)
				{
					Directory.SetCurrentDirectory(sp.strプラグインフォルダ);

					if (CDTXMania.Instance.act現在入力を占有中のプラグイン == null || CDTXMania.Instance.act現在入力を占有中のプラグイン == sp.plugin)
						sp.plugin.On進行描画(CDTXMania.Instance.Pad, CDTXMania.Instance.Input管理.Keyboard);
					else
						sp.plugin.On進行描画(null, null);

					Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
				}
				//---------------------
				#endregion

				#region [ DTX2WAVモード時、ステージが変わるたびに、そのことをDTX2WAVアプリ側に通知する ]
				if (DTX2WAVmode.Enabled && r現在のステージ != r1フレーム前のステージ)
				{
					r1フレーム前のステージ = r現在のステージ;
					//Trace.TraceInformation("Stage変更 to : " + r現在のステージ.eステージID.ToString());
					switch (r現在のステージ.eステージID)
					{
						case CStage.Eステージ.曲読み込み:
							DTX2WAVmode.SendMessage2DTX2WAV("LOAD");
							break;
						case CStage.Eステージ.演奏:
							DTX2WAVmode.SendMessage2DTX2WAV("PLAY");
							break;
						default:
							break;
					}
				}
				#endregion

				CScoreIni scoreIni = null;

				//if (Control.IsKeyLocked(Keys.CapsLock))             // #30925 2013.3.11 yyagi; capslock=ON時は、EnumSongsしないようにして、起動負荷とASIOの音切れの関係を確認する
				if (!CDTXMania.instance.ConfigIni.bEnumerateSongsInBoot)	// #40772 2020.10.12 yyagi
				{
					// → songs.db等の書き込み時だと音切れするっぽい
					actEnumSongs.On非活性化();
					EnumSongs.SongListEnumCompletelyDone();
					CDTXMania.Instance.stage選曲.bIsEnumeratingSongs = false;
				}
				#region [ 曲検索スレッドの起動/終了 ここに"Enumerating Songs..."表示を集約 ]
				if (!CDTXMania.Instance.bコンパクトモード)
				{
					actEnumSongs.On進行描画();                          // "Enumerating Songs..."アイコンの描画
				}
				switch (r現在のステージ.eステージID)
				{
					case CStage.Eステージ.タイトル:
					case CStage.Eステージ.コンフィグ:
					case CStage.Eステージ.選曲:
					case CStage.Eステージ.曲読み込み:
						if (EnumSongs != null)
						{
                            #region [ (特定条件時) 曲検索スレッドの起動・開始 ]
							if (r現在のステージ.eステージID == CStage.Eステージ.タイトル &&
									 r直前のステージ.eステージID == CStage.Eステージ.起動 &&
									 this.n進行描画の戻り値 == (int)CStageタイトル.E戻り値.継続 &&
									 !EnumSongs.IsSongListEnumStarted)
							{
								actEnumSongs.On活性化();
								CDTXMania.Instance.stage選曲.bIsEnumeratingSongs = true;
								EnumSongs.Init(CDTXMania.Instance.Songs管理.listSongsDB, CDTXMania.Instance.Songs管理.nSongsDBから取得できたスコア数); // songs.db情報と、取得した曲数を、新インスタンスにも与える
								EnumSongs.StartEnumFromDisk();      // 曲検索スレッドの起動・開始
								if (CDTXMania.Instance.Songs管理.nSongsDBから取得できたスコア数 == 0)    // もし初回起動なら、検索スレッドのプライオリティをLowestでなくNormalにする
								{
									EnumSongs.ChangeEnumeratePriority(ThreadPriority.Normal);
								}
							}
							#endregion

							#region [ 曲検索の中断と再開 ]
							if (r現在のステージ.eステージID == CStage.Eステージ.選曲 && !EnumSongs.IsSongListEnumCompletelyDone)
							{
								switch (this.n進行描画の戻り値)
								{
									case 0:     // 何もない
															//if ( CDTXMania.Instance.stage選曲.bIsEnumeratingSongs )
										if (!CDTXMania.Instance.stage選曲.bIsPlayingPremovie)
										{
											EnumSongs.Resume();                     // #27060 2012.2.6 yyagi 中止していたバックグランド曲検索を再開
											EnumSongs.IsSlowdown = false;
										}
										else
										{
											// EnumSongs.Suspend();					// #27060 2012.3.2 yyagi #PREMOVIE再生中は曲検索を低速化
											EnumSongs.IsSlowdown = true;
										}
										actEnumSongs.On活性化();
										break;

									case 2:     // 曲決定
										EnumSongs.Suspend();                        // #27060 バックグラウンドの曲検索を一時停止
										actEnumSongs.On非活性化();
										break;
								}
							}
							#endregion

							#region [ 曲探索中断待ち待機 ]
							if (r現在のステージ.eステージID == CStage.Eステージ.曲読み込み && !EnumSongs.IsSongListEnumCompletelyDone &&
									EnumSongs.thDTXFileEnumerate != null)                           // #28700 2012.6.12 yyagi; at Compact mode, enumerating thread does not exist.
							{
								EnumSongs.WaitUntilSuspended();                                 // 念のため、曲検索が一時中断されるまで待機
							}
							#endregion

							#region [ 曲検索が完了したら、実際の曲リストに反映する ]
							// CStage選曲.On活性化() に回した方がいいかな？
							if (EnumSongs.IsSongListEnumerated)
							{
								actEnumSongs.On非活性化();
								CDTXMania.Instance.stage選曲.bIsEnumeratingSongs = false;

								bool bRemakeSongTitleBar = (r現在のステージ.eステージID == CStage.Eステージ.選曲) ? true : false;
								CDTXMania.Instance.stage選曲.Refresh(EnumSongs.Songs管理, bRemakeSongTitleBar);
								EnumSongs.SongListEnumCompletelyDone();
							}
							#endregion
						}
						break;
				}
				#endregion

				switch (r現在のステージ.eステージID)
				{
					case CStage.Eステージ.何もしない:
						break;

					case CStage.Eステージ.起動:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							if (!bコンパクトモード)
							{
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ タイトル");
								stageタイトル.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageタイトル;
							}
							else
							{
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 曲読み込み");
								stage曲読み込み.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage曲読み込み;

							}
							foreach (STPlugin pg in this.listプラグイン)
							{
								Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
								pg.plugin.Onステージ変更();
								Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
							}

							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.タイトル:
                        #region [ *** ]
                        //-----------------------------
                        switch (this.n進行描画の戻り値)
						{
							case (int)CStageタイトル.E戻り値.GAMESTART:
								#region [ 選曲処理へ ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 選曲");
								stage選曲.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage選曲;
								//-----------------------------
								#endregion
								break;

							#region [ OPTION: 廃止済 ]
							//							case 2:									// #24525 OPTIONとCONFIGの統合に伴い、OPTIONは廃止
							//								#region [ *** ]
							//								//-----------------------------
							//								r現在のステージ.On非活性化();
							//								Trace.TraceInformation( "----------------------" );
							//								Trace.TraceInformation( "■ オプション" );
							//								stageオプション.On活性化();
							//								r直前のステージ = r現在のステージ;
							//								r現在のステージ = stageオプション;
							//								//-----------------------------
							//								#endregion
							//								break;
							#endregion

							case (int)CStageタイトル.E戻り値.CONFIG:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ コンフィグ");
								stageコンフィグ.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageコンフィグ;
								//-----------------------------
								#endregion
								break;

							case (int)CStageタイトル.E戻り値.EXIT:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 終了");
								stage終了.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage終了;
								//-----------------------------
								#endregion
								break;
						}

						foreach (STPlugin pg in this.listプラグイン)
						{
							Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
							pg.plugin.Onステージ変更();
							Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
						}

						//this.tガベージコレクションを実行する();		// #31980 2013.9.3 yyagi タイトル画面でだけ、毎フレームGCを実行して重くなっていた問題の修正
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.コンフィグ:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							switch (r直前のステージ.eステージID)
							{
								case CStage.Eステージ.タイトル:
									#region [ *** ]
									//-----------------------------
									r現在のステージ.On非活性化();
									Trace.TraceInformation("----------------------");
									Trace.TraceInformation("■ タイトル");
									stageタイトル.On活性化();
									r直前のステージ = r現在のステージ;
									r現在のステージ = stageタイトル;

									foreach (STPlugin pg in this.listプラグイン)
									{
										Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
										pg.plugin.Onステージ変更();
										Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
									}

									this.tガベージコレクションを実行する();
									break;
								//-----------------------------
								#endregion

								case CStage.Eステージ.選曲:
									#region [ *** ]
									//-----------------------------
									r現在のステージ.On非活性化();
									Trace.TraceInformation("----------------------");
									Trace.TraceInformation("■ 選曲");
									stage選曲.On活性化();
									r直前のステージ = r現在のステージ;
									r現在のステージ = stage選曲;

									foreach (STPlugin pg in this.listプラグイン)
									{
										Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
										pg.plugin.Onステージ変更();
										Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
									}

									this.tガベージコレクションを実行する();
									break;
									//-----------------------------
									#endregion
							}
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.選曲:
						#region [ *** ]
						//-----------------------------
						switch (this.n進行描画の戻り値)
						{
							case (int)CStage選曲.E戻り値.タイトルに戻る:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ タイトル");
								stageタイトル.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageタイトル;

								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.Onステージ変更();
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}

								this.tガベージコレクションを実行する();
								break;
							//-----------------------------
							#endregion

							case (int)CStage選曲.E戻り値.選曲した:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 曲読み込み");
								stage曲読み込み.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage曲読み込み;

								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.Onステージ変更();
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}

								this.tガベージコレクションを実行する();
								break;
							//-----------------------------
							#endregion

							case (int)CStage選曲.E戻り値.コンフィグ呼び出し:
								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ コンフィグ");
								stageコンフィグ.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageコンフィグ;

								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.Onステージ変更();
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}

								this.tガベージコレクションを実行する();
								break;
							//-----------------------------
							#endregion

							case (int)CStage選曲.E戻り値.スキン変更:

								#region [ *** ]
								//-----------------------------
								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ スキン切り替え");
								stageChangeSkin.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stageChangeSkin;
								break;
								//-----------------------------
								#endregion
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.曲読み込み:
						#region [ *** ]
						//-----------------------------
						DTXVmode.Refreshed = false;     // 曲のリロード中に発生した再リロードは、無視する。
						if (this.n進行描画の戻り値 != 0)
						{
							CDTXMania.Instance.Pad.st検知したデバイス.Clear();  // 入力デバイスフラグクリア(2010.9.11)
							r現在のステージ.On非活性化();
							#region [ ESC押下時は、曲の読み込みを中止して選曲画面に戻る ]
							if (this.n進行描画の戻り値 == (int)E曲読込画面の戻り値.読込中止)
							{
								//DTX.t全チップの再生停止();
								DTX.On非活性化();
								Trace.TraceInformation("曲の読み込みを中止しました。");
								this.tガベージコレクションを実行する();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 選曲");
								stage選曲.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage選曲;
								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.Onステージ変更();
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}
								break;
							}
							#endregion

							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ 演奏（ドラム画面）");
							r直前のステージ = r現在のステージ;
							r現在のステージ = stage演奏画面;

							foreach (STPlugin pg in this.listプラグイン)
							{
								Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
								pg.plugin.Onステージ変更();
								Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
							}

							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.演奏:
						#region [ *** ]
						//-----------------------------
						//long n1 = FDK.CSound管理.rc演奏用タイマ.nシステム時刻ms;
						//long n2 = FDK.CSound管理.SoundDevice.n経過時間ms;
						//long n3 = FDK.CSound管理.SoundDevice.tmシステムタイマ.nシステム時刻ms;
						//long n4 = FDK.CSound管理.rc演奏用タイマ.n現在時刻;
						//long n5 = FDK.CSound管理.SoundDevice.n経過時間を更新したシステム時刻ms;

						//swlist1.Add( Convert.ToInt32(n1) );
						//swlist2.Add( Convert.ToInt32(n2) );
						//swlist3.Add( Convert.ToInt32( n3 ) );
						//swlist4.Add( Convert.ToInt32( n4 ) );
						//swlist5.Add( Convert.ToInt32( n5 ) );

						#region [ DTXVモード中にDTXCreatorから指示を受けた場合の処理 ]
						if (DTXVmode.Enabled && DTXVmode.Refreshed)
						{
							DTXVmode.Refreshed = false;

							if (DTXVmode.Command == CDTXVmode.ECommand.Stop)
							{
								CDTXMania.Instance.stage演奏画面.t停止();

								if (previewSound != null)
								{
									this.previewSound.tサウンドを停止する();
									this.previewSound.Dispose();
									this.previewSound = null;
								}
								//{
								//    int lastd = 0;
								//    int f = 0;
								//    for ( int i = 0; i < swlist1.Count; i++ )
								//    {
								//        int d1 = swlist1[ i ];
								//        int d2 = swlist2[ i ];
								//        int d3 = swlist3[ i ];
								//        int d4 = swlist4[ i ];
								//        int d5 = swlist5[ i ];

								//        int dif = d1 - lastd;
								//        string s = "";
								//        if ( 16 <= dif && dif <= 17 )
								//        {
								//        }
								//        else
								//        {
								//            s = "★";
								//        }
								//        Trace.TraceInformation( "frame {0:D4}: {1:D3} ( {2:D3}, {3:D3} - {7:D3}, {4:D3} ) {5}, n現在時刻={6}", f, dif, d1, d2, d3, s, d4, d5 );
								//        lastd = d1;
								//        f++;
								//    }
								//    swlist1.Clear();
								//    swlist2.Clear();
								//    swlist3.Clear();
								//    swlist4.Clear();
								//    swlist5.Clear();

								//}
							}
							else if (DTXVmode.Command == CDTXVmode.ECommand.Play)
							{
								if (DTXVmode.NeedReload)
								{
									CDTXMania.Instance.stage演奏画面.t再読込();
									if (DTXVmode.GRmode)
									{
										CDTXMania.Instance.ConfigIni.eActiveInst.Value = EActiveInstrument.GBOnly;
									}
									else
									{
										CDTXMania.Instance.ConfigIni.eActiveInst.Value = EActiveInstrument.Both;
									}
									CDTXMania.Instance.ConfigIni.bTimeStretch.Value = DTXVmode.TimeStretch;
									CSound管理.bIsTimeStretch = DTXVmode.TimeStretch;
									if (CDTXMania.Instance.ConfigIni.bVSyncWait != DTXVmode.VSyncWait)
									{
										CDTXMania.Instance.ConfigIni.bVSyncWait.Value = DTXVmode.VSyncWait;
										CDTXMania.Instance.b次のタイミングで垂直帰線同期切り替えを行う = true;
									}
								}
								else
								{
									CDTXMania.Instance.stage演奏画面.t演奏位置の変更(CDTXMania.Instance.DTXVmode.nStartBar);
								}
							}
						}
						#endregion

						switch (this.n進行描画の戻り値)
						{
							case (int)E演奏画面の戻り値.再読込_再演奏:
								#region [ DTXファイルを再読み込みして、再演奏 ]
								DTX.t全チップの再生停止();
								DTX.On非活性化();
								r現在のステージ.On非活性化();
								stage曲読み込み.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage曲読み込み;
								this.tガベージコレクションを実行する();
								break;
							#endregion

							//case (int) E演奏画面の戻り値.再演奏:
							#region [ 再読み込み無しで、再演奏 ]
							#endregion
							//	break;

							case (int)E演奏画面の戻り値.継続:
								break;

							case (int)E演奏画面の戻り値.演奏中断:
								#region [ 演奏キャンセル ]
								//-----------------------------
								scoreIni = this.tScoreIniへBGMAdjustとHistoryとPlayCountを更新("Play canceled");
								if (CDTXMania.Instance.ConfigIni.bIsSwappedGuitarBass)      // #35417 2015.8.18 yyagi Gt/Bsを入れ替えていたなら、演奏設定を元に戻す
								{
									//CDTXMania.Instance.DTX.SwapGuitarBassInfos();						// 譜面情報も元に戻す (現在は再演奏機能なしのため、元に戻す必要はない)
								}

								//int lastd = 0;
								//int f = 0;
								//for (int i = 0; i < swlist1.Count; i++)
								//{
								//    int d1 = swlist1[ i ];
								//    int d2 = swlist2[ i ];
								//    int d3 = swlist3[ i ];
								//    int d4 = swlist4[ i ];

								//    int dif = d1 - lastd;
								//    string s = "";
								//    if ( 16 <= dif && dif <= 17 )
								//    {
								//    }
								//    else
								//    {
								//        s = "★";
								//    }
								//    Trace.TraceInformation( "frame {0:D4}: {1:D3} ( {2:D3}, {3:D3}, {4:D3} ) {5}, n現在時刻={6}", f, dif, d1, d2, d3, s, d4 );
								//    lastd = d1;
								//    f++;
								//}
								//swlist1.Clear();
								//swlist2.Clear();
								//swlist3.Clear();
								//swlist4.Clear();

								#region [ プラグイン On演奏キャンセル() の呼び出し ]
								//---------------------
								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.On演奏キャンセル(scoreIni);
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}
								//---------------------
								#endregion

								DTX.t全チップの再生停止();
								DTX.On非活性化();
								r現在のステージ.On非活性化();
								if (DTX2WAVmode.Enabled)
								{
									Environment.Exit(0);
								}
								if (bコンパクトモード)
								{
									base.Window.Close();
								}
								else
								{
									Trace.TraceInformation("----------------------");
									Trace.TraceInformation("■ 選曲");
									stage選曲.On活性化();
									r直前のステージ = r現在のステージ;
									r現在のステージ = stage選曲;

									#region [ プラグイン Onステージ変更() の呼び出し ]
									//---------------------
									foreach (STPlugin pg in this.listプラグイン)
									{
										Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
										pg.plugin.Onステージ変更();
										Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
									}
									//---------------------
									#endregion

									this.tガベージコレクションを実行する();
								}
								break;
							//-----------------------------
							#endregion

							case (int)E演奏画面の戻り値.ステージ失敗:
								#region [ 演奏失敗(StageFailed) ]
								//-----------------------------
								scoreIni = this.tScoreIniへBGMAdjustとHistoryとPlayCountを更新("Stage failed");

								#region [ プラグイン On演奏失敗() の呼び出し ]
								//---------------------
								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.On演奏失敗(scoreIni);
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}
								//---------------------
								#endregion

								DTX.t全チップの再生停止();
								DTX.On非活性化();
								r現在のステージ.On非活性化();
								if (bコンパクトモード)
								{
									base.Window.Close();
								}
								else
								{
									Trace.TraceInformation("----------------------");
									Trace.TraceInformation("■ 選曲");
									stage選曲.On活性化();
									r直前のステージ = r現在のステージ;
									r現在のステージ = stage選曲;

									#region [ プラグイン Onステージ変更() の呼び出し ]
									//---------------------
									foreach (STPlugin pg in this.listプラグイン)
									{
										Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
										pg.plugin.Onステージ変更();
										Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
									}
									//---------------------
									#endregion

									this.tガベージコレクションを実行する();
								}
								break;
							//-----------------------------
							#endregion

							case (int)E演奏画面の戻り値.ステージクリア:
								#region [ 演奏クリア ]
								//-----------------------------
								STDGBSValue<CScoreIni.C演奏記録> record;
								record = stage演奏画面.Record;

								double playskill = 0.0;

								for (EPart inst = EPart.Drums; inst <= EPart.Bass; ++inst)
								{
									if (!record[inst].b全AUTOである && record[inst].n全チップ数 > 0)
									{
										playskill = record[inst].db演奏型スキル値;
									}
								}

								string str = "Cleared";
								switch (CScoreIni.t総合ランク値を計算して返す(record))
								{
									case CScoreIni.ERANK.SS:
										str = string.Format("Cleared (SS: {0:F2})", playskill);
										break;

									case CScoreIni.ERANK.S:
										str = string.Format("Cleared (S: {0:F2})", playskill);
										break;

									case CScoreIni.ERANK.A:
										str = string.Format("Cleared (A: {0:F2})", playskill);
										break;

									case CScoreIni.ERANK.B:
										str = string.Format("Cleared (B: {0:F2})", playskill);
										break;

									case CScoreIni.ERANK.C:
										str = string.Format("Cleared (C: {0:F2})", playskill);
										break;

									case CScoreIni.ERANK.D:
										str = string.Format("Cleared (D: {0:F2})", playskill);
										break;

									case CScoreIni.ERANK.E:
										str = string.Format("Cleared (E: {0:F2})", playskill);
										break;

									case CScoreIni.ERANK.UNKNOWN:   // #23534 2010.10.28 yyagi add: 演奏チップが0個のとき
										str = "Cleared (No chips)";
										break;
								}

								scoreIni = this.tScoreIniへBGMAdjustとHistoryとPlayCountを更新(str);

								#region [ プラグイン On演奏クリア() の呼び出し ]
								//---------------------
								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.On演奏クリア(scoreIni);
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}
								//---------------------
								#endregion

								r現在のステージ.On非活性化();
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 結果");
								stage結果.st演奏記録 = record;
								stage結果.r空うちドラムチップ = stage演奏画面.GetNoChipDrums();
								stage結果.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage結果;

								#region [ プラグイン Onステージ変更() の呼び出し ]
								//---------------------
								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.Onステージ変更();
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}
								//---------------------
								#endregion

								break;
								//-----------------------------
								#endregion
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.結果:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							// #35417 2015.08.30 chnmr0 changed : ステージクリア処理で入れ替えるため元に戻した
							// #35417 2015.8.18 yyagi: AUTO系のフラグ入れ替えは削除可能!?。以後AUTOフラグに全くアクセスしておらず、意味がないため。
							if (CDTXMania.Instance.ConfigIni.bIsSwappedGuitarBass)      // #24415 2011.2.27 yyagi Gt/Bsを入れ替えていたなら、Auto状態をリザルト画面終了後に元に戻す
							{
								CDTXMania.Instance.ConfigIni.SwapGuitarBassInfos_AutoFlags();   // Auto入れ替え
							}

							DTX.t全チップの再生一時停止();
							DTX.On非活性化();
							r現在のステージ.On非活性化();
							if (!bコンパクトモード)
							{
								Trace.TraceInformation("----------------------");
								Trace.TraceInformation("■ 選曲");
								stage選曲.On活性化();
								r直前のステージ = r現在のステージ;
								r現在のステージ = stage選曲;

								foreach (STPlugin pg in this.listプラグイン)
								{
									Directory.SetCurrentDirectory(pg.strプラグインフォルダ);
									pg.plugin.Onステージ変更();
									Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
								}

								this.tガベージコレクションを実行する();
							}
							else
							{
								base.Window.Close();
							}
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.ChangeSkin:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							r現在のステージ.On非活性化();
							Trace.TraceInformation("----------------------");
							Trace.TraceInformation("■ 選曲");
							stage選曲.On活性化();
							r直前のステージ = r現在のステージ;
							r現在のステージ = stage選曲;
							this.tガベージコレクションを実行する();
						}
						//-----------------------------
						#endregion
						break;

					case CStage.Eステージ.終了:
						#region [ *** ]
						//-----------------------------
						if (this.n進行描画の戻り値 != 0)
						{
							base.Exit();
						}
						//-----------------------------
						#endregion
						break;
				}
			}
			this.Device.EndScene();
			// Present()は game.csのOnFrameEnd()に登録された、GraphicsDeviceManager.game_FrameEnd() 内で実行されるので不要
			// (つまり、Present()は、Draw()完了後に実行される)
#if !GPUFlushAfterPresent
			actFlushGPU.On進行描画();       // Flush GPU	// EndScene()～Present()間 (つまりVSync前) でFlush実行
#endif
			if (Sound管理.CurrentSoundDeviceType != ESoundDeviceType.DirectSound)
			{
				Sound管理.t再生中の処理をする();   // サウンドバッファの更新; 画面描画と同期させることで、スクロールをスムーズにする
			}

			#region [ マウスカーソル消去制御 ]
			if (cMouseHideControl != null) cMouseHideControl.tHideCursorIfNeed();
			#endregion
			#region [ 全画面・ウインドウ切り替え ]
			if (this.b次のタイミングで全画面_ウィンドウ切り替えを行う)
			{
				// ConfigIni.bFullScreen.Value = !ConfigIni.bFullScreen;
				Instance.t全画面_ウィンドウモード切り替え();
				this.b次のタイミングで全画面_ウィンドウ切り替えを行う = false;
			}
			#endregion
			#region [ 垂直基線同期切り替え ]
			if (this.b次のタイミングで垂直帰線同期切り替えを行う)
			{
				bool bIsMaximized = this.Window.IsMaximized;                                            // #23510 2010.11.3 yyagi: to backup current window mode before changing VSyncWait
				currentClientSize = this.Window.ClientSize;                                             // #23510 2010.11.3 yyagi: to backup current window size before changing VSyncWait
				DeviceSettings currentSettings = Instance.GraphicsDeviceManager.CurrentSettings;
				currentSettings.EnableVSync = ConfigIni.bVSyncWait;
				Instance.GraphicsDeviceManager.ChangeDevice(currentSettings);
				this.b次のタイミングで垂直帰線同期切り替えを行う = false;
				base.Window.ClientSize = new Size(currentClientSize.Width, currentClientSize.Height);   // #23510 2010.11.3 yyagi: to resume window size after changing VSyncWait
				if (bIsMaximized)
				{
					this.Window.WindowState = FormWindowState.Maximized;                                // #23510 2010.11.3 yyagi: to resume window mode after changing VSyncWait
				}
			}
			#endregion

			GC.Collect( 0, GCCollectionMode.Optimized, false );		// Rel105で処理が重くなっていることに対する、暫定処置。
																	// 重くなっている原因に対する適切な処置をして、処理が104程度に軽くなったら、
																	// この暫定処置は削除します。
		}

		/// <summary>
		/// XML ファイルからオブジェクトを生成します。
		/// </summary>
		/// <param name="xmlfile">オブジェクトが記述される XML のパス。これは DataContract によってシリアライズされていなければなりません。</param>
		/// <returns>生成したオブジェクト。正しく生成できなかった場合 null 。</returns>
		public static object DeserializeXML(string xmlpath, Type t)
		{
			object ret = null;
			try
			{
				if (File.Exists(xmlpath))
				{
					using (StreamReader reader = new StreamReader(xmlpath, Encoding.GetEncoding("shift_jis")))
					using (XmlReader xr = XmlReader.Create(reader))
					{
						DataContractSerializer serializer = new DataContractSerializer(t);
						ret = serializer.ReadObject(xr);
					}
				}
			}
			catch (Exception e)
			{
				Trace.TraceWarning( e.Message );
				ret = null;
			}
			return ret;
		}

		/// <summary>
		/// オブジェクトから XML ファイルを生成します。
		/// </summary>
		/// <param name="xmlfile">XML ファイルのパス。</param>
		/// <param name="obj">XML としてシリアライズするオブジェクト。DataContract 属性を持つクラスからインスタンス化されたオブジェクトです。</param>
		public static void SerializeXML(string xmlpath, object obj)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.IndentChars = "  ";
			settings.Indent = true;
			settings.NewLineChars = Environment.NewLine;
			settings.Encoding = new System.Text.UTF8Encoding(false);
            using (FileStreamSSD fsssd = new FileStreamSSD(xmlpath))
            {
                using (XmlWriter xw = XmlWriter.Create(fsssd, settings))
                {
                    DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                    serializer.WriteObject(xw, obj);
                }
            }
		}

		public void SaveConfig()
		{
			#region [ Skinパスの絶対パス→相対パス変換 ]
			string _strSystemSkinSubfolderPath = ConfigIni.strSystemSkinSubfolderPath.Value;
			Uri uriRoot = new Uri( System.IO.Path.Combine( this.strEXEのあるフォルダ, "System" + System.IO.Path.DirectorySeparatorChar ) );
			if ( ConfigIni.strSystemSkinSubfolderPath.Value != null && ConfigIni.strSystemSkinSubfolderPath.Value.Length == 0 )
			{
				// Config.iniが空の状態でDTXManiaをViewerとして起動・終了すると、strSystemSkinSubfolderFullName が空の状態でここに来る。
				// → 初期値として Default/ を設定する。
				ConfigIni.strSystemSkinSubfolderPath.Value = System.IO.Path.Combine( this.strEXEのあるフォルダ, "System" + System.IO.Path.DirectorySeparatorChar + "Default" + System.IO.Path.DirectorySeparatorChar );
			}

			// 起動直後は(Loadの前にSaveを通るため)Skinパスには初期値の相対パスが入っている場合がある。
			// そのため、以下の処理を通すために、いったん絶対パスに変換
			if ( !System.IO.Path.IsPathRooted( ConfigIni.strSystemSkinSubfolderPath.Value ) )
			{
				ConfigIni.strSystemSkinSubfolderPath.Value =
					Path.Combine( Path.Combine( this.strEXEのあるフォルダ, "System" ), ConfigIni.strSystemSkinSubfolderPath );
			}

			Uri uriPath = new Uri( System.IO.Path.Combine( ConfigIni.strSystemSkinSubfolderPath.Value, "." + System.IO.Path.DirectorySeparatorChar ) );
			string relPath = uriRoot.MakeRelativeUri( uriPath ).ToString();				// 相対パスを取得
			relPath = System.Web.HttpUtility.UrlDecode( relPath );						// デコードする
			relPath = relPath.Replace( '/', System.IO.Path.DirectorySeparatorChar );	// 区切り文字が\ではなく/なので置換する
			ConfigIni.strSystemSkinSubfolderPath.Value = relPath;
			#endregion
			ConfigIni.strDTXManiaVersion.Value = CDTXMania.VERSION;

			CDTXMania.SerializeXML( strEXEのあるフォルダ + "Config.xml", ConfigIni );

			// 元の絶対パスに戻す
			ConfigIni.strSystemSkinSubfolderPath.Value = _strSystemSkinSubfolderPath;
		}

		public void LoadConfig()
		{
			string path = strEXEのあるフォルダ + "Config.xml";

			if (!File.Exists(path))
			{
				SaveConfig();
			}
			if (File.Exists(path))
			{
				ConfigIni = (CConfigXml)CDTXMania.DeserializeXML(path, typeof(CConfigXml));
				if (ConfigIni == null)
				{
					ConfigIni = new CConfigXml();
					SaveConfig();
				}
				// Skinパスの相対パスを、絶対パスに変換
				if ( !System.IO.Path.IsPathRooted( ConfigIni.strSystemSkinSubfolderPath.Value ) )
				{
					ConfigIni.strSystemSkinSubfolderPath.Value =
						Path.Combine( Path.Combine( this.strEXEのあるフォルダ, "System" ), ConfigIni.strSystemSkinSubfolderPath );
				}
			}

			ConfigIni.UpgradeConfig();	// 本体version upに伴ってConfig.xmlの定義が更新される場合の、最低限のフォローアップ
		}
		/// <summary>
		/// 座標値を読み込む。Coordinates メンバ初期化後いつ呼び出しても構わない。
		/// </summary>
		public void UpdateCoordinates()
		{
			string coordXml = strEXEのあるフォルダ + "Coordinates.xml";
 
			// デシリアライズ
			if (File.Exists(coordXml))
			{
				using (XmlReader xr = XmlReader.Create(coordXml))
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Coordinates.CCoordinates));
					try
					{
						Coordinates = (Coordinates.CCoordinates) serializer.ReadObject( xr );
					}
					catch (SerializationException e)
					{
						Trace.TraceWarning( "Rel107以前の古いフォーマットのCoordinates.xmlが読み込まれました。無視します。\n" + e.Message );
					}
				}
			}
			// シリアライズ
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.IndentChars = "  ";
			settings.Indent = true;
			settings.NewLineChars = Environment.NewLine;
			settings.Encoding = new System.Text.UTF8Encoding( false );
			using ( XmlWriter xw = XmlTextWriter.Create( coordXml, settings ) )
			{
				//XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
				//ns.Add( String.Empty, String.Empty );

				//StreamWriter sw = new StreamWriter( "test2.xml", false, Encoding.UTF8 );
				//serializer.Serialize( sw, item, ns );
				//sw.Close

				DataContractSerializer serializer = new DataContractSerializer( typeof( Coordinates.CCoordinates ) );
				serializer.WriteObject( xw, Coordinates );
				//serializer.WriteStartObject( xw, Coordinates );
				//xw.WriteAttributeString( "xmlns", "d1p1", "http://www.w3.org/2000/xmlns/",
				//	"http://schemas.microsoft.com/2003/10/Serialization/" );
				//serializer.WriteObjectContent( xw, Coordinates );
				//serializer.WriteEndObject( xw );
			}

			// もう一度デシリアライズ
			if (File.Exists(coordXml))
			{
				using (XmlReader xr = XmlReader.Create(coordXml))
				{
					DataContractSerializer serializer = new DataContractSerializer(typeof(Coordinates.CCoordinates));
					Coordinates = (Coordinates.CCoordinates)serializer.ReadObject(xr);
				}
			}
		}


		/// <summary>
		/// 保存するxmlからnamespaceを削除するためのXmlTextWriter
		/// </summary>
		//public class MyXmlTextWriter : XmlTextWriter
		//{
		//	private bool _ignoreAttr = false;

		//	public MyXmlTextWriter( TextWriter w  )
		//		: base( w )
		//	{
		//		Debug.WriteLine( "create" );
		//	}

		//	public override string LookupPrefix( string ns )
		//	{
		//		Debug.WriteLine( "luprefix" );
		//		return string.Empty;
		//	}

		//	public override void WriteStartAttribute( string prefix, string localName, string ns )
		//	{
		//		Debug.WriteLine( "writestartattribute" );
		//		if ( String.Compare( prefix, "xmlns", true ) == 0 )
		//		{
		//			Debug.WriteLine( "[!]" );
		//			this._ignoreAttr = true;
		//			return;
		//		}
		//	}

		//	public override void WriteEndAttribute()
		//	{
		//		if ( this._ignoreAttr )
		//		{
		//			this._ignoreAttr = false;
		//			return;
		//		}
		//		base.WriteEndAttribute();
		//	}

		//	public override void WriteString( string text )
		//	{
		//		Debug.WriteLine( "ws" );
		//		if ( String.Compare( text, "http://www.w3.org/2001/XMLSchema-instance", true ) == 0 )
		//		{
		//			return;
		//		}
		//		base.WriteString( text );
		//	}

		//	public override void WriteStartElement( string prefix, string localName, string ns )
		//	{
		//		Debug.WriteLine( "wse" );
		//		base.WriteStartElement( null, localName, null );
		//	}
		//}

		public void ShowWindowTitleWithSoundType()
		{
			string delay = "";
			if (Sound管理.CurrentSoundDeviceType != ESoundDeviceType.DirectSound)
			{
				delay = "(" + Sound管理.GetSoundDelay() + "ms)";
			}
			base.Window.Text = strWindowTitle;
			if (!this.DTX2WAVmode.Enabled)
			{
				base.Window.Text += " (" + Sound管理.GetCurrentSoundDeviceType() + delay + ")";
			}
		}

		#region [ private ]
		//-----------------
		private bool b終了処理完了済み;
		private static CDTX dtx;
		private List<CActivity> listトップレベルActivities;
		private int n進行描画の戻り値;
		private MouseButtons mb = System.Windows.Forms.MouseButtons.Left;
		private string strWindowTitle
		{
			get
			{
				string strCPUmode = (Environment.Is64BitProcess) ? " [x64]" : " [x86]";

					if (DTXVmode.Enabled)
				{
					return "DTXMViewer release " + VERSION + strCPUmode;
				}
				else if (DTX2WAVmode.Enabled)
				{
					return "DTX2WAV (" + VERSION + "): " + Path.GetFileName(this.DTX2WAVmode.dtxfilename) + strCPUmode;
				}
				else
				{
					return "DTXMania .NET style release " + VERSION + strCPUmode;
				}
			}
		}
		private CSound previewSound;
		private CMouseHideControl cMouseHideControl = null;

		private void t終了処理()
		{
			if (!this.b終了処理完了済み)
			{
				Trace.TraceInformation("----------------------");
				Trace.TraceInformation("■ アプリケーションの終了");
				#region[ 電源プランの復元 ]
				CPowerPlan.RestoreCurrentPowerPlan();           // 電源プランを元のものに戻す
				#endregion
				#region [ 曲検索の終了処理 ]
				//---------------------
				if (actEnumSongs != null)
				{
					Trace.TraceInformation("曲検索actの終了処理を行います。");
					Trace.Indent();
					try
					{
						actEnumSongs.On非活性化();
						actEnumSongs = null;
						Trace.TraceInformation("曲検索actの終了処理を完了しました。");
					}
					catch (Exception e)
					{
						Trace.TraceError(e.Message);
						Trace.TraceError("曲検索actの終了処理に失敗しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ 現在のステージの終了処理 ]
				//---------------------
				if (CDTXMania.Instance.r現在のステージ != null && CDTXMania.Instance.r現在のステージ.b活性化してる)     // #25398 2011.06.07 MODIFY FROM
				{
					Trace.TraceInformation("現在のステージを終了します。");
					Trace.Indent();
					try
					{
						r現在のステージ.On非活性化();
						Trace.TraceInformation("現在のステージの終了処理を完了しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion

				#region [ 選曲ステージの終了処理 ]
				stage選曲.On非活性化();
				#endregion

				#region [ プラグインの終了処理 ]
				//---------------------
				if (this.listプラグイン != null && this.listプラグイン.Count > 0)
				{
					Trace.TraceInformation("すべてのプラグインを終了します。");
					Trace.Indent();
					try
					{
						foreach (STPlugin st in this.listプラグイン)
						{
							Directory.SetCurrentDirectory(st.strプラグインフォルダ);
							st.plugin.OnUnmanagedリソースの解放();
							st.plugin.OnManagedリソースの解放();
							st.plugin.On終了();
							Directory.SetCurrentDirectory(CDTXMania.Instance.strEXEのあるフォルダ);
						}
						PluginHost = null;
						Trace.TraceInformation("すべてのプラグインの終了処理を完了しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ 曲リストの終了処理 ]
				//---------------------
				if (Songs管理 != null)
				{
					Trace.TraceInformation("曲リストの終了処理を行います。");
					Trace.Indent();
					try
					{
						Songs管理 = null;
						Trace.TraceInformation("曲リストの終了処理を完了しました。");
					}
					catch (Exception exception)
					{
						Trace.TraceError(exception.Message);
						Trace.TraceError("曲リストの終了処理に失敗しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				CAvi.t終了();
				//---------------------
				#endregion
				#region [ スキンの終了処理 ]
				//---------------------
				if (Skin != null)
				{
					Trace.TraceInformation("スキンの終了処理を行います。");
					Trace.Indent();
					try
					{
						Skin.Dispose();
						Skin = null;
						Trace.TraceInformation("スキンの終了処理を完了しました。");
					}
					catch (Exception exception2)
					{
						Trace.TraceError(exception2.Message);
						Trace.TraceError("スキンの終了処理に失敗しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ DirectSoundの終了処理 ]
				//---------------------
				if (Sound管理 != null)
				{
					Trace.TraceInformation("DirectSound の終了処理を行います。");
					Trace.Indent();
					try
					{
						Sound管理.Dispose();
						Sound管理 = null;
						Trace.TraceInformation("DirectSound の終了処理を完了しました。");
					}
					catch (Exception exception3)
					{
						Trace.TraceError(exception3.Message);
						Trace.TraceError("DirectSound の終了処理に失敗しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ パッドの終了処理 ]
				//---------------------
				if (Pad != null)
				{
					Trace.TraceInformation("パッドの終了処理を行います。");
					Trace.Indent();
					try
					{
						Pad = null;
						Trace.TraceInformation("パッドの終了処理を完了しました。");
					}
					catch (Exception exception4)
					{
						Trace.TraceError(exception4.Message);
						Trace.TraceError("パッドの終了処理に失敗しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ DirectInput, MIDI入力の終了処理 ]
				//---------------------
				if (Input管理 != null)
				{
					Trace.TraceInformation("DirectInput, MIDI入力の終了処理を行います。");
					Trace.Indent();
					try
					{
						Input管理.Dispose();
						Input管理 = null;
						Trace.TraceInformation("DirectInput, MIDI入力の終了処理を完了しました。");
					}
					catch (Exception exception5)
					{
						Trace.TraceError(exception5.Message);
						Trace.TraceError("DirectInput, MIDI入力の終了処理に失敗しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ 文字コンソールの終了処理 ]
				//---------------------
				if (act文字コンソール != null)
				{
					Trace.TraceInformation("文字コンソールの終了処理を行います。");
					Trace.Indent();
					try
					{
						act文字コンソール.On非活性化();
						act文字コンソール = null;
						Trace.TraceInformation("文字コンソールの終了処理を完了しました。");
					}
					catch (Exception exception6)
					{
						Trace.TraceError(exception6.Message);
						Trace.TraceError("文字コンソールの終了処理に失敗しました。");
					}
					finally
					{
						Trace.Unindent();
					}
				}
				//---------------------
				#endregion
				#region [ FPSカウンタの終了処理 ]
				//---------------------
				Trace.TraceInformation("FPSカウンタの終了処理を行います。");
				Trace.Indent();
				try
				{
					if (FPS != null)
					{
						FPS = null;
					}
					Trace.TraceInformation("FPSカウンタの終了処理を完了しました。");
				}
				finally
				{
					Trace.Unindent();
				}
				//---------------------
				#endregion
				#region [ タイマの終了処理 ]
				//---------------------
				Trace.TraceInformation("タイマの終了処理を行います。");
				Trace.Indent();
				try
				{
					if (Timer != null)
					{
						Timer.Dispose();
						Timer = null;
						Trace.TraceInformation("タイマの終了処理を完了しました。");
					}
					else
					{
						Trace.TraceInformation("タイマは使用されていません。");
					}
				}
				finally
				{
					Trace.Unindent();
				}
				//---------------------
				#endregion
				#region [ Config.iniの出力 ]
				//---------------------
				Trace.TraceInformation("Config.xml を出力します。");
				//				if ( ConfigIni.bIsSwappedGuitarBass )			// #24063 2011.1.16 yyagi ギターベースがスワップしているときは元に戻す
				if (ConfigIni.bIsSwappedGuitarBass_AutoFlagsAreSwapped) // #24415 2011.2.21 yyagi FLIP中かつ演奏中にalt-f4で終了したときは、AUTOのフラグをswapして戻す
				{
					ConfigIni.SwapGuitarBassInfos_AutoFlags();
				}
				/*
					if (ConfigIni.bIsSwappedGuitarBass_PlaySettingsAreSwapped)  // #35417 2015/8/18 yyagi FLIP中かつ演奏中にalt-f4で終了したときは、演奏設定のフラグをswapして戻す
					{
							ConfigIni.SwapGuitarBassInfos_PlaySettings();
					}
				 */
				string str = strEXEのあるフォルダ + "Config.xml";
				Trace.Indent();
				try
				{
					if (DTXVmode.Enabled)
					{
						DTXVmode.tUpdateConfigIni();
						Trace.TraceInformation("DTXVモードの設定情報を、Config.xmlに保存しました。");
					}
					else if (DTX2WAVmode.Enabled)
					{
						DTX2WAVmode.tUpdateConfigIni();
						Trace.TraceInformation("DTX2WAVモードの設定情報を、Config.xmlに保存しました。");
						DTX2WAVmode.SendMessage2DTX2WAV("TERM");
					}
					else
					{
						CDTXMania.Instance.SaveConfig();
						Trace.TraceInformation("保存しました。({0})", str);
					}
				}
				catch (Exception e)
				{
					Trace.TraceError(e.Message);
					Trace.TraceError("Config.xml の出力に失敗しました。({0})", str);
				}
				finally
				{
					Trace.Unindent();
				}
				//---------------------
				#endregion
				#region [ DTXVmodeの終了処理 ]
				//---------------------
				//Trace.TraceInformation( "DTXVモードの終了処理を行います。" );
				//Trace.Indent();
				try
				{
					if (DTXVmode != null)
					{
						DTXVmode = null;
						//Trace.TraceInformation( "DTXVモードの終了処理を完了しました。" );
					}
					else
					{
						//Trace.TraceInformation( "DTXVモードは使用されていません。" );
					}
				}
				finally
				{
					//Trace.Unindent();
				}
				//---------------------
				#endregion
				#region [ DirectXの終了処理 ]
				//---------------------
				base.GraphicsDeviceManager.Dispose();
				//---------------------
				#endregion
				Trace.TraceInformation( "アプリケーションの終了処理を完了しました。" );


				this.b終了処理完了済み = true;
			}
		}
		private CScoreIni tScoreIniへBGMAdjustとHistoryとPlayCountを更新(string str新ヒストリ行)
		{
			STDGBSValue<bool> isUpdated = new STDGBSValue<bool>();
			string strFilename = DTX.strファイル名の絶対パス + ".score.ini";
			CScoreIni ini = new CScoreIni(strFilename);
			if (!File.Exists(strFilename))
			{
				ini.stファイル.Title = DTX.TITLE;
				ini.stファイル.Name = DTX.strファイル名;
				ini.stファイル.Hash = CScoreIni.tファイルのMD5を求めて返す(DTX.strファイル名の絶対パス);
				for (EPart i = EPart.Drums; i <= EPart.Bass; ++i)
				{
					ini.stセクション.HiScore[i].nPerfectになる範囲ms = nPerfect範囲ms;
					ini.stセクション.HiScore[i].nGreatになる範囲ms = nGreat範囲ms;
					ini.stセクション.HiScore[i].nGoodになる範囲ms = nGood範囲ms;
					ini.stセクション.HiScore[i].nPoorになる範囲ms = nPoor範囲ms;

					ini.stセクション.HiSkill[i].nPerfectになる範囲ms = nPerfect範囲ms;
					ini.stセクション.HiSkill[i].nGreatになる範囲ms = nGreat範囲ms;
					ini.stセクション.HiSkill[i].nGoodになる範囲ms = nGood範囲ms;
					ini.stセクション.HiSkill[i].nPoorになる範囲ms = nPoor範囲ms;

					ini.stセクション.LastPlay[i].nPerfectになる範囲ms = nPerfect範囲ms;
					ini.stセクション.LastPlay[i].nGreatになる範囲ms = nGreat範囲ms;
					ini.stセクション.LastPlay[i].nGoodになる範囲ms = nGood範囲ms;
					ini.stセクション.LastPlay[i].nPoorになる範囲ms = nPoor範囲ms;
				}
			}
			ini.stファイル.BGMAdjust = DTX.nBGMAdjust;
			isUpdated = CScoreIni.t更新条件を取得する();
			if (isUpdated.Drums || isUpdated.Guitar || isUpdated.Bass)
			{
				if (isUpdated.Drums)
				{
					ini.stファイル.PlayCountDrums++;
				}
				if (isUpdated.Guitar)
				{
					ini.stファイル.PlayCountGuitar++;
				}
				if (isUpdated.Bass)
				{
					ini.stファイル.PlayCountBass++;
				}
				ini.tヒストリを追加する(str新ヒストリ行);
				if (!bコンパクトモード)
				{
					stage選曲.r現在選択中のスコア.譜面情報.演奏回数.Drums = ini.stファイル.PlayCountDrums;
					stage選曲.r現在選択中のスコア.譜面情報.演奏回数.Guitar = ini.stファイル.PlayCountGuitar;
					stage選曲.r現在選択中のスコア.譜面情報.演奏回数.Bass = ini.stファイル.PlayCountBass;
					for (int j = 0; j < ini.stファイル.History.Length; j++)
					{
						stage選曲.r現在選択中のスコア.譜面情報.演奏履歴[j] = ini.stファイル.History[j];
					}
				}
			}
			if (ConfigIni.bScoreIni)
			{
				ini.t書き出し(strFilename);
			}

			return ini;
		}
		private void tガベージコレクションを実行する()
		{
			// LOHに対するコンパクションを要求
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

			GC.Collect(0, GCCollectionMode.Optimized, true );
			GC.WaitForPendingFinalizers();
			GC.Collect(0, GCCollectionMode.Forced, true );
			GC.WaitForPendingFinalizers();

			// 通常通り、LOHへのGCを抑制
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.Default;
		}
		private void tプラグイン検索と生成()
		{
			this.listプラグイン = new List<STPlugin>();

			string strIPluginActivityの名前 = typeof(IPluginActivity).FullName;
			string strプラグインフォルダパス = strEXEのあるフォルダ + "Plugins\\";

			this.t指定フォルダ内でのプラグイン検索と生成(strプラグインフォルダパス, strIPluginActivityの名前);

			if (this.listプラグイン.Count > 0)
				Trace.TraceInformation(this.listプラグイン.Count + " 個のプラグインを読み込みました。");
		}

		private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var domain = (AppDomain)sender;

			foreach (var assembly in domain.GetAssemblies())
			{
				if (assembly.FullName == args.Name)
					return assembly;
			}
			return null;
		}
		private void t指定フォルダ内でのプラグイン検索と生成(string strプラグインフォルダパス, string strプラグイン型名)
		{
			// 指定されたパスが存在しないとエラー
			if (!Directory.Exists(strプラグインフォルダパス))
			{
				Trace.TraceWarning("プラグインフォルダが存在しません。(" + strプラグインフォルダパス + ")");
				return;
			}

			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;


			// (1) すべての *.dll について…
			string[] strDLLs = System.IO.Directory.GetFiles(strプラグインフォルダパス, "*.dll");
			foreach (string dllName in strDLLs)
			{
				if (Path.GetExtension(dllName).ToLower() != ".dll")
				{
					continue;
				}
				try
				{
					// (1-1) dll をアセンブリとして読み込む。
					System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom(dllName);

					// (1-2) アセンブリ内のすべての型について、プラグインとして有効か調べる
					foreach (Type t in asm.GetTypes())
					{
						//  (1-3) ↓クラスであり↓Publicであり↓抽象クラスでなく↓IPlugin型のインスタンスが作れる　型を持っていれば有効
						if (t.IsClass && t.IsPublic && !t.IsAbstract && t.GetInterface(strプラグイン型名) != null)
						{
							// (1-4) クラス名からインスタンスを作成する
							var st = new STPlugin()
							{
								plugin = (IPluginActivity)asm.CreateInstance(t.FullName),
								strプラグインフォルダ = Path.GetDirectoryName(dllName),
								strアセンブリ簡易名 = asm.GetName().Name,
								Version = asm.GetName().Version,
							};

							// (1-5) プラグインリストへ登録
							this.listプラグイン.Add(st);
							Trace.TraceInformation("プラグイン {0} ({1}, {2}, {3}) を読み込みました。", t.FullName, Path.GetFileName(dllName), st.strアセンブリ簡易名, st.Version.ToString());
						}
					}
				}
				catch (System.Reflection.ReflectionTypeLoadException e)
				{
					Trace.TraceInformation(dllName + " からプラグインを生成することに失敗しました。スキップします。");
					Trace.TraceInformation(e.ToString());
					Trace.TraceInformation(e.Message);
					{
						StringBuilder sb = new StringBuilder();
						foreach (Exception exSub in e.LoaderExceptions)
						{
							sb.AppendLine(exSub.Message);
							FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
							if (exFileNotFound != null)
							{
								if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
								{
									sb.AppendLine("Fusion Log:");
									sb.AppendLine(exFileNotFound.FusionLog);
								}
							}
							sb.AppendLine();
						}
						string errorMessage = sb.ToString();
						//Display or log the error based on your application.
						Trace.TraceInformation(errorMessage);
					}
				}
				catch (Exception e)
				{
					Trace.TraceInformation(dllName + " からプラグインを生成することに失敗しました。スキップします。");
					Trace.TraceInformation(e.ToString());
					Trace.TraceInformation(e.Message);
				}
			}

			// (2) サブフォルダがあれば再帰する
			string[] strDirs = Directory.GetDirectories(strプラグインフォルダパス, "*");
			foreach (string dir in strDirs)
				this.t指定フォルダ内でのプラグイン検索と生成(dir + "\\", strプラグイン型名);
		}
		//-----------------
		#region [ Windowイベント処理 ]
		private void Window_ApplicationActivated( object sender, EventArgs e )
		{
			this.bApplicationActive = true;
		}
		private void Window_ApplicationDeactivated(object sender, EventArgs e)
		{
			this.bApplicationActive = false;
			if (cMouseHideControl != null) cMouseHideControl.Show();
		}
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Menu)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if ((e.KeyCode == Keys.Return) && e.Alt)
			{
				if (ConfigIni != null)
				{
					ConfigIni.bウィンドウモード = !ConfigIni.bウィンドウモード;
					this.t全画面_ウィンドウモード切り替え();
				}
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else
			{
				for (int i = 0; i < CConfigXml.AssignableCodes; i++)
				{
					var captureCode = (SlimDX.DirectInput.Key) ConfigIni.KeyAssign[ EPad.Capture ][ i ].コード;

					if( (int) captureCode > 0 &&
						e.KeyCode == DeviceConstantConverter.KeyToKeys( captureCode ) )
					{
						// Debug.WriteLine( "capture: " + string.Format( "{0:2x}", (int) e.KeyCode ) + " " + (int) e.KeyCode );
						string strFullPath =
								 Path.Combine( CDTXMania.Instance.strEXEのあるフォルダ, "Capture_img" );
						strFullPath = Path.Combine( strFullPath, DateTime.Now.ToString( "yyyyMMddHHmmss" ) + ".png" );
						SaveResultScreen( strFullPath );
					}
				}
			}
		}
		private void Window_MouseUp(object sender, MouseEventArgs e)
		{
			mb = e.Button;
		}
		private void Window_MouseDown(object sender, MouseEventArgs e)
		{
			currentMousePosition.X = Control.MousePosition.X;
			currentMousePosition.Y = Control.MousePosition.Y;
		}

		private void Window_MouseDoubleClick(object sender, MouseEventArgs e)   // #23510 2010.11.13 yyagi: to go full screen mode
		{
			if (mb.Equals(MouseButtons.Left) && ConfigIni.bIsAllowedDoubleClickFullscreen)  // #26752 2011.11.27 yyagi
			{
				ConfigIni.bウィンドウモード = !ConfigIni.bウィンドウモード;
				this.t全画面_ウィンドウモード切り替え();
			}
		}
		private Point currentMousePosition = new Point(-1,-1);
		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			if (cMouseHideControl != null) cMouseHideControl.tResetCursorState(ConfigIni.bウィンドウモード, this.bApplicationActive);
			if (Control.MouseButtons.HasFlag(MouseButtons.Left))
			{
				int X = base.Window.Location.X;
				X += (Control.MousePosition.X - currentMousePosition.X);
				int Y = base.Window.Location.Y;
				Y += (Control.MousePosition.Y - currentMousePosition.Y);

				base.Window.Location = new Point(X, Y);

				currentMousePosition.X = Control.MousePosition.X;
				currentMousePosition.Y = Control.MousePosition.Y;
			}
		}
		private void Window_ResizeEnd(object sender, EventArgs e)               // #23510 2010.11.20 yyagi: to get resized window size
		{
			if (ConfigIni.bウィンドウモード)
			{
				ConfigIni.rcWindow.X = base.Window.Location.X; // #30675 2013.02.04 ikanick add
				ConfigIni.rcWindow.Y = base.Window.Location.Y; //
			}

			ConfigIni.rcWindow.W = (ConfigIni.bウィンドウモード) ? base.Window.ClientSize.Width : currentClientSize.Width;   // #23510 2010.10.31 yyagi add
			ConfigIni.rcWindow.H = (ConfigIni.bウィンドウモード) ? base.Window.ClientSize.Height : currentClientSize.Height;
		}
		#endregion

		//Stopwatch sw = new Stopwatch();
		//List<int> swlist1, swlist2, swlist3, swlist4, swlist5;

		#endregion

		private class CMouseHideControl
		{
			private Point lastPosition;
			private CCounter ccMouseShow;
			private bool bマウスカーソル表示中;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public CMouseHideControl()
			{
				ccMouseShow = new CCounter();
				lastPosition = Cursor.Position;
				bマウスカーソル表示中 = true;
				t開始();
			}

			public void t開始()
			{
				ccMouseShow.t開始(0, 20, 100, CDTXMania.instance.Timer);
			}

			public void tHideCursorIfNeed()
			{
				ccMouseShow.t進行();
//Trace.TraceInformation("n現在の経過時間ms" + ccMouseShow.n現在の経過時間ms + ", n現在の値=" + ccMouseShow.n現在の値 + ", b終了値に達した=" + ccMouseShow.b終了値に達した);
				if (bマウスカーソル表示中 && ccMouseShow.b終了値に達した)
				{
					Point client_point = CDTXMania.Instance.Window.PointToClient(Cursor.Position);
					if (client_point.Y >= 0)	// タイトルバー上にマウスカーソルがある場合は、隠さない
					{
						Hide();
					}
				}

			}

			public void tResetCursorState(bool bWindowed, bool bApplicationActive)
			{
//Trace.TraceInformation("マウス移動: " + Cursor.Position.X + "," + Cursor.Position.Y);
				if ((bWindowed == true && bマウスカーソル表示中 == false) || bApplicationActive == false)   // #36168 2016.3.19 yyagi: do not to show mouse cursor in full screen mode
				{
					Point currentPosition = Cursor.Position;
//Trace.TraceInformation("current=" + currentPosition.ToString() + ", last=" + lastPosition.ToString());
					if (lastPosition != currentPosition)
					{
//Trace.TraceInformation("移動発生");
						lastPosition = currentPosition;
						Show();
						t開始();
					}
				}
			}

			public void Show()
			{
				Cursor.Show();
				bマウスカーソル表示中 = true;
			}
			public void Hide()
			{
				Cursor.Hide();
				bマウスカーソル表示中 = false;
			}
		}
	}
}
