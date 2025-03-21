
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Manage_Coffee.Models;
using System.ComponentModel;
using Manage_Coffee.ViewModels;
using X.PagedList;
using Manage_Coffee.Helpers;
using System.Globalization;
using System.Text;

namespace Manage_Coffee.Controllers
{
    public class SanPhamsController : Controller
    {
        private readonly Cf2Context _context;

        public SanPhamsController(Cf2Context context)
        {
            _context = context;
        }


        public async Task<IActionResult> Sanpham1(string category, int? page, string keyword)
        {
            int pageSize = 8;
            int pageNumber = page == null || page <= 0 ? 1 : page.Value;

            // Lọc sản phẩm theo trạng thái và danh mục
            var lst = _context.SanPhams.Where(x => x.TrangThai == true);

            if (!string.IsNullOrEmpty(category))
            {
                lst = lst.Where(x => x.Maloai == category);
            }

            var products = await lst.ToListAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                var normalizedKeyword = keyword.ToLower(); // Chỉ chuyển đổi thành chữ thường

                // Thử tìm kiếm bằng Contains trước
                var filteredProducts = products
                    .Where(x => x.Ten.ToLower().Contains(normalizedKeyword))
                    .ToList();

                // Nếu không tìm thấy kết quả, sử dụng thuật toán Levenshtein
                if (!filteredProducts.Any())
                {
                    filteredProducts = products
                        .Where(x => CalculateLevenshteinDistance(x.Ten.ToLower(), normalizedKeyword) <= 3)
                        .ToList();
                }

                products = filteredProducts;
            }

            ViewBag.Query = new Dictionary<string, string> { { "category", category }, { "keyword", keyword } };

            var pagedList = products.ToPagedList(pageNumber, pageSize);

            if (pagedList == null || !pagedList.Any())
            {
                ViewBag.Message = "Không tìm thấy sản phẩm nào.";
                return View(pagedList);
            }

            return View(pagedList);
        }

        public async Task<IActionResult> GetSuggestions(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return Json(new List<string>());
            }

            var normalizedKeyword = keyword.ToLower(); // Chuyển từ khóa thành chữ thường
            Console.WriteLine($"Normalized Keyword: {normalizedKeyword}"); // Log từ khóa

            // Lấy danh sách sản phẩm từ cơ sở dữ liệu
            var suggestions = await _context.SanPhams
                .AsNoTracking()
                .Select(x => x.Ten.ToLower()) // Chỉ chuyển đổi thành chữ thường
                .ToListAsync();

            Console.WriteLine($"Total suggestions found: {suggestions.Count}"); // Log số lượng sản phẩm

