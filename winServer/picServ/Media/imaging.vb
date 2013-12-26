Imports System.Drawing.Imaging

Public Class imaging


    Public Shared Function downSizeImage(ByVal p As String) As Byte()
        Dim SourceImage As New System.Drawing.Bitmap(p)

        Dim bitmap As System.Drawing.Bitmap = New System.Drawing.Bitmap(SourceImage.Width, SourceImage.Height, SourceImage.PixelFormat)

        If bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format1bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format4bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format8bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Undefined Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.DontCare Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format16bppArgb1555 Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format16bppGrayScale Then
            Throw New NotSupportedException("Pixel format of the image is not supported.")
        End If

        Dim graphicsImage As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bitmap)
        graphicsImage.Clear(Drawing.Color.Black)

        graphicsImage.SmoothingMode = Drawing.Drawing2D.SmoothingMode.HighQuality
        graphicsImage.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
        graphicsImage.DrawImage(SourceImage, 0, 0, SourceImage.Width, SourceImage.Height)
        graphicsImage.Dispose()
        bitmap.SetResolution(72, 72)

        Dim imageBytes As Byte()
        Using ms As New IO.MemoryStream()
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg)
            imageBytes = ms.ToArray()
        End Using

        SourceImage.Dispose()
        bitmap.Dispose()
        Return imageBytes

    End Function


    Public Shared Function resize(ByVal SourceImage As System.Drawing.Image, ByVal NewHeight As Int32, ByVal NewWidth As Int32) As System.Drawing.Image

        Dim SourceWidth As Integer = NewWidth
        Dim SourceHeight As Integer = NewHeight

        If SourceImage.Width > SourceImage.Height Then
            SourceHeight = SourceHeight * (SourceImage.Height / SourceImage.Width)
        Else
            SourceWidth = SourceWidth * (SourceImage.Width / SourceImage.Height)
        End If

        Dim bitmap As System.Drawing.Bitmap = New System.Drawing.Bitmap(SourceWidth, SourceHeight, SourceImage.PixelFormat)

        If bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format1bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format4bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format8bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Undefined Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.DontCare Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format16bppArgb1555 Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format16bppGrayScale Then
            Throw New NotSupportedException("Pixel format of the image is not supported.")
        End If

        Dim graphicsImage As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bitmap)
        graphicsImage.Clear(Drawing.Color.Black)

        graphicsImage.SmoothingMode = Drawing.Drawing2D.SmoothingMode.HighQuality
        graphicsImage.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
        graphicsImage.DrawImage(SourceImage, 0, 0, SourceWidth, SourceHeight)
        graphicsImage.Dispose()
        Return bitmap

    End Function

    Public Shared Function FixRotatation(ByVal img As System.Drawing.Image) As System.Drawing.Image
        Dim rft As RotateFlipType = RotateFlipType.RotateNoneFlipNone
        Dim properties As PropertyItem() = img.PropertyItems
        Dim bReturn As Boolean = False
        For Each p As PropertyItem In properties
            If p.Id = 274 Then
                Dim orientation As Short = BitConverter.ToInt16(p.Value, 0)
                Select Case orientation
                    Case 1
                        rft = RotateFlipType.RotateNoneFlipNone
                    Case 3
                        rft = RotateFlipType.Rotate180FlipNone
                    Case 6
                        rft = RotateFlipType.Rotate90FlipNone
                    Case 8
                        rft = RotateFlipType.Rotate270FlipNone
                End Select
            End If
        Next

        img.RotateFlip(rft)
        Dim bitmap As System.Drawing.Bitmap = New System.Drawing.Bitmap(img.Width, img.Height, img.PixelFormat)

        If bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format1bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format4bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format8bppIndexed Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Undefined Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.DontCare Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format16bppArgb1555 Or _
            bitmap.PixelFormat = Drawing.Imaging.PixelFormat.Format16bppGrayScale Then
            Throw New NotSupportedException("Pixel format of the image is not supported.")
        End If

        Dim graphicsImage As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bitmap)
        graphicsImage.Clear(Drawing.Color.Black)

        graphicsImage.SmoothingMode = Drawing.Drawing2D.SmoothingMode.HighQuality
        graphicsImage.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
        graphicsImage.DrawImage(img, 0, 0, img.Width, img.Height)
        If rft <> RotateFlipType.RotateNoneFlipNone Then
            graphicsImage.RotateTransform(rft)
        End If

        graphicsImage.Dispose()

        Return bitmap

    End Function

End Class
