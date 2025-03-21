namespace Manage_Coffee.Areas.Admin.Models
{
    public class SanPhamBanDuoc
    {
        public string MaSp { get; set; }   // Mã sản phẩm
        public string TenSp { get; set; }   // Tên sản phẩm
        public int SoLuong { get; set; }    // Số lượng bán được
        public decimal TongTien { get; set; } // Tổng doanh thu từ sản phẩm
    }
}
