namespace MoveReminder;

/// <summary>
/// 将图片以「等比放大直至铺满容器、超出部分裁切」的方式布局（类似 CSS object-fit: cover），
/// 避免 <see cref="PictureBoxSizeMode.Zoom"/> 为保留整图而产生的黑边，同时不会像无脑拉伸那样变形。
/// </summary>
internal static class ImageCoverLayout
{
    public static void Attach(Panel host, PictureBox picture, Image image)
    {
        picture.Dock = DockStyle.None;
        picture.SizeMode = PictureBoxSizeMode.StretchImage;

        void OnLayout(object? sender, EventArgs e) => Apply(host, picture, image);

        host.Resize += OnLayout;
        Apply(host, picture, image);
    }

    public static void AttachCenteredFit(Panel host, PictureBox picture, Image image, int sizePercent)
    {
        picture.Dock = DockStyle.None;
        picture.SizeMode = PictureBoxSizeMode.StretchImage;

        void OnLayout(object? sender, EventArgs e) => ApplyCenteredFit(host, picture, image, sizePercent);

        host.Resize += OnLayout;
        ApplyCenteredFit(host, picture, image, sizePercent);
    }

    private static void Apply(Panel host, PictureBox picture, Image image)
    {
        if (image.Width <= 0 || image.Height <= 0)
        {
            return;
        }

        var pw = Math.Max(1, host.ClientSize.Width);
        var ph = Math.Max(1, host.ClientSize.Height);
        var ratio = Math.Max(pw / (double)image.Width, ph / (double)image.Height);
        var w = Math.Max(1, (int)Math.Round(image.Width * ratio));
        var h = Math.Max(1, (int)Math.Round(image.Height * ratio));
        picture.Size = new Size(w, h);
        picture.Location = new Point((pw - w) / 2, (ph - h) / 2);
    }

    private static void ApplyCenteredFit(Panel host, PictureBox picture, Image image, int sizePercent)
    {
        if (image.Width <= 0 || image.Height <= 0)
        {
            return;
        }

        var pw = Math.Max(1, host.ClientSize.Width);
        var ph = Math.Max(1, host.ClientSize.Height);
        var percent = Math.Clamp(sizePercent, 10, 100);
        var maxW = Math.Max(1, (int)Math.Round(pw * percent / 100.0));
        var maxH = Math.Max(1, (int)Math.Round(ph * percent / 100.0));
        var ratio = Math.Min(maxW / (double)image.Width, maxH / (double)image.Height);
        var w = Math.Max(1, (int)Math.Round(image.Width * ratio));
        var h = Math.Max(1, (int)Math.Round(image.Height * ratio));
        picture.Size = new Size(w, h);
        picture.Location = new Point((pw - w) / 2, (ph - h) / 2);
    }
}
