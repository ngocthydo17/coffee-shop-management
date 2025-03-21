namespace Manage_Coffee.Models
{
    public class CtTopping
    {
        public string MaTopping { get; set; } = null!;

        public string MaSp { get; set; } = null!;
        public string MaPhieuonl { get; set; }
        public string MaKH { get; set; }
        public virtual SanPham MaToppingNavigation { get; set; } = null!;
        public virtual Phieudhonl Phieudhonl { get; set; }
        public virtual SanPham MaSpNavigation { get; set; } = null!;
    }
}