            // Tìm kiếm theo từ khóa chính xác
            var exactMatchSuggestions = suggestions
                .Where(name => name.Equals(normalizedKeyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (exactMatchSuggestions.Any())
            {
                // Nếu có kết quả khớp chính xác, trả về ngay
                Console.WriteLine($"Exact match suggestions count: {exactMatchSuggestions.Count}"); // Log số lượng kết quả khớp chính xác
                return Json(exactMatchSuggestions);
            }

            // Nếu không có kết quả khớp chính xác, tiếp tục với Contains
            var filteredSuggestions = suggestions
                .Where(name => name.Contains(normalizedKeyword)) // Tìm các sản phẩm chứa từ khóa
                .ToList();

            // Nếu không tìm thấy sản phẩm nào với Contains, sử dụng thuật toán Levenshtein
            if (!filteredSuggestions.Any())
            {
                filteredSuggestions = suggestions
                    .Where(name => name.Contains(normalizedKeyword) ||
                                   CalculateLevenshteinDistance(name, normalizedKeyword) <= 7) // Tìm sản phẩm có chứa từ khóa hoặc có khoảng cách Levenshtein <= 7
                    .OrderBy(name => CalculateLevenshteinDistance(name, normalizedKeyword)) // Sắp xếp theo khoảng cách Levenshtein
                    .Take(5) // Giới hạn kết quả
                    .ToList();
            }

            Console.WriteLine($"Filtered suggestions count: {filteredSuggestions.Count}"); // Log số lượng kết quả sau khi lọc

            return Json(filteredSuggestions);
        }


        private int CalculateLevenshteinDistance(string source, string target)
        {
            int[,] dp = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
            {
                for (int j = 0; j <= target.Length; j++)
                {
                    if (i == 0)
                        dp[i, j] = j;
                    else if (j == 0)
                        dp[i, j] = i;
                    else
                        dp[i, j] = Math.Min(
                            Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                            dp[i - 1, j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1)
                        );
                }
            }

            return dp[source.Length, target.Length];
        }




        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        public async Task<IActionResult> Index()
        {
            var cf2Context = _context.SanPhams.Include(s => s.MaToppingNavigation).Include(s => s.MaloaiNavigation);
            return View(await cf2Context.ToListAsync());
        }

		// GET: SanPhams/Details/5
		public async Task<IActionResult> Details(string id)
		{
			if (id == null || _context.SanPhams == null)
			{
				return NotFound();
			}
            var sanPham = await _context.SanPhams .FirstOrDefaultAsync(m => m.MaSp == id);
            if (sanPham == null)
			{
				return NotFound();
			}
            var makh = "";
            if (HttpContext.Session.GetString("UserName") == null)
            {
                var makhClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH");
                makh = makhClaim?.Value;
            }
            else
            {
                makh = HttpContext.Session.GetString("UserPhone");
            }
            ViewBag.makh = makh;
            var toppings = await _context.SanPhams
		   .Where(l => l.Maloai == "L0003")
		   .ToListAsync();
            //Danh sach san pham tuong tu 
            List<SanPham> sanPhamTuongTu;
            if (sanPham.Maloai == "L0004") 
            {
                sanPhamTuongTu = await _context.SanPhams
                    .Where(sp => sp.Maloai != "L0004" && sp.MaSp != id)
                    .Take(4) 
                    .ToListAsync();
            }
            else 
            {
                sanPhamTuongTu = await _context.SanPhams
                    .Where(sp => sp.Maloai == "L0004" && sp.MaSp != id)
                    .Take(4)
                    .ToListAsync();
            }
            //san pham kit 
            var sizes = await _context.Sizes
             .Where(l => l.MaSize == "SZ001")  // Kiểm tra null
             .ToListAsync();

            // Kiểm tra sản phẩm có thuộc `Kit` không
            var kitDetails =  _context.Kits
                .Where(k => k.MaKit == id)
                .Include(k => k.MaSpNavigation)  // Bao gồm thông tin sản phẩm liên quan đến Kit
                .ToList();

            string kitInfo = string.Empty;
            if (kitDetails.Any())
            {
                foreach (var kit in kitDetails)
                {
                    kitInfo += $" +{kit.MaSpNavigation.Ten} x {kit.SoLuong} ";
                }
                ViewBag.IsInKit = true;  // Sản phẩm thuộc Kit
            }
            else
            {
                ViewBag.IsInKit = false; // Sản phẩm không thuộc Kit
            }
            if (sanPham.Maloai == "L0003" && sanPham.Maloai == "L0004")
            {
                sizes =  _context.Sizes.Where(l => l.MaSize == "SZ001").ToList();
            }
            //size từng sản phẩm 
            List<Size>  sizes1;
            
          sizes1 = await _context.Sizes.ToListAsync();
                foreach (var size in sizes1)
                {
                    size.Trongluong = size.Ten switch
                    {
                        "Nhỏ" => "300ml",
                        "Vừa" => "500ml",
                        "Lớn" => "700ml",
                        _ => "Unknown"
                    };
                }
            

        
            var viewModel = new ProductDetailViewModel
            {
				SanPhamTuongTu = sanPhamTuongTu,
				Product = sanPham,
				Toppings = toppings,
                Size1 = sizes1,
                Sizes = sizes
            };
            
            // Lấy sản phẩm từ database
            var product = _context.SanPhams
				.FirstOrDefault(p => p.MaSp == id);

			if (product == null)
			{
				return NotFound();
			}
			// Tính toán số sao trung bình và số lượt đánh giá
			var ratings = _context.ChitietDanhGia
				.Where(r => r.MaSp == id)
				.ToList();
			double averageRating = 0;
			int ratingCount = 0;
			if (ratings != null)
			{
				 averageRating = ratings.Any() ? Math.Round(ratings.Average(r => r.SoSao), 2) : 0;
				 ratingCount = ratings.Count;
			}
            ViewBag.AverageRating = averageRating;
			ViewBag.RatingCount = ratingCount;
			ViewBag.MaSp = id;
            ViewBag.KitInfo = kitInfo;
            return View(viewModel);
		}
		// GET: SanPhams/Create
		public IActionResult Create1()
        {
            ViewData["MaTopping"] = new SelectList(_context.SanPhams, "MaSp", "MaSp");
            ViewData["Maloai"] = new SelectList(_context.Loais, "Maloai", "Maloai");
            return View();
        }

        // POST: SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create1([Bind("MaSp,Ten,Dongia,Dvt,Mota,Anh,Maloai,MaTopping")] SanPham sanPham)
        {
            if (ModelState.IsValid)
            {
                // Nếu MaTopping không có giá trị, hãy đặt nó thành null
                if (string.IsNullOrEmpty(sanPham.MaTopping))
                {
                    sanPham.MaTopping = null; // Không cần thiết, nhưng có thể thêm để rõ ràng
                }

                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, tái tạo danh sách chọn cho MaTopping và Maloai
            ViewData["MaTopping"] = new SelectList(_context.SanPhams, "MaSp", "MaSp", sanPham.MaTopping);
            ViewData["Maloai"] = new SelectList(_context.Loais, "Maloai", "Maloai", sanPham.Maloai);
            return View(sanPham);
        }
		[SessionOrAuthorize]
		public IActionResult DanhGia(string ten, decimal dongia, string masp)
		{
			// Truyền dữ liệu đến View để hiển thị
			ViewBag.TenSanPham = ten;
			ViewBag.DonGia = dongia;
			ViewBag.MaSp = masp;

			// Lấy mã khách hàng từ session hoặc từ claims
			var maKh = HttpContext.Session.GetString("UserPhone") ?? HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH")?.Value;

			if (string.IsNullOrEmpty(maKh))
			{
				return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập nếu không có mã khách hàng
			}
			var nhanxet = _context.ChitietDanhGia
				.Where(r => r.MaKh == maKh && r.MaSp == masp)
				.Select(r => r.NhanXet)
				.FirstOrDefault();
			// Lấy đánh giá của khách hàng từ cơ sở dữ liệu
			var sosao = _context.ChitietDanhGia
				.Where(r => r.MaKh == maKh && r.MaSp == masp)
				.Select(r => r.SoSao)
				.FirstOrDefault();



			// Kiểm tra xem khách hàng đã mua sản phẩm chưa
			var hasPurchased = _context.Ctsponls
				.Any(c => c.MaSp == masp && c.MaKh == maKh);
			if (hasPurchased == false)
			{
				sosao = 0;
			}
			ViewBag.NhanXet = nhanxet ?? "";
			ViewBag.SoSao = sosao;
			ViewBag.HasPurchased = hasPurchased;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SubmitRating(int soSao, string maSp, string nhanXet)
		{
			if (soSao < 1 || soSao > 5)
			{
				return BadRequest("Số sao không hợp lệ.");
			}

			var maKh = HttpContext.Session.GetString("UserPhone") ??
						HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH")?.Value;

			if (string.IsNullOrEmpty(maKh))
			{
				return RedirectToAction("Login", "Account");
			}

			// Kiểm tra xem khách hàng đã mua sản phẩm chưa
			var hasPurchased = _context.Ctsponls.Any(c => c.MaSp == maSp && c.MaKh == maKh);
			if (!hasPurchased)
			{
				// Sử dụng JavaScript để hiển thị alert
				return Content("<script>alert('\\u0042\\u1ea1\\u006e \\u0063\\u0068\\u01b0\\u1ea3 \\u006d\\u0075\\u0061 \\u0073\\u1ea3\\u006e \\u0070\\u0068\\u1ea9\\u006d \\u006e\\u00e0\\u0079 \\u006e\\u00ean \\u006b\\u0068\\u00f4\\u006e\\u0067 \\u0074\\u1ec3 \\u0111\\u00e1nh \\u0067\\u0069\\u00e1.'); window.history.back();</script>", "text/html");
			}

			var existingRating = _context.ChitietDanhGia
				.FirstOrDefault(r => r.MaSp == maSp && r.MaKh == maKh);

			if (existingRating != null)
			{
				// Cập nhật đánh giá
				existingRating.SoSao = soSao;
				existingRating.NhanXet = nhanXet;
				_context.ChitietDanhGia.Update(existingRating);
			}
			else
			{
				// Tạo mới đánh giá
				var rating = new ChitietDanhGium
				{
					SoSao = soSao,
					MaSp = maSp,
					MaKh = maKh,
					NhanXet = nhanXet

				};

				_context.ChitietDanhGia.Add(rating);
			}

			_context.SaveChanges();

			return RedirectToAction("Details", new { id = maSp });
		}
		public IActionResult DanhSachDanhGia(string maSp)
		{
			// Lấy danh sách đánh giá của sản phẩm
			var danhSachDanhGia = _context.ChitietDanhGia
				.Where(dg => dg.MaSp == maSp)
				.Select(dg => new {
					TenKhachHang = dg.MaKhNavigation.Ten, // Lấy tên khách hàng từ bảng KhachHang
					SoSao = dg.SoSao,
					NhanXet = dg.NhanXet
				}).ToList();
			// Truyền danh sách đánh giá vào ViewBag hoặc ViewModel
			ViewBag.DanhSachDanhGia = danhSachDanhGia;
			ViewBag.MaSp = maSp;

			return View();
		}





		private bool SanPhamExists(string id)
        {
            return (_context.SanPhams?.Any(e => e.MaSp == id)).GetValueOrDefault();
        }

    }
}
