using System.Drawing;

namespace Manage_Coffee.Models
{
    public class CartItem
    {
		
		public string ProductID { get; set; } 

        public string Ten { get; set; } 

        public int Dongia { get; set; }
        public string Anh { get; set; }

        public int Soluong { get; set; }
        public int SizePrice { get; set; } // Thêm thuộc tính SizePrice


        public IEnumerable<DanhMucKm> Discounts { get; set; }

		// Thêm Size vào CartItem
		public string SizeID { get; set; }
	    public string SizeName { get; set; }
        public string DaID { get; set; }
        public string DaName { get; set; }
        public string DuongID { get; set; }
        public string DuongName { get; set; }
        public int TriGia { get; set; }
        public string Ghichu { get; set; }
        public List<Kit> Kits { get; set; }// Thêm danh sách kit
        public List<CTKIT> CTKITs { get; set; }
        public List<SanPham> SanPhams { get; set; }
		public List<SanPham> Toppings { get; set; } = new List<SanPham>();

		public int Total { get {
                int toppingPrice = Toppings.Sum(t => t.Dongia);
                return Soluong *(Dongia+ TriGia + toppingPrice); } }
        public CartItem()
        {
        
        }
        public CartItem(SanPham sanPham,Size size,Duong duong,Da da, string ghiChu = "")
        {
            ProductID = sanPham.MaSp;
            Ten = sanPham.Ten;
            Dongia = sanPham.Dongia;
            Soluong = 1;
            Anh = sanPham.Anh;
            SizeID = size.MaSize;  
	    	SizeName = size.Ten;
            TriGia = size.TriGia;
            Ghichu = ghiChu;
            DaID = da.MaDa;
			DaName = da.Ten;
            DuongID = duong.MaDuong;
			DuongName = duong.Ten;
            Kits = new List<Kit>();
			Toppings = new List<SanPham>();
		}
    }
}
