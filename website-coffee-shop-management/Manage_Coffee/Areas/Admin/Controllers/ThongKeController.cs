using System;
using System.Collections.Generic;
using System.Linq;
using Manage_Coffee.Areas.Admin.Models;
using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Manage_Coffee.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongKeController : Controller
    {
        private readonly Cf2Context _context;

        public ThongKeController(Cf2Context context)
        {
            _context = context;
        }

        public IActionResult Index(string loaiThongKe = "nam", int? nam = null, DateTime? tuNgay = null, DateTime? denNgay = null, string loaiBanHang = "all", string? selectedLabel = null)
        {
            var maCN = HttpContext.Session.GetString("MaCn");
            var danhSachThongKe = new List<ThongKeSanPham>();

            switch (loaiThongKe)
            {
                case "nam":
                    danhSachThongKe = ThongKeTheoNam(maCN, loaiBanHang);
                    break;

                case "thang":
                    if (nam == null) nam = DateTime.Now.Year;
                    danhSachThongKe = ThongKeTheoThang(maCN, nam.Value, loaiBanHang);
                    break;

                case "quy":
                    if (nam == null) nam = DateTime.Now.Year;
                    danhSachThongKe = ThongKeTheoQuy(maCN, nam.Value, loaiBanHang);
                    break;

                case "khoangthoigian":
                    if (tuNgay == null || denNgay == null)
                    {
                        tuNgay = DateTime.Now.AddMonths(-1);
                        denNgay = DateTime.Now;
                    }
                    danhSachThongKe = ThongKeTheoKhoangThoiGian(maCN, tuNgay.Value, denNgay.Value, loaiBanHang);
                    break;
            }
            // Gọi hàm GetSalesByTime để lấy doanh số sản phẩm cho sản phẩm được chọn
            if (!string.IsNullOrEmpty(selectedLabel))
            {
                var salesData = GetSalesByTime(selectedLabel, loaiThongKe, loaiBanHang);

                if (salesData is JsonResult result)
                {
                    var jsonString = JsonConvert.SerializeObject(result.Value);
                    var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                    if (jsonObject != null)
                    {
                        // Truy cập dữ liệu từ JObject
                        var productLabels = jsonObject["productLabels"]?.ToObject<List<string>>();
                        var productSales = jsonObject["productSalesData"]?.ToObject<List<decimal>>();
                        var productSLData = jsonObject["productSLData"]?.ToObject<List<int>>();

                        // Chỉ cập nhật danh sách doanh số cho các sản phẩm có doanh số
                        foreach (var item in danhSachThongKe)
                        {
                            foreach (var sanPham in item.SanPhamBanDuoc)
                            {
                                // Giả sử SanPhamBanDuoc chứa thông tin tên sản phẩm
                                var matchingIndex = productLabels?.IndexOf(sanPham.TenSp); // Thay thế TenSp bằng thuộc tính thực tế nếu cần

                                if (matchingIndex != -1 && matchingIndex.HasValue)
                                {
                                    // Nếu sản phẩm có doanh số, thêm thông tin doanh số vào YourProductSales
                                    item.YourProductSales.Add(new YourProductSalesModel
                                    {
                                        ProductName = productLabels[matchingIndex.Value],
                                        SalesAmount = productSales[matchingIndex.Value],
                                        Soluong = productSLData[matchingIndex.Value]
                                    });
                                }
                            }
                        }
                    }
                }
            }
            ViewBag.chon = selectedLabel;
            ViewBag.LoaiThongKe = loaiThongKe;
            ViewBag.Nam = nam;
            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.LoaiBanHang = loaiBanHang;

            return View(danhSachThongKe);
        }

        private List<ThongKeSanPham> ThongKeTheoNam(string maCN, string loaiBanHang)
        {
            var currentYear = DateTime.Now.Year;
            var danhSachThongKe = new List<ThongKeSanPham>();

            for (int i = 0; i < 5; i++)
            {
                var year = currentYear - i;
                var doanhThu = TinhTongDoanhThu(maCN, loaiBanHang, nam: year);
                var sanPhamBanDuoc = LaySanPhamBanDuoc(maCN, loaiBanHang: loaiBanHang);

                danhSachThongKe.Add(new ThongKeSanPham { Nam = year, TongDoanhThu = doanhThu, SanPhamBanDuoc = sanPhamBanDuoc });
            }
            return danhSachThongKe;
        }

        private List<ThongKeSanPham> ThongKeTheoThang(string maCN, int nam, string loaiBanHang)
        {
            var danhSachThongKe = new List<ThongKeSanPham>();

            for (int month = 1; month <= 12; month++)
            {
                var doanhThu = TinhTongDoanhThu(maCN, loaiBanHang, nam, month);
                var sanPhamBanDuoc = LaySanPhamBanDuoc(maCN, nam, loaiBanHang);
                danhSachThongKe.Add(new ThongKeSanPham { Nam = nam, Thang = month, TongDoanhThu = doanhThu, SanPhamBanDuoc = sanPhamBanDuoc });
            }
            return danhSachThongKe;
        }

        private List<ThongKeSanPham> ThongKeTheoQuy(string maCN, int nam, string loaiBanHang)
        {
            var danhSachThongKe = new List<ThongKeSanPham>();

            for (int quarter = 1; quarter <= 4; quarter++)
            {
                int startMonth;
                int endMonth;

                switch (quarter)
                {
                    case 1:
                        startMonth = 1;
                        endMonth = 3;
                        break;
                    case 2:
                        startMonth = 4;
                        endMonth = 6;
                        break;
                    case 3:
                        startMonth = 7;
                        endMonth = 9;
                        break;
                    case 4:
                        startMonth = 10;
                        endMonth = 12;
                        break;
                    default:
                        startMonth = 0;
                        endMonth = 0;
                        break;
                }

                decimal doanhThuQuy = 0;
                var sanPhamBanDuoc = new List<SanPhamBanDuoc>(); // Danh sách sản phẩm bán được cho quý

                for (int month = startMonth; month <= endMonth; month++)
                {
                    doanhThuQuy += TinhTongDoanhThu(maCN, loaiBanHang, nam, month);
                    sanPhamBanDuoc = LaySanPhamBanDuoc(maCN, nam, loaiBanHang);
                    // sanPhamBanDuoc.AddRange(sanPhamBanDuoc);
                }

                danhSachThongKe.Add(new ThongKeSanPham { Nam = nam, Quy = quarter, TongDoanhThu = doanhThuQuy, SanPhamBanDuoc = sanPhamBanDuoc });
            }

            return danhSachThongKe;
        }

        private List<ThongKeSanPham> ThongKeTheoKhoangThoiGian(string maCN, DateTime tuNgay, DateTime denNgay, string loaiBanHang)
        {
            var danhSachThongKe = new List<ThongKeSanPham>();

            // Lấy doanh thu từ các phiếu order
            var doanhThuList = _context.PhieuOrders
                .Where(po => po.MaCn == maCN && po.Ngaygiodat >= tuNgay && po.Ngaygiodat <= denNgay)
                .GroupBy(po => po.Ngaygiodat.Date)
                .Select(g => new ThongKeSanPham
                {
                    Ngay = g.Key,
                    TongDoanhThu = g.Sum(po => po.Tongtien),
                    // Giả sử bạn đã định nghĩa một thuộc tính mặc định
                    SanPhamBanDuoc = new List<SanPhamBanDuoc>() // Khởi tạo danh sách rỗng nếu không có sản phẩm
                })
                .ToList(); // Lấy dữ liệu vào client

            // Lấy doanh thu từ các phiếu đặt hàng online
            var doanhThuOnlineList = _context.Phieudhonls
                .Where(po => po.MaCn == maCN && po.Ngaygiodat >= tuNgay && po.Ngaygiodat <= denNgay)
                .GroupBy(po => po.Ngaygiodat.Date)
                .Select(g => new ThongKeSanPham
                {
                    Ngay = g.Key,
                    TongDoanhThu = (decimal)g.Sum(po => po.TongTien),
                    SanPhamBanDuoc = new List<SanPhamBanDuoc>() // Khởi tạo danh sách rỗng nếu không có sản phẩm
                })
                .ToList(); // Lấy dữ liệu vào client

            // Kết hợp doanh thu online và offline
            var combined = doanhThuList.Concat(doanhThuOnlineList)
                .GroupBy(x => x.Ngay) // Nhóm theo ngày
                .Select(g => new ThongKeSanPham
                {
                    Ngay = g.Key,
                    TongDoanhThu = g.Sum(x => x.TongDoanhThu),
                    SanPhamBanDuoc = LaySanPhamBanDuocTheoKhoangThoiGian(maCN, tuNgay, denNgay, "all")
                })
                .ToList();

            // Chọn danh sách theo loại bán hàng
            if (loaiBanHang == "onl")
            {
                danhSachThongKe = doanhThuOnlineList.Select(d => new ThongKeSanPham
                {
                    Ngay = d.Ngay,
                    TongDoanhThu = d.TongDoanhThu,
                    SanPhamBanDuoc = LaySanPhamBanDuocTheoKhoangThoiGian(maCN, tuNgay, denNgay, "onl") // Gọi hàm để lấy sản phẩm bán online
                }).ToList();
            }
            else if (loaiBanHang == "off")
            {
                danhSachThongKe = doanhThuList.Select(d => new ThongKeSanPham
                {
                    Ngay = d.Ngay,
                    TongDoanhThu = d.TongDoanhThu,
                    SanPhamBanDuoc = LaySanPhamBanDuocTheoKhoangThoiGian(maCN, tuNgay, denNgay, "off") // Gọi hàm để lấy sản phẩm bán tại quán
                }).ToList();
            }
            else
            {
                // Kết quả đã được lưu trong combined
                danhSachThongKe = combined;
            }

            return danhSachThongKe;
        }




        private decimal TinhTongDoanhThu(string maCN, string loaiBanHang, int? nam = null, int? thang = null, int? startMonth = null, int? endMonth = null)
        {
            IQueryable<decimal> offlineQuery = _context.PhieuOrders
                .Where(po => po.MaCn == maCN && (nam == null || po.Ngaygiodat.Year == nam)
                            && (thang == null || po.Ngaygiodat.Month == thang)
                            && (startMonth == null || po.Ngaygiodat.Month >= startMonth)
                            && (endMonth == null || po.Ngaygiodat.Month <= endMonth))
                .Select(po => (decimal)po.Tongtien);
            IQueryable<decimal> onlineQuery = _context.Phieudhonls
                .Where(po => po.MaCn == maCN && (nam == null || po.Ngaygiodat.Year == nam)
                            && (thang == null || po.Ngaygiodat.Month == thang)
                            && (startMonth == null || po.Ngaygiodat.Month >= startMonth)
                            && (endMonth == null || po.Ngaygiodat.Month <= endMonth))
                .Select(po => (decimal)po.TongTien);
            return loaiBanHang switch
            {
                "onl" => onlineQuery.Sum(),
                "off" => offlineQuery.Sum(),
                _ => offlineQuery.Sum() + onlineQuery.Sum()
            };
        }

        private List<SanPhamBanDuoc> LaySanPhamBanDuoc(string maCN, int? nam = null, string loaiBanHang = "all")
        {
            var sanPhamBanDuoc = new List<SanPhamBanDuoc>();

            // Lấy sản phẩm từ phiếu bán tại quán
            if (loaiBanHang == "off" || loaiBanHang == "all")
            {
                var donHangOff = _context.PhieuOrders
                    .Where(po => po.MaCn == maCN && (nam == null || po.Ngaygiodat.Year == nam))
                    .SelectMany(po => po.CtsanPhams)
                    .GroupBy(ct => new { ct.MaSp, ct.MaSpNavigation.Ten })
                    .Select(g => new SanPhamBanDuoc
                    {
                        MaSp = g.Key.MaSp,
                        TenSp = g.Key.Ten,
                        SoLuong = g.Sum(ct => ct.Soluong),
                        TongTien = g.Sum(ct => ct.TongTien)
                    });

                sanPhamBanDuoc.AddRange(donHangOff);
            }

            // Lấy sản phẩm từ phiếu đặt hàng online
            if (loaiBanHang == "onl" || loaiBanHang == "all")
            {
                var donHangOn = _context.Phieudhonls
                    .Where(po => po.MaCn == maCN && (nam == null || po.Ngaygiodat.Year == nam))
                    .SelectMany(po => po.Ctsponls)
                    .GroupBy(ct => new { ct.MaSp, ct.MaSpNavigation.Ten })
                    .Select(g => new SanPhamBanDuoc
                    {
                        MaSp = g.Key.MaSp,
                        TenSp = g.Key.Ten,
                        SoLuong = g.Sum(ct => ct.Soluong),
                        TongTien = g.Sum(ct => ct.Tongtien)
                    });

                sanPhamBanDuoc.AddRange(donHangOn);
            }

            return sanPhamBanDuoc
                .GroupBy(sp => new { sp.MaSp, sp.TenSp })
                .Select(g => new SanPhamBanDuoc
                {
                    MaSp = g.Key.MaSp,
                    TenSp = g.Key.TenSp,
                    SoLuong = g.Sum(sp => sp.SoLuong),
                    TongTien = g.Sum(sp => sp.TongTien)
                }).ToList();
        }
        private List<SanPhamBanDuoc> LaySanPhamBanDuocTheoKhoangThoiGian(string maCN, DateTime tuNgay, DateTime denNgay, string loaiBanHang = "all")
        {
            var sanPhamBanDuoc = new List<SanPhamBanDuoc>();

            // Lấy sản phẩm từ phiếu bán tại quán
            if (loaiBanHang == "off" || loaiBanHang == "all")
            {
                var donHangOff = _context.PhieuOrders
                    .Where(po => po.MaCn == maCN && po.Ngaygiodat >= tuNgay && po.Ngaygiodat <= denNgay)
                    .SelectMany(po => po.CtsanPhams)
                    .GroupBy(ct => new { ct.MaSp, ct.MaSpNavigation.Ten })
                    .Select(g => new SanPhamBanDuoc
                    {
                        MaSp = g.Key.MaSp,
                        TenSp = g.Key.Ten,
                        SoLuong = g.Sum(ct => ct.Soluong),
                        TongTien = g.Sum(ct => ct.TongTien)
                    });

                sanPhamBanDuoc.AddRange(donHangOff);
            }

            // Lấy sản phẩm từ phiếu đặt hàng online
            if (loaiBanHang == "onl" || loaiBanHang == "all")
            {
                var donHangOn = _context.Phieudhonls
                    .Where(po => po.MaCn == maCN && po.Ngaygiodat >= tuNgay && po.Ngaygiodat <= denNgay)
                    .SelectMany(po => po.Ctsponls)
                    .GroupBy(ct => new { ct.MaSp, ct.MaSpNavigation.Ten })
                    .Select(g => new SanPhamBanDuoc
                    {
                        MaSp = g.Key.MaSp,
                        TenSp = g.Key.Ten,
                        SoLuong = g.Sum(ct => ct.Soluong),
                        TongTien = g.Sum(ct => ct.Tongtien)
                    });

                sanPhamBanDuoc.AddRange(donHangOn);
            }
            return sanPhamBanDuoc
                .GroupBy(sp => new { sp.MaSp, sp.TenSp })
                .Select(g => new SanPhamBanDuoc
                {
                    MaSp = g.Key.MaSp,
                    TenSp = g.Key.TenSp,
                    SoLuong = g.Sum(sp => sp.SoLuong),
                    TongTien = g.Sum(sp => sp.TongTien)
                }).ToList();
        }
        // Phương thức để lấy doanh số sản phẩm theo thời gian
        public JsonResult GetSalesByTime(string time, string timeType, string hinhthuc)
        {
            var maCN = HttpContext.Session.GetString("MaCn");
            var productSalesData = new List<YourProductSalesModel>(); // Tạo danh sách để lưu dữ liệu doanh số sản phẩm

            // Kiểm tra giá trị của hinhthuc
            if (hinhthuc != "all" && hinhthuc != "off" && hinhthuc != "onl")
            {
                return Json(new { error = "Lỗi: Giá trị của hình thức không hợp lệ. Chỉ chấp nhận 'all', 'off', hoặc 'onl'." });
            }

            // Lấy dữ liệu từ cơ sở dữ liệu dựa trên time và timeType
            if (timeType == "nam")
            {
                if (!int.TryParse(time, out int year))
                {
                    return Json(new { error = "Lỗi: Định dạng năm không hợp lệ." });
                }

                // Lấy doanh số sản phẩm từ phiếu bán tại quán (PhieuOrder)
                if (hinhthuc == "all" || hinhthuc == "off")
                {
                    productSalesData.AddRange(
                        _context.PhieuOrders
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year)
                            .SelectMany(order => order.CtsanPhams)
                            .GroupBy(ct => ct.MaSpNavigation.Ten) // Nhóm theo tên sản phẩm
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.TongTien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }

                // Lấy doanh số sản phẩm từ phiếu đặt hàng online (Phieudhonl)
                if (hinhthuc == "all" || hinhthuc == "onl")
                {
                    productSalesData.AddRange(
                        _context.Phieudhonls
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year)
                            .SelectMany(order => order.Ctsponls)
                            .GroupBy(ct => ct.MaSpNavigation.Ten) // Nhóm theo tên sản phẩm
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.Tongtien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }
            }
            else if (timeType == "thang")
            {
                DateTime date;
                if (!DateTime.TryParseExact(time, "M/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                    return Json(new { error = "Lỗi: Định dạng tháng không hợp lệ. Vui lòng nhập theo định dạng M/yyyy." });
                }

                int month = date.Month;
                int year = date.Year;

                // Lấy doanh số cho tháng từ phiếu bán tại quán
                if (hinhthuc == "all" || hinhthuc == "off")
                {
                    productSalesData.AddRange(
                        _context.PhieuOrders
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year && order.Ngaygiodat.Month == month)
                            .SelectMany(order => order.CtsanPhams)
                            .GroupBy(ct => ct.MaSpNavigation.Ten)
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.TongTien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }

                // Lấy doanh số cho tháng từ phiếu đặt hàng online
                if (hinhthuc == "all" || hinhthuc == "onl")
                {
                    productSalesData.AddRange(
                        _context.Phieudhonls
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year && order.Ngaygiodat.Month == month)
                            .SelectMany(order => order.Ctsponls)
                            .GroupBy(ct => ct.MaSpNavigation.Ten)
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.Tongtien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }
            }
            else if (timeType == "quy")
            {
                int quarter = 0;
                int year = 0;

                // Kiểm tra và lấy ra quý và năm
                if (string.IsNullOrEmpty(time))
                {
                    return Json(new { error = "Lỗi: Giá trị thời gian không được null hoặc rỗng." });
                }

                if (time.StartsWith("Q") && time.Length > 2 && time.Contains("/"))
                {
                    // Lấy số quý và năm từ chuỗi
                    quarter = int.Parse(time.Substring(1, 1));
                    year = int.Parse(time.Split('/')[1]);
                }
                else
                {
                    return Json(new { error = "Lỗi: Định dạng quý không hợp lệ. Vui lòng nhập theo định dạng QX/YYYY." });
                }

                // Lấy tháng tương ứng cho quý
                int startMonth = (quarter - 1) * 3 + 1;
                int endMonth = startMonth + 2;

                // Doanh số cho quý từ phiếu bán tại quán
                if (hinhthuc == "all" || hinhthuc == "off")
                {
                    productSalesData.AddRange(
                        _context.PhieuOrders
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year && order.Ngaygiodat.Month >= startMonth && order.Ngaygiodat.Month <= endMonth)
                            .SelectMany(order => order.CtsanPhams)
                            .GroupBy(ct => ct.MaSpNavigation.Ten)
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.TongTien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }

                // Doanh số cho quý từ phiếu đặt hàng online
                if (hinhthuc == "all" || hinhthuc == "onl")
                {
                    productSalesData.AddRange(
                        _context.Phieudhonls
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year && order.Ngaygiodat.Month >= startMonth && order.Ngaygiodat.Month <= endMonth)
                            .SelectMany(order => order.Ctsponls)
                            .GroupBy(ct => ct.MaSpNavigation.Ten)
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.Tongtien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }
            }
            else if (timeType == "khoangthoigian")
            {
                DateTime date;
                if (!DateTime.TryParseExact(time, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                {
                    return Json(new { error = "Lỗi: Định dạng ngày không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy." });
                }
                int day = date.Day;
                int month = date.Month;
                int year = date.Year;
                // Doanh số cho ngày từ phiếu bán tại quán
                if (hinhthuc == "all" || hinhthuc == "off")
                {
                    productSalesData.AddRange(
                        _context.PhieuOrders
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year && order.Ngaygiodat.Month == month && order.Ngaygiodat.Day == day)
                            .SelectMany(order => order.CtsanPhams)
                            .GroupBy(ct => ct.MaSpNavigation.Ten)
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.TongTien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }

                // Doanh số cho ngày từ phiếu đặt hàng online
                if (hinhthuc == "all" || hinhthuc == "onl")
                {
                    productSalesData.AddRange(
                        _context.Phieudhonls
                            .Where(order => order.MaCn == maCN && order.Ngaygiodat.Year == year && order.Ngaygiodat.Month == month && order.Ngaygiodat.Day == day)
                            .SelectMany(order => order.Ctsponls)
                            .GroupBy(ct => ct.MaSpNavigation.Ten)
                            .Select(g => new YourProductSalesModel
                            {
                                ProductName = g.Key,
                                SalesAmount = g.Sum(ct => ct.Tongtien),
                                Soluong = g.Sum(ct => ct.Soluong)
                            })
                    );
                }
            }
            // Chuyển đổi dữ liệu thành định dạng JSON trả về
            return Json(new
            {
                productLabels = productSalesData.Select(p => p.ProductName).ToList(),
                productSalesData = productSalesData.Select(p => p.SalesAmount).ToList(),
                productSLData = productSalesData.Select(p => p.Soluong).ToList()
            });
        }
    }

}
