using Manage_Coffee.Models;

public class ChiTietSanPhamViewModel
{
	public string MaSp { get; set; }
	public string Ten { get; set; }
	public string MaSize { get; set; }
	public string TenSize { get; set; }
	public string MaDa { get; set; }
	public string TenDa { get; set; }
	public string MaDuong { get; set; }
	public string TenDuong { get; set; }

	public int Soluong { get; set; }
	public int Gia { get; set; }
	public int Tongtien { get; set; }
	public string? Ghichu { get; set; }
	public List<Kit> Kits { get; set; }
    public List<CTKIT> CTKITs { get; set; }// Danh sách Kit liên quan
}