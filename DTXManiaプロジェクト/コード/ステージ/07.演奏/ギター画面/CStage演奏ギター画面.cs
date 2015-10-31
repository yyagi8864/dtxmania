﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using SlimDX.Direct3D9;
using FDK;

namespace DTXMania
{
	internal class CStage演奏ギター画面 : CStage演奏画面共通
	{
		// コンストラクタ

		public CStage演奏ギター画面()
		{
			base.eステージID = CStage.Eステージ.演奏;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.b活性化してない = true;
			base.list子Activities.Add( this.actStageFailed = new CAct演奏ステージ失敗() );
			base.list子Activities.Add( this.actDANGER = new CAct演奏GuitarDanger() );
			base.list子Activities.Add( this.actAVI = new CAct演奏AVI() );
			base.list子Activities.Add( this.actBGA = new CAct演奏BGA() );
			base.list子Activities.Add( this.actPanel = new CAct演奏パネル文字列() );
			base.list子Activities.Add( this.act譜面スクロール速度 = new CAct演奏スクロール速度() );
			base.list子Activities.Add( this.actStatusPanels = new CAct演奏Guitarステータスパネル() );
			base.list子Activities.Add( this.actWailingBonus = new CAct演奏GuitarWailingBonus() );
			base.list子Activities.Add( this.actScore = new CAct演奏Guitarスコア() );
			base.list子Activities.Add( this.actRGB = new CAct演奏GuitarRGB() );
			base.list子Activities.Add( this.actLaneFlushGB = new CAct演奏GuitarレーンフラッシュGB() );
			base.list子Activities.Add( this.actJudgeString = new CAct演奏Guitar判定文字列() );
			base.list子Activities.Add( this.actGauge = new CAct演奏Guitarゲージ() );
			base.list子Activities.Add( this.actCombo = new CAct演奏Guitarコンボ() );
			base.list子Activities.Add( this.actChipFireGB = new CAct演奏Guitarチップファイア() );
			base.list子Activities.Add( this.actPlayInfo = new CAct演奏演奏情報() );
			base.list子Activities.Add( this.actFI = new CActFIFOBlack() );
			base.list子Activities.Add( this.actFO = new CActFIFOBlack() );
			base.list子Activities.Add( this.actFOClear = new CActFIFOWhite() );
		}


		// メソッド

		public void t演奏結果を格納する( out CScoreIni.C演奏記録 Drums, out CScoreIni.C演奏記録 Guitar, out CScoreIni.C演奏記録 Bass )
		{
			Drums = new CScoreIni.C演奏記録();

			base.t演奏結果を格納する_ギター( out Guitar );
			base.t演奏結果を格納する_ベース( out Bass );

//			if ( CDTXMania.ConfigIni.bIsSwappedGuitarBass )		// #24063 2011.1.24 yyagi Gt/Bsを入れ替えていたなら、演奏結果も入れ替える
//			{
//				CScoreIni.C演奏記録 t;
//				t = Guitar;
//				Guitar = Bass;
//				Bass = t;
//			
//				CDTXMania.DTX.SwapGuitarBassInfos();			// 譜面情報も元に戻す
//			}
		}
		

		// CStage 実装

