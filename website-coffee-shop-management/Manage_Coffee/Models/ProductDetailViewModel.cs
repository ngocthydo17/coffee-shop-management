using Manage_Coffee.Models;
using System.Collections.Generic;

namespace Manage_Coffee.ViewModels
{
    public class ProductDetailViewModel
    {
        public SanPham Product { get; set; }
        public List<SanPham> Toppings { get; set; } = new List<SanPham>();
        public List<Size> Size1 { get; set; } = new List<Size>();
        public List<Size> Sizes { get; set; }
        public List<SanPham> SanPhamTuongTu { get; set; } = new List<SanPham>();
    }
}
