using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class DashboardController : Controller
    {
        // GET: /Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: /Dashboard/WarrantyRegistration
        public IActionResult WarrantyRegistration()
        {
            return PartialView("_WarrantyRegistration");
        }

        // GET: /Dashboard/WarrantyPending
        public IActionResult WarrantyPending(String dealerId)
        {
            return PartialView("_WarrantyPending");
        }

        // GET: /Dashboard/WarrantyApproved
        public IActionResult WarrantyApproved(String dealerId)
        {
            return PartialView("_WarrantyApproved");
        }
    }
}