		public override void On活性化()
		{
			dtLastQueueOperation = DateTime.MinValue;
			base.On活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				//this.t背景テクスチャの生成();
				this.txチップ = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\ScreenPlayGuitar chips.png" ) );
				this.txヒットバー = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\ScreenPlayGuitar hit-bar.png" ) );
				//this.txWailing枠 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\ScreenPlay wailing cursor.png" ) );
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				//CDTXMania.tテクスチャの解放( ref this.tx背景 );
				CDTXMania.tテクスチャの解放( ref this.txチップ );
				CDTXMania.tテクスチャの解放( ref this.txヒットバー );
				//CDTXMania.tテクスチャの解放( ref this.txWailing枠 );
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				bool bIsFinishedPlaying = false;
				bool bIsFinishedFadeout = false;

				if( base.b初めての進行描画 )
				{
                    CSound管理.rc演奏用タイマ.tリセット();
					CDTXMania.Timer.tリセット();
					this.ctチップ模様アニメ.Guitar = new CCounter( 0, 0x17, 20, CDTXMania.Timer );
					this.ctチップ模様アニメ.Bass = new CCounter( 0, 0x17, 20, CDTXMania.Timer );
					this.ctチップ模様アニメ[ 0 ] = null;
					this.ctWailingチップ模様アニメ = new CCounter( 0, 4, 50, CDTXMania.Timer );
					base.eフェーズID = CStage.Eフェーズ.共通_フェードイン;
					this.actFI.tフェードイン開始();

					if ( CDTXMania.DTXVmode.Enabled )			// DTXVモードなら
					{
						#region [ DTXV用の再生設定にする(全AUTOなど) ]
						tDTXV用の設定();
						#endregion
						t演奏位置の変更( CDTXMania.DTXVmode.nStartBar );
					}

					CDTXMania.Sound管理.tDisableUpdateBufferAutomatically();
					base.b初めての進行描画 = false;
				}
				if( CDTXMania.ConfigIni.bSTAGEFAILED有効 && ( base.eフェーズID == CStage.Eフェーズ.共通_通常状態 ) )
				{
//					bool flag3 = ( CDTXMania.ConfigIni.bAutoPlay.Guitar || !CDTXMania.DTX.bチップがある.Guitar ) || ( this.actGauge.db現在のゲージ値.Guitar <= -0.1 );				// #23630
//					bool flag4 = ( CDTXMania.ConfigIni.bAutoPlay.Bass || !CDTXMania.DTX.bチップがある.Bass ) || ( this.actGauge.db現在のゲージ値.Bass <= -0.1 );					// #23630
					bool bFailedGuitar = this.actGauge.IsFailed( E楽器パート.GUITAR );		// #23630 2011.11.12 yyagi: deleted AutoPlay condition: not to be failed at once
					bool bFailedBass   = this.actGauge.IsFailed( E楽器パート.BASS );		// #23630
					bool bFailedNoChips = (!CDTXMania.DTX.bチップがある.Guitar && !CDTXMania.DTX.bチップがある.Bass);	// #25216 2011.5.21 yyagi add condition
					if ( bFailedGuitar || bFailedBass || bFailedNoChips )						// #25216 2011.5.21 yyagi: changed codition: && -> ||
					{
						this.actStageFailed.Start();
						CDTXMania.DTX.t全チップの再生停止();
						base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_FAILED;
					}
				}
				this.t進行描画_AVI();
				this.t進行描画_背景();
				this.t進行描画_MIDIBGM();
				this.t進行描画_パネル文字列();
				this.t進行描画_スコア();
				this.t進行描画_BGA();
				this.t進行描画_ステータスパネル();
				this.t進行描画_レーンフラッシュGB();
				this.t進行描画_ギターベース判定ライン();
				this.t進行描画_ゲージ();
				this.t進行描画_DANGER();
				if ( this.e判定表示優先度 == E判定表示優先度.Chipより下 )
				{
					this.t進行描画_RGBボタン();
					this.t進行描画_判定文字列();
					this.t進行描画_コンボ();
				}
				this.t進行描画_WailingBonus();
				this.t進行描画_譜面スクロール速度();
				this.t進行描画_チップアニメ();
				bIsFinishedPlaying = this.t進行描画_チップ(E楽器パート.GUITAR);
				if ( this.e判定表示優先度 == E判定表示優先度.Chipより上 )
				{
					this.t進行描画_RGBボタン();
					this.t進行描画_判定文字列();
					this.t進行描画_コンボ();
				}
				this.t進行描画_演奏情報();
				this.t進行描画_Wailing枠();
				this.t進行描画_チップファイアGB();
				this.t進行描画_STAGEFAILED();
				bIsFinishedFadeout = this.t進行描画_フェードイン_アウト();
				if( bIsFinishedPlaying && ( base.eフェーズID == CStage.Eフェーズ.共通_通常状態 ) )
				{
					if ( CDTXMania.DTXVmode.Enabled )
					{
						if ( CDTXMania.Timer.b停止していない )
						{
							this.actPanel.Stop();				// PANEL表示停止
							CDTXMania.Timer.t一時停止();		// 再生時刻カウンタ停止
						}
						Thread.Sleep( 5 );
						// DTXCからの次のメッセージを待ち続ける
					}
					else
					{
						this.eフェードアウト完了時の戻り値 = E演奏画面の戻り値.ステージクリア;
						base.eフェーズID = CStage.Eフェーズ.演奏_STAGE_CLEAR_フェードアウト;
						this.actFOClear.tフェードアウト開始();
					} 
				}
				if ( this.eフェードアウト完了時の戻り値 == E演奏画面の戻り値.再読込_再演奏)
				{
					bIsFinishedFadeout = true;
				}
				if ( bIsFinishedFadeout )
				{
					return (int) this.eフェードアウト完了時の戻り値;
				}

				ManageMixerQueue();

				// キー入力

				if( CDTXMania.act現在入力を占有中のプラグイン == null )
				{
					this.tキー入力();
				}
			}
			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		protected override E判定 tチップのヒット処理( long nHitTime, CDTX.CChip pChip, bool bCorrectLane )
		{
			E判定 eJudgeResult = tチップのヒット処理( nHitTime, pChip, E楽器パート.GUITAR, bCorrectLane );
			return eJudgeResult;
		}
		protected override void tチップのヒット処理_BadならびにTight時のMiss( E楽器パート part )
		{
			this.tチップのヒット処理_BadならびにTight時のMiss( part, 0, E楽器パート.GUITAR );
		}
		protected override void tチップのヒット処理_BadならびにTight時のMiss( E楽器パート part, int nLane )
		{
			this.tチップのヒット処理_BadならびにTight時のMiss( part, nLane, E楽器パート.GUITAR );
		}

