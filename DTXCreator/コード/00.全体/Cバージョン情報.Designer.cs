﻿
namespace DTXCreator
{
	partial class Cバージョン情報
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		/// 
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( this.components != null ) )
			{
				this.components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// Cバージョン情報
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = global::DTXCreator.Properties.Resources.バージョン情報;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ClientSize = new System.Drawing.Size(600, 312);
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Cバージョン情報";
			this.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Click += new System.EventHandler(this.Cバージョン情報_Click);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.Cバージョン情報_Paint);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Cバージョン情報_KeyDown);
			this.ResumeLayout(false);

		}
		
		#endregion

	}
}
