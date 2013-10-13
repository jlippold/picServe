Imports System.Data
Imports System.Data.Common
Imports System.Data.SQLite
Imports System.IO

Public Class db
    Public DBPath As String = ""

    Public Shared Sub killAndCreate()
        deleteDB()
        createDB()
    End Sub

    Public Shared Sub killAndCreateIfDoesntExist()
        If File.Exists(getDBPath) = False Then
            deleteDB()
            createDB()
        End If
    End Sub

    Public Shared Sub createDB()
        Try
            SQLiteConnection.CreateFile(getDBPath)

            'create the table
            Dim sql As String = "CREATE TABLE Pictures ( " & _
                "FullName varchar(260), " & _
                "DateTaken datetime, " & _
                "Dimensions varchar(255), " & _
                "Camera varchar(255), " & _
                "FileName varchar(255), " & _
                "FilePath varchar(255), " & _
                "PRIMARY KEY (FullName)); "

            sql &= "CREATE TABLE FolderProps (" & _
                "FolderName varchar(260), " & _
                "DateModified datetime, " & _
                "DateIndexed datetime, " & _
                "PRIMARY KEY (FolderName)); "

            ExecuteNonQuery(sql)
        Catch ex As Exception
            Util.log("open DB error: " & ex.Message)
        End Try


    End Sub

    Public Shared Sub createTable(ByVal tableName As String)
        Util.killFile(getDBPath())
    End Sub

    Public Shared Sub deleteDB()
        Util.killFile(getDBPath())
    End Sub

    Public Shared Sub ExecuteNonQuery(ByVal sql As String)
        Using conn = New SQLiteConnection(getConnStr())
            Using cmd = conn.CreateCommand()
                conn.Open()
                cmd.CommandText = sql
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub


    Shared Function getConnStr() As String
        Return "Data Source='" & getDBPath() & "';Version=3;"
    End Function

    Public Shared Function getDBPath()
        Return Util.getAppDataPath & "/picServ.sqlite"
    End Function


End Class
