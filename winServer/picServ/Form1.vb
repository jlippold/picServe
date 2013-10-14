
Public Class Form1
    Public firstLoad As Boolean = True

    Private Sub Form1_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        'hide app
        e.Cancel = True
        Me.WindowState = FormWindowState.Minimized
        Me.Visible = False
    End Sub

    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        For Each ip As Net.IPAddress In System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName).AddressList
            ' Eg: Display with message box
            If (ip.ToString.Contains(":") = False) Then
                If Me.IP.Text <> "" Then
                    Me.IP.Text &= vbCrLf
                End If
                Me.IP.Text &= ip.ToString
            End If
        Next

        Me.Port.Text = My.Settings.Port
        Me.Password.Text = My.Settings.APIKey
        Me.uploadPath.Text = My.Settings.UploadPath
        Me.UploadPort.Text = My.Settings.UploadPort

        Dim arr() As String = My.Settings.Folders.Split(";")
        For Each f In arr
            If System.IO.Directory.Exists(f) Then
                Me.folderList.Items.Add(f)
            End If
        Next

        Me.ToolStripStatusLabel1.DisplayStyle = ToolStripItemDisplayStyle.None
        nfi.Visible = True
    End Sub

    Private Sub Form1_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        If firstLoad Then
            If WebServer.ValidateSettings Then
                WebServer.ValidateAndBegin()
                triggerCache()
                Me.Close()
            End If
        End If
        firstLoad = False
    End Sub

    Private Sub IndexFoldersToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles IndexFoldersToolStripMenuItem.Click
        cache.startWorker()
    End Sub

    Public Shared Sub SetIcon(ByVal ServerOnline As Boolean)
        If ServerOnline Then
            Form1.ServerStatus.Text = "Online"
            Form1.ServerStatus.Image = Form1.ImageList1.Images.Item("green.png")
        Else
            Form1.ServerStatus.Text = "Offline"
            Form1.ServerStatus.Image = Form1.ImageList1.Images.Item("red.png")
        End If
    End Sub

    Public Shared Sub SetControlStateForServer()

        Dim serverOnline As Boolean = WebServer.isRunning
        SetIcon(serverOnline)
        Form1.Port.Enabled = Not (serverOnline)
        Form1.Password.Enabled = Not (serverOnline)
        Form1.AddFolder.Enabled = Not (serverOnline)
        Form1.RemoveFolder.Enabled = Not (serverOnline)
        Form1.folderList.Enabled = Not (serverOnline)
        Form1.ChooseUpload.Enabled = Not (serverOnline)
        Form1.uploadPath.Enabled = Not (serverOnline)
        Form1.UploadPort.Enabled = Not (serverOnline)

        If serverOnline Then
            Form1.toggleServer.Text = "Stop Server"
        Else
            Form1.toggleServer.Text = "Start Server"
        End If

        Application.DoEvents()
        Form1.Refresh()

    End Sub
    Private Sub toggleServer_Click(sender As System.Object, e As System.EventArgs) Handles toggleServer.Click
        If WebServer.isRunning Then
            WebServer.endServer()
            uploadServer.endServer()
        Else
            WebServer.ValidateAndBegin()
            Timer1_Tick(Nothing, Nothing)
        End If
        SetControlStateForServer()
    End Sub

    Private Sub AddFolder_Click(sender As System.Object, e As System.EventArgs) Handles AddFolder.Click
        Dim BrowseFolder As New FolderBrowserDialog
        BrowseFolder.ShowDialog()
        If System.IO.Directory.Exists(BrowseFolder.SelectedPath) Then
            Me.folderList.Items.Add(BrowseFolder.SelectedPath)
        End If
    End Sub

    Private Sub RemoveFolder_Click(sender As System.Object, e As System.EventArgs) Handles RemoveFolder.Click
        If Me.folderList.SelectedIndex > -1 Then
            Me.folderList.Items.RemoveAt(Me.folderList.SelectedIndex)
        Else
            MsgBox("Please select a folder to remove", vbInformation)
        End If
    End Sub

    Private Sub ExitApp_Click(sender As Object, e As System.EventArgs) Handles ExitApp.Click
        nfi.Visible = False
        End
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        nfi.Visible = False
        End
    End Sub

    Private Sub OpenLogDirectoryToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenLogDirectoryToolStripMenuItem.Click
        Process.Start("explorer.exe", Util.getAppDataPath())
    End Sub

    Private Sub ChooseUpload_Click(sender As System.Object, e As System.EventArgs) Handles ChooseUpload.Click
        Dim BrowseFolder As New FolderBrowserDialog
        BrowseFolder.ShowDialog()
        If System.IO.Directory.Exists(BrowseFolder.SelectedPath) Then
            Me.uploadPath.Text = BrowseFolder.SelectedPath
        End If
    End Sub

    Private Sub Timer1_Tick(sender As System.Object, e As System.EventArgs) Handles Timer1.Tick
        triggerCache()
    End Sub
    Public Sub triggerCache()
        If cache.isRunning = False And Me.folderList.Items.Count > 0 Then
            cache.startWorker()
        End If
    End Sub

    Private Sub nfi_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles nfi.MouseClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        Me.Activate()
    End Sub

    Private Sub WipeCacheToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles WipeCacheToolStripMenuItem.Click
        If cache.isRunning Then
            cache.cancelWorker()
            Exit Sub
        End If

        cache.ClearCache()

    End Sub


End Class
