Public Class Auth

    Public Shared Function validate(ByVal APIKey As String) As Boolean

        If (APIKey <> My.Settings.APIKey) Then
            Return False
        Else
            Return True
        End If

    End Function


End Class