		protected override void t進行描画_AVI()
		{
		    base.t進行描画_AVI( 682, 112 );
		}
		protected override void t進行描画_BGA()
		{
		    base.t進行描画_BGA( 682, 112 );
		}
		protected override void t進行描画_DANGER()			// #23631 2011.4.19 yyagi
		{
			//this.actDANGER.t進行描画( false, this.actGauge.db現在のゲージ値.Guitar < 0.3, this.actGauge.db現在のゲージ値.Bass < 0.3 );
			this.actDANGER.t進行描画( false, this.actGauge.IsDanger(E楽器パート.GUITAR), this.actGauge.IsDanger(E楽器パート.BASS) );
		}

		protected override void t進行描画_Wailing枠()
		{
			int yG = this.演奏判定ライン座標.n判定ラインY座標( E楽器パート.GUITAR, true, bReverse[ (int) E楽器パート.GUITAR ], true );
			int yB = this.演奏判定ライン座標.n判定ラインY座標( E楽器パート.BASS,   true, bReverse[ (int) E楽器パート.BASS   ], true );
			base.t進行描画_Wailing枠(
				(int) ( 139 * Scale.X ),
				(int) ( 593 * Scale.X ),
				yG,
				yB
				//CDTXMania.ConfigIni.bReverse.Guitar ? 340 : 11,
				//CDTXMania.ConfigIni.bReverse.Bass ?   340 : 11
			);
		}
		private void t進行描画_ギターベース判定ライン()	// yyagi: ドラム画面とは座標が違うだけですが、まとめづらかったのでそのまま放置してます。
		{
			if ( ( CDTXMania.ConfigIni.eDark != Eダークモード.FULL ) && CDTXMania.ConfigIni.bGuitar有効 )
			{
				if ( CDTXMania.DTX.bチップがある.Guitar )
				{
					int y = this.演奏判定ライン座標.n判定ラインY座標( E楽器パート.GUITAR, true, bReverse[ (int) E楽器パート.GUITAR ] )
							- (int) ( 3 * Scale.Y );
															// #31602 2013.6.23 yyagi 描画遅延対策として、判定ラインの表示位置をオフセット調整できるようにする
					if ( this.txヒットバー != null )
					{
						for ( int i = 0; i < 4; i++ )
						{
							this.txヒットバー.t2D描画( CDTXMania.app.Device,
								( 23 + ( 28 * i ) ) * Scale.X,
								y,
								new Rectangle(
									0,
									(int) ( i * 8 * Scale.Y ),
									(int) ( 28 * Scale.X ),
									(int) ( 8 * Scale.Y )
								)
							);
						}
					}
				}
				if ( CDTXMania.DTX.bチップがある.Bass )
				{
					int y = this.演奏判定ライン座標.n判定ラインY座標( E楽器パート.BASS, true, bReverse[ (int) E楽器パート.BASS   ] )
							- (int) ( 3 * Scale.Y );
															// #31602 2013.6.23 yyagi 描画遅延対策として、判定ラインの表示位置をオフセット調整できるようにする
					if ( this.txヒットバー != null )
					{
						for ( int j = 0; j < 4; j++ )
						{
							this.txヒットバー.t2D描画(
								CDTXMania.app.Device,
								( 477 + ( 28 * j ) ) * Scale.X,
								y,
								new Rectangle(
									0,
									(int) ( j * 8 * Scale.Y ),
									(int) ( 28 * Scale.X ),
									(int) ( 8 * Scale.Y )
								)
							);							
						}
					}
				}
			}
		}

		protected override void t進行描画_パネル文字列()
		{
			base.t進行描画_パネル文字列( 0xb5, 430 );
		}

		protected override void t進行描画_演奏情報()
		{
			base.t進行描画_演奏情報( 0xb5, 50 );
		}

		protected override void ドラムスクロール速度アップ()
		{
			// ギタレボモードでは何もしない
		}
		protected override void ドラムスクロール速度ダウン()
		{
			// ギタレボモードでは何もしない
		}

		protected override void t入力処理_ドラム()
		{
			// ギタレボモードでは何もしない
		}

