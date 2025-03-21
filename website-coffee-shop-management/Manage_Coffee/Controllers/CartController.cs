
using Manage_Coffee.Helpers;
using Manage_Coffee.Models;
using Manage_Coffee.Models.ViewModels;
using Manage_Coffee.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using static iTextSharp.text.pdf.AcroFields;

namespace Manage_Coffee.Controllers
{
    public class CartController : Controller
    {
        private readonly Cf2Context _context;
        private readonly PaypalClient _paypalClient;
        private readonly IVnPayService _vnPayservice;
        public CartController(Cf2Context context, PaypalClient paypalClient, IVnPayService vnPayservice)
        {
            _paypalClient = paypalClient;
            _context = context;
            _vnPayservice = vnPayservice;
        }
        // Comment test
        public string GetCustomerId()
        {
            string makh = "";
            if (HttpContext.Session.GetString("UserName") == null)
            {
                var makhClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH");
                makh = makhClaim?.Value;
            }
            else
            {
                makh = HttpContext.Session.GetString("UserPhone");
            }

            return makh;
        }


        public IActionResult Cart()
        {
            List<CartItem> Cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key) ?? new List<CartItem>();
            foreach (var item in Cart)
            {
                // Lấy kit cho sản phẩm
                item.Kits = _context.Kits.Where(k => k.MaKit == item.ProductID).Include(k => k.MaSpNavigation).ToList();
                item.SanPhams = _context.SanPhams.Where(s => s.MaSp == item.ProductID).ToList();

                // Lấy topping cho sản phẩm
                item.Toppings = _context.SanPhams
                    .Where(sp => item.Toppings.Select(t => t.MaSp).Contains(sp.MaSp))
                    .ToList();
                item.CTKITs = _context.CTKITs
                        .Where(ct =>  ct.MaPhieuonl == "khong" ) // Điều kiện tìm kiếm
                        .Include(ct => ct.MaSizeNavigation) // Nếu có, bao gồm thông tin kích thước
                        .ToList();
            }

            // Tính tổng tiền, bao gồm cả giá topping
            decimal grandTotal = Cart.Sum(X => X.Soluong *
                (X.Dongia + X.TriGia + X.Toppings.Sum(t => t.Dongia))); // Thêm giá topping vào tổng tiền

            CartItemViewModel cart1 = new()
            {
                CartItems = Cart,
                GrandTotal = grandTotal,
                Discounts = _context.DanhMucKms.ToList() // Lấy danh sách thẻ giảm giá từ cơ sở dữ liệu
            };
            return View(cart1);
        }

        [HttpPost]
        public IActionResult ApplyDiscount(string discountCode)
        {
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);
            if (cart == null || !cart.Any())
            {
                return BadRequest("Giỏ hàng trống.");
            }

            var discount = _context.DanhMucKms.SingleOrDefault(d => d.MaKm == discountCode);
            if (discount == null || discount.Soluong <= 0 || discount.Ngayhethan < DateTime.Now)
            {
                return BadRequest("Mã giảm giá không hợp lệ.");
            }

            // Tính toán tổng tiền có cả giá topping
            decimal grandTotal = cart.Sum(item => (item.Dongia + item.TriGia + item.Toppings.Sum(t => t.Dongia)) * item.Soluong);

            // Tính toán và áp dụng giảm giá
            decimal discountAmount = discount.GiaTri; // Giả sử là giá trị giảm giá
            decimal newTotal = grandTotal * (1 - discountAmount / 100); // Áp dụng giảm giá

            // Cập nhật lại tổng tiền trong ViewBag hoặc Model
            ViewBag.DiscountApplied = discountAmount;
            ViewBag.NewTotal = newTotal;

            // Quay lại View giỏ hàng
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public async Task<IActionResult> Add(string id, string idsize, string ghiChu, List<string> kits, string idda, string idduong, List<string> toppings)
        {
            SanPham sanPham = await _context.SanPhams.FindAsync(id);

            // Default values if specific product types are selected
            if (sanPham.Maloai == "L0004")
            {
                idsize = "SZ001";
                idda = "DD001";
                idduong = "NQ001";
            }
            if (sanPham.Maloai == "L0005")
            {
                idda = "DD001";
                idduong = "NQ001";
            }

            if (string.IsNullOrEmpty(idsize))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn kích thước trước khi thêm sản phẩm vào giỏ hàng.";
                return Redirect(Request.Headers["Referer"].ToString());
            }
			if (string.IsNullOrEmpty(idda))
			{
				TempData["ErrorMessage"] = "Vui lòng chọn lượng đá trước khi thêm sản phẩm vào giỏ hàng.";
				return Redirect(Request.Headers["Referer"].ToString());
			}
			if (string.IsNullOrEmpty(idduong))
			{
				TempData["ErrorMessage"] = "Vui lòng chọn lượng đường trước khi thêm sản phẩm vào giỏ hàng.";
				return Redirect(Request.Headers["Referer"].ToString());
			}
			Size size = await _context.Sizes.FindAsync(idsize);
            Da da = await _context.Das.FindAsync(idda);
            Duong duong = await _context.Duongs.FindAsync(idduong);

