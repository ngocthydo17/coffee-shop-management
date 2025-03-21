﻿using Manage_Coffee.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Manage_Coffee.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ListOrderController : Controller
    {
        private readonly Cf2Context _context;
        private readonly IAntiforgery _antiforgery;

        public ListOrderController(IAntiforgery antiforgery, Cf2Context context)
        {
            _antiforgery = antiforgery;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("List-order")]
        public IActionResult Orders()
        {
            var TenNhanVien = HttpContext.Session.GetString("Ten");
            var chucVu = HttpContext.Session.GetString("NhanVienChucVu");
            if (chucVu == "Phục vụ")
            {
                ViewData["Layout"] = "~/Views/Shared/_Layout1.cshtml";
            }
            else
            {
                ViewData["Layout"] = "~/Views/Shared/_Layout2.cshtml";
            }
            if (string.IsNullOrEmpty(TenNhanVien))
            {
                return RedirectToAction("LoginAdmin", "AccountAdmin", new { area = "Admin" });
            }
            var orders = LoadOrders();

            if (orders == null || !orders.Any()) // Kiểm tra nếu không có đơn hàng
            {
                ViewBag.Message = "Không có đơn đặt hàng nào.";
            }

            return View(orders);
        }
		public List<Phieudhonl> LoadOrders()
		{
			var maCn = HttpContext.Session.GetString("MaCn");
			var listOrders = _context.Phieudhonls.Where(c => c.TrangThai == true && c.MaCn == maCn).ToList();
			return listOrders; // Trả về danh sách đơn hàng
		}

		//Load các đơn hàng chờ xác nhận
		[Route("List-pending-order")]
        public IActionResult PendingOrders()
        {
            var TenNhanVien = HttpContext.Session.GetString("Ten");
            if (string.IsNullOrEmpty(TenNhanVien))
            {
                return RedirectToAction("LoginAdmin", "AccountAdmin", new { area = "Admin" });
            }
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            ViewBag.AntiForgeryToken = tokens.RequestToken;
            var orders = LoadOrdersPending();

            if (orders == null || !orders.Any()) // Kiểm tra nếu không có đơn hàng
            {
                ViewBag.Message = "Không có đơn đặt hàng nào.";
            }

            return View(orders);
        }
        //Load các đơn hàng đang chờ xác nhận

        public List<Phieudhonl> LoadOrdersPending()
        {
            var listOrdersPending = _context.Phieudhonls.Where(c => c.TrangThai == false).ToList();
            return listOrdersPending; // Trả về danh sách đơn hàng
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOrder([FromBody] string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return Json(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }
            var order = _context.Phieudhonls.FirstOrDefault(o => o.MaPhieuonl == orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }
            if (order.TrangThai == false)
            {
                order.TrangThai = true; // Đã xác nhận
                try
                {
                    _context.SaveChanges();
                    return Json( new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi cập nhật đơn hàng: " + ex.Message });
                }
            }
            
            return Json(new { success = false, message = "Đơn hàng đã được xử lý." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder([FromBody] string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return Json(new { success = false, message = "Mã đơn hàng không hợp lệ." });
            }
            var order = _context.Phieudhonls.FirstOrDefault(o => o.MaPhieuonl == orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }
            if (order.TrangThai == false)
            {
                order.TrangThai = null; // Đã hủy
                try
                {
                    _context.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi cập nhật đơn hàng: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Đơn hàng đã được xử lý." });
        }
    }
}
