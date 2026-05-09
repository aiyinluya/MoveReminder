using System.Drawing.Drawing2D;

namespace MoveReminder;

/// <summary>在固定矩形内等比例缩放绘制图片（留白居中，不拉伸变形）。</summary>
internal static class ImagePreviewHelper
{
    public static Bitmap CreateUniformFitBitmap(string path, int boxWidth, int boxHeight, Color? letterboxColor = null)
    {
        using var src = Image.FromFile(path);
        return CreateUniformFitBitmap(src, boxWidth, boxHeight, letterboxColor);
    }

    public static Bitmap CreateUniformFitBitmap(Image source, int boxWidth, int boxHeight, Color? letterboxColor = null)
    {
        var bg = letterboxColor ?? Color.FromArgb(248, 250, 252);
        var bmp = new Bitmap(Math.Max(1, boxWidth), Math.Max(1, boxHeight));
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(bg);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            if (source.Width < 1 || source.Height < 1)
                return bmp;

            var ratio = Math.Min((float)boxWidth / source.Width, (float)boxHeight / source.Height);
            var w = Math.Max(1, (int)Math.Round(source.Width * ratio));
            var h = Math.Max(1, (int)Math.Round(source.Height * ratio));
            var x = (boxWidth - w) / 2;
            var y = (boxHeight - h) / 2;
            g.DrawImage(source, x, y, w, h);
        }

        return bmp;
    }
}
