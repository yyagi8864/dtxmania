﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using FDK;

namespace DTXMania
{
	internal class CAct演奏ステージ失敗 : CActivity
	{
		public CAct演奏ステージ失敗()
		{
			base.b活性化してない = true;
		}

		public void Start()
		{
			this.ct進行 = new CCounter(0, 0x3e8, 2, CDTXMania.Instance.Timer);
		}

		public override void On活性化()
		{
			this.sd効果音 = null;
			this.b効果音再生済み = false;
			this.ct進行 = new CCounter();
			base.On活性化();
		}

		public override void On非活性化()
		{
			this.ct進行 = null;
			if (this.sd効果音 != null)
			{
				CDTXMania.Instance.Sound管理.tサウンドを破棄する(this.sd効果音);
				this.sd効果音 = null;
			}
			base.On非活性化();
		}

		public override void OnManagedリソースの作成()
		{
			if (!base.b活性化してない)
			{
				this.txStageFailed = TextureFactory.tテクスチャの生成(CSkin.Path(@"Graphics\ScreenPlay stage failed.jpg"));
				base.OnManagedリソースの作成();
			}
		}

		public override void OnManagedリソースの解放()
		{
			if (!base.b活性化してない)
			{
				TextureFactory.tテクスチャの解放(ref this.txStageFailed);
				base.OnManagedリソースの解放();
			}
		}

		public override int On進行描画()
		{
			if (base.b活性化してない)
			{
				return 0;
			}
			if ((this.ct進行 == null) || this.ct進行.b停止中)
			{
				return 0;
			}
			this.ct進行.t進行();
			if (this.ct進行.n現在の値 < 100)
			{
				int x = (int)(320.0 * Math.Cos((Math.PI / 2 * this.ct進行.n現在の値) / 100.0));
				if ((x != 320) && (this.txStageFailed != null))
				{
					this.txStageFailed.t2D描画(CDTXMania.Instance.Device,
						0, 0,
						new Rectangle((int)(x * Scale.X), 0, (int)((320 - x) * Scale.X), (int)(480 * Scale.Y)));
					this.txStageFailed.t2D描画(CDTXMania.Instance.Device,
						(int)((320 + x) * Scale.X), 0,
						new Rectangle((int)(320 * Scale.X), 0, (int)((320 - x) * Scale.X), (int)(480 * Scale.Y)));
				}
			}
			else
			{
				if (this.txStageFailed != null)
				{
					this.txStageFailed.t2D描画(CDTXMania.Instance.Device, 0, 0);
				}
				if (this.ct進行.n現在の値 <= 250)
				{
					int num2 = CDTXMania.Instance.Random.Next(5) - 2;
					int y = CDTXMania.Instance.Random.Next(5) - 2;
					if (this.txStageFailed != null)
					{
						this.txStageFailed.t2D描画(CDTXMania.Instance.Device, num2 * Scale.X, y * Scale.Y);
					}
				}
				if (!this.b効果音再生済み)
				{
					if (((CDTXMania.Instance.DTX.SOUND_STAGEFAILED != null) && (CDTXMania.Instance.DTX.SOUND_STAGEFAILED.Length > 0)) && File.Exists(CDTXMania.Instance.DTX.strフォルダ名 + CDTXMania.Instance.DTX.SOUND_STAGEFAILED))
					{
						try
						{
							if (this.sd効果音 != null)
							{
								CDTXMania.Instance.Sound管理.tサウンドを破棄する(this.sd効果音);
								this.sd効果音 = null;
							}
							this.sd効果音 = CDTXMania.Instance.Sound管理.tサウンドを生成する(CDTXMania.Instance.DTX.strフォルダ名 + CDTXMania.Instance.DTX.SOUND_STAGEFAILED);
							this.sd効果音.t再生を開始する();
						}
						catch
						{
						}
					}
					else
					{
						CDTXMania.Instance.Skin.soundSTAGEFAILED音.t再生する();
					}
					this.b効果音再生済み = true;
				}
			}
			if (!this.ct進行.b終了値に達した)
			{
				return 0;
			}
			return 1;
		}

		private bool b効果音再生済み;
		private CCounter ct進行;
		private CSound sd効果音;
		private CTexture txStageFailed;
	}
}
