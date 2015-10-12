namespace FrontListNames
{
    partial class ListName
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
            this.addbutton = new System.Windows.Forms.Button();
            this.addtextbox = new System.Windows.Forms.TextBox();
            this.listbutton = new System.Windows.Forms.Button();
            this.listextbox = new System.Windows.Forms.TextBox();
            this.clearbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // addbutton
            // 
            this.addbutton.Location = new System.Drawing.Point(12, 12);
            this.addbutton.Name = "addbutton";
            this.addbutton.Size = new System.Drawing.Size(75, 23);
            this.addbutton.TabIndex = 0;
            this.addbutton.Text = "Add";
            this.addbutton.UseVisualStyleBackColor = true;
            this.addbutton.Click += new System.EventHandler(this.addbutton_Click);
            // 
            // addtextbox
            // 
            this.addtextbox.Location = new System.Drawing.Point(104, 14);
            this.addtextbox.Name = "addtextbox";
            this.addtextbox.Size = new System.Drawing.Size(266, 20);
            this.addtextbox.TabIndex = 1;
            this.addtextbox.TextChanged += new System.EventHandler(this.addtextbox_TextChanged);
            // 
            // listbutton
            // 
            this.listbutton.Location = new System.Drawing.Point(12, 42);
            this.listbutton.Name = "listbutton";
            this.listbutton.Size = new System.Drawing.Size(75, 23);
            this.listbutton.TabIndex = 2;
            this.listbutton.Text = "List";
            this.listbutton.UseVisualStyleBackColor = true;
            this.listbutton.Click += new System.EventHandler(this.listbutton_Click);
            // 
            // listextbox
            // 
            this.listextbox.Location = new System.Drawing.Point(104, 44);
            this.listextbox.Multiline = true;
            this.listextbox.Name = "listextbox";
            this.listextbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.listextbox.Size = new System.Drawing.Size(266, 241);
            this.listextbox.TabIndex = 3;
            this.listextbox.Text = "\r\n";
            this.listextbox.TextChanged += new System.EventHandler(this.listextbox_TextChanged);
            // 
            // clearbutton
            // 
            this.clearbutton.Location = new System.Drawing.Point(13, 261);
            this.clearbutton.Name = "clearbutton";
            this.clearbutton.Size = new System.Drawing.Size(75, 23);
            this.clearbutton.TabIndex = 4;
            this.clearbutton.Text = "clear";
            this.clearbutton.UseVisualStyleBackColor = true;
            this.clearbutton.Click += new System.EventHandler(this.clearbutton_Click);
            // 
            // ListName
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 297);
            this.Controls.Add(this.clearbutton);
            this.Controls.Add(this.listextbox);
            this.Controls.Add(this.listbutton);
            this.Controls.Add(this.addtextbox);
            this.Controls.Add(this.addbutton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ListName";
            this.Text = "Lista Nomes";
            this.Load += new System.EventHandler(this.ListNames_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button addbutton;
        private System.Windows.Forms.TextBox addtextbox;
        private System.Windows.Forms.Button listbutton;
        private System.Windows.Forms.Button clearbutton;
        public System.Windows.Forms.TextBox listextbox;
    }
}

