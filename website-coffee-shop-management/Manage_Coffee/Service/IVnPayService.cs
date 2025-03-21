using Manage_Coffee.ViewModels;
using Manage_Coffee.Models;
using Manage_Coffee.Models.ViewModels;

namespace Manage_Coffee.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
