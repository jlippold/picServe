<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.nfi = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ExitApp = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenLogDirectoryToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.IndexFoldersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.WipeCacheToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripProgressBar1 = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ServerStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.Port = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Password = New System.Windows.Forms.TextBox()
        Me.RemoveFolder = New System.Windows.Forms.Button()
        Me.AddFolder = New System.Windows.Forms.Button()
        Me.folderList = New System.Windows.Forms.ListBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.toggleServer = New System.Windows.Forms.Button()
        Me.IP = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.uploadPath = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.ChooseUpload = New System.Windows.Forms.Button()
        Me.UploadPort = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.ContextMenuStrip1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'nfi
        '
        Me.nfi.ContextMenuStrip = Me.ContextMenuStrip1
        Me.nfi.Icon = CType(resources.GetObject("nfi.Icon"), System.Drawing.Icon)
        Me.nfi.Text = "picServ"
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitApp})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(139, 26)
        '
        'ExitApp
        '
        Me.ExitApp.Name = "ExitApp"
        Me.ExitApp.Size = New System.Drawing.Size(138, 22)
        Me.ExitApp.Text = "Quit picServ"
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(514, 24)
        Me.MenuStrip1.TabIndex = 7
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenLogDirectoryToolStripMenuItem, Me.IndexFoldersToolStripMenuItem, Me.WipeCacheToolStripMenuItem, Me.ToolStripSeparator1, Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'OpenLogDirectoryToolStripMenuItem
        '
        Me.OpenLogDirectoryToolStripMenuItem.Name = "OpenLogDirectoryToolStripMenuItem"
        Me.OpenLogDirectoryToolStripMenuItem.Size = New System.Drawing.Size(177, 22)
        Me.OpenLogDirectoryToolStripMenuItem.Text = "Open Log Directory"
        '
        'IndexFoldersToolStripMenuItem
        '
        Me.IndexFoldersToolStripMenuItem.Name = "IndexFoldersToolStripMenuItem"
        Me.IndexFoldersToolStripMenuItem.Size = New System.Drawing.Size(177, 22)
        Me.IndexFoldersToolStripMenuItem.Text = "Refresh Cache"
        '
        'WipeCacheToolStripMenuItem
        '
        Me.WipeCacheToolStripMenuItem.Name = "WipeCacheToolStripMenuItem"
        Me.WipeCacheToolStripMenuItem.Size = New System.Drawing.Size(177, 22)
        Me.WipeCacheToolStripMenuItem.Text = "Wipe Cache"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(174, 6)
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(177, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripProgressBar1, Me.ToolStripStatusLabel1, Me.ServerStatus})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 223)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(514, 22)
        Me.StatusStrip1.SizingGrip = False
        Me.StatusStrip1.TabIndex = 8
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripProgressBar1
        '
        Me.ToolStripProgressBar1.Name = "ToolStripProgressBar1"
        Me.ToolStripProgressBar1.Size = New System.Drawing.Size(150, 16)
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.AutoSize = False
        Me.ToolStripStatusLabel1.Image = CType(resources.GetObject("ToolStripStatusLabel1.Image"), System.Drawing.Image)
        Me.ToolStripStatusLabel1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ToolStripStatusLabel1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(300, 17)
        Me.ToolStripStatusLabel1.Text = " "
        Me.ToolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ServerStatus
        '
        Me.ServerStatus.Image = CType(resources.GetObject("ServerStatus.Image"), System.Drawing.Image)
        Me.ServerStatus.Name = "ServerStatus"
        Me.ServerStatus.Size = New System.Drawing.Size(59, 17)
        Me.ServerStatus.Text = "Offline"
        '
        'Port
        '
        Me.Port.Location = New System.Drawing.Point(231, 62)
        Me.Port.Name = "Port"
        Me.Port.Size = New System.Drawing.Size(112, 20)
        Me.Port.TabIndex = 0
        Me.Port.Text = "0"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(147, 65)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(63, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Server Port:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(154, 42)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(56, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Password:"
        '
        'Password
        '
        Me.Password.Location = New System.Drawing.Point(231, 39)
        Me.Password.Name = "Password"
        Me.Password.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.Password.Size = New System.Drawing.Size(112, 20)
        Me.Password.TabIndex = 2
        '
        'RemoveFolder
        '
        Me.RemoveFolder.Location = New System.Drawing.Point(418, 154)
        Me.RemoveFolder.Name = "RemoveFolder"
        Me.RemoveFolder.Size = New System.Drawing.Size(75, 23)
        Me.RemoveFolder.TabIndex = 6
        Me.RemoveFolder.Text = "Remove"
        Me.RemoveFolder.UseVisualStyleBackColor = True
        '
        'AddFolder
        '
        Me.AddFolder.Location = New System.Drawing.Point(418, 126)
        Me.AddFolder.Name = "AddFolder"
        Me.AddFolder.Size = New System.Drawing.Size(75, 23)
        Me.AddFolder.TabIndex = 5
        Me.AddFolder.Text = "Add"
        Me.AddFolder.UseVisualStyleBackColor = True
        '
        'folderList
        '
        Me.folderList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.folderList.FormattingEnabled = True
        Me.folderList.Location = New System.Drawing.Point(10, 125)
        Me.folderList.Name = "folderList"
        Me.folderList.Size = New System.Drawing.Size(391, 54)
        Me.folderList.TabIndex = 3
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(7, 109)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(73, 13)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "Media Folders"
        '
        'toggleServer
        '
        Me.toggleServer.Location = New System.Drawing.Point(383, 60)
        Me.toggleServer.Name = "toggleServer"
        Me.toggleServer.Size = New System.Drawing.Size(112, 23)
        Me.toggleServer.TabIndex = 10
        Me.toggleServer.Text = "Start Server"
        Me.toggleServer.UseVisualStyleBackColor = True
        '
        'IP
        '
        Me.IP.Enabled = False
        Me.IP.Location = New System.Drawing.Point(12, 50)
        Me.IP.Multiline = True
        Me.IP.Name = "IP"
        Me.IP.Size = New System.Drawing.Size(123, 44)
        Me.IP.TabIndex = 11
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(9, 34)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(51, 13)
        Me.Label4.TabIndex = 12
        Me.Label4.Text = "Server IP"
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "green.png")
        Me.ImageList1.Images.SetKeyName(1, "red.png")
        '
        'uploadPath
        '
        Me.uploadPath.Location = New System.Drawing.Point(102, 190)
        Me.uploadPath.Name = "uploadPath"
        Me.uploadPath.Size = New System.Drawing.Size(218, 20)
        Me.uploadPath.TabIndex = 13
        Me.uploadPath.Text = " "
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(10, 193)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(86, 13)
        Me.Label5.TabIndex = 14
        Me.Label5.Text = "Upload Directory"
        '
        'ChooseUpload
        '
        Me.ChooseUpload.Location = New System.Drawing.Point(326, 188)
        Me.ChooseUpload.Name = "ChooseUpload"
        Me.ChooseUpload.Size = New System.Drawing.Size(75, 23)
        Me.ChooseUpload.TabIndex = 15
        Me.ChooseUpload.Text = "Browse"
        Me.ChooseUpload.UseVisualStyleBackColor = True
        '
        'UploadPort
        '
        Me.UploadPort.Location = New System.Drawing.Point(231, 86)
        Me.UploadPort.Name = "UploadPort"
        Me.UploadPort.Size = New System.Drawing.Size(112, 20)
        Me.UploadPort.TabIndex = 16
        Me.UploadPort.Text = "0"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(147, 89)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(66, 13)
        Me.Label6.TabIndex = 17
        Me.Label6.Text = "Upload Port:"
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 1800000
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(514, 245)
        Me.Controls.Add(Me.UploadPort)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.ChooseUpload)
        Me.Controls.Add(Me.uploadPath)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.IP)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.toggleServer)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.folderList)
        Me.Controls.Add(Me.AddFolder)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Controls.Add(Me.RemoveFolder)
        Me.Controls.Add(Me.Port)
        Me.Controls.Add(Me.Password)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form1"
        Me.Text = "picServ"
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents nfi As System.Windows.Forms.NotifyIcon
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripProgressBar1 As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents IndexFoldersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Password As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Port As System.Windows.Forms.TextBox
    Friend WithEvents folderList As System.Windows.Forms.ListBox
    Friend WithEvents AddFolder As System.Windows.Forms.Button
    Friend WithEvents RemoveFolder As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents toggleServer As System.Windows.Forms.Button
    Friend WithEvents IP As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents ServerStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ImageList1 As System.Windows.Forms.ImageList
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ExitApp As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenLogDirectoryToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents uploadPath As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents ChooseUpload As System.Windows.Forms.Button
    Friend WithEvents UploadPort As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents WipeCacheToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