            // Retrieve the selected toppings from the database
            List<SanPham> toppingProducts = new List<SanPham>();
            if (toppings != null && toppings.Count > 0)
            {
                toppingProducts = await _context.SanPhams
                    .Where(sp => toppings.Contains(sp.MaSp))
                    .ToListAsync();
            }
            // Retrieve or initialize the cart
            List<CartItem> Cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key) ?? new List<CartItem>();
            CartItem cartItem = Cart
                .FirstOrDefault(c => c.ProductID == id && c.SizeID == idsize && c.DaID == idda && c.DuongID == idduong);

            // If the item doesn't exist in the cart, create a new one
            if (cartItem == null)
            {
                cartItem = new CartItem(sanPham, size, duong, da, ghiChu)
                {
                    Toppings = toppingProducts // Assign selected toppings
                };
                Cart.Add(cartItem);
            }
            else
            {
                cartItem.Soluong += 1;
                cartItem.Ghichu = ghiChu;

                // Update the toppings list if there are new toppings selected
                if (toppingProducts.Count > 0)
                {
                    cartItem.Toppings = toppingProducts;
                }
            }
            if (sanPham.Maloai == "L0005")
            {
                var listkits = _context.Kits.Where(s => s.MaKit == id).ToList();

                foreach (var kitDetail in listkits)
                {
                    var kitProduct = await _context.SanPhams.FindAsync(kitDetail.MaSp);
                    if (kitProduct != null && (kitProduct.Maloai == "L0001" || kitProduct.Maloai == "L0002"))
                    {
                        // Kiểm tra sự tồn tại của CTKIT trước khi thêm mới
                        bool exists = await _context.CTKITs.AnyAsync(ct =>
                            ct.MaKH == GetCustomerId() &&
                            ct.MaPhieuonl == "khong" &&
                            ct.MaSize == idsize &&
                            ct.MaKit == id &&
                            ct.MaSp == kitDetail.MaSp);

                        if (!exists) // Nếu không tồn tại, thì thêm mới
                        {
                            var ctKit = new CTKIT
                            {
                                MaKH = GetCustomerId(), // Sử dụng mã khách hàng đã lấy
                                MaPhieuonl = "khong", // Thay thế với ID hóa đơn thực tế
                                MaSize = idsize,
                                MaKit = id,
                                MaSp = kitDetail.MaSp,
                                SoLuong = kitDetail.SoLuong // Sử dụng số lượng từ kit
                            };

                            await _context.CTKITs.AddAsync(ctKit);
                        }
                    }
                }
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            }



            // Update session with the new cart data
            HttpContext.Session.Set(MySetting.Cart_key, Cart);

            return Redirect(Request.Headers["Referer"].ToString());
        }
         public async Task<IActionResult> Decrease(string id, string idsize)
        {
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key) ?? new List<CartItem>();
            var ctKitsToUpdate = _context.CTKITs.Where(ct => ct.MaPhieuonl == "khong" && ct.MaKH == GetCustomerId() && ct.MaKit==id && ct.MaSize==idsize).ToList();
            CartItem cartItem = cart.Where(c => c.ProductID == id && c.SizeID == idsize).FirstOrDefault();
            if (cartItem.Soluong > 1)
            {
                --cartItem.Soluong;
            }
            else
            {
                cart.Remove(cartItem);
                foreach (var ctKit in ctKitsToUpdate)
                {
                    _context.CTKITs.Remove(ctKit);  // Xóa ctKit hiện tại
                }
                _context.SaveChanges();
            }
            HttpContext.Session.Set(MySetting.Cart_key, cart);

            return RedirectToAction("Cart");
        }
        public async Task<IActionResult> Increase(string id, string idsize)
        {
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);
            CartItem cartItem = cart.Where(c => c.ProductID == id && c.SizeID == idsize).FirstOrDefault();
            if (cartItem.Soluong >= 1)
            {
                ++cartItem.Soluong;
            }
            else
            {
                cart.RemoveAll(p => p.Soluong > 0);
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove(MySetting.Cart_key);
            }
            else
            {
                HttpContext.Session.Set(MySetting.Cart_key, cart);
            }
            return RedirectToAction("Cart");
        }
        public async Task<IActionResult> Remove(string id, string idsize)
        {
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);
            var ctKitsToUpdate = _context.CTKITs.Where(ct => ct.MaPhieuonl == "khong" && ct.MaKH == GetCustomerId() && ct.MaKit == id && ct.MaSize == idsize).ToList();
            cart.RemoveAll(c => c.ProductID == id && c.SizeID == idsize);

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
                foreach (var ctKit in ctKitsToUpdate)
                {
                    _context.CTKITs.Remove(ctKit);  // Xóa ctKit hiện tại
                }
                _context.SaveChanges();
            }
            else
            {
                HttpContext.Session.Set(MySetting.Cart_key, cart);
            }
            return RedirectToAction("Cart");
        }
        public async Task<IActionResult> Clear(string id)
        {
            var ctKitsToUpdate = _context.CTKITs.Where(ct => ct.MaPhieuonl == "khong" && ct.MaKH == GetCustomerId() && ct.MaKit == id ).ToList();
            foreach (var ctKit in ctKitsToUpdate)
            {
                _context.CTKITs.Remove(ctKit);  // Xóa ctKit hiện tại
            }
            _context.SaveChanges();
            HttpContext.Session.Remove(MySetting.Cart_key);
            return RedirectToAction("Sanpham1");
        }
        [SessionOrAuthorize]
        [HttpGet]

        public IActionResult Checkout()
        {
            var makh = "";


            var kits = _context.Kits.ToList();
            if (HttpContext.Session.GetString("UserName") == null)
            {
                var makhClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH");
                makh = makhClaim?.Value;
            }
            else
            {
                makh = HttpContext.Session.GetString("UserPhone");
            }
            var khachHang = _context.KhachHangs
                .Where(r => r.MaKh == makh)
                .FirstOrDefault();
            // Gán số xu vào ViewBag
            if (khachHang != null)
            {
                ViewBag.SoXu = khachHang.Xu;
            }
            else
            {
                ViewBag.SoXu = 0; // Trường hợp khách hàng chưa đăng nhập hoặc không có số xu
            }
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);
            foreach (var items in cart)
            {
                items.SanPhams = _context.SanPhams.Where(s => s.MaSp == items.ProductID).ToList();
            }
            if (cart == null || cart.Count == 0)
            {
                return Redirect("/");
            }

            ViewBag.PaypalClientdId = _paypalClient.ClientId;

            var viewModel = new CartItemViewModel
            {
                KitsAsKit = kits,
                CartItems = cart, // Gán danh sách cart vào thuộc tính CartItems
                Discounts = new List<DanhMucKm>() // Khởi tạo danh sách Discounts
            };

            viewModel.Discounts = GetDiscounts(); // Ví dụ, hãy đảm bảo phương thức này trả về danh sách giảm giá

            return View(viewModel);
        }
        private List<DanhMucKm> GetDiscounts()
        {
            // Giả sử bạn có một dịch vụ hoặc repository để lấy giảm giá
            // Ví dụ, nếu bạn sử dụng Entity Framework
            using (var context = new Cf2Context())
            {
                return context.DanhMucKms.ToList(); // Lấy danh sách giảm giá từ cơ sở dữ liệu
            }
        }

        //Cart
        //[Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutKH checkoutKH, string macn, string MaKm, 
            decimal totalValue, string MaCn, int SoXu, string payment = "COD")
        {
            if (string.IsNullOrEmpty(MaCn))
            {
                TempData["Error"] = "Vui lòng chọn mã chi nhánh trước khi tiếp tục thanh toán.";
                return RedirectToAction("Checkout"); // Thay thế "CheckoutPage" bằng tên action hiển thị trang Checkout của bạn
            }
            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);

            var makh = "";
            int tongtien = 0;
            if (HttpContext.Session.GetString("UserName") == null)
            {
                var makhClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH");
                makh = makhClaim?.Value;
            }
            else
            {
                makh = HttpContext.Session.GetString("UserPhone");
            }
            if (string.IsNullOrEmpty(makh))
            {
                throw new Exception("Mã khách hàng không tồn tại trong Claims!");
            }

            var khachhang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == makh);
            if (khachhang == null)
            {
                throw new Exception($"Không tìm thấy khách hàng với mã: {makh}");
            }
            var tongChiTieu = _context.Phieudhonls
                .Where(hd => hd.MaKh == makh)
                .Sum(hd => hd.TongTien);
            if (tongChiTieu > 120000)
            {
                khachhang.Role = "Vàng";
            }
            else if (tongChiTieu > 100000)
            {
                khachhang.Role = "Bạc";
            }
            else
            {
                khachhang.Role = "Đồng";
            }
            _context.Update(khachhang);
            _context.SaveChanges();

            Console.WriteLine($"Khách hàng: {khachhang.Ten}, Role: {khachhang.Role}");
            ViewBag.Role = khachhang.Role;
            var hoadon = new Phieudhonl
            {
                MaPhieuonl = makh + DateTime.Now,
                MaKh = makh,
                DiaChi = checkoutKH.DiaChi ?? khachhang.Diachi,
                Ngaygiodat = DateTime.Now,
                Ptnh = "GRAB",
                Pttt = "COD",
                TrangThai = false,
                MaKm = MaKm,
                MaCn = MaCn,
                TongTien = 0,
                TienShip = 15000
            };
            // Bắt đầu giao dịch
            using (var transaction = _context.Database.BeginTransaction())
            {

                try
                {
                    tongtien = 0;
                    var ctsp = new List<Ctsponl>();
                    var ctToppings = new List<CtTopping>();
                    var ctKitsToUpdate = _context.CTKITs.Where(ct => ct.MaPhieuonl == "khong" && ct.MaKH == GetCustomerId()).ToList();
                    foreach (var item in cart)
                    {
                        int toppingPrice = item.Toppings.Sum(t => t.Dongia);
                        int totalProductPrice = (item.Dongia + item.TriGia + toppingPrice) * item.Soluong;
                        tongtien += totalProductPrice;
                        var ctsponlItem = new Ctsponl
                        {
                            MaKh = makh,
                            MaPhieuonl = hoadon.MaPhieuonl,
                            Soluong = item.Soluong,
                            MaSp = item.ProductID,
                            MaSize = item.SizeID,
                            MaDa = item.DaID,
                            MaDuong = item.DuongID,
                            Gia = item.Dongia,
                            Ghichu = item.Ghichu,
                            Tongtien = totalProductPrice
                        };

                        ctsp.Add(ctsponlItem);

                        // Thêm chi tiết topping vào danh sách cho từng topping của sản phẩm này
                        foreach (var topping in item.Toppings)
                        {
                            ctToppings.Add(new CtTopping
                            {
                                MaTopping = topping.MaSp,
                                MaSp = item.ProductID,
                                MaPhieuonl = hoadon.MaPhieuonl,
                                MaKH = makh,

                            });
                        }
                        //foreach (var topping in item.Toppings)
                        //{
                        //    ctToppings.Add(new CtTopping
                        //    {
                        //        MaTopping = topping.MaSp,
                        //        MaSp = item.ProductID,
                        //        MaPhieuonl = hoadon.MaPhieuonl,
                        //        MaKH = makh,

                        //    });
                        //}

                    }
                    if (totalValue == 0)
                    {
                        hoadon.TongTien = tongtien + hoadon.TienShip;
                    }
                    else
                    {
                        hoadon.TongTien = (int)totalValue + hoadon.TienShip;
                    }
                    hoadon.TongTien = totalValue == 0 ? tongtien + hoadon.TienShip : (int)totalValue + hoadon.TienShip;
					HttpContext.Session.SetString("MaKm", MaKm ?? string.Empty);
                    var ctKitDetails = ctKitsToUpdate.Select(ctKit => new
                    {
                        ctKit.MaKH,
                        ctKit.MaSize,
                        ctKit.MaKit,
                        ctKit.MaSp,
                        ctKit.SoLuong,
                    }).ToList();

                    // Xóa các bản ghi cũ
                    foreach (var ctKit in ctKitsToUpdate)
                    {
                        _context.CTKITs.Remove(ctKit);
                    }

                    // Thêm các bản ghi mới với MaPhieuonl đã cập nhật
                    foreach (var ctKitDetail in ctKitDetails)
                    {
                        var newCtKit = new CTKIT
                        {
                            MaPhieuonl = hoadon.MaPhieuonl,  // Gán mã phiếu mới
                            MaKH = ctKitDetail.MaKH,
                            MaSize = ctKitDetail.MaSize,
                            MaKit = ctKitDetail.MaKit,
                            MaSp = ctKitDetail.MaSp,
                            SoLuong = ctKitDetail.SoLuong,
                            // Gán các thuộc tính khác đã lưu từ ctKit cũ
                        };
                        _context.CTKITs.Add(newCtKit);
                    }

                    _context.Add(hoadon);
                    _context.SaveChanges();
                    _context.AddRange(ctsp);
                    _context.AddRange(ctToppings);
                    _context.SaveChanges();
                    var khachHang = _context.KhachHangs
                    .Where(r => r.MaKh == makh)
                    .FirstOrDefault();
                    if (khachHang != null && khachHang.Xu >= SoXu)
                    {
                        khachHang.Xu -= SoXu; // Giảm xu
                        _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    }
                    var km = _context.DanhMucKms
                    .Where(r => r.MaKm == MaKm)
                    .FirstOrDefault();
                    if (km != null)
                    {
                        km.Soluong--; // Giảm xu
                        _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    }
					HttpContext.Session.SetString("SoXu", SoXu.ToString());

					transaction.Commit();

                    if (payment == "Thanh toán VNPay")
                    {
                        hoadon.Pttt = "VNPay";
						var maKmFromSession = HttpContext.Session.GetString("MaKm");
						var soXu = HttpContext.Session.GetString("SoXu");
						return ProcessVnPayPayment(hoadon.TongTien, khachhang, maKmFromSession,soXu);
					}
                    HttpContext.Session.Set<List<CartItem>>(MySetting.Cart_key, new List<CartItem>());
                    return RedirectToAction("XemHoaDon", new { maPhieuonl = hoadon.MaPhieuonl, soxu = SoXu });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        public IActionResult XemHoaDon(string maPhieuonl, int soxu)
        {
            var phieuDh = _context.Phieudhonls
             .Include(p => p.Ctsponls)
                 .ThenInclude(ct => ct.MaSizeNavigation)
             .Include(p => p.Ctsponls)
                 .ThenInclude(ct => ct.MaDaNavigation)
             .Include(p => p.Ctsponls)
                 .ThenInclude(ct => ct.MaDuongNavigation)
             .Include(p => p.Ctsponls)
                 .ThenInclude(ct => ct.MaSpNavigation)
             .Include(p => p.CtToppings)
                 .ThenInclude(ct => ct.MaToppingNavigation)
             .Include(p => p.MaCnNavigation)
             .Include(p => p.MaKhNavigation)
             .Include(p => p.MaKmNavigation)
             .FirstOrDefault(p => p.MaPhieuonl == maPhieuonl);
            if (phieuDh == null)
            {
                return NotFound("Hóa đơn không tồn tại.");
            }
            HttpContext.Session.SetString("MaPhieuonl", maPhieuonl);            // Lấy chi tiết sản phẩm và danh sách Kit
            var chiTietSanPham = phieuDh.Ctsponls.Select(ct => new ChiTietSanPhamViewModel
            {
                MaSp = ct.MaSp,
                Ten = ct.MaSpNavigation.Ten,
                MaSize = ct.MaSize,
                MaDuong = ct.MaDuong,
                MaDa = ct.MaDa,
                TenSize = ct.MaSizeNavigation?.Ten ?? "Không xác định",
                TenDa = ct.MaDaNavigation?.Ten ?? "Không xác định",
                TenDuong = ct.MaDuongNavigation?.Ten ?? "Không xác định",
                Soluong = ct.Soluong,
                Gia = ct.Gia,
                Tongtien = ct.Tongtien,
                Ghichu = ct.Ghichu,
                Kits = _context.Kits.Where(k => k.MaKit == ct.MaSp).Include(k => k.MaSpNavigation).ToList(), // Lọc danh sách Kit theo mã sản phẩm
                CTKITs = _context.CTKITs.Where(c => c.MaPhieuonl == ct.MaPhieuonl && c.MaKit==ct.MaSp).Include(ct => ct.MaSizeNavigation).ToList()
            }).ToList();
            var chiTietTopping = phieuDh.CtToppings.Select(ct => new ChitiettoppingViewModel
            {
                MaTopping = ct.MaTopping,
                MaSp=ct.MaSp,
                TenTopping = ct.MaToppingNavigation?.Ten ?? "không xác định ",
            }).ToList();
            var hoaDon = new HoaDonViewModel
            {
                MaPhieuonl = phieuDh.MaPhieuonl,
                Ngaygiodat = phieuDh.Ngaygiodat,
                TongTien = phieuDh.TongTien,
                DiaChi = phieuDh.DiaChi,
                TrangThai = phieuDh.TrangThai,
                Pttt = phieuDh.Pttt,
                TienShip = phieuDh.TienShip,
                SoXu = soxu,
                Ptnh = phieuDh.Ptnh,
                MaKh = phieuDh.MaKh,
                MaCn = phieuDh.MaCn,
                MaKm = phieuDh.MaKm,
                ChiTietSanPham = chiTietSanPham,
                ChiTiettopping = chiTietTopping,
                TenKhachHang = phieuDh.MaKhNavigation?.Ten ?? "Không xác định",
                EmailKhachHang = phieuDh.MaKhNavigation?.Email ?? "Không xác định",
                TenChiNhanh = phieuDh.MaCnNavigation?.Ten ?? "Không xác định",
                TenKhuyenMai = phieuDh.MaKmNavigation != null ? phieuDh.MaKmNavigation.Ten : "Không có khuyến mãi",
                Role = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == phieuDh.MaKh)?.Role ?? "Không xác định",
                GiaTrikm = phieuDh.MaKmNavigation?.GiaTri
            };
			var soXu = TempData["SoXu"] as string;
			ViewBag.SoXu = soXu;

			return View(hoaDon); // Trả về view hiển thị hóa đơn
        }


        [HttpPost]
        public async Task<IActionResult> SendInvoice(string email)
        {
            var maPhieuonl = HttpContext.Session.GetString("MaPhieuonl");
            if (string.IsNullOrEmpty(maPhieuonl))
            {
                TempData["AlertMessage"] = "Mã hóa đơn không hợp lệ.";
                TempData["AlertType"] = "alert-warning";
                return RedirectToAction("XemHoaDon", new { maPhieuonl });
            }

            if (string.IsNullOrEmpty(email))
            {
                TempData["AlertMessage"] = "Vui lòng nhập email.";
                TempData["AlertType"] = "alert-warning";
                return RedirectToAction("XemHoaDon", new { maPhieuonl });
            }

            var phieuDh = await _context.Phieudhonls
                .Include(p => p.Ctsponls)
                    .ThenInclude(ct => ct.MaSizeNavigation)
                .Include(p => p.Ctsponls)
                    .ThenInclude(ct => ct.MaDaNavigation)
                .Include(p => p.Ctsponls)
                    .ThenInclude(ct => ct.MaDuongNavigation)
                .Include(p => p.Ctsponls)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .Include(p => p.CtToppings)
                    .ThenInclude(ct => ct.MaToppingNavigation)
                .Include(p => p.MaCnNavigation)
                .Include(p => p.MaKhNavigation)
                .Include(p => p.MaKmNavigation)
                .FirstOrDefaultAsync(p => p.MaPhieuonl == maPhieuonl);

            if (phieuDh == null)
            {
                TempData["AlertMessage"] = "Hóa đơn không tồn tại.";
                TempData["AlertType"] = "alert-warning";
                return RedirectToAction("XemHoaDon", new { maPhieuonl });
            }

            var hoaDon = new HoaDonViewModel
            {
                MaPhieuonl = phieuDh.MaPhieuonl,
                Ngaygiodat = phieuDh.Ngaygiodat,
                TongTien = phieuDh.TongTien,
                DiaChi = phieuDh.DiaChi,
                TrangThai = phieuDh.TrangThai,
                TenKhachHang = phieuDh.MaKhNavigation?.Ten ?? "Không xác định",
                TenChiNhanh = phieuDh.MaCnNavigation?.Ten ?? "Không xác định",
                TenKhuyenMai = phieuDh.MaKmNavigation != null ? phieuDh.MaKmNavigation.Ten : "Không có khuyến mãi",
                Role = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == phieuDh.MaKh)?.Role ?? "Không xác định",
                Pttt = phieuDh.Pttt,
                ChiTietSanPham = phieuDh.Ctsponls.Select(ct => new ChiTietSanPhamViewModel
                {
                    TenSize = ct.MaSizeNavigation?.Ten ?? "Không xác định",
                    TenDa = ct.MaDaNavigation?.Ten ?? "Không xác định",
                    TenDuong = ct.MaDuongNavigation?.Ten ?? "Không xác định",
                    MaSp = ct.MaSp,
                    Ten = ct.MaSpNavigation.Ten,
                    Soluong = ct.Soluong,
                    Gia = ct.Gia,
                    Tongtien = ct.Tongtien
                }).ToList(),
                EmailKhachHang = email
            };

            await SendInvoiceEmail(hoaDon);
            TempData["AlertMessage"] = "Gửi hóa đơn thành công!";
            TempData["AlertType"] = "alert-success";
            return RedirectToAction("XemHoaDon", new { maPhieuonl });
        }

        private IActionResult ProcessVnPayPayment(int? totalInvoiceAmount, KhachHang khachhang, string maKm, string soXu)
        {
            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = (double)totalInvoiceAmount,
                CreatedDate = DateTime.Now,
                Description = $"{khachhang.Ten} {khachhang.Sdt}",
                FullName = khachhang.Ten,
                OrderId = new Random().Next(1000, 100000)
            };
			HttpContext.Session.SetString("SoXu", soXu.ToString());
			HttpContext.Session.SetString("MaKm", maKm);
			HttpContext.Session.SetString("VnPayAmount", vnPayModel.Amount.ToString());
            return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
        }

        private async Task SendInvoiceEmail(HoaDonViewModel hoaDon)
        {
            Random random = new Random();
            string key = random.Next(100000, 999999).ToString();

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("LE PETIT CAFE", "qynhnguyen14@gmail.com"));
                message.To.Add(new MailboxAddress("Khách hàng", hoaDon.EmailKhachHang));
                message.Subject = "HÓA ĐƠN CỦA BẠN TỪ QUÁN LE PETIT CAFE";

                // Tạo nội dung HTML với phong cách chuyên nghiệp
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; background-color:#27408B ; border-radius: 8px; border: 1px solid #ddd;'>
                <div style='text-align: center; margin-bottom: 20px;'>
                
                    <h2 style='color: #b5651d;'>Le Petit Café</h2>
                </div>
                <p>Chào <strong>{hoaDon.TenKhachHang}</strong>,</p>
                <p>Cảm ơn bạn đã đặt hàng. Dưới đây là hóa đơn của bạn:</p>

                <div style='background: #ffffff; padding: 15px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 8px;'><strong>Tên Chi Nhánh:</strong></td>
                            <td>{hoaDon.TenChiNhanh}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px;'><strong>Thành Viên:</strong></td>
                            <td>{hoaDon.Role}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px;'><strong>Khuyến Mãi:</strong></td>
                            <td>{hoaDon.TenKhuyenMai}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px;'><strong>Ngày Đặt:</strong></td>
                            <td>{hoaDon.Ngaygiodat}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px;'><strong>Tổng Tiền:</strong></td>
                            <td>{hoaDon.TongTien} VND</td>
                        </tr>
                    </table>

                    <h3 style='color: #b5651d; margin-top: 20px;'>Chi tiết sản phẩm:</h3>
                    <ul style='list-style-type: none; padding: 0;'>
                        {string.Join("", hoaDon.ChiTietSanPham.Select(item => $@"
                            <li style='padding: 10px; border-bottom: 1px solid #eee;'>
                                <strong>{item.Ten}</strong>: {item.Soluong} x {item.Gia} = {item.Tongtien} VND<br/>
                                {(string.IsNullOrEmpty(item.TenSize) ? "" : $"<small>Size: {item.TenSize}</small><br/>")}
                                {(string.IsNullOrEmpty(item.TenDa) ? "" : $"<small>Đá: {item.TenDa}</small><br/>")}
                                {(string.IsNullOrEmpty(item.TenDuong) ? "" : $"<small>Đường: {item.TenDuong}</small>")}
                            </li>"))}
                    </ul>

                    <p><strong>Địa chỉ giao hàng:</strong> {hoaDon.DiaChi}</p>
                    <p><strong>Phương thức thanh toán:</strong> {hoaDon.Pttt}</p>
                </div>

                <h4 style='margin-top: 20px;'>Xin cảm ơn và hẹn gặp lại!</h4>
            </div>";

                message.Body = bodyBuilder.ToMessageBody();

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync("qynhnguyen14@gmail.com", "chlx wthd qwtq cfmp");
                    await smtp.SendAsync(message);
                    await smtp.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
            }
        }



        public IActionResult PaymentSuccess()
        {
            return View("XemHoaDon");
        }
        public IActionResult PaymentSuccess1()
        {
            return View("Success");
        }

        #region Paypal payment
        //[Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder([FromBody] PaypalOrderRequest request, CancellationToken cancellationToken)
        {
            //var maKm = "";
            var totalValue = "";


            List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);
            if (/*request != null &&*/ request.TotalValue != 0)
            {
                totalValue = request.TotalValue.ToString();
            }
            else
            {
                totalValue = cart.Sum(q => q.Total).ToString();
            }
            if (cart == null || cart.Count == 0)
            {
                return BadRequest("Giỏ hàng trống.");
            }

            // Thông tin đơn hàng gửi qua Paypal
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                // Tạo đơn hàng PayPal với tổng giá trị sau giảm
                var response = await _paypalClient.CreateOrder(totalValue, donViTienTe, maDonHangThamChieu);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }



        //[Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, [FromBody] PaypalOrderRequest request, CancellationToken cancellationToken)
        {
            var totalValue = 0;
            int tongtien = 0;
            var maCn = request.MaCn.ToString();
            var maKm = "";
            try
            {
                // Gọi PayPal API để capture order
                var response = await _paypalClient.CaptureOrder(orderID);

                // Tiến hành lưu hóa đơn với thông tin mã giảm giá
                List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);
                if (request.TotalValue != 0)
                {
                    totalValue = (int)request.TotalValue;
                }
                else
                {
                    totalValue = (int)cart.Sum(q => q.Total);
                }
                if (cart == null || cart.Count == 0)
                {
                    return BadRequest("Giỏ hàng trống");
                }
                if (request.MaKm != "")
                {
                    maKm = request.MaKm;
                }
                else
                {
                    maKm = null;
                }


                // Lấy thông tin khách hàng từ Claims hoặc Session
                var makh = ""; //HttpContext.Session.GetString("UserPhone") ?? HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH")?.Value;
                if (HttpContext.Session.GetString("UserName") == null)
                {
                    var makhClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH");
                    makh = makhClaim?.Value;
                }
                else
                {
                    makh = HttpContext.Session.GetString("UserPhone");
                }
                if (string.IsNullOrEmpty(makh))
                {
                    return BadRequest("Mã khách hàng không tồn tại");
                }

                var khachhang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == makh);
                if (khachhang == null)
                {
                    return BadRequest($"Không tìm thấy khách hàng với mã: {makh}");
                }

                // Tạo hóa đơn
                var hoadon = new Phieudhonl
                {
                    MaPhieuonl = makh + DateTime.Now,
                    MaKh = makh,
                    DiaChi = request.DiaChi ?? khachhang.Diachi,
                    Ngaygiodat = DateTime.Now,
                    Ptnh = "GRAB",
                    Pttt = "PayPal",
                    TrangThai = true,
                    MaKm = maKm, // Gán mã giảm giá vào hóa đơn
                    MaCn = maCn,
                    TongTien = (int)totalValue, // Gán tổng giá trị sau giảm
                    TienShip = 15000// Hoặc tính toán phí ship từ trước
                };

                // Tạo danh sách chi tiết sản phẩm
                var ctsp = new List<Ctsponl>();
                var ctToppings = new List<CtTopping>();
                var ctKitsToUpdate = _context.CTKITs.Where(ct => ct.MaPhieuonl == "khong" && ct.MaKH == GetCustomerId()).ToList();
                foreach (var item in cart)
                {
                    tongtien = 0;
                    int toppingPrice = item.Toppings.Sum(t => t.Dongia);
                    int totalProductPrice = (item.Dongia + item.TriGia + toppingPrice) * item.Soluong;
                    tongtien += totalProductPrice;
                    var ctsponlItem = new Ctsponl
                    {
                        MaKh = makh,
                        MaPhieuonl = hoadon.MaPhieuonl,
                        Soluong = item.Soluong,
                        MaSp = item.ProductID,
                        MaSize = item.SizeID,
                        MaDa = item.DaID,
                        MaDuong = item.DuongID,
                        Gia = item.Dongia,
                        Ghichu = item.Ghichu,
                        Tongtien = totalProductPrice
                    };

                    ctsp.Add(ctsponlItem);

                    // Thêm chi tiết topping vào danh sách cho từng topping của sản phẩm này
                    foreach (var topping in item.Toppings)
                    {
                        ctToppings.Add(new CtTopping
                        {
                            MaTopping = topping.MaSp,
                            MaSp = item.ProductID,
                            MaPhieuonl = hoadon.MaPhieuonl,
                            MaKH = makh,

                        });
                    }

                }

                // Bắt đầu giao dịch
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var ctKitDetails = ctKitsToUpdate.Select(ctKit => new
                        {
                            ctKit.MaKH,
                            ctKit.MaSize,
                            ctKit.MaKit,
                            ctKit.MaSp,
                            ctKit.SoLuong,
                        }).ToList();

                        // Xóa các bản ghi cũ
                        foreach (var ctKit in ctKitsToUpdate)
                        {
                            _context.CTKITs.Remove(ctKit);
                        }

                        // Thêm các bản ghi mới với MaPhieuonl đã cập nhật
                        foreach (var ctKitDetail in ctKitDetails)
                        {
                            var newCtKit = new CTKIT
                            {
                                MaPhieuonl = hoadon.MaPhieuonl,  // Gán mã phiếu mới
                                MaKH = ctKitDetail.MaKH,
                                MaSize = ctKitDetail.MaSize,
                                MaKit = ctKitDetail.MaKit,
                                MaSp = ctKitDetail.MaSp,
                                SoLuong = ctKitDetail.SoLuong,
                                // Gán các thuộc tính khác đã lưu từ ctKit cũ
                            };
                            _context.CTKITs.Add(newCtKit);
                        }

                        // Lưu thay đổi vào cơ sở dữ liệu
                        await _context.SaveChangesAsync();
                        // Lưu hóa đơn
                        _context.Add(hoadon);
                        await _context.SaveChangesAsync();

                        // Lưu chi tiết hóa đơn
                        _context.AddRange(ctsp);
                        await _context.SaveChangesAsync();

                        decimal totalOrderValue = (decimal)totalValue; // Tổng giá trị hóa đơn đã tính phí ship
                        decimal bonusPoints = 0;

                        // Tùy vào role mà cộng phần trăm tương ứng
                        if (khachhang.Role == "Đồng")
                        {
                            bonusPoints = totalOrderValue * 0.01m;

                        }

                        else if (khachhang.Role == "Bạc")
                        {
                            bonusPoints = totalOrderValue * 0.02m;
                        }
                        else if (khachhang.Role == "Vàng")
                        {
                            bonusPoints = totalOrderValue * 0.03m;
                        }
                        // Cộng xu vào tài khoản khách hàng
                        if (khachhang.Xu == null)
                        {
                            khachhang.Xu = (int)bonusPoints;
                        }
                        else
                        {
                            khachhang.Xu += (int)bonusPoints;
                        }
                        _context.KhachHangs.Update(khachhang);
                        await _context.SaveChangesAsync();

                        int SoXu = (int)request.SoXu;
                        var khachHang = _context.KhachHangs
                        .Where(r => r.MaKh == makh)
                        .FirstOrDefault();
                        if (khachHang != null)
                        {
                            khachHang.Xu -= SoXu; // Giảm xu
                            _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                        }
                        var km = _context.DanhMucKms
                        .Where(r => r.MaKm == maKm)
                        .FirstOrDefault();
                        if (km != null)
                        {
                            km.Soluong--; // Giảm xu
                            _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                        }
                        // Commit giao dịch
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Rollback giao dịch nếu có lỗi
                        transaction.Rollback();
                        throw;
                    }
                }

                // Xóa giỏ hàng sau khi thanh toán thành công
                HttpContext.Session.Set<List<CartItem>>(MySetting.Cart_key, new List<CartItem>());

                //return Ok(response); // Trả về phản hồi thành công
                return Json(new { maPhieuonl = hoadon.MaPhieuonl });
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        #endregion

        public IActionResult PaymentFail()
        {
            return View();
        }


        public IActionResult PaymentCallBack(CheckoutKH checkoutKH, string macn, decimal totalValue, string MaCn, string payment = "COD")
        {
         
            var response = _vnPayservice.PaymentExecute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VNPay: {response?.VnPayResponseCode ?? "Không có phản hồi"}";
                return RedirectToAction("PaymentFail");
            }
            var makh = HttpContext.Session.GetString("UserName") == null
                ? HttpContext.User.Claims.FirstOrDefault(c => c.Type == "MAKH")?.Value
                : HttpContext.Session.GetString("UserPhone");

            if (string.IsNullOrEmpty(makh))
            {
                throw new Exception("Mã khách hàng không tồn tại trong Claims!");
            }

            var khachhang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == makh);
            if (khachhang == null)
            {
                throw new Exception($"Không tìm thấy khách hàng với mã: {makh}");
            }

            var amountStr = HttpContext.Session.GetString("VnPayAmount");
            int amount;
            if (!int.TryParse(amountStr, out amount))
            {
                TempData["Message"] = "Không thể lấy tổng tiền từ Session.";
                return RedirectToAction("PaymentFail");
            }
            var MaKm = HttpContext.Session.GetString("MaKm");
			// Lưu thông tin hóa đơn
			var hoadon = new Phieudhonl
            {
                MaPhieuonl = makh + DateTime.Now.ToString("yyyyMMddHHmmss"), // Mã hóa đơn duy nhất
                MaKh = makh,
                MaCn = "CN001", 
                DiaChi = khachhang.Diachi,
                Ngaygiodat = DateTime.Now,
                Pttt = "VNPay", // Phương thức thanh toán
                TrangThai = true, // Đánh dấu hóa đơn đã thanh toán
                TongTien = amount,
                TienShip = 15000,
                MaKm = MaKm,

            };

            // Bắt đầu giao dịch
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    List<CartItem> cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key);
                    var ctsp = new List<Ctsponl>();
                    var ctToppings = new List<CtTopping>();
                    var ctKitsToUpdate = _context.CTKITs.Where(ct => ct.MaPhieuonl == "khong" && ct.MaKH == GetCustomerId()).ToList();
                    int tongtien = 0;
                    if (cart != null && cart.Count > 0)
                    {
                        foreach (var item in cart)
                        {
                            int toppingPrice = item.Toppings.Sum(t => t.Dongia);
                            int totalProductPrice = (item.Dongia + item.TriGia + toppingPrice) * item.Soluong;
                            tongtien += totalProductPrice;

                            var ctsponlItem = new Ctsponl
                            {
                                MaKh = makh,
                                MaPhieuonl = hoadon.MaPhieuonl,
                                Soluong = item.Soluong,
                                MaSp = item.ProductID,
                                MaSize = item.SizeID,
                                MaDa = item.DaID,
                                MaDuong = item.DuongID,
                                Gia = item.Dongia,
                                Ghichu = item.Ghichu,
                                Tongtien = totalProductPrice
                            };

                            ctsp.Add(ctsponlItem);
                            foreach (var topping in item.Toppings)
                            {
                                ctToppings.Add(new CtTopping
                                {
                                    MaTopping = topping.MaSp,
                                    MaSp = item.ProductID,
                                    MaPhieuonl = hoadon.MaPhieuonl,
                                    MaKH = makh,
                                });
                            }
                           
                        }
                        foreach (var ctKit in ctKitsToUpdate)
                        {
                            ctKit.MaPhieuonl = hoadon.MaPhieuonl; // Cập nhật mã phiếu
                        }
                        var ctKitDetails = ctKitsToUpdate.Select(ctKit => new
                        {
                            ctKit.MaKH,
                            ctKit.MaSize,
                            ctKit.MaKit,
                            ctKit.MaSp,
                            ctKit.SoLuong,
                        }).ToList();

                        // Xóa các bản ghi cũ
                        foreach (var ctKit in ctKitsToUpdate)
                        {
                            _context.CTKITs.Remove(ctKit);
                        }

                        // Thêm các bản ghi mới với MaPhieuonl đã cập nhật
                        foreach (var ctKitDetail in ctKitDetails)
                        {
                            var newCtKit = new CTKIT
                            {
                                MaPhieuonl = hoadon.MaPhieuonl,  // Gán mã phiếu mới
                                MaKH = ctKitDetail.MaKH,
                                MaSize = ctKitDetail.MaSize,
                                MaKit = ctKitDetail.MaKit,
                                MaSp = ctKitDetail.MaSp,
                                SoLuong = ctKitDetail.SoLuong,
                                // Gán các thuộc tính khác đã lưu từ ctKit cũ
                            };
                            _context.CTKITs.Add(newCtKit);
                        }
                        // Lưu thay đổi vào cơ sở dữ liệu
                        _context.SaveChangesAsync();
                        _context.Add(hoadon);
                        _context.AddRange(ctsp); 
                        _context.AddRange(ctToppings); 
                        _context.SaveChanges(); 
						transaction.Commit();
                    }
                    else
                    {
                        TempData["Message"] = "Giỏ hàng trống.";
                        return RedirectToAction("PaymentFail");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback giao dịch nếu có lỗi
                    TempData["Message"] = "Có lỗi khi lưu đơn hàng: " + ex.Message; // Thông báo lỗi cụ thể
                    return RedirectToAction("PaymentFail");
                }
            }
	

			TempData["Message"] = "Thanh toán VNPay thành công";
            HttpContext.Session.Set<List<CartItem>>(MySetting.Cart_key, new List<CartItem>());
			var soXu = HttpContext.Session.GetString("SoXu");

			TempData["SoXu"] = soXu;
			return RedirectToAction("XemHoaDon", new { maPhieuonl = hoadon.MaPhieuonl , });
        }
    }
    }
