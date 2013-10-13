Public Class imaging

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
End Class
