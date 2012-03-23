/*
 * PowerpointViewerLib - PowerPoint Viewer 2003/2007 Controller
 * 
 * Copyright (c) 2012 Kai Patrick Reisert
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace PowerpointViewerLib
{
	public class PowerpointViewerController
	{
		internal class PowerpointViewerOpenException : ApplicationException { }

		internal const string DllName = "pptviewlib.dll";

		[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
		internal delegate int CallbackDelegate(int msg, int param);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int OpenPPT(StringBuilder command, CallbackDelegate func, IntPtr hParentWnd, int x, int y, int width, int height);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void ClosePPT(int id);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SetDebug(bool onOff);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void Shutdown();

		private static bool debugMode = false; // this is the default set in pptviewlib.cpp

		public static bool DebugMode
		{
			get
			{
				return debugMode;
			}
			set
			{
				debugMode = value;
				SetDebug(debugMode);
			}
		}

		private static string viewerPath;

		public static string ViewerPath
		{
			get
			{
				if (viewerPath == null)
					viewerPath = GetPPTViewerPath();
				return viewerPath;
			}
		}

		public static bool IsAvailable
		{
			get
			{
				if (viewerPath == null)
					viewerPath = GetPPTViewerPath();
				return (viewerPath != null);
			}
		}

		private PowerpointViewerController()
		{
			viewerPath = GetPPTViewerPath();
		}

		public static PowerpointViewerDocument Open(string filename, Rectangle rect, bool generateThumbnails = true, bool openHidden = false)
		{
			if (!IsAvailable)
				throw new InvalidOperationException("Can't open a file: Powerpoint Viewer could not be found.");

			var file = new FileInfo(filename);

			if (!file.Exists)
				throw new FileNotFoundException(file.FullName);

			try
			{
				PowerpointViewerDocument doc = new PowerpointViewerDocument(file.FullName, rect, generateThumbnails, openHidden);
				return doc;
			}
			catch (PowerpointViewerOpenException)
			{
				return null;
			}
		}

		/// <summary>
		/// Get the path of the PowerPoint viewer from the registry
		/// </summary>
		private static string GetPPTViewerPath()
		{
			// The following registry settings are for, respectively, (I think)
			// PPT Viewer 2007 (older versions. Latest not in registry) & PPT Viewer 2010
			// PPT Viewer 2003 (recent versions)
			// PPT Viewer 2003 (older versions) 
			// PPT Viewer 97

			string path = TryGetPPTViewerPathFromRegKey("PowerPointViewer.Show.12\\shell\\Show\\command");
			if (path == null)
				path = TryGetPPTViewerPathFromRegKey("PowerPointViewer.Show.11\\shell\\Show\\command");
			if (path == null)
				path = TryGetPPTViewerPathFromRegKey("Applications\\PPTVIEW.EXE\\shell\\open\\command");
			if (path == null)
				path = TryGetPPTViewerPathFromRegKey("Applications\\PPTVIEW.EXE\\shell\\Show\\command");

			if (path != null && File.Exists(path))
				return path;

			// This is where it gets ugly. PPT2007 it seems no longer stores its
			// location in the registry. So we have to use the defaults which will
			// upset those who like to put things somewhere else

			// Viewer 2007 in 64bit Windows:
			path = "C:\\Program Files (x86)\\Microsoft Office\\Office12\\PPTVIEW.EXE";
			if (File.Exists(path))
				return path;

			// Viewer 2007 in 32bit Windows:
			path = "C:\\Program Files\\Microsoft Office\\Office12\\PPTVIEW.EXE";
			if (File.Exists(path))
				return path;

			// Give them the opportunity to place it in the same folder as the app
			path = Directory.GetCurrentDirectory() + "\\PPTVIEW.EXE";
			if (File.Exists(path))
				return path;

			return null;
		}

		private static string TryGetPPTViewerPathFromRegKey(string keyname)
		{
			var key = Registry.ClassesRoot.OpenSubKey(keyname);
			if (key != null)
			{
				string value = key.GetValue("") as string;
				if (value != null)
				{
					return value.Substring(0, value.Length - 4);
				}
			}

			return null;
		}

		~PowerpointViewerController()
		{
			Shutdown();
		}
	}
}
