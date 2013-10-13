﻿Imports System.Net
Imports System.Globalization
Imports System.Collections.Specialized
Imports System.Web
Imports System.IO
Imports System.Drawing.Imaging
Imports System.Data.SQLite
Imports Newtonsoft.Json
Imports System.Text

Public Class webRouting
    Public Shared Sub Route(ByVal context As HttpListenerContext)

        Dim url As String = context.Request.Url.ToString
        Dim response As HttpListenerResponse = context.Response
        Dim uri As New Uri(url)
        Dim webPath As String = uri.AbsolutePath.ToLower
        Dim query As String = uri.Query
        Dim QueryString As NameValueCollection = HttpUtility.ParseQueryString(query)

        If Auth.validate(QueryString("Key")) = False Then
            WebServer.writeText("Unauthorized", response)
            Exit Sub
        End If

        QueryString("HOST") = uri.Host & ":" & uri.Port
        Select Case webPath
            Case "/" 'UICollectionView json
                WebServer.writeText(picUtil.getBaseFolderList(QueryString), response, "application/json")
            Case "/getfile/"
                serveFile(QueryString, response, context.Request.Headers("Range"))
            Case "/getdynamic/" 'sql
                WebServer.writeText(picUtil.runDynamicSQL(QueryString), response, "application/json")
            Case "/deletefile/"
                WebServer.writeText(picUtil.deleteFile(QueryString("Path")), response, "text/html")
            Case "/getroot/" 'UITableView json
                WebServer.writeText(picUtil.getRoot(QueryString), response, "application/json")
            Case "/getmovies/"
                WebServer.writeText(picUtil.getMovies(QueryString), response, "application/json")
            Case "/zip/"
                picUtil.serveZip(QueryString, response)
            Case Else
                WebServer.writeText("", response)
        End Select
    End Sub

    Public Shared Function getEndpoints() As String
        Dim html As String = ""
        Dim qs As String = "?Key=" & My.Settings.APIKey & "&"
        html &= "<a href='/" & qs & "'>Root</a><br />"
        html &= "<a href='/" & qs & "Path=I:\\Pictures\\Jed-iPhone'>Some Folder</a><br />"
        html &= "<a href='/getFile/" & qs & "Path=I:\\Pictures\\Jed-iPhone\\IMG_07-04-21_AB762E25-E67C-41EA-8824-CF739015303B.JPG'>Some Pic</a><br />"
        html &= "<a href='/getFile/" & qs & "mode=thumbnail&Path=I:\\Pictures\\Jed-iPhone\\IMG_07-04-21_AB762E25-E67C-41EA-8824-CF739015303B.JPG'>Some Thumb</a><br />"
        html &= "<a href='/getDynamic/" & qs & "v=2012-01'>Query</a><br />"
        html &= "<a href='/getRoot/" & qs & "'>TableView</a><br />"
        html &= "<a href='/zip/" & qs & "Path=i.pictures.jed-iphone.jed older.zip'>Zip</a><br />"
        Return html
    End Function



    Public Shared Sub serveFile(ByVal QueryString As NameValueCollection, response As HttpListenerResponse, Optional ByVal range As String = "")
        Dim output As String = ""
        Dim p As String = QueryString("Path")

        If picUtil.isValidPath(p) AndAlso p <> "" Then
            If File.Exists(p) Then
                Dim fileTypes As Dictionary(Of String, String) = picUtil.getAllowedTypes()
                Dim thisExtension As String = Path.GetExtension(p).ToLower()
                If fileTypes.ContainsKey(thisExtension) Then

                    If QueryString("mode") = "" And fileTypes(thisExtension).Contains("image") Then
                        Util.log("Requested Full Size Image: " & p)
                        WebServer.writeImageFromPath(p, response)
                        Exit Sub
                    End If

                    If QueryString("mode") = "thumbnail" And fileTypes(thisExtension).Contains("image") Then
                        Dim cache As String = picUtil.getWebName(p.Replace("\\", "\")).ToLower()
                        cache = picUtil.getCacheDirectory() & "\img\" & cache & ".cache"
                        If File.Exists(cache) Then
                            Dim base64 As String = File.ReadAllText(cache)
                            base64 = base64.Substring(22)
                            Dim binaryData() As Byte = System.Convert.FromBase64String(base64)
                            WebServer.writeBinary(binaryData, response, "text/html")
                        Else
                            Try
                                Dim oldPic As New System.Drawing.Bitmap(p)
                                Dim newPic As System.Drawing.Bitmap = imaging.resize(oldPic, 200, 200)
                                oldPic.Dispose()

                                Dim byteArray As Byte() = New Byte(-1) {}
                                Using stream As New MemoryStream()
                                    newPic.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
                                    stream.Close()
                                    byteArray = stream.ToArray()
                                End Using
                                WebServer.writeImageFromByteArray(byteArray, response)
                                newPic.Dispose()
                            Catch ex As Exception
                                'WebServer.writeText(ex.Message, response)
                            End Try
                        End If
                        Exit Sub
                    End If

                End If

                If thisExtension = ".zip" Then
                    WebServer.writeFileFromPath(p, response, fileTypes(thisExtension))
                End If

                If thisExtension = ".mov" Then
                    If QueryString("mode") = "thumbnail" Then
                        Try
                            Dim newPic As New System.Drawing.Bitmap(My.Application.Info.DirectoryPath & "\images\video.png")
                            Dim byteArray As Byte() = New Byte(-1) {}
                            Using stream As New MemoryStream()
                                newPic.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
                                stream.Close()
                                byteArray = stream.ToArray()
                            End Using
                            WebServer.writeImageFromByteArray(byteArray, response)
                            newPic.Dispose()
                        Catch ex As Exception
                            'WebServer.writeText(ex.Message, response)
                        End Try
                    Else
                        'Util.log("Requested Movie: " & p)
                        WebServer.writeVideoFromPath(p, response, fileTypes(thisExtension), range)
                    End If


                End If
            Else
                response.StatusCode = 404
                response.StatusDescription = "Not Found"
                WebServer.writeText("404", response)
            End If
        Else
            response.StatusCode = 404
            response.StatusDescription = "Not Found"
            WebServer.writeText("404", response)
        End If

    End Sub
End Class
