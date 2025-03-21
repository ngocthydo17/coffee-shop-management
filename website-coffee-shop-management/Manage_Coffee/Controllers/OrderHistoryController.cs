using Manage_Coffee.Helpers;
using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using static iTextSharp.text.pdf.AcroFields;

namespace Manage_Coffee.Controllers
{
    public class OrderHistoryController : Controller
    {
        private readonly Cf2Context _context;
        public OrderHistoryController(Cf2Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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
            var khachHang = _context.Phieudhonls
                  .Where(o => o.MaKh == makh)
           .OrderByDescending(o => o.Ngaygiodat)
                .ToList();
            // Lấy danh sách đơn hàng của khách hàng dựa vào mã khách hàng
            // Sắp xếp theo ngày đặt

            return View(khachHang);
        }
        public IActionResult Detail(string maPhieuonl)
        {
            // Lấy chi tiết đơn hàng cùng với topping dựa trên mã phiếu
            var orderDetail = _context.Ctsponls
                .Include(d => d.MaSpNavigation)
                .Include(d => d.MaSizeNavigation)
                .Include(d => d.MaDuongNavigation)
                .Include(d => d.MaDaNavigation)
                .Where(d => d.MaPhieuonl == maPhieuonl)
                .ToList();

            if (orderDetail == null || !orderDetail.Any())
            {
                return NotFound();
            }

            // Load topping cho đơn hàng dựa vào mã phiếu
            var toppings = _context.CtToppings
                .Where(t => t.MaPhieuonl == maPhieuonl)
                .Include(t => t.MaToppingNavigation) // Bao gồm chi tiết topping
                .ToList();

            ViewBag.MaPhieuonl = maPhieuonl;
            ViewBag.Toppings = toppings;
            return View(orderDetail);
        }
        [HttpPost]
        public async Task<IActionResult> Reorder(string maPhieuonl)
        {
            var orderDetails = await _context.Ctsponls
                .Include(d => d.MaSpNavigation)
                .Include(d => d.MaSizeNavigation)
                .Include(d => d.MaDaNavigation)
                .Include(d => d.MaDuongNavigation)
                .Where(d => d.MaPhieuonl == maPhieuonl)
                .ToListAsync();

            if (orderDetails == null || !orderDetails.Any())
            {
                return NotFound();
            }

            List<CartItem> Cart = HttpContext.Session.Get<List<CartItem>>(MySetting.Cart_key) ?? new List<CartItem>();

            // Lấy topping cho đơn hàng dựa vào mã phiếu
            var toppings = await _context.CtToppings
                .Where(t => t.MaPhieuonl == maPhieuonl)
                .Include(t => t.MaToppingNavigation)
                .ToListAsync();

            foreach (var item in orderDetails)
            {
                var cartItem = Cart.FirstOrDefault(c => c.ProductID == item.MaSp && c.SizeID == item.MaSize && c.DaID == item.MaDa && c.DuongID == item.MaDuong);
                if (cartItem == null)
                {
                    cartItem = new CartItem(item.MaSpNavigation, item.MaSizeNavigation, item.MaDuongNavigation, item.MaDaNavigation, item.Ghichu)
                    {
                        Soluong = item.Soluong
                    };

                    // Thêm topping vào `Toppings` của `cartItem`
                    cartItem.Toppings = toppings
                        .Where(t => t.MaSp == item.MaSp)  // lọc topping theo mã sản phẩm
                        .Select(t => new SanPham
                        {
                            MaSp = t.MaTopping,
                            Ten = t.MaToppingNavigation.Ten,
                            Dongia = t.MaToppingNavigation.Dongia
                        })
                        .ToList();

                    Cart.Add(cartItem);
                }
                else
                {
                    cartItem.Soluong += item.Soluong;
                    cartItem.Ghichu = item.Ghichu;
                }
            }

            HttpContext.Session.Set(MySetting.Cart_key, Cart);

            return RedirectToAction("Cart", "Cart");
        }
    }
    }
