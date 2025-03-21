namespace Manage_Coffee.Areas.Admin.Models
{
	public class ThongKeSanPham
	{
        public int? MaCN { get; set; }
        public int? Nam { get; set; }       // Năm thống kê
		public int? Thang { get; set; }     // Tháng thống kê
		public int? Quy { get; set; }       // Quý thống kê
		public DateTime? Ngay { get; set; } // Ngày thống kê (dùng cho khoảng thời gian)
		public decimal TongDoanhThu { get; set; } // Tổng doanh thu

		// Các thuộc tính bổ sung nếu cần
		public string LoaiBanHang { get; set; }
		public List<SanPhamBanDuoc> SanPhamBanDuoc { get; set; } = new List<SanPhamBanDuoc>();
		public List<YourProductSalesModel> YourProductSales { get; set; } = new List<YourProductSalesModel>();
	}


}