		protected override void t背景テクスチャの生成()
		{
			Rectangle bgrect = new Rectangle( (int) ( 181 * Scale.X ), (int) ( 50 * Scale.Y ), (int) ( 278 * Scale.X ), (int) ( 355 * Scale.Y ) );
			string DefaultBgFilename = @"Graphics\ScreenPlayGuitar background.png";
			string DefaultLaneFilename = "";
			string BgFilename = "";
			string BACKGROUND = null;
			if ( ( CDTXMania.DTX.BACKGROUND_GR != null ) && ( CDTXMania.DTX.BACKGROUND_GR.Length > 0 ) )
			{
				BACKGROUND = CDTXMania.DTX.BACKGROUND_GR;
			}
			else if ( ( CDTXMania.DTX.BACKGROUND != null ) && ( CDTXMania.DTX.BACKGROUND.Length > 0 ) )
			{
				BACKGROUND = CDTXMania.DTX.BACKGROUND;
			}
			if ( ( BACKGROUND != null ) && ( BACKGROUND.Length > 0 ) )
			{
				BgFilename = CDTXMania.DTX.strフォルダ名 + BACKGROUND;
			}
			base.t背景テクスチャの生成( DefaultBgFilename, DefaultLaneFilename, bgrect, BgFilename );
		}

		protected override void t進行描画_チップ_ドラムス( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip )
		{
			// int indexSevenLanes = this.nチャンネル0Atoレーン07[ pChip.nチャンネル番号 - 0x11 ];
			if ( !pChip.bHit && ( pChip.nバーからの距離dot.Drums < 0 ) )
			{
				pChip.bHit = true;
				this.tサウンド再生( pChip, CSound管理.rc演奏用タイマ.n前回リセットした時のシステム時刻 + pChip.n発声時刻ms, E楽器パート.DRUMS, dTX.nモニタを考慮した音量( E楽器パート.DRUMS ) );
			}
		}
		protected override void t進行描画_チップ_ギターベース( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip, E楽器パート inst )
		{
			base.t進行描画_チップ_ギターベース( configIni, ref dTX, ref pChip, inst,
				演奏判定ライン座標.n判定ラインY座標( inst, true, false ),	// 40
				演奏判定ライン座標.n判定ラインY座標( inst, true, true ),	// 369
				(int) ( 0 * Scale.Y ), (int) ( 409 * Scale.Y ),				// Y軸表示範囲
				26, 480,					// openチップのX座標(Gt, Bs)
				0, 192, 103, 8, 32,			// オープンチップregionの x, y, w, h, 通常チップのw
				26, 98, 480, 552,			// GtのX, Gt左利きのX, BsのX, Bs左利きのX,
				36, 32						// 描画のX座標間隔, テクスチャのX座標間隔
			);
		}
#if false
		protected override void t進行描画・チップ・ギターベース( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip, E楽器パート inst )
		{
			int instIndex = (int) inst;
			if ( configIni.bGuitar有効 )
			{
				if ( configIni.bSudden[instIndex ] )
				{
					pChip.b可視 = pChip.nバーからの距離dot[ instIndex ] < 200;
				}
				if ( configIni.bHidden[ instIndex ] && ( pChip.nバーからの距離dot[ instIndex ] < 100 ) )
				{
					pChip.b可視 = false;
				}

				bool bChipHasR = ( ( pChip.nチャンネル番号 & 4 ) > 0 );
				bool bChipHasG = ( ( pChip.nチャンネル番号 & 2 ) > 0 );
				bool bChipHasB = ( ( pChip.nチャンネル番号 & 1 ) > 0 );
				bool bChipHasW = ( ( pChip.nチャンネル番号 & 0x0F ) == 0x08 );
				bool bChipIsO  = ( ( pChip.nチャンネル番号 & 0x0F ) == 0x00 );

				int OPEN = ( inst == E楽器パート.GUITAR ) ? 0x20 : 0xA0;
				if ( !pChip.bHit && pChip.b可視 )
				{
					int y = configIni.bReverse[ instIndex ] ? ( 369 - pChip.nバーからの距離dot[ instIndex ]) : ( 40 + pChip.nバーからの距離dot[ instIndex ] );
					if ( ( y > 0 ) && ( y < 409 ) )
					{
						if ( this.txチップ != null )
						{
							int nアニメカウンタ現在の値 = this.ctチップ模様アニメ[ instIndex ].n現在の値;
							if ( pChip.nチャンネル番号 == OPEN )
							{
								{
									int xo = ( inst == E楽器パート.GUITAR ) ? 26 : 480;
									this.txチップ.t2D描画( CDTXMania.app.Device, xo, y - 4, new Rectangle( 0, 192 + ( ( nアニメカウンタ現在の値 % 5 ) * 8 ), 103, 8 ) );
								}
							}
							Rectangle rc = new Rectangle( 0, nアニメカウンタ現在の値 * 8, 32, 8 );
							int x;
							if ( inst == E楽器パート.GUITAR )
							{
								x = ( configIni.bLeft.Guitar ) ? 98 : 26;
							}
							else
							{
								x = ( configIni.bLeft.Bass ) ? 552 : 480;
							}
							int deltaX = ( configIni.bLeft[ instIndex ] ) ? -36 : +36; 
							if ( bChipHasR )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, x, y - 4, rc );
							}
							rc.X += 32;
							if ( bChipHasG )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, x, y - 4, rc );
							}
							rc.X += 32;
							if ( bChipHasB )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, x, y - 4, rc );
							}
						}
					}
				}
				// if ( ( configIni.bAutoPlay.Guitar && !pChip.bHit ) && ( pChip.nバーからの距離dot.Guitar < 0 ) )
				if ( ( !pChip.bHit ) && ( pChip.nバーからの距離dot[ instIndex ] < 0 ) )
				{
					int lo = ( inst == E楽器パート.GUITAR ) ? 0 : 3;	// lane offset
					bool autoR = ( inst == E楽器パート.GUITAR ) ? bIsAutoPlay.GtR : bIsAutoPlay.BsR;
					bool autoG = ( inst == E楽器パート.GUITAR ) ? bIsAutoPlay.GtG : bIsAutoPlay.BsG;
					bool autoB = ( inst == E楽器パート.GUITAR ) ? bIsAutoPlay.GtB : bIsAutoPlay.BsB;
					if ( ( bChipHasR || bChipIsO ) && autoR )
					{
						this.actChipFireGB.Start( 0 + lo );
					}
					if ( ( bChipHasG || bChipIsO ) && autoG )
					{
						this.actChipFireGB.Start( 1 + lo );
					}
					if ( ( bChipHasB || bChipIsO ) && autoB )
					{
						this.actChipFireGB.Start( 2 + lo );
					}
					if ( ( inst == E楽器パート.GUITAR && bIsAutoPlay.GtPick ) || ( inst == E楽器パート.BASS && bIsAutoPlay.BsPick ) )
					{
						bool pushingR = CDTXMania.Pad.b押されている( inst, Eパッド.R );
						bool pushingG = CDTXMania.Pad.b押されている( inst, Eパッド.G );
						bool pushingB = CDTXMania.Pad.b押されている( inst, Eパッド.B );
						bool bMiss = true;
						if ( ( ( bChipIsO == true ) && ( !pushingR | autoR ) && ( !pushingG | autoG ) && ( !pushingB | autoB ) ) ||
							( ( bChipHasR == ( pushingR | autoR ) ) && ( bChipHasG == ( pushingG | autoG ) ) && ( bChipHasB == ( pushingB | autoB ) ) )
						)
						{
							bMiss = false;
						}
						pChip.bHit = true;
						this.tサウンド再生( pChip, CSound管理.rc演奏用タイマ.n前回リセットした時のシステム時刻 + pChip.n発声時刻ms, inst, dTX.nモニタを考慮した音量( inst ) );
						this.r次にくるギターChip = null;
						this.tチップのヒット処理( pChip.n発声時刻ms, pChip );
					}
				}
				// break;
				return;
			}
			if ( !pChip.bHit && ( pChip.nバーからの距離dot[ instIndex ] < 0 ) )
			{
				pChip.bHit = true;
				this.tサウンド再生( pChip, CSound管理.rc演奏用タイマ.n前回リセットした時のシステム時刻 + pChip.n発声時刻ms, inst, dTX.nモニタを考慮した音量( inst ) );
			}
		}
