﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;

namespace DTXMania
{
	/// <summary>
	/// システム環境のログを出力する
	/// </summary>
	public static class CPutSystemLog
	{
		public static void PutSystemLog()
		{
			System.Management.ManagementClass mc =
				new System.Management.ManagementClass("Win32_Processor");
			System.Management.ManagementObjectCollection moc = mc.GetInstances();
			foreach (System.Management.ManagementObject mo in moc)
			{
				Trace.TraceInformation("-------------------------");
				Trace.TraceInformation("CPU Information:");
				//Trace.TraceInformation("DeviceID      = {0}", mo["DeviceID"]);
				Trace.TraceInformation("Name          = {0}", mo["Name"]);
				Trace.TraceInformation("MaxClockSpeed = {0}MHz", mo["MaxClockSpeed"]);
				Trace.TraceInformation("L2CacheSize   = {0}KB", mo["L2CacheSize"]);
				Trace.TraceInformation("L3CacheSize   = {0}KB", mo["L3CacheSize"]);
				Trace.TraceInformation("NumberOfCores = {0}", mo["NumberOfCores"]);
				Trace.TraceInformation("NumberOfLogicalProcessors = {0}", mo["NumberOfLogicalProcessors"]);
			}
			moc.Dispose();
			mc.Dispose();

			//System.Management.ManagementClass mc =
			//	new System.Management.ManagementClass("Win32_OperatingSystem");
			//System.Management.ManagementObjectCollection moc = mc.GetInstances();
			mc =
				new System.Management.ManagementClass("Win32_OperatingSystem");
			moc = mc.GetInstances();
			foreach (System.Management.ManagementObject mo in moc)
			{
				Trace.TraceInformation("-------------------------");
				Trace.TraceInformation("OS Information:");
				//簡単な説明（Windows 8.1では「Microsoft Windows 8.1 Pro」等）
				Trace.TraceInformation("OS: " + mo["Caption"]);
				//バージョン（Windows 8.1では、「6.3.9600」）
				Trace.TraceInformation("Version: " + mo["Version"]);
				//ビルド番号（Windows 8.1では「9600」）
				//Trace.TraceInformation( "BuildNumber: " + mo["BuildNumber"]);

				//サービスパック（Windows 8.1ではNULL）
				Trace.TraceInformation("CSDVersion (ServicePack): " + mo["CSDVersion"]);
				//言語（日本語は「1041」）
				Trace.TraceInformation("OSLanguage: " + mo["OSLanguage"]);

				Trace.TraceInformation("OSArchitecture: " + mo["OSArchitecture"]);

				//Trace.TraceInformation("TotalVisibleMemorySize = {0}", mo["TotalVisibleMemorySize"]);
			}
			moc.Dispose();
			mc.Dispose();

			Trace.TraceInformation("-------------------------");
			Trace.TraceInformation("General Environment Information:");
			//Trace.TraceInformation("OS Version: " + Environment.OSVersion);	// fake version will be returned (due to the lack of manifest settings)
			//Trace.TraceInformation("ProcessorCount: " + Environment.ProcessorCount.ToString());
			Trace.TraceInformation("CLR Version: " + Environment.Version.ToString());
			Trace.TraceInformation("SystemPageSize: " + Environment.SystemPageSize.ToString());

			var cominfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
			Trace.TraceInformation("TotalPhysicalMemorySize: {0:F2}GB", (cominfo.TotalPhysicalMemory / 1024f / 1024f / 1024f));
			Trace.TraceInformation("VirtialMemorySize: {0:F2}GB", (cominfo.TotalVirtualMemory / 1024f / 1024f / 1024f));
			Trace.TraceInformation("FreePhysicalMemorySize: {0:F2}MB", (cominfo.AvailablePhysicalMemory / 1024f / 1024f));
			Trace.TraceInformation("FreeVirtualMemorySize: {0:F2}MB", (cominfo.AvailableVirtualMemory / 1024f / 1024f));
			//Trace.TraceInformation(cominfo.OSFullName + ", " + cominfo.OSPlatform + ", " + cominfo.OSVersion);

			mc = new System.Management.ManagementClass("Win32_PhysicalMemory");
			moc = mc.GetInstances();
			Trace.TraceInformation("-------------------------");
			Trace.TraceInformation("Physical Memory Information:");
			foreach (System.Management.ManagementObject mo in moc)
			{
				Trace.TraceInformation("Capacity: {0:F2}GB", (Convert.ToInt64(mo["Capacity"]) / 1024f / 1024f / 1024f));
			}
			moc.Dispose();
			mc.Dispose();

			mc = new System.Management.ManagementClass("Win32_DisplayControllerConfiguration");
			moc = mc.GetInstances();
			foreach (System.Management.ManagementObject mo in moc)
			{
				Trace.TraceInformation("-------------------------");
				Trace.TraceInformation("Display Adapter Information:");
				Trace.TraceInformation("Name: " + mo["Name"]);
				Trace.TraceInformation("VideoMode: " + mo["VideoMode"]);
				Trace.TraceInformation("HorizontalResolution: " + mo["HorizontalResolution"]);
				Trace.TraceInformation("VerticalResolution: " + mo["VerticalResolution"]);
				Trace.TraceInformation("RefreshRate: " + mo["RefreshRate"]);
			}
			moc.Dispose();
			mc.Dispose();

			mc = new System.Management.ManagementClass("Win32_VideoController");
			moc = mc.GetInstances();
			foreach (System.Management.ManagementObject mo in moc)
			{
				Trace.TraceInformation("-------------------------");
				Trace.TraceInformation("Video Controller Information:");
				Trace.TraceInformation("Description: " + mo["Description"]);
				Trace.TraceInformation("AdapterRAM: {0}MB", (Convert.ToInt64(mo["AdapterRAM"]) / 1024f / 1024f));
				Trace.TraceInformation("CapabilityDescriptions: " + mo["CapabilityDescriptions"]);
			}
			moc.Dispose();
			mc.Dispose();

			mc = new System.Management.ManagementClass("Win32_DesktopMonitor");
			moc = mc.GetInstances();
			foreach (System.Management.ManagementObject mo in moc)
			{
				Trace.TraceInformation("-------------------------");
				Trace.TraceInformation("Display Information:");
				Trace.TraceInformation("Description: " + mo["Description"]);
				Trace.TraceInformation("PixelsPerXLogicalInch: " + mo["PixelsPerXLogicalInch"]);
				Trace.TraceInformation("PixelsPerYLogicalInch: " + mo["PixelsPerYLogicalInch"]);
				Trace.TraceInformation("ScreenWidth: " + mo["ScreenWidth"]);
				Trace.TraceInformation("ScreenHeight: " + mo["ScreenHeight"]);
			}
			moc.Dispose();
			mc.Dispose();

			mc = new System.Management.ManagementClass("Win32_SoundDevice");
			moc = mc.GetInstances();
			foreach (System.Management.ManagementObject mo in moc)
			{
				Trace.TraceInformation("-------------------------");
				Trace.TraceInformation("Audio Information:");
				//Trace.TraceInformation("Caption: " + mo["Caption"]);
				//Trace.TraceInformation("ProductName: " + mo["ProductName"]);
				//Trace.TraceInformation("DMABufferSize: " + mo["DMABufferSize"]);

				foreach (PropertyData property in mo.Properties)
				{
					Trace.TraceInformation("{0}:{1}", property.Name, property.Value);
				}
			}
			moc.Dispose();
			mc.Dispose();

			Trace.TraceInformation("----------------------");
			Trace.TraceInformation("DTXMania settings:");
			Trace.TraceInformation("VSyncWait: " + CDTXMania.Instance.ConfigIni.bVSyncWait.ToString());
			Trace.TraceInformation("Fullscreen: " + CDTXMania.Instance.ConfigIni.bFullScreen.ToString());
			Trace.TraceInformation("----------------------");
		}
	}
}
