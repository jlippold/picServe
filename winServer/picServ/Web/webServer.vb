Imports System.Net
Imports System.Globalization
Imports System.ComponentModel
Imports System.Collections.Specialized
Imports System.Text
Imports System.IO
Imports System.Threading

Public Class WebServer
    Public Shared isRunning As Boolean = False
    Public Shared worker As BackgroundWorker
    Public Shared listener As HttpListener

    Public Shared Sub ValidateAndBegin()
        Form1.SetControlStateForServer()
        If WebServer.ValidateSettings Then
            WebServer.saveSettings()
            WebServer.start()
            uploadServer.start()
        End If
    End Sub

    Public Shared Sub saveSettings()
        My.Settings.APIKey = Form1.Password.Text
        My.Settings.Port = Form1.Port.Text
        My.Settings.UploadPort = Form1.UploadPort.Text

        Dim folders As String = ""
        For Each f In Form1.folderList.Items
            If folders = "" Then
                folders &= f
            Else
                folders &= ";" & f
            End If
        Next
        My.Settings.Folders = folders
        My.Settings.UploadPath = Form1.uploadPath.Text
        My.Settings.Save()
    End Sub

    Public Shared Function ValidateSettings() As Boolean

        If IsNumeric(Form1.Port.Text) = False Then
            MsgBox("Invalid Server Port", vbExclamation)
            Return False
        End If

        If IsNumeric(Form1.UploadPort.Text) = False Then
            MsgBox("Invalid Upload Port", vbExclamation)
            Return False
        End If

        If (Form1.Password.Text.Length) < 6 Then
            MsgBox("Password must be atleast 6 characters", vbExclamation)
            Return False
        End If

        If (Form1.folderList.Items.Count = 0) Then
            MsgBox("Please add alteast one media folder", vbExclamation)
            Return False
        End If

        If (Directory.Exists(Form1.uploadPath.Text) = False) Then
            MsgBox("Upload directory does not exist", vbExclamation)
            Return False
        End If
        Return True
    End Function

    Public Shared Sub endServer()
        If (worker IsNot Nothing) Then
            worker.CancelAsync()
            worker.Dispose()
        End If
        WebServer.isRunning = False
    End Sub

    Public Shared Sub start()

        If (worker IsNot Nothing) Then
            worker.CancelAsync()
            worker.Dispose()
        End If

        worker = New BackgroundWorker()
        worker.WorkerSupportsCancellation = True
        AddHandler worker.DoWork, AddressOf Worker_DoWork
        worker.RunWorkerAsync()
        WebServer.isRunning = True
        Form1.SetControlStateForServer()

    End Sub

    Public Shared Sub Worker_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)
        Init()
    End Sub

    Public Shared Sub Init()
        ProcessRequests()
    End Sub


    Public Shared Sub requestWait(ByVal ar As IAsyncResult)
        If Not listener.IsListening Then
            Return
        End If
        Dim c = listener.EndGetContext(ar)
        listener.BeginGetContext(AddressOf requestWait, Nothing)
        webRouting.Route(c)
    End Sub

    Public Shared Sub ProcessRequests()
        If Not System.Net.HttpListener.IsSupported Then
            MsgBox("HTTP Listener is not supported.", vbExclamation)
            WebServer.isRunning = False
            Form1.SetControlStateForServer()
            Exit Sub
        End If

        WebServer.isRunning = True
        Form1.SetControlStateForServer()

        ServicePointManager.DefaultConnectionLimit = 500
        ServicePointManager.Expect100Continue = False
        ServicePointManager.MaxServicePoints = 500

        System.Threading.ThreadPool.SetMaxThreads(50, 1000)
        System.Threading.ThreadPool.SetMinThreads(50, 50)


        listener = New HttpListener()
        listener.Prefixes.Add("http://*:" & My.Settings.Port & "/")
        listener.IgnoreWriteExceptions = True

        Try
            listener.Start()
            listener.BeginGetContext(AddressOf requestWait, Nothing)

            Do While WebServer.isRunning
                Try
                    ' Note: GetContext blocks while waiting for a request.
                    'Dim context As HttpListenerContext = listener.GetContext()
                    'webRouting.Route(context)


                    'Dim context As HttpListenerContext = listener.GetContext()
                    'ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf webRouting.Route), context)

                Catch ex As HttpListenerException
                    Util.log(ex.Message)
                Finally

                End Try
            Loop
        Catch ex As HttpListenerException
            Util.log(ex.Message)
        Finally
            ' Stop listening for requests.
            listener.Close()
        End Try
    End Sub

    Public Shared Sub writeText(ByVal str As String, response As HttpListenerResponse, Optional ByVal contentType As String = "")
        Try

            If contentType <> "" Then
                response.ContentType = contentType
            End If

            Dim responseString As String = str
            Dim buffer() As Byte = System.Text.Encoding.UTF8.GetBytes(responseString)
            response.ContentLength64 = buffer.Length
            Dim output As System.IO.Stream = response.OutputStream
            output.Write(buffer, 0, buffer.Length)

        Catch ex As Exception

        End Try


        If response IsNot Nothing Then
            response.Close()
        End If
    End Sub

    Public Shared Sub writeBinary(ByVal content() As Byte, response As HttpListenerResponse, contentType As String)
        Try
            response.ContentType = contentType
            response.ContentLength64 = content.Length
            response.OutputStream.Write(content, 0, content.Length)
        Catch ex As Exception

        End Try

        If response IsNot Nothing Then
            response.Close()
        End If
    End Sub

    Public Shared Sub writeImageFromPath(ByVal path As String, response As HttpListenerResponse)
        response.ContentType = "image/jpeg"
        Dim content() As Byte = My.Computer.FileSystem.ReadAllBytes(path)
        response.ContentLength64 = content.Length
        Try
            response.OutputStream.Write(content, 0, content.Length)
        Catch ex As Exception

        End Try

        If response IsNot Nothing Then
            response.Close()
        End If
    End Sub

    Public Shared Sub writeFileFromPath(ByVal p As String, response As HttpListenerResponse, ByVal mimetype As String, Optional ByVal useContentDisposition As Boolean = True)
        response.ContentType = mimetype
        If useContentDisposition Then
            response.Headers.Add("Content-Disposition: attachment; filename=""" & System.IO.Path.GetFileName(p) & """")
        End If

        Dim content() As Byte = My.Computer.FileSystem.ReadAllBytes(p)
        response.ContentLength64 = content.Length
        response.OutputStream.Write(content, 0, content.Length)
        If response IsNot Nothing Then
            response.Close()
        End If
    End Sub

    Public Shared Sub writeVideoFromPath(ByVal p As String, response As HttpListenerResponse, ByVal mimetype As String, rangeHeader As String)

        Dim file_info As New System.IO.FileInfo(p)


        Dim fSize As Long = (New System.IO.FileInfo(p)).Length
        Dim startbyte As Long = 0
        Dim endbyte As Long = fSize - 1
        Dim statuscode As Integer = 200

        'Reset and set response headers. The Accept-Ranges Bytes header is important to allow
        'resuming videos.
        response.AddHeader("Accept-Ranges", "bytes")

        If rangeHeader <> "" Then
            'Get the actual byte range from the range header string, and set the starting byte.
            Dim range As String() = rangeHeader.Split(New Char() {"="c, "-"c})
            startbyte = Convert.ToInt64(range(1))
            If range.Length > 2 AndAlso range(2) <> "" Then
                endbyte = Convert.ToInt64(range(2))
            End If
            If startbyte <> 0 OrElse endbyte <> fSize - 1 OrElse range.Length > 2 AndAlso range(2) = "" Then
                statusCode = 206                                  
            End If
        End If

        Dim desSize As Long = endbyte - startbyte + 1
        response.StatusCode = statusCode
        response.ContentType = "video/quicktime"
        response.ContentLength64 = desSize
        response.Headers.Add("Content-Range", String.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize))

        'Write the video file to the output stream, starting from the specified byte position.
        'Try
        'Dim bytearr() As Byte = File.ReadAllBytes(p)
        'response.OutputStream.Write(bytearr, startbyte, desSize)

        Dim buffer = New Byte(1023) {}

        Try
            Using fs = File.OpenRead(p)
                fs.Position = startbyte
                Dim endbytes As Long = buffer.Length
                If endbytes > desSize Then
                    endbytes = desSize
                End If
                Dim read As Integer
                While (InlineAssignHelper(read, fs.Read(buffer, 0, endbytes))) > 0
                    response.OutputStream.Write(buffer, 0, read)
                End While
            End Using
            response.OutputStream.Close()
        Catch ex As Exception
            Util.log(ex.Message)
        End Try



        ' Catch ex As Exception

        ' End Try

    End Sub

   
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function

    Public Shared Sub writeImageFromByteArray(ByVal content() As Byte, response As HttpListenerResponse)
        response.ContentType = "image/jpeg"
        response.ContentLength64 = content.Length
        response.OutputStream.Write(content, 0, content.Length)
        If response IsNot Nothing Then
            response.Close()
        End If
    End Sub

    Sub poo()

    End Sub
End Class