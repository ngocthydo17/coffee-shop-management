using Microsoft.AspNetCore.Mvc;

using Manage_Coffee.Models;

[Route("api/KHController")]
[ApiController]
public class KHController : ControllerBase
{
    private readonly Cf2Context _context;

    public KHController(Cf2Context context)
    {
        _context = context;
    }

    [HttpPost("AddXu")]
    public async Task<IActionResult> AddXu([FromBody] CongXuRequest request)
    {
        // Tìm khách hàng theo mã khách hàng
        var khachHang = await _context.KhachHangs.FindAsync(request.MaKh);

        if (khachHang == null)
        {
            return NotFound("Khách hàng không tồn tại.");
        }

        // Cộng Xu cho khách hàng
        khachHang.Xu = (khachHang.Xu ?? 0) + request.Xu;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Cộng Xu thành công", XuHienTai = khachHang.Xu  });
    }


}

public class CongXuRequest
{
    public string MaKh { get; set; } = null!;
    public int Xu { get; set; }
}
