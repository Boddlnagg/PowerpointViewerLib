/*
 * PowerpointViewerLib - PowerPoint Viewer 2003/2007 Controller
 * 
 * Copyright (c) 2012 Kai Patrick Reisert
 * Portions copyright (c) 2008-2011 Raoul Snyman, Tim Bentley, Jonathan Corwin,
 * Michael Gorven, Scott Guerrieri, Matthias Hub, Meinert Jordan, Armin Köhler,
 * Andreas Preikschat, Mattias Põldaru, Christian Richter, Philip Ridout,
 * Maikel Stuivenberg, Martin Thompson, Jon Tibble, Frode Woldsund
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PowerpointViewerLib
{
	public class ErrorEventArgs : EventArgs
	{
		public Exception Exception { get; private set; }

		public ErrorEventArgs(Exception e)
		{
			this.Exception = e;
		}
	}

	public class PowerpointViewerDocument
	{
		class ThumbnailWrapper
		{
			public Bitmap Bitmap { get; set; }
			public int Slide { get; set; }
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, UIntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError=true)]
		private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, UIntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32.dll")]
		private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

		[DllImport("user32.dll")]
		private static extern int GetWindowRect(IntPtr hwnd, out Rectangle rc);

		[DllImport("User32.dll")]
		private static extern int SetForegroundWindow(IntPtr hWnd);

		const uint WM_MOUSEWHEEL = 0x020A;
		const uint WM_KEYUP = 0x0101;
		const uint WM_KEYDOWN = 0x0100;
		const uint WM_CHAR = 0x0102;
		const uint WM_SETFOCUS = 0x7;
		const uint VK_RETURN = 0x0D;

		private static IntPtr MakeWParam(int loWord, int hiWord)
		{
			return new IntPtr(loWord + hiWord * 65536);
		}

		private enum State
		{
			Starting,
			Loading,
			Resetting,
			Running
		}

		private State state = State.Starting;
		private volatile int currentSlide = -1;
		private Dictionary<int, int> slideSteps = new Dictionary<int, int>();
		private Dictionary<int, int> slideIds = new Dictionary<int, int>();

		private IntPtr hWnd;
		private IntPtr hWnd2;

		public IntPtr WindowHandle
		{
			get
			{
				return hWnd;
			}
		}

		private Rectangle rect;
		private bool closed = false;
		private bool openHidden;
		private volatile List<ThumbnailWrapper> captureThumbs;
		private readonly int thumbnailWidth;

		public event EventHandler SlideChanged;
		public event EventHandler Closed;
		public event EventHandler Loaded;
		public event EventHandler<ErrorEventArgs> Error;

		PowerpointViewerController.CallbackDelegate del;

		public List<Bitmap> Thumbnails
		{
			get;
			private set;
		}

		public bool HasLoaded
		{
			get
			{
				return (state == State.Running);
			}
		}

		private int id = -1;

		internal PowerpointViewerDocument(string filename, Rectangle rect, bool generateThumbnails, bool openHidden, int thumbnailWidth)
		{
			this.rect = rect;
			this.openHidden = openHidden;

			if (generateThumbnails)
			{
				this.captureThumbs = new List<ThumbnailWrapper>();
				this.thumbnailWidth = thumbnailWidth;
			}

			string cmd = PowerpointViewerController.ViewerPath + " /F /S \"" + filename + "\"";

			del = new PowerpointViewerController.CallbackDelegate(Callback);

			this.id = PowerpointViewerController.OpenPPT(new StringBuilder(cmd), del, IntPtr.Zero, rect.X, rect.Y, rect.Width, rect.Height);
			if (this.id == -1)
				throw new PowerpointViewerController.PowerpointViewerOpenException();
		}

		private int Callback(int msg, int param)
		{
			switch (msg)
			{ 
				case 1: // Receive window handle
					hWnd = new IntPtr(param);
					break;
				case 2: // Receive second window handle, the window is now setup completely
					hWnd2 = new IntPtr(param);
					Debug("Started, now loading ...");
					state = State.Loading;
					break;
				case 3: // We're moving one step further while loading
					if (state == State.Loading)
					{
						if (currentSlide >= 0)
						{
							slideSteps[currentSlide]++;
							Debug("Updating steps of slide #" + currentSlide + ": " + slideSteps[currentSlide]);
						}
					}
					break;
				case 4: // Slide change
					new Thread(() =>
						{
							try
							{
								Debug("Slide changed: " + param + " (" + state.ToString() + ")");

								bool repeating = false;

								if ((state == State.Starting || state == State.Loading) && param != 0)
								{
									currentSlide++;
									if (!slideIds.ContainsKey(param))
									{
										slideIds.Add(param, currentSlide);
										slideSteps.Add(currentSlide, 0);
									}
									else
									{
										repeating = true;
									}
								}
								else if (state == State.Running || state == State.Resetting)
								{
									if (param == 0)
									{
										PrevStep();
									}
									else
									{
										if (state == State.Resetting && !slideIds.ContainsKey(param))
										{
											throw new PowerpointViewerController.PowerpointViewerOpenException();
										}
										else
										{
											currentSlide = slideIds[param];
										}

										if (state == State.Running)
										{
											OnSlideChanged();
										}
									}
								}

								if (state == State.Loading && currentSlide != -1)
								{
									if (param == 0 || repeating) // reached last slide or noticed that slide id's are repeating
									{
										// If we don't go back every single step, PowerPoint remembers
										// that the slides have been animated and won't show the animations
										// the next time.

										state = State.Resetting;
										PrevStep();
									}
								}
								else if (state == State.Resetting)
								{
									if (captureThumbs != null)
									{
										Bitmap bmp = Capture(this.thumbnailWidth);
										captureThumbs.Add(new ThumbnailWrapper { Bitmap = bmp, Slide = currentSlide });
									}

									int goBackSteps = currentSlide == 0 ? slideSteps[currentSlide] - 1 : slideSteps[currentSlide];

									for (int i = 0; i < goBackSteps; i++)
									{
										Debug("Going back (slide #" + currentSlide + ")");
										PrevStep();
									}
									if (currentSlide == 0) // back at the beginning
									{
										state = State.Running;
										if (captureThumbs != null)
										{
											this.Thumbnails = (from t in captureThumbs orderby t.Slide ascending select t.Bitmap).ToList();
										}
										Unblank();
										if (!openHidden)
											Show();
										OnLoaded();
										OnSlideChanged();
										Debug("Now running ...");
									}
								}
							}
							catch (Exception e)
							{
								Close();
								OnError(e);
							}
						}).Start();
					break;
				case 5: // Slideshow window has been closed (e.g. ESC)
					Debug("Window has been closed");
					new Thread(() => Close()).Start();
					break;
				case 6: // The slideshow is closing
					Debug("Close event");
					new Thread(() => 
						{
							OnClosed();
							closed = true;
						}).Start();
					break;
				default:
					Debug("Unknown message: " + msg + "(param: "+param+")");
					break;
			}
			return 0;
		}

		private void Debug(string p)
		{
			System.Diagnostics.Debug.WriteLine("#" + this.id + " - " + p);
		}

		private void OnLoaded()
		{
			if (Loaded != null)
				Loaded(this, EventArgs.Empty);
		}

		private void OnClosed()
		{
			if (Closed != null)
				Closed(this, EventArgs.Empty);
		}

		private void OnSlideChanged()
		{
			if (SlideChanged != null)
				SlideChanged(this, EventArgs.Empty);
		}

		private void OnError(Exception e)
		{
			if (Error != null)
				Error(this, new ErrorEventArgs(e));
		}

		/// <summary>
		/// Gets the total number of slides.
		/// </summary>
		public int SlideCount
		{
			get
			{
				if (state == State.Starting || state == State.Loading)
					return -1;

				return slideIds.Count;
			}
		}

		/// <summary>
		/// Gets the 0-based index of the current slide.
		/// </summary>
		public int CurrentSlide
		{
			get
			{
				if (state != State.Running)
					return -1;

				return currentSlide;
			}
		}

		/// <summary>
		/// Gets the number of steps of the slide with the given index.
		/// 1 means that there is no animation on that slide.
		/// </summary>
		/// <param name="slide">The slide index.</param>
		/// <returns></returns>
		public int GetSlideStepCount(int slide)
		{
			if (state != State.Running)
				throw new InvalidOperationException("Slideshow not loaded yet.");

			if (slide < 0 || slide >= SlideCount)
				throw new ArgumentOutOfRangeException("slide");

			return slideSteps[slide];
		}

		/// <summary>
		/// Sets the focus to the Powerpoint Viewer window.
		/// </summary>
		public void Focus()
		{
			SetForegroundWindow(hWnd);
		}

		/// <summary>
		/// Moves one step back in the presentation.
		/// </summary>
		public void PrevStep()
		{
			SendMessage(hWnd, WM_SETFOCUS, IntPtr.Zero, UIntPtr.Zero);
			PostMessage(hWnd2, WM_MOUSEWHEEL, MakeWParam(0, 120), UIntPtr.Zero);
		}

		/// <summary>
		/// Moves one step further in the presentation.
		/// </summary>
		public void NextStep()
		{
			if (currentSlide >= SlideCount) return;

			SendMessage(hWnd, WM_SETFOCUS, IntPtr.Zero, UIntPtr.Zero);
			PostMessage(hWnd2, WM_MOUSEWHEEL, MakeWParam(0, -120), UIntPtr.Zero);
		}

		/// <summary>
		/// Moves to a given slide.
		/// </summary>
		/// <param name="slide">The 0-based index of the slide.</param>
		public void GotoSlide(int slide)
		{
			if (slide < 0)
				throw new ArgumentException();

			int num = slide + 1;
			char[] digits = num.ToString().ToCharArray();

			SendMessage(hWnd, WM_SETFOCUS, IntPtr.Zero, UIntPtr.Zero);

			foreach (char c in digits)
			{
				PostMessage(hWnd2, WM_KEYDOWN, new IntPtr((int)c), new UIntPtr(0));
				PostMessage(hWnd2, WM_KEYUP, new IntPtr((int)c), new UIntPtr(0xC0000001));
			}
			Thread.Sleep(10);
			PostMessage(hWnd2, WM_KEYDOWN, new IntPtr((int)VK_RETURN), new UIntPtr(0));
			PostMessage(hWnd2, WM_KEYUP, new IntPtr((int)VK_RETURN), new UIntPtr(0xC0000001));
		}

		/// <summary>
		/// Blanks the presentation (blackscreen).
		/// </summary>
		public void Blank()
		{
			// Unblank first, using any key ('A' in this case), then blank
			SendMessage(hWnd, WM_SETFOCUS, IntPtr.Zero, UIntPtr.Zero);
			PostMessage(hWnd2, WM_KEYDOWN, new IntPtr((int)'A'), new UIntPtr(0));
			PostMessage(hWnd2, WM_KEYUP, new IntPtr((int)'A'), new UIntPtr(0xC0000001));
			Thread.Sleep(10);
			PostMessage(hWnd2, WM_KEYDOWN, new IntPtr((int)'B'), new UIntPtr(0));
			PostMessage(hWnd2, WM_KEYUP, new IntPtr((int)'B'), new UIntPtr(0xC0000001));
		}

		/// <summary>
		/// Unblanks the presentation.
		/// </summary>
		public void Unblank()
		{
			SendMessage(hWnd, WM_SETFOCUS, IntPtr.Zero, UIntPtr.Zero);
			PostMessage(hWnd2, WM_KEYDOWN, new IntPtr((int)'A'), new UIntPtr(0));
			PostMessage(hWnd2, WM_KEYUP, new IntPtr((int)'A'), new UIntPtr(0xC0000001));
		}

		/// <summary>
		/// Hides the presentation by moving it outside of the screen.
		/// </summary>
		public void Hide()
		{
			MoveWindow(hWnd, -32000, -32000, this.rect.Width, this.rect.Height, true);
		}

		/// <summary>
		/// Shows the presentation by moving it back to it's normal position.
		/// </summary>
		public void Show()
		{
			MoveWindow(hWnd, this.rect.Left, this.rect.Top, this.rect.Width,this.rect.Height, true);
		}

		/// <summary>
		/// Moves the presentation window.
		/// </summary>
		/// <param name="x">The new x position.</param>
		/// <param name="y">The new y position.</param>
		public void Move(int x, int y)
		{
			this.rect.X = x;
			this.rect.Y = y;
			Show();
		}

		/// <summary>
		/// Closes the presentation.
		/// </summary>
		public void Close()
		{
			if (!closed)
			{
				PowerpointViewerController.ClosePPT(this.id);
				closed = true;
			}
		}

		/// <summary>
		/// Captures a screenshot of the presentation window as a bitmap
		/// (only works while the window is shown).
		/// </summary>
		/// <param name="width">The desired width of the bitmap.</param>
		/// <returns>A bitmap containing the screenshow.</returns>
		public Bitmap CaptureWindow(int width)
		{
			return Capture(width);
		}

		~PowerpointViewerDocument()
		{
			this.Close();
		}

		private Bitmap Capture(int thumbnailWidth)
		{
			Rectangle rc;
			GetWindowRect(hWnd, out rc);

			Bitmap bm = new Bitmap(rect.Width, rect.Height);
			Graphics g = Graphics.FromImage(bm);
			IntPtr hdc = g.GetHdc();

			PrintWindow(hWnd, hdc, 0);

			g.ReleaseHdc(hdc);
			g.Flush();
			g.Dispose();

			if (thumbnailWidth > 0)
			{
				double ratio = (double)rect.Width / rect.Height;
				int thumbnailHeight = (int)(thumbnailWidth / ratio);

				Bitmap result = new Bitmap(thumbnailWidth, thumbnailHeight);
				using (Graphics gg = Graphics.FromImage((Image)result))
					gg.DrawImage(bm, 0, 0, thumbnailWidth, thumbnailHeight);
				bm.Dispose();
				return result;
			}
			else
			{
				return bm; // do not resize
			}
		}
	}
}
