namespace DerelictCore.BigPeek;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        PickWindowButton = new Button();
        StatusBox = new RichTextBox();
        StatusLabel = new Label();
        SuspendLayout();
        // 
        // PickWindowButton
        // 
        PickWindowButton.Location = new Point(12, 372);
        PickWindowButton.Name = "PickWindowButton";
        PickWindowButton.Size = new Size(760, 177);
        PickWindowButton.TabIndex = 0;
        PickWindowButton.Text = "Pick Button";
        PickWindowButton.UseVisualStyleBackColor = true;
        PickWindowButton.Click += PickWindowButton_Click;
        // 
        // StatusBox
        // 
        StatusBox.Location = new Point(12, 27);
        StatusBox.Name = "StatusBox";
        StatusBox.Size = new Size(760, 339);
        StatusBox.TabIndex = 1;
        StatusBox.Text = "";
        // 
        // StatusLabel
        // 
        StatusLabel.AutoSize = true;
        StatusLabel.Location = new Point(12, 9);
        StatusLabel.Name = "StatusLabel";
        StatusLabel.Size = new Size(39, 15);
        StatusLabel.TabIndex = 2;
        StatusLabel.Text = "Status";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 561);
        Controls.Add(StatusLabel);
        Controls.Add(StatusBox);
        Controls.Add(PickWindowButton);
        Name = "Form1";
        Text = "Big Peek!";
        FormClosing += Form1_FormClosing;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button PickWindowButton;
    private RichTextBox StatusBox;
    private Label StatusLabel;
}