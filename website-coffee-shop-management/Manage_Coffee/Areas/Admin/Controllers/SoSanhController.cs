using Manage_Coffee.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace Manage_Coffee.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SoSanhController : Controller
    {
        private readonly string _connectionString = "Server=MANHKHANG;Database=cf29;Trusted_Connection=True;TrustServerCertificate=True;"; // Đổi thành chuỗi kết nối của bạn

        //public IActionResult Index()
        //{
        //    return View(new List<DoanhThu>());
        //}

        public async Task<IActionResult> Index(int thang1Month, int thang1Year, int thang2Month, int thang2Year, string maCN="CN001")
        {
            if (string.IsNullOrEmpty(maCN))
            {
                return BadRequest("Mã chi nhánh không hợp lệ.");
            }

            var doanhThuList = new List<DoanhThu>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
        SELECT S.Ten AS SanPham, 
            SUM(CASE WHEN MONTH(P.Ngaygiodat) = @Thang1Month AND YEAR(P.Ngaygiodat) = @Thang1Year THEN C.TongTien ELSE 0 END) AS DoanhThuThang1,
            SUM(CASE WHEN MONTH(P.Ngaygiodat) = @Thang2Month AND YEAR(P.Ngaygiodat) = @Thang2Year THEN C.TongTien ELSE 0 END) AS DoanhThuThang2
        FROM CTSanPham C
        JOIN PhieuOrder P ON C.MaOrder = P.MaOrder
        JOIN SanPham S ON C.MaSP = S.MaSP
        WHERE (MONTH(P.Ngaygiodat) = @Thang1Month OR MONTH(P.Ngaygiodat) = @Thang2Month) 
          AND (YEAR(P.Ngaygiodat) = @Thang1Year OR YEAR(P.Ngaygiodat) = @Thang2Year)
          AND P.MaCN = @MaCN 
        GROUP BY S.Ten

        UNION ALL

        SELECT S.Ten AS SanPham,
            SUM(CASE WHEN MONTH(Ph.Ngaygiodat) = @Thang1Month AND YEAR(Ph.Ngaygiodat) = @Thang1Year THEN C.TongTien ELSE 0 END) AS DoanhThuThang1,
            SUM(CASE WHEN MONTH(Ph.Ngaygiodat) = @Thang2Month AND YEAR(Ph.Ngaygiodat) = @Thang2Year THEN C.TongTien ELSE 0 END) AS DoanhThuThang2
        FROM CTSPonl C
        JOIN Phieudhonl Ph ON C.MaPhieuonl = Ph.MaPhieuonl
        JOIN SanPham S ON C.MaSP = S.MaSP
        WHERE (MONTH(Ph.Ngaygiodat) = @Thang1Month OR MONTH(Ph.Ngaygiodat) = @Thang2Month) 
          AND (YEAR(Ph.Ngaygiodat) = @Thang1Year OR YEAR(Ph.Ngaygiodat) = @Thang2Year)
          AND Ph.MaCN = @MaCN 
        GROUP BY S.Ten;";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Thang1Month", thang1Month);
                    command.Parameters.AddWithValue("@Thang1Year", thang1Year);
                    command.Parameters.AddWithValue("@Thang2Month", thang2Month);
                    command.Parameters.AddWithValue("@Thang2Year", thang2Year);
                    command.Parameters.AddWithValue("@MaCN", maCN);

                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var doanhThu = new DoanhThu
                                {
                                    SanPham = reader["SanPham"].ToString(),
                                    DoanhThuThang1 = reader.IsDBNull(reader.GetOrdinal("DoanhThuThang1")) ? 0 : reader.GetInt32(reader.GetOrdinal("DoanhThuThang1")),
                                    DoanhThuThang2 = reader.IsDBNull(reader.GetOrdinal("DoanhThuThang2")) ? 0 : reader.GetInt32(reader.GetOrdinal("DoanhThuThang2"))
                                };
                                doanhThuList.Add(doanhThu);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Xử lý ngoại lệ (log lỗi, thông báo cho người dùng, v.v.)
                        return StatusCode(500, "Đã xảy ra lỗi khi truy xuất dữ liệu.");
                    }
                }
            }

            ViewBag.Thang1Month = thang1Month;
            ViewBag.Thang1Year = thang1Year;
            ViewBag.Thang2Month = thang2Month;
            ViewBag.Thang2Year = thang2Year;
            ViewBag.maCN = maCN;

            return View(doanhThuList);
        }



    }
}
