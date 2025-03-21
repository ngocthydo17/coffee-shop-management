namespace Manage_Coffee.Models
{
    public class CTKIT
    {
        public int SoLuong { get; set; }

        public string MaKit { get; set; } = null!;

        public string MaSp { get; set; } = null!;
        public string MaKH { get; set; } = null!;

        public string MaPhieuonl { get; set; } = null!;
        public string MaSize { get; set; } = null!;

        public virtual SanPham MaKitNavigation { get; set; } = null!;

        public virtual SanPham MaSpNavigation { get; set; } = null!;
        public virtual KhachHang MaKHNavigation { get; set; } = null!;
        public virtual Size MaSizeNavigation { get; set; } = null!;

    }
}
