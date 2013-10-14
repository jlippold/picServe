Imports System.Net
Imports System.Globalization
Imports System.ComponentModel
Imports System.Collections.Specialized
Imports System.Text
Imports System.IO
Imports System.Web

Public Class uploadServer
    Public Shared isRunning As Boolean = False
    Public Shared worker As BackgroundWorker

    Public Shared Sub start()
        If (worker IsNot Nothing) Then
            worker.CancelAsync()
            worker.Dispose()
        End If
        worker = New BackgroundWorker()
        worker.WorkerSupportsCancellation = True
        AddHandler worker.DoWork, AddressOf Worker_DoWork
        worker.RunWorkerAsync()
        uploadServer.isRunning = True
        Form1.SetControlStateForServer()
    End Sub

    Public Shared Sub Worker_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)
        ProcessRequests()
    End Sub

    Public Shared Sub ProcessRequests()
        If Not System.Net.HttpListener.IsSupported Then
            Exit Sub
        End If

        uploadServer.isRunning = True
        ServicePointManager.DefaultConnectionLimit = 500
        ServicePointManager.Expect100Continue = False
        ServicePointManager.MaxServicePoints = 500

        Dim listener As System.Net.HttpListener = New System.Net.HttpListener()
        listener.Prefixes.Add("http://*:" & My.Settings.UploadPort & "/")

        Try
            listener.Start()
            Do While uploadServer.isRunning
                Try
                    ' Note: GetContext blocks while waiting for a request.
                    Dim context As HttpListenerContext = listener.GetContext()
                    uploadServer.Route(context)
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

    Public Shared Sub Route(ByVal context As HttpListenerContext)
        Dim url As String = context.Request.Url.ToString
        Dim response As HttpListenerResponse = context.Response
        Dim uri As New Uri(url)
        Dim webPath As String = uri.AbsolutePath.ToLower
        Dim query As String = uri.Query
        Dim QueryString As NameValueCollection = HttpUtility.ParseQueryString(query)
        If Auth.validate(QueryString("Key")) = False Then
            uploadServer.writeText("Unauthorized", response)
            Exit Sub
        End If
        QueryString("HOST") = uri.Host & ":" & uri.Port
        Select Case webPath
            Case "/upload/"
                uploadServer.uploadFile(QueryString, response, context)
            Case "/cacherefresh/"
                If cache.isRunning = False And Form1.folderList.Items.Count > 0 Then
                    cache.startWorker()
                End If
                uploadServer.writeText("done", response)
            Case Else
                uploadServer.writeText("", response)
        End Select
    End Sub

    Public Shared Sub uploadFile(ByVal QueryString As NameValueCollection, response As HttpListenerResponse, context As HttpListenerContext)
        Util.log("Uploaded " & QueryString("FileName"))
        Dim dir As String = makeUploadDirectory(QueryString)
        Dim saveTo As String = dir & "\" & QueryString("FileName")
        uploadServer.SaveFile(context.Request.ContentEncoding, uploadServer.GetBoundary(context.Request.ContentType), context.Request.InputStream, saveTo)
        uploadServer.writeText("Success", response)
    End Sub

    Public Shared Function makeUploadDirectory(ByVal QueryString As NameValueCollection) As String
        Dim p As String = My.Settings.UploadPath
        If QueryString("Device") <> "" Then
            p &= "\" & QueryString("Device")
            If Directory.Exists(p) = False Then
                Directory.CreateDirectory(p)
            End If
        End If
        p = getUploadSubDirectory(p)
        Return p
    End Function

    Public Shared Function getUploadSubDirectory(ByVal p As String) As String
        Dim root As String() = System.IO.Directory.GetDirectories(p)
        System.Array.Sort(Of String)(root)
        System.Array.Reverse(root)
        Dim subfolder As String = ""
        For Each s As String In root 'try to find subfolder to put pics
            Dim subitems As Int16 = Directory.GetFiles(s, "*.*", SearchOption.TopDirectoryOnly).Length()
            If subitems < 250 Then
                subfolder = Path.GetFileName(s)
            End If
        Next

        If subfolder = "" Then
            'Make new upload folder
            subfolder = DateTime.Now().ToString("yyyy-MM")
            If Directory.Exists(p & "\" & subfolder) Then
                subfolder &= "_1"
            End If
        End If

        subfolder = p & "\" & subfolder
        If Directory.Exists(subfolder) = False Then
            Directory.CreateDirectory(subfolder)
        End If

        Return subfolder
    End Function

    Public Shared Function GetBoundary(c As String) As String
        Return "--" + c.Split(";")(2).Split("=")(1)
    End Function
    Public Shared Sub writeText(ByVal str As String, response As HttpListenerResponse, Optional ByVal contentType As String = "")
        If contentType <> "" Then
            response.ContentType = contentType
        End If
        Dim responseString As String = str
        Dim buffer() As Byte = System.Text.Encoding.UTF8.GetBytes(responseString)
        response.ContentLength64 = buffer.Length
        Dim output As System.IO.Stream = response.OutputStream
        output.Write(buffer, 0, buffer.Length)
        If response IsNot Nothing Then
            response.Close()
        End If
    End Sub

    Public Shared Sub SaveFile(enc As Encoding, boundary As [String], input As Stream, path As String)
        Dim boundaryBytes As [Byte]() = enc.GetBytes(boundary)
        Dim boundaryLen As Int32 = boundaryBytes.Length

        Using output As New FileStream(path, FileMode.Create, FileAccess.Write)
            Dim buffer As [Byte]() = New [Byte](1023) {}
            Dim len As Int32 = input.Read(buffer, 0, 1024)
            Dim startPos As Int32 = -1

            ' Find start boundary
            While True
                If len = 0 Then
                    Throw New Exception("Start Boundaray Not Found")
                End If

                startPos = IndexOf(buffer, len, boundaryBytes)
                If startPos >= 0 Then
                    Exit While
                Else
                    Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen)
                    len = input.Read(buffer, boundaryLen, 1024 - boundaryLen)
                End If
            End While

            ' Skip four lines (Boundary, Content-Disposition, Content-Type, and a blank)
            For i As Int32 = 0 To 3
                While True
                    If len = 0 Then
                        Throw New Exception("Preamble not Found.")
                    End If

                    startPos = Array.IndexOf(buffer, enc.GetBytes(vbLf)(0), startPos)
                    If startPos >= 0 Then
                        startPos += 1
                        Exit While
                    Else
                        len = input.Read(buffer, 0, 1024)
                    End If
                End While
            Next

            Array.Copy(buffer, startPos, buffer, 0, len - startPos)
            len = len - startPos

            While True
                Dim endPos As Int32 = IndexOf(buffer, len, boundaryBytes)
                If endPos >= 0 Then
                    If endPos > 0 Then
                        output.Write(buffer, 0, endPos)
                    End If
                    Exit While
                ElseIf len <= boundaryLen Then
                    Throw New Exception("End Boundaray Not Found")
                Else
                    output.Write(buffer, 0, len - boundaryLen)
                    Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen)
                    len = input.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen
                End If
            End While
        End Using
    End Sub

    Public Shared Function IndexOf(buffer As [Byte](), len As Int32, boundaryBytes As [Byte]()) As Int32
        For i As Int32 = 0 To len - boundaryBytes.Length
            Dim match As [Boolean] = True
            Dim j As Int32 = 0
            While j < boundaryBytes.Length AndAlso match
                match = buffer(i + j) = boundaryBytes(j)
                j += 1
            End While

            If match Then
                Return i
            End If
        Next

        Return -1

    End Function
    Public Shared Sub endServer()
        If (worker IsNot Nothing) Then
            worker.CancelAsync()
            worker.Dispose()
        End If
        uploadServer.isRunning = False
    End Sub
End Class
