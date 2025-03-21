using Manage_Coffee.Models;
using Manage_Coffee.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Manage_Coffee.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IUserService _userService;
		private readonly IEmailService _emailService;
		private readonly Cf2Context _context; // Sử dụng DbContext để lấy dữ liệu khách hàng

		public HomeController(ILogger<HomeController> logger, IUserService userService, IEmailService emailService, Cf2Context context)
		{
			_logger = logger;
			_userService = userService;
			_emailService = emailService;
			_context = context; // Inject DbContext
		}

		public async Task<ViewResult> Index() // Đây là trang chủ
		{
			var userId = _userService.GetUserId();
			var isLoggedIn = _userService.IsAuthenticated();
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

          

            var khachhang = _context.KhachHangs.SingleOrDefault(kh => kh.MaKh == makh);
            if (khachhang != null)
            {
                ViewData["Role"] = khachhang.Role; 
                HttpContext.Session.SetString("UserRole", khachhang.Role); 
            }
            else
            {
                ViewData["Role"] = "Guest"; 
                HttpContext.Session.SetString("UserRole", "Guest");
            }

            return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult LienHe()
		{
			return View();
		}

		public IActionResult CuaHang()
		{
			return View();
		}
        public IActionResult TuyenDung()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