#endif
		protected void t進行描画_チップ_ギター_ウェイリング( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip )
		{
			//if ( configIni.bGuitar有効 )
			//{
			//    //
			//    // 後日、以下の部分を何とかCStage演奏画面共通.csに移したい。
			//    //
			//    if ( !pChip.bHit && pChip.b可視 )
			//    {
			//        if ( this.txチップ != null )
			//        {
			//            this.txチップ.n透明度 = pChip.n透明度;
			//        }
			//        int[] y_base = {
			//            演奏判定ライン座標.n判定ラインY座標( E楽器パート.GUITAR, true, false ),		// 40
			//            演奏判定ライン座標.n判定ラインY座標( E楽器パート.GUITAR, true, true )		// 369
			//        };			// ドラム画面かギター画面かで変わる値
			//        int offset = 0;						// ドラム画面かギター画面かで変わる値

			//        const int WailingWidth  = (int) ( 20 * Scale.X );		// 4種全て同じ値
			//        const int WailingHeight = (int) ( 50 * Scale.Y );		// 4種全て同じ値
			//        const int baseTextureOffsetX = (int) ( 96 * Scale.X );	// ドラム画面かギター画面かで変わる値
			//        const int baseTextureOffsetY = (int) (  0 * Scale.Y );	// ドラム画面かギター画面かで変わる値
			//        const int drawX = (int) ( 140 * Scale.X );				// 4種全て異なる値

			//        const int numA = (int) ( 29 * Scale.Y );				// ドラム画面かギター画面かで変わる値
			//        int y = configIni.bReverse.Guitar ?
			//            ( y_base[ 1 ] - (int) ( pChip.nバーからの距離dot.Guitar * Scale.Y ) ) :
			//            ( y_base[ 0 ] + (int) ( pChip.nバーからの距離dot.Guitar * Scale.Y ) );
			//        int numB = y - offset;				// 4種全て同じ定義
			//        int numC = 0;						// 4種全て同じ初期値
			//        const int showRangeY1 = (int) ( 409 * Scale.Y );				// ドラム画面かギター画面かで変わる値
			//        if ( ( numB < ( showRangeY1 + numA ) ) && ( numB > -numA ) )	// 以下のロジックは4種全て同じ
			//        {
			//            int c = this.ctWailingチップ模様アニメ.n現在の値;
			//            Rectangle rect = new Rectangle(
			//                baseTextureOffsetX + ( c * WailingWidth ),
			//                baseTextureOffsetY,
			//                WailingWidth,
			//                WailingHeight
			//            );
			//            if ( numB < numA )
			//            {
			//                rect.Y += numA - numB;
			//                rect.Height -= numA - numB;
			//                numC = numA - numB;
			//            }
			//            if ( numB > ( showRangeY1 - numA ) )
			//            {
			//                rect.Height -= numB - ( showRangeY1 - numA );
			//            }
			//            if ( ( rect.Bottom > rect.Top ) && ( this.txチップ != null ) )
			//            {
			//                this.txチップ.t2D描画(
			//                    CDTXMania.app.Device,
			//                    drawX,
			//                    ( ( y - numA ) + numC ),
			//                    rect
			//                );
			//            }
			//        }
			//    }
			//}
			base.t進行描画_チップ_ギター_ウェイリング( configIni, ref dTX, ref pChip, true );
		}
		protected override void t進行描画_チップ_フィルイン( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip )
		{
			if ( !pChip.bHit && ( pChip.nバーからの距離dot.Drums < 0 ) )
			{
				pChip.bHit = true;
			}
#if TEST_NOTEOFFMODE	// 2011.1.1 yyagi TEST
			switch ( pChip.n整数値 )
			{
				case 0x04:	// HH消音あり(従来同等)
					CDTXMania.DTX.b演奏で直前の音を消音する.HH = true;
					break;
				case 0x05:	// HH消音無し
					CDTXMania.DTX.b演奏で直前の音を消音する.HH = false;
					break;
				case 0x06:	// ギター消音あり(従来同等)
					CDTXMania.DTX.b演奏で直前の音を消音する.Guitar = true;
					break;
				case 0x07:	// ギター消音無し
					CDTXMania.DTX.b演奏で直前の音を消音する.Guitar = false;
					break;
				case 0x08:	// ベース消音あり(従来同等)
					CDTXMania.DTX.b演奏で直前の音を消音する.Bass = true;
					break;
				case 0x09:	// ベース消音無し
					CDTXMania.DTX.b演奏で直前の音を消音する.Bass = false;
					break;
			}
#endif

		}
