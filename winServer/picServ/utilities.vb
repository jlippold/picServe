Imports System.IO

Public Class Util

    Public Shared Sub log(ByVal str As String)
        Dim logPath As String = String.Format(Util.getAppDataPath & "\Logs")
        If Directory.Exists(logPath) = False Then
            Directory.CreateDirectory(logPath)
        End If
        Dim strFile As String = String.Format(logPath & "\{0}_Log.txt", DateTime.Today.ToString("yyyy-MM-dd"))
        Try
            File.AppendAllText(strFile, String.Format("{0}: {1}{2}", DateTime.Now, str, Environment.NewLine))
        Catch ex As Exception

        End Try

    End Sub

    Public Shared Function getAppDataPath() As String
        Dim appData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        appData &= "\picServ"
        If Directory.Exists(appData) = False Then
            Directory.CreateDirectory(appData)
        End If
        Return appData
    End Function

    Public Shared Function getTempDataPath() As String
        Dim appData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        appData &= "\picServ"
        If Directory.Exists(appData) = False Then
            Directory.CreateDirectory(appData)
        End If
        appData &= "\temp"
        If Directory.Exists(appData) = False Then
            Directory.CreateDirectory(appData)
        End If
        Return appData
    End Function

    Public Shared Function BytesToString(byteCount As Long) As [String]
        Dim suf As String() = {"B", "KB", "MB", "GB", "TB", "PB", _
         "EB"}
        'Longs run out around EB
        If byteCount = 0 Then
            Return "0" + suf(0)
        End If
        Dim bytes As Long = Math.Abs(byteCount)
        Dim place As Integer = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)))
        Dim num As Double = Math.Round(bytes / Math.Pow(1024, place), 1)
        Return (Math.Sign(byteCount) * num).ToString() + suf(place)
    End Function

    Public Shared Function killFile(ByVal FileName As String) As Boolean
        Try
            If (File.Exists(FileName)) Then
                File.Delete(FileName)
            End If
        Catch ex As Exception
            Util.log(ex.Message & " " & FileName)
        End Try

        Return File.Exists(FileName)
    End Function

End Class
