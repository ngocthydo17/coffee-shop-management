using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manage_Coffee.ViewComponents
{
	public class SelectToppingViewComponent :ViewComponent
	{
		private readonly Cf2Context _context;

		public SelectToppingViewComponent(Cf2Context context)
		{
			_context = context;
		}


		public async Task<IViewComponentResult> InvokeAsync()
		{
			List<SanPham> toppings = await _context.SanPhams
		   .Where(sp => sp.Maloai == "L0003" ) // Giả sử L0004 và L0005 là loại topping
		   .ToListAsync();

			return View(toppings);
		}
	}
}
