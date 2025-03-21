using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manage_Coffee.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyController : Controller
    {
        
        private readonly Cf2Context _context;

        public QuanLyController(Cf2Context context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var manv = HttpContext.Session.GetString("Manv");
            if (manv == null)
            {
                return RedirectToAction("LoginAdmin", "AccountAdmin", new { area = "Admin" });
            }
            var nvl = LoadNvl();
            return View(nvl);
        }
        public List<NguyenVatLieu> LoadNvl()
        {
            var nvl = _context.NguyenVatLieus.ToList();
            return nvl;
        }
        // Phương thức GET để hiển thị form thêm nguyên vật liệu
       
        // Phương thức POST để xử lý form khi người dùng submit
        public IActionResult Create()
        {
            return View();
        }

        // Phương thức POST để xử lý form khi người dùng submit
        [HttpPost]
        public async Task<IActionResult> Create(string Ten, int Dongia, string Dvt, string Mota, string Anh, double Soluong)
        {
            var MaNvl = $"NVL{new Random().Next(10, 99)}";
            Soluong = 0;
            if (ModelState.IsValid)
            {
                // Kiểm tra mã nguyên vật liệu
                var existingNVL = await _context.NguyenVatLieus.FirstOrDefaultAsync(nvl => nvl.MaNvl == MaNvl);
                while (existingNVL != null)
                {
                    MaNvl = $"NVL{new Random().Next(10, 99)}";
                    existingNVL = await _context.NguyenVatLieus.FirstOrDefaultAsync(nvl => nvl.MaNvl == MaNvl);

                }

                // Tạo mới nguyên vật liệu
                var nguyenVatLieu = new NguyenVatLieu
                {
                    MaNvl = MaNvl,
                    Ten = Ten,
                    Dongia = Dongia,
                    Dvt = Dvt,
                    Mota = Mota,
                    Anh = Anh,
                    SoLuong = Soluong
                };

                // Lưu nguyên vật liệu vào cơ sở dữ liệu
                _context.NguyenVatLieus.Add(nguyenVatLieu);
                await _context.SaveChangesAsync();

                // Điều hướng về trang danh sách nguyên vật liệu
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu không hợp lệ, trả về lại view với lỗi
            return View();
        }
        // Hiển thị form chỉnh sửa
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var nvl = _context.NguyenVatLieus.FirstOrDefault(x => x.MaNvl == id);
            if (nvl == null)
            {
                return NotFound();
            }
            return View(nvl);
        }

        // Xử lý dữ liệu từ form gửi lên
        [HttpPost]
        public IActionResult Edit(string MaNvl, string Ten, int Dongia, string Dvt, string Anh, string Mota)
        {
            // Tìm NVL cần sửa trong cơ sở dữ liệu
            var nvl = _context.NguyenVatLieus.FirstOrDefault(x => x.MaNvl == MaNvl);
            if (nvl != null)
            {
                // Cập nhật thông tin mới
                nvl.Ten = Ten;
                nvl.Dongia = Dongia;
                nvl.Dvt = Dvt;
                nvl.Anh = Anh;
                nvl.Mota = Mota;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.NguyenVatLieus.Update(nvl);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            // Trường hợp không tìm thấy NVL
            return NotFound();
        }

        //Hàm load nhân viên
        public IActionResult LoadEmployee()
        {
            var employee = Employee();
            return View(employee);
        }
        public List<NhanVien> Employee()
        {
            var MaCn = HttpContext.Session.GetString("MaCn");
            var chucVu = HttpContext.Session.GetString("NhanVienChucVu");
            if (chucVu == "Quản lý")
            {
                ViewData["Layout"] = "~/Views/Shared/_Layout2.cshtml";
            }
            else
            {
                ViewData["Layout"] = "~/Views/Shared/_Layout3.cshtml";
            }
            if (MaCn == null)
            {
                MaCn = "CN001";
            }
            var listEmployee = _context.NhanViens
                .Where(cv => cv.Chucvu == "Phục vụ" && cv.MaCn == MaCn)
                .ToList();
            if (listEmployee == null || !listEmployee.Any())
            {
                throw new Exception("Danh sách nhân viên không tồn tại!");
            }

            return listEmployee;
        }
        // Hàm load phiếu nhập xuất
        public IActionResult LoadPhieuNhapXuat()
        {
            var phieuList = GetPhieuNhapXuat();
            if (phieuList == null || !phieuList.Any())
            {
                throw new Exception("Không có phiếu nhập xuất.");
            }

            return View(phieuList); // Truyền danh sách phiếu vào View
        }

        public List<PhieuNhapXuat> GetPhieuNhapXuat()
        {
            var phieuList = _context.PhieuNhapXuats
            .Select(phieu => new PhieuNhapXuat
            {
                MaPhieu = phieu.MaPhieu,
                NgayLap = phieu.NgayLap,
                Loai = phieu.Loai,
                Diachi = phieu.Diachi,
                MaNv = _context.NhanViens
                .Where(nv => nv.MaNv == phieu.MaNv)
                .Select(nv => nv.Ten)
                .FirstOrDefault() ?? "Không rõ",

                MaCcap = _context.Nhacungcaps
                .Where(ncc => ncc.MaCcap == phieu.MaCcap)
                .Select(ncc => ncc.Ten)
                .FirstOrDefault() ?? "Không rõ"
            }).ToList();
            if (!phieuList.Any() || phieuList == null)
            {
                throw new Exception("Không có phiếu nhập xuất.");
            }
            return phieuList;
        }

        public IActionResult CreateMaterial()
        {
            ViewBag.NguyenVatLieu = _context.NguyenVatLieus.ToList(); // Load danh sách NVL
            ViewBag.NhaCungCapList = _context.Nhacungcaps.ToList(); // Load danh sách NVL
            return View();
        }

        // POST: Xử lý tạo phiếu nhập/xuất
        [HttpPost]
        public IActionResult CreateMaterial(
      string loai,
      List<string> maNvlList,
      List<int> soLuongList,
      string diaChi,
      string? maCcap)
        {
            // Lấy mã nhân viên từ session
            var maNv = HttpContext.Session.GetString("Manv");
            if (string.IsNullOrEmpty(maNv))
            {
                ModelState.AddModelError("", "Không tìm thấy mã nhân viên. Vui lòng đăng nhập lại.");
                return View();
            }

            // 1. Tạo phiếu mới
            var phieu = new PhieuNhapXuat
            {
                MaPhieu = GenerateMaPhieu(),  // Sinh mã phiếu từ hàm GenerateMaPhieu
                NgayLap = DateTime.Now,
                Loai = loai,
                Diachi = diaChi,
                MaNv = maNv,
                MaCcap = maCcap
            };
            _context.PhieuNhapXuats.Add(phieu);
            _context.SaveChanges();

            // 2. Lưu chi tiết phiếu cho từng sản phẩm
            for (int i = 0; i < maNvlList.Count; i++)
            {
                var ctPhieu = new Ctphieu
                {
                    MaPhieu = phieu.MaPhieu,
                    MaNvl = maNvlList[i],
                    Soluong = soLuongList[i]
                };
                _context.Ctphieus.Add(ctPhieu);

                // 3. Cập nhật số lượng tồn kho của từng nguyên vật liệu
                var nvl = _context.NguyenVatLieus.Find(maNvlList[i]);
                if (nvl != null)
                {
                    if (loai.ToLower() == "nhap")
                    {
                        nvl.SoLuong += soLuongList[i];
                    }
                    else if (loai.ToLower() == "xuat")
                    {
                        if (nvl.SoLuong >= soLuongList[i])
                        {
                            nvl.SoLuong -= soLuongList[i];
                        }
                        else
                        {
                            ModelState.AddModelError("",
                                $"Số lượng tồn kho của {nvl.Ten} không đủ để xuất.");
                            return View();
                        }
                    }
                }
            }

            _context.SaveChanges();  // Lưu tất cả thay đổi vào DB
            return RedirectToAction("LoadPhieuNhapXuat");
        }
        // Hàm tạo mã phiếu PNX + 2 số ngẫu nhiên
        private string GenerateMaPhieu()
        {
            Random random = new Random();
            int randomNumber = random.Next(10, 99); // Sinh số ngẫu nhiên từ 10 đến 99
            return $"PNX{randomNumber}";
        }
        public IActionResult ViewPhieuDetail(string maPhieu)
        {
            // Lấy thông tin phiếu nhập/xuất
            var phieu = _context.PhieuNhapXuats
                .Where(p => p.MaPhieu == maPhieu)
                .Select(p => new PhieuNhapXuat
                {
                    MaPhieu = p.MaPhieu,
                    NgayLap = p.NgayLap,
                    Loai = p.Loai,
                    Diachi = p.Diachi,
                    MaNv = _context.NhanViens
                        .Where(nv => nv.MaNv == p.MaNv)
                        .Select(nv => nv.Ten)
                        .FirstOrDefault() ?? "Không rõ",
                    MaCcap = _context.Nhacungcaps
                        .Where(ncc => ncc.MaCcap == p.MaCcap)
                        .Select(ncc => ncc.Ten)
                        .FirstOrDefault() ?? "Không rõ"
                })
                .FirstOrDefault();

            if (phieu == null)
            {
                return NotFound("Không tìm thấy phiếu nhập/xuất này.");
            }

            // Lấy danh sách nguyên vật liệu trong phiếu nhập/xuất
            var chiTietPhieu = _context.Ctphieus
                .Where(ct => ct.MaPhieu == maPhieu)
                .Select(ct => new
                {
                    MaNvl = ct.MaNvl,
                    TenNvl = _context.NguyenVatLieus
                        .Where(nvl => nvl.MaNvl == ct.MaNvl)
                        .Select(nvl => nvl.Ten)
                        .FirstOrDefault(),
                    SoLuong = ct.Soluong
                })
                .ToList();

            // Gửi dữ liệu sang View
            ViewBag.Phieu = phieu;
            ViewBag.ChiTietPhieu = chiTietPhieu;

            return View();
        }

    }
}
