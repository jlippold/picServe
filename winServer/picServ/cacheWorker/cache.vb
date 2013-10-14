Imports System.ComponentModel
Imports System.IO
Imports System.Web
Imports System.Net
Imports System.Drawing
Imports System.Drawing.Imaging
Imports Ionic.Zip
Imports System.Text
Imports System.Data.SqlClient
Imports Microsoft.WindowsAPICodePack.Shell
Imports System.Data.SQLite

Public Class cache

    Public Shared worker As BackgroundWorker
    Public Shared isRunning = False
    Public Shared folders As List(Of String)
    Public Shared counter As Int64
    Public Shared Sub cancelWorker()
        worker.CancelAsync()
    End Sub
    Public Shared Sub startWorker()
        cache.isRunning = True
        Form1.ToolStripStatusLabel1.Text = "Caching Media..."
        Form1.ToolStripStatusLabel1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText

        worker = New BackgroundWorker()
        worker.WorkerReportsProgress = True
        worker.WorkerSupportsCancellation = True
        AddHandler worker.DoWork, AddressOf Worker_DoWork
        AddHandler worker.ProgressChanged, AddressOf Worker_ProgressChanged
        AddHandler worker.RunWorkerCompleted, AddressOf Worker_RunWorkerCompleted
        worker.RunWorkerAsync()
    End Sub

    Public Shared Sub Worker_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs)
        Form1.ToolStripProgressBar1.Maximum = folders.Count
        Form1.ToolStripProgressBar1.Minimum = 0
        Form1.ToolStripStatusLabel1.Text = (counter & "/" & folders.Count & " folders remaining. ( " & folders.Item(counter - 1) & " " & e.ProgressPercentage.ToString & "% complete )")
        Form1.ToolStripProgressBar1.Value = counter
    End Sub

    Public Shared Sub Worker_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        If e.Cancelled = True Then
            cache.ClearCache()
        End If
        cache.isRunning = False
        Form1.ToolStripStatusLabel1.Text = "Caching Complete"
        Form1.ToolStripProgressBar1.Value = 0
        Form1.ToolStripStatusLabel1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
    End Sub

    Public Shared Sub ClearCache()
        db.ExecuteNonQuery("DELETE FROM FolderProps")
        Try
            System.IO.Directory.Delete(Util.getAppDataPath() & "\base64img\", True)
            System.IO.Directory.Delete(Util.getAppDataPath() & "\zips\", True)
        Catch ex As Exception

        End Try
        MsgBox("Cache Deleted")
    End Sub

    Public Shared Sub Worker_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)

        db.killAndCreateIfDoesntExist()

        folders = New List(Of String)
        For Each f As String In picUtil.getBaseDirectories()
            GetAllFolders(f)
        Next

        counter = 0

        For Each f In folders



            Dim insertStatements As List(Of List(Of String))
            Dim sqlChunk As List(Of String)
            insertStatements = New List(Of List(Of String))
            sqlChunk = New List(Of String)

            counter += 1

            worker.ReportProgress(0)



            Dim zipDestination As String = getZipDirectory() & getZipName(f) & ".zip"

            Dim fl As Dictionary(Of String, String) = getFiles(f)
            Dim blnHasFile As Boolean = False
            Dim x As Integer = 0
            Dim tot As Integer = fl.Count
            Dim di As DirectoryInfo = New DirectoryInfo(f)

            'Check if folder has changed since last run
            Dim flLastModified As DateTime = di.LastWriteTime
            Dim flLastCached As DateTime = #1/1/1971#
            Using conn As New SQLiteConnection(db.getConnStr)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "SELECT DateModified FROM FolderProps Where FolderName = @f"
                cmd.Parameters.Add(New SQLiteParameter("@f", f))
                Dim rst = cmd.ExecuteReader()
                If rst.HasRows Then
                    While rst.Read()
                        flLastCached = DateTime.Parse(rst("DateModified"))
                    End While
                End If
                rst.Close()
                conn.Close()
            End Using

            If flLastModified.ToString("yyyy-MM-dd HH:mm:ss") = flLastCached.ToString("yyyy-MM-dd HH:mm:ss") Then
                log("Skipping: " & f)
                Continue For 'break already cached
            End If

            log("Working " & counter & " of " & folders.Count & ": " & f)
            db.ExecuteNonQuery("DELETE FROM Pictures Where FilePath = '" & sql_safe(f) & "'")

            For Each pair As KeyValuePair(Of String, String) In fl

                If worker.CancellationPending = True Then
                    e.Cancel = True
                    Exit Sub
                End If

                x = x + 1
                'log("Working " & x & " of " & tot)
                worker.ReportProgress((x / tot) * 100)
                Dim fPath As String = pair.Key
                Dim webPath As String = pair.Value

                If sqlChunk.Count > 490 Then
                    insertStatements.Add(sqlChunk)
                    sqlChunk = New List(Of String)
                End If

                sqlChunk.Add(getFileProps(pair.Key))

                Dim cacheName As String = getCacheDirectory() & webPath & ".cache"

                If File.Exists(cacheName) = False Then

                    If System.IO.Path.GetExtension(fPath).ToLower = ".mov" Then

                        Try

                            Dim Duration As String = TimeSpan.FromSeconds(GetDuration(fPath) * 0.3).ToString '00:00:01
                            Dim jpegname As String = Util.getTempDataPath() & "\" & Path.GetFileNameWithoutExtension(fPath) & ".jpg"
                            Util.killFile(jpegname)

                            Dim myProcess As New Process()
                            Dim processinfo = New ProcessStartInfo()
                            processinfo.WorkingDirectory = My.Application.Info.DirectoryPath & "\ffmpeg\"
                            processinfo.FileName = "ffmpeg.exe"
                            processinfo.Arguments = "-i """ & fPath & """ -vf scale='min(200,iw)':-1 -ss " & Duration & " -f image2 -y -vframes 1 """ & jpegname & """"
                            processinfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                            processinfo.UseShellExecute = True
                            myProcess.StartInfo = processinfo
                            myProcess.Start()
                            myProcess.WaitForExit()
                            myProcess = Nothing

                            Dim newPic As New System.Drawing.Bitmap(jpegname)
                            Dim imgStream As MemoryStream = New MemoryStream()
                            newPic.Save(imgStream, ImageFormat.Png)
                            newPic.Dispose()
                            
                            Dim byteArray As Byte() = imgStream.ToArray()
                            Try
                                File.WriteAllText(cacheName, "data:image/png;base64," & Convert.ToBase64String(byteArray))
                            Catch ex2 As Exception
                                log("base64 error: " & ex2.Message)
                            End Try

                            newPic.Dispose()
                            imgStream.Close()
                            imgStream.Dispose()
                            Util.killFile(jpegname)

                        Catch ex As Exception

                        End Try

                    Else
                        Try

                            Dim oldPic As New System.Drawing.Bitmap(fPath)

                            Dim er As New Goheer.EXIF.EXIFextractor(oldPic, ",")
                            If er("Orientation") = "8" Then
                                oldPic.RotateFlip(RotateFlipType.Rotate270FlipNone)
                            End If
                            If er("Orientation") = "3" Then
                                oldPic.RotateFlip(RotateFlipType.Rotate180FlipNone)
                            End If
                            If er("Orientation") = "6" Then
                                oldPic.RotateFlip(RotateFlipType.Rotate90FlipNone)
                            End If

                            Dim newPic As System.Drawing.Bitmap = imaging.resize(oldPic, 200, 200)
                            oldPic.Dispose()
                            Dim imgStream As MemoryStream = New MemoryStream()
                            newPic.Save(imgStream, ImageFormat.Jpeg)
                            imgStream.Close()
                            imgStream.Dispose()
                            newPic.Dispose()

                            Dim byteArray As Byte() = imgStream.ToArray()
                            Try
                                File.WriteAllText(cacheName, "data:image/jpg;base64," & Convert.ToBase64String(byteArray))
                            Catch ex2 As Exception
                                log("base64 error: " & ex2.Message)
                            End Try
                        Catch ex As Exception
                            log("open file error: " & ex.Message)
                        End Try

                    End If


                End If
                blnHasFile = True
            Next

            If sqlChunk.Count > 0 Then
                insertStatements.Add(sqlChunk)
                sqlChunk = New List(Of String)
            End If

            For Each chunk In insertStatements
                db.ExecuteNonQuery("Insert into Pictures ( " & _
                    "FullName, DateTaken, Dimensions, Camera, FileName, FilePath " & _
                    ") VALUES " & String.Join(",", chunk))
            Next

            'zip the folder
            Util.killFile(zipDestination)

            If blnHasFile Then
                Try
                    Using zip As ZipFile = New ZipFile()
                        For Each pair As KeyValuePair(Of String, String) In fl
                            If File.Exists(getCacheDirectory() & pair.Value & ".cache") Then
                                zip.AddItem(getCacheDirectory() & pair.Value & ".cache", "")
                            End If
                        Next
                        zip.Save(zipDestination)
                    End Using
                Catch ex As Exception
                    log("Zip Error: " & ex.Message)
                End Try
            End If

            'Store info about the folder

            db.ExecuteNonQuery("DELETE FROM FolderProps Where FolderName = '" & sql_safe(f) & "'")
            db.ExecuteNonQuery("INSERT INTO FolderProps (FolderName, DateModified, DateIndexed) VALUES ('" & sql_safe(f) & "','" & di.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss") & "','" & DateTime.Now().ToString("yyyy-MM-dd HH:mm:ss") & "')")
            Try
                Directory.Delete(Util.getTempDataPath(), True)
            Catch ex As Exception

            End Try

        Next



        log("Complete")


    End Sub

    Public Shared Sub GetAllFolders(strPath As String)
        Dim objRoot As New DirectoryInfo(strPath)
        If objRoot.Exists Then
            folders.Add(strPath.ToLower)
            For Each subf As String In System.IO.Directory.GetDirectories(strPath)
                If Path.GetFileName(subf).StartsWith(".") = False Then
                    folders.Add(subf.ToLower)
                    GetAllFolders(subf.ToLower)
                End If
            Next
        End If
    End Sub

    Public Shared Function getCacheDirectory() As String
        Dim p As String = Util.getAppDataPath() & "\base64img\"
        If Directory.Exists(p) = False Then
            Directory.CreateDirectory(p)
        End If
        Return p
    End Function

    Public Shared Function getZipDirectory() As String
        Dim p As String = Util.getAppDataPath() & "\zips\"
        If Directory.Exists(p) = False Then
            Directory.CreateDirectory(p)
        End If
        Return p
    End Function

    Public Shared Sub log(ByVal str As String)
        Dim logPath As String = String.Format(Util.getAppDataPath & "\Logs")
        If Directory.Exists(logPath) = False Then
            Directory.CreateDirectory(logPath)
        End If
        Dim strFile As String = String.Format(logPath & "\{0}_WorkerLog.txt", DateTime.Today.ToString("yyyy-MMM-dd"))
        File.AppendAllText(strFile, String.Format("{0}: {1}{2}", DateTime.Now, str, Environment.NewLine))
    End Sub

    Public Shared Function getWebName(strPath As String) As String
        strPath = strPath.Replace("/", "//")
        Return Microsoft.JScript.GlobalObject.escape(strPath)
    End Function

    Public Shared Function getZipName(strPath As String) As String
        strPath = strPath.Replace("\", ".")
        strPath = strPath.Replace(":", "")
        Return strPath
    End Function

    Public Shared Function getFiles(strPath As String) As Dictionary(Of String, String)
        Dim files() As String
        Dim lstFiles As New Dictionary(Of String, String)
        files = Directory.GetFiles(strPath, "*.*", SearchOption.TopDirectoryOnly)
        For Each f As String In files
            Dim ext As String = Path.GetExtension(f).ToLower()
            Dim fname As String = Path.GetFileNameWithoutExtension(f).ToLower()

            If ext = ".jpg" Or ext = ".png" Or ext = ".mov" Then
                If (fname.StartsWith(".") = False) Then
                    lstFiles.Add(f.ToLower, getWebName(f).ToLower)
                End If
            End If
        Next
        Return lstFiles
    End Function

    Public Shared Function getFileProps(ByVal fileName As String) As String
        Dim dt As DateTime = File.GetCreationTime(fileName)
        dt = GetDateTaken(fileName, dt)
        Dim info As New FileInfo(fileName)

        Dim output As String = "("
        output &= "'" & sql_safe(fileName) & "', "
        output &= "'" & dt.ToString("yyyy-MM-dd HH:mm:ss") & "', "
        output &= "'" & sql_safe(getShellVariable(fileName, "Dimensions")) & "', "
        output &= "'" & sql_safe(getShellVariable(fileName, "Camera")) & "', "
        output &= "'" & sql_safe(Path.GetFileName(fileName)) & "', "
        output &= "'" & sql_safe(Path.GetDirectoryName(fileName)) & "'"
        output &= ")"
        Return output

    End Function

    Public Shared Function sql_safe(ByVal val As String) As String
        val = val.Replace("'", "''")
        Return val
    End Function

    Public Shared Function GetDateTaken(ByVal f As String, defDate As DateTime) As DateTime
        Dim so As ShellFile = ShellFile.FromFilePath(f)
        Dim d As String = so.Properties.System.Photo.DateTaken.Value.ToString()
        Dim n As Date
        If Date.TryParse(d, n) Then
            defDate = n 'DateTime.Parse(d)
        End If
        so = Nothing
        Return defDate
    End Function

    Public Shared Function getShellVariable(ByVal f As String, var As String) As String
        Dim so As ShellFile = ShellFile.FromFilePath(f)
        Dim out As String = ""
        Try
            If var = "Dimensions" Then
                out = so.Properties.System.Image.Dimensions.Value.ToString()
            End If

            If var = "Camera" Then
                out = so.Properties.System.Photo.CameraManufacturer.Value.ToString() & " "
                out &= so.Properties.System.Photo.CameraModel.Value.ToString()
            End If
        Catch ex As Exception

        End Try

        so = Nothing

        Return out
    End Function

    Public Shared Function GetDuration(ByVal f As String) As Integer
        Dim nanoseconds As Double = 0
        Try
            If File.Exists(f) Then
                Dim so As ShellFile = ShellFile.FromFilePath(f)
                Double.TryParse(so.Properties.System.Media.Duration.Value.ToString(), nanoseconds)
            End If
        Catch ex As Exception

        End Try

        Return Math.Round((nanoseconds * 0.0001 / 1000), 1)
    End Function

End Class
