namespace PowerpointViewerDemo
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.textBoxX = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxY = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxWidth = new System.Windows.Forms.TextBox();
            this.textBoxHeight = new System.Windows.Forms.TextBox();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxFilename = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.labelSlide = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.labelCount = new System.Windows.Forms.Label();
            this.buttonPrev = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonChooseFile = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonMove = new System.Windows.Forms.Button();
            this.listBoxDocuments = new System.Windows.Forms.ListBox();
            this.buttonUnblank = new System.Windows.Forms.Button();
            this.buttonBlank = new System.Windows.Forms.Button();
            this.textBoxGoto = new System.Windows.Forms.TextBox();
            this.buttonGoto = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonResume = new System.Windows.Forms.Button();
            this.buttonHide = new System.Windows.Forms.Button();
            this.pictureBoxThumb = new System.Windows.Forms.PictureBox();
            this.buttonFocus = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumb)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxX
            // 
            this.textBoxX.Location = new System.Drawing.Point(69, 13);
            this.textBoxX.Name = "textBoxX";
            this.textBoxX.Size = new System.Drawing.Size(57, 20);
            this.textBoxX.TabIndex = 0;
            this.textBoxX.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(46, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "X:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(46, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y:";
            // 
            // textBoxY
            // 
            this.textBoxY.Location = new System.Drawing.Point(69, 39);
            this.textBoxY.Name = "textBoxY";
            this.textBoxY.Size = new System.Drawing.Size(57, 20);
            this.textBoxY.TabIndex = 3;
            this.textBoxY.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Breite:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Höhe:";
            // 
            // textBoxWidth
            // 
            this.textBoxWidth.Location = new System.Drawing.Point(69, 65);
            this.textBoxWidth.Name = "textBoxWidth";
            this.textBoxWidth.Size = new System.Drawing.Size(57, 20);
            this.textBoxWidth.TabIndex = 6;
            this.textBoxWidth.Text = "800";
            // 
            // textBoxHeight
            // 
            this.textBoxHeight.Location = new System.Drawing.Point(69, 91);
            this.textBoxHeight.Name = "textBoxHeight";
            this.textBoxHeight.Size = new System.Drawing.Size(57, 20);
            this.textBoxHeight.TabIndex = 7;
            this.textBoxHeight.Text = "600";
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(11, 87);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(252, 53);
            this.buttonOpen.TabIndex = 8;
            this.buttonOpen.Text = "Öffnen";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Datei:";
            // 
            // textBoxFilename
            // 
            this.textBoxFilename.Location = new System.Drawing.Point(6, 32);
            this.textBoxFilename.Name = "textBoxFilename";
            this.textBoxFilename.Size = new System.Drawing.Size(257, 20);
            this.textBoxFilename.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 111);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Slide:";
            // 
            // labelSlide
            // 
            this.labelSlide.AutoSize = true;
            this.labelSlide.Location = new System.Drawing.Point(45, 111);
            this.labelSlide.Name = "labelSlide";
            this.labelSlide.Size = new System.Drawing.Size(10, 13);
            this.labelSlide.TabIndex = 14;
            this.labelSlide.Text = "-";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(84, 111);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Anzahl:";
            // 
            // labelCount
            // 
            this.labelCount.AutoSize = true;
            this.labelCount.Location = new System.Drawing.Point(132, 111);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(10, 13);
            this.labelCount.TabIndex = 16;
            this.labelCount.Text = "-";
            // 
            // buttonPrev
            // 
            this.buttonPrev.Location = new System.Drawing.Point(6, 19);
            this.buttonPrev.Name = "buttonPrev";
            this.buttonPrev.Size = new System.Drawing.Size(75, 23);
            this.buttonPrev.TabIndex = 17;
            this.buttonPrev.Text = "Vorherige";
            this.buttonPrev.UseVisualStyleBackColor = true;
            this.buttonPrev.Click += new System.EventHandler(this.buttonPrev_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Location = new System.Drawing.Point(87, 19);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 18;
            this.buttonNext.Text = "Nächste";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonChooseFile);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.textBoxFilename);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.buttonOpen);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(416, 146);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Setup";
            // 
            // buttonChooseFile
            // 
            this.buttonChooseFile.Location = new System.Drawing.Point(188, 58);
            this.buttonChooseFile.Name = "buttonChooseFile";
            this.buttonChooseFile.Size = new System.Drawing.Size(75, 23);
            this.buttonChooseFile.TabIndex = 14;
            this.buttonChooseFile.Text = "Durchsuchen";
            this.buttonChooseFile.UseVisualStyleBackColor = true;
            this.buttonChooseFile.Click += new System.EventHandler(this.buttonChooseFile_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBoxX);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBoxY);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBoxWidth);
            this.groupBox2.Controls.Add(this.textBoxHeight);
            this.groupBox2.Location = new System.Drawing.Point(269, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(141, 121);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Fenster";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.buttonFocus);
            this.groupBox3.Controls.Add(this.buttonMove);
            this.groupBox3.Controls.Add(this.listBoxDocuments);
            this.groupBox3.Controls.Add(this.buttonUnblank);
            this.groupBox3.Controls.Add(this.buttonBlank);
            this.groupBox3.Controls.Add(this.textBoxGoto);
            this.groupBox3.Controls.Add(this.buttonGoto);
            this.groupBox3.Controls.Add(this.buttonClose);
            this.groupBox3.Controls.Add(this.buttonResume);
            this.groupBox3.Controls.Add(this.buttonHide);
            this.groupBox3.Controls.Add(this.buttonPrev);
            this.groupBox3.Controls.Add(this.buttonNext);
            this.groupBox3.Controls.Add(this.labelCount);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.labelSlide);
            this.groupBox3.Location = new System.Drawing.Point(12, 164);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(416, 136);
            this.groupBox3.TabIndex = 20;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Steuerung";
            // 
            // buttonMove
            // 
            this.buttonMove.Location = new System.Drawing.Point(168, 48);
            this.buttonMove.Name = "buttonMove";
            this.buttonMove.Size = new System.Drawing.Size(75, 23);
            this.buttonMove.TabIndex = 31;
            this.buttonMove.Text = "Move";
            this.buttonMove.UseVisualStyleBackColor = true;
            this.buttonMove.Click += new System.EventHandler(this.buttonMove_Click);
            // 
            // listBoxDocuments
            // 
            this.listBoxDocuments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxDocuments.FormattingEnabled = true;
            this.listBoxDocuments.Location = new System.Drawing.Point(249, 22);
            this.listBoxDocuments.Name = "listBoxDocuments";
            this.listBoxDocuments.Size = new System.Drawing.Size(161, 108);
            this.listBoxDocuments.TabIndex = 30;
            this.listBoxDocuments.SelectedIndexChanged += new System.EventHandler(this.listBoxDocuments_SelectedIndexChanged);
            // 
            // buttonUnblank
            // 
            this.buttonUnblank.Location = new System.Drawing.Point(87, 77);
            this.buttonUnblank.Name = "buttonUnblank";
            this.buttonUnblank.Size = new System.Drawing.Size(75, 23);
            this.buttonUnblank.TabIndex = 29;
            this.buttonUnblank.Text = "Unblank";
            this.buttonUnblank.UseVisualStyleBackColor = true;
            this.buttonUnblank.Click += new System.EventHandler(this.buttonUnblank_Click);
            // 
            // buttonBlank
            // 
            this.buttonBlank.Location = new System.Drawing.Point(6, 77);
            this.buttonBlank.Name = "buttonBlank";
            this.buttonBlank.Size = new System.Drawing.Size(75, 23);
            this.buttonBlank.TabIndex = 28;
            this.buttonBlank.Text = "Blank";
            this.buttonBlank.UseVisualStyleBackColor = true;
            this.buttonBlank.Click += new System.EventHandler(this.buttonBlank_Click);
            // 
            // textBoxGoto
            // 
            this.textBoxGoto.Location = new System.Drawing.Point(167, 108);
            this.textBoxGoto.Name = "textBoxGoto";
            this.textBoxGoto.Size = new System.Drawing.Size(26, 20);
            this.textBoxGoto.TabIndex = 26;
            this.textBoxGoto.Text = "0";
            // 
            // buttonGoto
            // 
            this.buttonGoto.Location = new System.Drawing.Point(199, 106);
            this.buttonGoto.Name = "buttonGoto";
            this.buttonGoto.Size = new System.Drawing.Size(44, 23);
            this.buttonGoto.TabIndex = 25;
            this.buttonGoto.Text = "Goto";
            this.buttonGoto.UseVisualStyleBackColor = true;
            this.buttonGoto.Click += new System.EventHandler(this.buttonGoto_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(168, 77);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 24;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonResume
            // 
            this.buttonResume.Location = new System.Drawing.Point(87, 48);
            this.buttonResume.Name = "buttonResume";
            this.buttonResume.Size = new System.Drawing.Size(75, 23);
            this.buttonResume.TabIndex = 23;
            this.buttonResume.Text = "Show";
            this.buttonResume.UseVisualStyleBackColor = true;
            this.buttonResume.Click += new System.EventHandler(this.buttonResume_Click);
            // 
            // buttonHide
            // 
            this.buttonHide.Location = new System.Drawing.Point(6, 48);
            this.buttonHide.Name = "buttonHide";
            this.buttonHide.Size = new System.Drawing.Size(75, 23);
            this.buttonHide.TabIndex = 22;
            this.buttonHide.Text = "Hide";
            this.buttonHide.UseVisualStyleBackColor = true;
            this.buttonHide.Click += new System.EventHandler(this.buttonHide_Click);
            // 
            // pictureBoxThumb
            // 
            this.pictureBoxThumb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxThumb.BackColor = System.Drawing.Color.White;
            this.pictureBoxThumb.Location = new System.Drawing.Point(434, 14);
            this.pictureBoxThumb.Name = "pictureBoxThumb";
            this.pictureBoxThumb.Size = new System.Drawing.Size(238, 286);
            this.pictureBoxThumb.TabIndex = 21;
            this.pictureBoxThumb.TabStop = false;
            // 
            // buttonFocus
            // 
            this.buttonFocus.Location = new System.Drawing.Point(168, 19);
            this.buttonFocus.Name = "buttonFocus";
            this.buttonFocus.Size = new System.Drawing.Size(75, 23);
            this.buttonFocus.TabIndex = 32;
            this.buttonFocus.Text = "Focus";
            this.buttonFocus.UseVisualStyleBackColor = true;
            this.buttonFocus.Click += new System.EventHandler(this.buttonFocus_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 312);
            this.Controls.Add(this.pictureBoxThumb);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(700, 350);
            this.Name = "MainForm";
            this.Text = "Powerpoint Viewer Demo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumb)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxX;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxY;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxWidth;
        private System.Windows.Forms.TextBox textBoxHeight;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxFilename;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelSlide;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button buttonPrev;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button buttonResume;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TextBox textBoxGoto;
        private System.Windows.Forms.Button buttonGoto;
        private System.Windows.Forms.Button buttonChooseFile;
        private System.Windows.Forms.Button buttonUnblank;
        private System.Windows.Forms.Button buttonBlank;
        private System.Windows.Forms.Button buttonHide;
        private System.Windows.Forms.ListBox listBoxDocuments;
        private System.Windows.Forms.Button buttonMove;
        private System.Windows.Forms.PictureBox pictureBoxThumb;
        private System.Windows.Forms.Button buttonFocus;
    }
}