#if false
		protected override void t進行描画・チップ・ベース( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip )
		{
			if ( configIni.bGuitar有効 )
			{
				if ( configIni.bSudden.Bass )
				{
					pChip.b可視 = pChip.nバーからの距離dot.Bass < 200;
				}
				if ( configIni.bHidden.Bass && ( pChip.nバーからの距離dot.Bass < 100 ) )
				{
					pChip.b可視 = false;
				}
				if ( !pChip.bHit && pChip.b可視 )
				{
					int num8 = configIni.bReverse.Bass ? ( 0x171 - pChip.nバーからの距離dot.Bass ) : ( 40 + pChip.nバーからの距離dot.Bass );
					if ( ( num8 > 0 ) && ( num8 < 0x199 ) )
					{
						int num9 = this.ctチップ模様アニメ.Bass.n現在の値;
						if ( pChip.nチャンネル番号 == 160 )
						{
							if ( this.txチップ != null )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, 480, num8 - 4, new Rectangle( 0, 0xc0 + ( ( num9 % 5 ) * 8 ), 0x67, 8 ) );
							}
						}
						else if ( !configIni.bLeft.Bass )
						{
							Rectangle rectangle3 = new Rectangle( 0, num9 * 8, 0x20, 8 );
							if ( ( ( pChip.nチャンネル番号 & 4 ) != 0 ) && ( this.txチップ != null ) )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, 480, num8 - 4, rectangle3 );
							}
							rectangle3.X += 0x20;
							if ( ( ( pChip.nチャンネル番号 & 2 ) != 0 ) && ( this.txチップ != null ) )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, 0x204, num8 - 4, rectangle3 );
							}
							rectangle3.X += 0x20;
							if ( ( ( pChip.nチャンネル番号 & 1 ) != 0 ) && ( this.txチップ != null ) )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, 0x228, num8 - 4, rectangle3 );
							}
						}
						else
						{
							Rectangle rectangle4 = new Rectangle( 0, num9 * 8, 0x20, 8 );
							if ( ( ( pChip.nチャンネル番号 & 4 ) != 0 ) && ( this.txチップ != null ) )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, 0x228, num8 - 4, rectangle4 );
							}
							rectangle4.X += 0x20;
							if ( ( ( pChip.nチャンネル番号 & 2 ) != 0 ) && ( this.txチップ != null ) )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, 0x204, num8 - 4, rectangle4 );
							}
							rectangle4.X += 0x20;
							if ( ( ( pChip.nチャンネル番号 & 1 ) != 0 ) && ( this.txチップ != null ) )
							{
								this.txチップ.t2D描画( CDTXMania.app.Device, 480, num8 - 4, rectangle4 );
							}
						}
					}
				}
				if ( ( configIni.bAutoPlay.Bass && !pChip.bHit ) && ( pChip.nバーからの距離dot.Bass < 0 ) )
				{
					pChip.bHit = true;
					if ( ( ( pChip.nチャンネル番号 & 4 ) != 0 ) || ( pChip.nチャンネル番号 == 0xA0 ) )
					{
						this.actChipFireGB.Start( 3 );
					}
					if ( ( ( pChip.nチャンネル番号 & 2 ) != 0 ) || ( pChip.nチャンネル番号 == 0xA0 ) )
					{
						this.actChipFireGB.Start( 4 );
					}
					if ( ( ( pChip.nチャンネル番号 & 1 ) != 0 ) || ( pChip.nチャンネル番号 == 0xA0 ) )
					{
						this.actChipFireGB.Start( 5 );
					}
					this.tサウンド再生( pChip, CSound管理.rc演奏用タイマ.n前回リセットした時のシステム時刻 + pChip.n発声時刻ms, E楽器パート.BASS, dTX.nモニタを考慮した音量( E楽器パート.BASS ) );
					this.r次にくるベースChip = null;
					this.tチップのヒット処理( pChip.n発声時刻ms, pChip );
				}
				return;
			}
			if ( !pChip.bHit && ( pChip.nバーからの距離dot.Bass < 0 ) )
			{
				pChip.bHit = true;
				this.tサウンド再生( pChip, CSound管理.rc演奏用タイマ.n前回リセットした時のシステム時刻 + pChip.n発声時刻ms, E楽器パート.BASS, dTX.nモニタを考慮した音量( E楽器パート.BASS ) );
			}
		}
