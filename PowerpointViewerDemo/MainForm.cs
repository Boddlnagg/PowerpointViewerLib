using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PowerpointViewerLib;
using System.Runtime.InteropServices;

namespace PowerpointViewerDemo
{
	public partial class MainForm : Form
	{
		[DllImport("User32.dll")]
		private static extern int SetForegroundWindow(IntPtr hWnd);

		List<PowerpointViewerDocument> openDocuments = new List<PowerpointViewerDocument>();
		PowerpointViewerDocument activeDocument;
		int counter = 0;

		public MainForm()
		{
			InitializeComponent();

			PowerpointViewerController.DebugMode = true;

		}

		private void buttonOpen_Click(object sender, EventArgs e)
		{
			int x = int.Parse(textBoxX.Text);
			int y = int.Parse(textBoxY.Text);
			int width = int.Parse(textBoxWidth.Text);
			int height = int.Parse(textBoxHeight.Text);
			string filename = textBoxFilename.Text;

			if (string.IsNullOrEmpty(filename))
				return;

			Rectangle rect = new Rectangle(x, y, width, height);
			try
			{
				activeDocument = PowerpointViewerController.Open(filename, rect, thumbnailWidth: 200);
				activeDocument.Loaded += new EventHandler(activeDocument_Loaded);
				activeDocument.SlideChanged += new EventHandler(activeDocument_SlideChanged);
				activeDocument.Closed += new EventHandler(activeDocument_Closed);
				openDocuments.Add(activeDocument);
				listBoxDocuments.SelectedIndex = listBoxDocuments.Items.Add("Document #" + (counter++));
				this.UpdateStats();
				SetForegroundWindow(this.Handle);
			}
			catch (PowerpointViewerController.PowerpointViewerOpenException)
			{
				MessageBox.Show("Loading failed.");
			}
		}

		// VORSICHT: Die Events kommen von einem anderen Thread, deshalb this.Invoke(...)
		void activeDocument_SlideChanged(object sender, EventArgs e)
		{
			this.Invoke(new Action(this.UpdateStats));
		}

		void activeDocument_Loaded(object sender, EventArgs e)
		{
			this.Invoke(new Action(() => pictureBoxThumb.Image = (sender as PowerpointViewerDocument).Thumbnails[0]));
		}

		void activeDocument_Closed(object sender, EventArgs e)
		{
			if (openDocuments != null)
			{
				this.Invoke(new Action<PowerpointViewerDocument>((doc) =>
				{
					pictureBoxThumb.Image = null;
					int i = openDocuments.IndexOf(doc);
					openDocuments.Remove(doc);
					listBoxDocuments.Items.RemoveAt(i);
				}), sender as PowerpointViewerDocument);
			}
		}

		public void UpdateStats()
		{
			if (activeDocument == null)
			{
				labelSlide.Text = "-";
				labelCount.Text = "-";
			}
			else
			{
				labelSlide.Text = activeDocument.CurrentSlide.ToString();
				labelCount.Text = activeDocument.SlideCount.ToString();
				if (activeDocument.CurrentSlide != -1)
				{
					pictureBoxThumb.Image = activeDocument.Thumbnails[activeDocument.CurrentSlide];
				}
			}
		}

		private void buttonPrev_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.PrevStep();
		}

		private void buttonNext_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.NextStep();
		}

		private void buttonHide_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.Hide();
		}

		private void buttonResume_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.Show();
		}

		private void buttonClose_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.Close();
			activeDocument = null;
		}

		private void buttonGoto_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.GotoSlide(int.Parse(textBoxGoto.Text));
		}

		private void buttonChooseFile_Click(object sender, EventArgs e)
		{
			var ofd = new OpenFileDialog();
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				textBoxFilename.Text = ofd.FileName;
			}
		}

		private void buttonBlank_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.Blank();
		}

		private void buttonUnblank_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;
			activeDocument.Unblank();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			var tmp = openDocuments;
			openDocuments = null;
			foreach (var doc in tmp)
			{
				doc.Close();
			}
		}

		private void listBoxDocuments_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listBoxDocuments.SelectedIndex == -1)
				activeDocument = null;
			else
				activeDocument = openDocuments[listBoxDocuments.SelectedIndex];
			UpdateStats();
		}

		private void buttonMove_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;

			int x = int.Parse(textBoxX.Text);
			int y = int.Parse(textBoxY.Text);
			activeDocument.Move(x, y);
		}

		private void buttonFocus_Click(object sender, EventArgs e)
		{
			if (activeDocument == null)
				return;

			activeDocument.Focus();
		}
	}
}
