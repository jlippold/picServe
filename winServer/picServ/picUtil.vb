Imports Microsoft.VisualBasic
Imports System.Collections
Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Threading
Imports System.Diagnostics
Imports Newtonsoft.Json
Imports System.Management
Imports Microsoft.JScript
Imports Microsoft.WindowsAPICodePack.Shell
Imports System.Data.SQLite
Imports System.Collections.Specialized



Public Class picUtil

    Structure picList
        Public ItemName As String
        Public ItemPath As String
        Public DateCreated As Double
        Public ItemType As String
        Public FileSize As String
        Public Extension As String
        Public Heading As String
    End Structure


    Public Shared Function getBaseDirectories() As List(Of String)
        Dim MyString As String = My.Settings.Folders
        Dim MyArray() As String = MyString.Split(";")
        Dim baseFolders As List(Of String) = MyArray.ToList()
        baseFolders.Add(My.Settings.UploadPath)
        Return baseFolders
    End Function

    Public Shared Function getConnectionString() As String
        Return "Server=localhost;Database=tblPics;User Id=jed;Password=urchin;Initial Catalog=picServe;"
    End Function

    Public Shared Function getCacheDirectory() As String
        Dim cachePath As String = String.Format(Util.getAppDataPath & "\base64img")

        If Directory.Exists(cachePath) = False Then
            Directory.CreateDirectory(cachePath)
        End If

        Return cachePath
    End Function

    Public Shared Function getAllowedTypes() As Dictionary(Of String, String)
        Dim myTypes As New Dictionary(Of String, String)
        myTypes.Add(".jpg", "image/jpg")
        myTypes.Add(".png", "image/png")
        myTypes.Add(".gif", "image/gif")
        myTypes.Add(".zip", "application/zip")

        myTypes.Add(".mov", "video/mp4")
        'myTypes.Add(".mp4", "video/mp4" )
        'myTypes.Add(".m4v", "video/x-m4v" )
        Return myTypes
    End Function

    Public Shared Function isValidPath(ByVal requestPath As String) As Boolean
        Dim isValid As Boolean = False
        Dim d As List(Of String) = getBaseDirectories()
        d.Add(cache.getZipDirectory)
        requestPath = Trim("" & requestPath)

        Dim requestedFolder As String = requestPath.Replace("\\", "\").ToLower
        If Directory.Exists(requestPath) = False And requestPath <> "" Then
            requestedFolder = Path.GetDirectoryName(requestPath).ToLower() & "\"
        End If

        For Each f In d
            If requestedFolder.StartsWith(f.ToLower) Or requestedFolder = f.ToLower Then
                isValid = True
                Exit For
            End If
        Next

        Return isValid
    End Function

    Public Shared Function GetDateTaken(ByVal f As String, defDate As DateTime) As DateTime
        Dim so As ShellFile = ShellFile.FromFilePath(f)
        Dim d As String = so.Properties.System.Photo.DateTaken.Value.ToString()
        Dim n As Date
        If Date.TryParse(d, n) Then
            'Util.debug(d)
            defDate = n 'DateTime.Parse(d)
        Else
            'Util.debug(n)
        End If
        so = Nothing
        Return defDate
    End Function

    Public Shared Function getWebName(strPath As String) As String
        strPath = strPath.Replace("/", "//")
        Return Microsoft.JScript.GlobalObject.escape(strPath)
    End Function

    Public Shared Function getBaseListing(ByVal d As List(Of String)) As String

        Dim mypics As New List(Of picList)
        Dim fileTypes As Dictionary(Of String, String) = getAllowedTypes()
        For Each fld As String In d
            Try
                Dim di As DirectoryInfo = New DirectoryInfo(fld)
                If di.Exists Then

                    Dim r As New picList
                    'Dim dt As DateTime = Directory.GetCreationTime(subf)
                    r.ItemName = Path.GetFileName(fld)
                    r.ItemPath = fld
                    r.Heading = "Folders"
                    r.DateCreated = 9998353673.0# 'DateTimeToEpoch(dt)
                    r.ItemType = "Folder"
                    mypics.Add(r)
                    r = Nothing

                End If

            Catch ex As Exception
                JsonConvert.SerializeObject(ex.Message)
            End Try

        Next

        Return JsonConvert.SerializeObject(mypics)

    End Function

    Public Shared Function getRoot(ByVal QueryString As NameValueCollection) As String

        Dim itemlist As New List(Of Dictionary(Of String, String))
        Util.log("Root Folder Loaded")
        Dim allFolders As List(Of String) = picUtil.getBaseDirectories
        For Each subf In allFolders
            If Path.GetFileName(subf).StartsWith(".") = False Then
                Dim d As New Dictionary(Of String, String)
                d.Add("textLabel", Path.GetFileName(subf))
                d.Add("detailTextLabel", "")
                d.Add("icon", "greyarrow")
                d.Add("sectionHeader", "Folders")
                d.Add("drillVal", subf)
                itemlist.Add(d)
                d = Nothing
            End If
        Next

        Dim c As New Dictionary(Of String, String)
        c.Add("textLabel", "All Movies")
        c.Add("detailTextLabel", "")
        c.Add("icon", "greyarrow")
        c.Add("sectionHeader", "Movies")
        c.Add("drillVal", "Movies")
        itemlist.Add(c)
        c = Nothing

        Using conn As New SQLiteConnection(db.getConnStr)
            conn.Open()
            Dim cmd = conn.CreateCommand()
            Dim strsql As String = "SELECT strftime('%Y', DateTaken) AS Year, strftime('%m', DateTaken) as m, COUNT(FullName) AS Pics " & _
                      "FROM         Pictures GROUP BY strftime('%Y', DateTaken), strftime('%m', DateTaken) " & _
                      "ORDER BY strftime('%Y', DateTaken) DESC, strftime('%m', DateTaken) DESC"

            cmd.CommandText = strsql
            Dim rst As SQLiteDataReader = cmd.ExecuteReader()
            While rst.Read()
                Dim d As New Dictionary(Of String, String)
                d.Add("textLabel", MonthName(CInt(rst("m"))) & " " & rst("Year"))
                d.Add("detailTextLabel", rst("Pics") & " images")
                d.Add("icon", "greyarrow")
                Dim age As Integer = DatePart("yyyy", DateTime.Now()) - rst("Year")
                If age > 1 Then
                    d.Add("sectionHeader", rst("Year") & " - " & age & " years ago")
                Else
                    d.Add("sectionHeader", rst("Year"))
                End If
                d.Add("drillVal", rst("Year") & "-" & rst("m"))
                If rst("Year") <= DatePart("yyyy", DateTime.Now()) Then
                    itemlist.Add(d)
                End If
                d = Nothing
            End While
            rst.Close()
            conn.Close()
        End Using


        Return (JsonConvert.SerializeObject(itemlist))
    End Function

    Public Shared Function getMovies(ByVal QueryString As NameValueCollection) As String

        Dim output As String = ""
        Dim itemlist As New List(Of Dictionary(Of String, String))
        Util.log("Requested Movies")

        Using conn As New SQLiteConnection(db.getConnStr)
            conn.Open()
            Dim cmd = conn.CreateCommand()
            cmd.CommandText = "SELECT strftime('%Y', DateTaken) AS Year, strftime('%m', DateTaken) as m, * FROM Pictures where FileName like '%.mov' ORDER BY DateTaken"
            Dim rst = cmd.ExecuteReader()
            If rst.HasRows Then
                While rst.Read()
                    Dim d As New Dictionary(Of String, String)
                    d.Add("sectionHeader", MonthName(CInt(rst("m"))) & " " & rst("Year"))
                    d.Add("image", "http://" & QueryString("HOST") & "/getFile/?key=" & My.Settings.APIKey & "&Path=" & Microsoft.JScript.GlobalObject.escape(rst("FullName")) & "&mode=thumbnail")
                    d.Add("name", rst("FileName"))
                    d.Add("Path", rst("FilePath"))
                    d.Add("Type", "Video")
                    d.Add("DateCreated", picUtil.DateTimeToEpoch(rst("DateTaken")))
                    d.Add("cachePath", Microsoft.JScript.GlobalObject.escape(rst("FullName")).ToLower())
                    itemlist.Add(d)
                    d = Nothing
                End While
                output = JsonConvert.SerializeObject(itemlist)
            End If

            rst.Close()
            conn.Close()
        End Using

        Return output
    End Function


    Public Shared Function getContentsOfFolder(ByVal QueryString As NameValueCollection) As String

        Dim output As String = ""
        Dim itemlist As New List(Of Dictionary(Of String, String))
        Dim y As String = QueryString("Path").Replace("||", "\")

        Util.log("Requested Query for: " & QueryString("Path"))

        Using conn As New SQLiteConnection(db.getConnStr)
            conn.Open()
            Dim cmd = conn.CreateCommand()
            cmd.CommandText = "SELECT * FROM Pictures where filepath = @Path ORDER BY FullName"
            cmd.Parameters.Add(New SQLiteParameter("@Path", y))

            Dim rst = cmd.ExecuteReader()
            If rst.HasRows Then
                While rst.Read()
                    Dim d As New Dictionary(Of String, String)
                    d.Add("sectionHeader", QueryString("t"))
                    d.Add("image", "http://" & QueryString("HOST") & "/getFile/?key=" & My.Settings.APIKey & "&Path=" & Microsoft.JScript.GlobalObject.escape(rst("FullName")) & "&mode=thumbnail")
                    d.Add("name", rst("FileName"))
                    d.Add("Path", rst("FilePath"))
                    If rst("FileName").ToString().ToLower().EndsWith(".mov") Then
                        d.Add("Type", "Video")
                    Else
                        d.Add("Type", "Image")
                    End If
                    d.Add("DateCreated", picUtil.DateTimeToEpoch(rst("DateTaken")))
                    d.Add("cachePath", Microsoft.JScript.GlobalObject.escape(rst("FullName")).ToLower())
                    itemlist.Add(d)
                    d = Nothing
                End While
                output = JsonConvert.SerializeObject(itemlist)
            End If

            rst.Close()
            conn.Close()
        End Using

        Return output
    End Function


    Public Shared Function runDynamicSQL(ByVal QueryString As NameValueCollection) As String

        Dim output As String = ""
        Dim itemlist As New List(Of Dictionary(Of String, String))
        Dim y As String = QueryString("v").Split("-")(0)
        Dim m As String = QueryString("v").Split("-")(1)

        Util.log("Requested Query for: " & QueryString("v"))

        Using conn As New SQLiteConnection(db.getConnStr)
            conn.Open()
            Dim cmd = conn.CreateCommand()
            cmd.CommandText = "SELECT * FROM Pictures where strftime('%Y', DateTaken) = @y and strftime('%m', DateTaken) = @m ORDER BY DateTaken"

            cmd.Parameters.Add(New SQLiteParameter("@y", y))
            cmd.Parameters.Add(New SQLiteParameter("@m", m))
            Dim rst = cmd.ExecuteReader()
            If rst.HasRows Then
                While rst.Read()
                    Dim d As New Dictionary(Of String, String)
                    d.Add("sectionHeader", QueryString("t"))
                    d.Add("image", "http://" & QueryString("HOST") & "/getFile/?key=" & My.Settings.APIKey & "&Path=" & Microsoft.JScript.GlobalObject.escape(rst("FullName")) & "&mode=thumbnail")
                    d.Add("name", rst("FileName"))
                    d.Add("Path", rst("FilePath"))
                    If rst("FileName").ToString().ToLower().EndsWith(".mov") Then
                        d.Add("Type", "Video")
                    Else
                        d.Add("Type", "Image")
                    End If
                    d.Add("DateCreated", picUtil.DateTimeToEpoch(rst("DateTaken")))
                    d.Add("cachePath", Microsoft.JScript.GlobalObject.escape(rst("FullName")).ToLower())
                    itemlist.Add(d)
                    d = Nothing
                End While
                output = JsonConvert.SerializeObject(itemlist)
            End If

            rst.Close()
            conn.Close()
        End Using

        Return output
    End Function

    Public Shared Function getAllFolderList(ByVal QueryString As NameValueCollection) As String

        Dim output As String = ""
        Dim Folders As New List(Of Dictionary(Of String, String))

        Using conn As New SQLiteConnection(db.getConnStr)
            conn.Open()
            Dim cmd = conn.CreateCommand()
            cmd.CommandText = "SELECT * FROM folderProps order by FolderName"
            Dim rst = cmd.ExecuteReader()
            If rst.HasRows Then
                While rst.Read()
                    Dim d As New Dictionary(Of String, String)
                    Dim p As String = rst("FolderName")
                    If System.IO.Directory.Exists(p) Then
                        Dim indexer As String = New DirectoryInfo(Path.GetDirectoryName(p & "\")).Name
                        Dim peices As String() = p.Split("\")
                        d.Add("Name", indexer)
                        For i = UBound(peices) - 1 To 0 Step -1
                            If i > 0 Then
                                indexer = indexer & " " & peices(i)
                            End If
                        Next

                        d.Add("Indexer", indexer)
                        d.Add("FullPath", p)
                        d.Add("QSPath", p.Replace("\", "||"))
                        d.Add("DateModified", rst("DateModified"))
                        Folders.Add(d)
                    End If


                End While
                output = JsonConvert.SerializeObject(Folders)
            End If

            rst.Close()
            conn.Close()
        End Using

        Return output
    End Function


    Public Shared Function getBaseFolderList(ByVal QueryString As NameValueCollection) As String

        Dim output As String = ""
        If QueryString("path") = "" Then
            Util.log("Loaded Root")
            Dim baseFolders As List(Of String) = picUtil.getBaseDirectories()
            output = (picUtil.getBaseListing(baseFolders))
        Else
            Util.log(QueryString("path"))
            Dim d As New List(Of String)
            If picUtil.isValidPath(QueryString("path")) Then
                d.Add(QueryString("path"))
            End If
            output = (picUtil.getPicList(d))
        End If

        Return output
    End Function

    Public Shared Function deleteFile(ByVal p As String) As String
        Dim output As String = ""
        If picUtil.isValidPath(p) AndAlso p <> "" Then
            Util.log("Deleted file: " & p)
            If File.Exists(p) Then
                Dim fileTypes As Dictionary(Of String, String) = picUtil.getAllowedTypes()
                Dim thisExtension As String = Path.GetExtension(p).ToLower()
                Dim newName As String = Path.GetFileNameWithoutExtension(p)
                Dim deletedPath As String = picUtil.getCacheDirectory.Replace("\base64img", "\") & ".DeletedPics\"
                If Directory.Exists(deletedPath) = False Then
                    Directory.CreateDirectory(deletedPath)
                End If

                Dim newPath As String = deletedPath & newName & thisExtension
                Dim i As Integer = 1
                Do Until File.Exists(newPath) = False
                    newPath = deletedPath & newName & "_" & i & thisExtension
                Loop


                If fileTypes.ContainsKey(thisExtension) Then
                    Try
                        File.Move(p, newPath)
                        db.ExecuteNonQuery("Delete from pictures where fullname = '" & p.Replace("\\", "\").Replace("\\", "\") & "'")
                        output = ("success")
                    Catch ex As Exception
                        output = (ex.Message)
                    End Try
                End If
            Else
                db.ExecuteNonQuery("Delete from pictures where fullname = '" & p.Replace("\\", "\").Replace("\\", "\") & "'")
                output = ("success")
            End If
        End If

        Return output
    End Function

    Public Shared Sub serveZip(ByVal QueryString As NameValueCollection, response As HttpListenerResponse)
        Dim p As String = QueryString("Path")
        QueryString("Path") = cache.getZipDirectory & p
        webRouting.serveFile(QueryString, response)
    End Sub


    Public Shared Function getPicList(ByVal d As List(Of String)) As String



        Dim mypics As New List(Of picList)
        Dim fileTypes As Dictionary(Of String, String) = getAllowedTypes()
        For Each fld As String In d
            Try

                Dim di As DirectoryInfo = New DirectoryInfo(fld)
                If di.Exists Then
                    For Each subf As String In System.IO.Directory.GetDirectories(di.FullName)
                        If Path.GetFileName(subf).StartsWith(".") = False Then
                            Dim r As New picList
                            'Dim dt As DateTime = Directory.GetCreationTime(subf)
                            r.ItemName = Path.GetFileName(subf)
                            r.ItemPath = subf
                            r.Heading = "Folders"
                            r.DateCreated = 9998353673.0# 'DateTimeToEpoch(dt)
                            r.ItemType = "Folder"
                            mypics.Add(r)
                            r = Nothing
                        End If
                    Next
                    For Each fl As String In System.IO.Directory.GetFiles(di.FullName)
                        If (fileTypes.ContainsKey(Path.GetExtension(fl).ToLower())) AndAlso Left(Path.GetFileName(fl), 1) <> "." Then
                            Dim r As New picList
                            Dim dt As DateTime = Directory.GetCreationTime(fl)

                            dt = GetDateTaken(fl, dt)

                            Dim info As New FileInfo(fl)

                            r.ItemName = Path.GetFileName(fl)
                            r.ItemPath = fl
                            r.Heading = dt.ToString("MMMM, yyyy")
                            r.DateCreated = DateTimeToEpoch(dt)
                            r.Extension = Path.GetExtension(fl).ToLower()
                            If r.Extension = ".mov" Then
                                r.ItemType = "Video"
                            Else
                                r.ItemType = "Image"
                            End If

                            r.FileSize = Util.BytesToString(info.Length)

                            mypics.Add(r)
                            r = Nothing
                        End If
                    Next
                End If

            Catch ex As Exception
                JsonConvert.SerializeObject(ex.Message)
            End Try

        Next

        Return JsonConvert.SerializeObject(mypics)

    End Function

    Public Shared Function DateTimeToEpoch(ByVal DateTimeValue As Date) As Integer
        Try
            Return CInt(DateTimeValue.Subtract(CDate("1.1.1970 00:00:00")).TotalSeconds)
        Catch ex As System.OverflowException
            Return -1
        End Try

    End Function
End Class