#endif
		protected void t進行描画_チップ_ベース_ウェイリング( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip )
		{
			if ( configIni.bGuitar有効 )
			{
				//
				// 後日、以下の部分を何とかCStage演奏画面共通.csに移したい。
				//
				//if ( !pChip.bHit && pChip.b可視 )
				//{
				//    if ( this.txチップ != null )
				//    {
				//        this.txチップ.n透明度 = pChip.n透明度;
				//    }
				//    int[] y_base = {
				//        演奏判定ライン座標.n判定ラインY座標( E楽器パート.BASS, true, false ),		// 40
				//        演奏判定ライン座標.n判定ラインY座標( E楽器パート.BASS, true, true )			// 369
				//    };			// ドラム画面かギター画面かで変わる値
				//    int offset = 0;						// ドラム画面かギター画面かで変わる値

				//    const int WailingWidth  = (int) ( 20 * Scale.X );		// 4種全て同じ値
				//    const int WailingHeight = (int) ( 50 * Scale.Y );		// 4種全て同じ値
				//    const int baseTextureOffsetX = (int) ( 96 * Scale.X );	// ドラム画面かギター画面かで変わる値
				//    const int baseTextureOffsetY = (int) (  0 * Scale.Y );	// ドラム画面かギター画面かで変わる値
				//    const int drawX =(int) ( 594 * Scale.X );				// 4種全て異なる値

				//    const int numA = (int) ( 29 * Scale.Y );				// ドラム画面かギター画面かで変わる値
				//    int y = configIni.bReverse.Bass ?
				//        ( y_base[ 1 ] - (int) ( pChip.nバーからの距離dot.Bass * Scale.Y ) ) :
				//        ( y_base[ 0 ] + (int) ( pChip.nバーからの距離dot.Bass * Scale.Y ) );
				//    int numB = y - offset;				// 4種全て同じ定義
				//    int numC = 0;						// 4種全て同じ初期値
				//    const int showRangeY1 = (int) ( 409 * Scale.Y );				// ドラム画面かギター画面かで変わる値
				//    if ( ( numB < ( showRangeY1 + numA ) ) && ( numB > -numA ) )	// 以下のロジックは4種全て同じ
				//    {
				//        int c = this.ctWailingチップ模様アニメ.n現在の値;
				//        Rectangle rect = new Rectangle(
				//            baseTextureOffsetX + ( c * WailingWidth ),
				//            baseTextureOffsetY,
				//            WailingWidth,
				//            WailingHeight
				//        );
				//        if ( numB < numA )						// 上にスクロールして、見切れる場合
				//        {
				//            rect.Y += numA - numB;
				//            rect.Height -= numA - numB;
				//            numC = numA - numB;
				//        }
				//        if ( numB > ( showRangeY1 - numA ) )	// 下にスクロールして、見切れる場合
				//        {
				//            rect.Height -= numB - ( showRangeY1 - numA );
				//        }
				//        if ( ( rect.Bottom > rect.Top ) && ( this.txチップ != null ) )
				//        {
				//            this.txチップ.t2D描画(
				//                CDTXMania.app.Device,
				//                drawX,
				//                ( ( y - numA ) + numC ),
				//                rect
				//            );
				//        }
				//    }
				//}
				base.t進行描画_チップ_ベース_ウェイリング( configIni, ref dTX, ref pChip, true );
			}
		}
		protected override void t進行描画_チップ_空打ち音設定_ドラム( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip )
		{
			if ( !pChip.bHit && ( pChip.nバーからの距離dot.Drums < 0 ) )
			{
				pChip.bHit = true;
			}
		}
		protected override void t進行描画_チップ_小節線( CConfigIni configIni, ref CDTX dTX, ref CDTX.CChip pChip )
		{
			int n小節番号plus1 = pChip.n発声位置 / 0x180;
			if ( !pChip.bHit && ( pChip.nバーからの距離dot.Drums < 0 ) )
			{
				pChip.bHit = true;
				this.actPlayInfo.n小節番号 = n小節番号plus1 - 1;
				if ( configIni.bWave再生位置自動調整機能有効 && ( bIsDirectSound || bUseOSTimer ) )
				{
					dTX.tWave再生位置自動補正();
				}
			}
			if ( ( pChip.b可視 && configIni.bGuitar有効 ) && ( configIni.eDark != Eダークモード.FULL ) && ( this.txチップ != null ) )
			{
				this.txチップ.n透明度 = 255;
				#region [ Guitarの小節線 ]
				//int y = configIni.bReverse.Guitar ? ( ( 0x171 - pChip.nバーからの距離dot.Guitar ) - 1 ) : ( ( 40 + pChip.nバーからの距離dot.Guitar ) - 1 );
				int y = 演奏判定ライン座標.n判定ラインY座標( E楽器パート.GUITAR, true, configIni.bReverse.Guitar );
				if ( configIni.bReverse.Guitar )
				{
					y = y - (int) ( pChip.nバーからの距離dot.Guitar ) - 1;
				}
				else
				{
					y = y + (int) ( pChip.nバーからの距離dot.Guitar ) - 1;
				}
				int n小節線消失距離dot;
				// Reverse時の小節線消失位置を、RGBボタンの真ん中程度に。
				// 非Reverse時の消失処理は、従来通りt進行描画・チップ()にお任せ。
				n小節線消失距離dot = configIni.bReverse.Guitar ?
					(int) ( -100 * Scale.Y ) :
					( configIni.e判定位置.Guitar == E判定位置.標準 ) ? (int) ( -36 * Scale.Y ) : (int) ( -25 * Scale.Y );

				if ( dTX.bチップがある.Guitar &&
					( 0 < y ) && ( y < (int) ( 0x199 * Scale.Y ) ) &&
					( pChip.nバーからの距離dot.Guitar >= n小節線消失距離dot )
					)
				{
					this.txチップ.t2D描画(
						CDTXMania.app.Device,
						0x1a * Scale.X,
						y,
						new Rectangle(
							0,
							(int) ( 0xeb * Scale.Y ),
							(int) ( 0x68 * Scale.X ),
							(int) ( 1 * Scale.Y )
						)
					);
				}
				#endregion

				#region [ Bassの小節線 ]
				//y = configIni.bReverse.Bass ? ( ( 0x171 - pChip.nバーからの距離dot.Bass ) - 1 ) : ( ( 40 + pChip.nバーからの距離dot.Bass ) - 1 );
				y = 演奏判定ライン座標.n判定ラインY座標( E楽器パート.BASS, true, configIni.bReverse.Bass );
				if ( configIni.bReverse.Bass )
				{
					y = y - (int) ( pChip.nバーからの距離dot.Bass ) - 1;
				}
				else
				{
					y = y + (int) ( pChip.nバーからの距離dot.Bass ) - 1;
				}
				n小節線消失距離dot = configIni.bReverse.Bass ?
					(int) ( -100 * Scale.Y ) :
					( configIni.e判定位置.Bass == E判定位置.標準 ) ? (int) ( -36 * Scale.Y ) : (int) ( -25 * Scale.Y ); 
				if ( dTX.bチップがある.Bass &&
					( 0 < y ) && ( y < (int) ( 0x199 * Scale.Y ) ) &&
					( pChip.nバーからの距離dot.Bass >= n小節線消失距離dot )
					)
				{
					this.txチップ.t2D描画(
						CDTXMania.app.Device,
						480 * Scale.X,
						y,
						new Rectangle(
							0,
							(int) ( 0xeb * Scale.Y ),
							(int) ( 0x68 * Scale.X ),
							(int) ( 1 * Scale.Y )
						)
					);
				}
				#endregion
			}
		}

		#endregion
	}
}
