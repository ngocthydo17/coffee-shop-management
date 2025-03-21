using Manage_Coffee.Models;
using Microsoft.AspNetCore.Mvc;

namespace Manage_Coffee.ViewComponents
{
    public class CategoryADViewComponent : ViewComponent
    {
        private readonly Cf2Context _context;

        public CategoryADViewComponent(Cf2Context context)
        {
            _context = context;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var loais = await Task.Run(() => _context.Loais.ToList());
            return View(loais);
        }
    }
}
