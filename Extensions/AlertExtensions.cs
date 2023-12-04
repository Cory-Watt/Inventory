using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures; // Ensure this using directive is included

namespace Inventory.Extensions
{
    public static class AlertExtensions
    {
        // Extension method for Controller to add success message
        public static RedirectToActionResult WithSuccess(this Controller controller, string actionName, string message)
        {
            controller.TempData["SuccessMessage"] = message;
            return controller.RedirectToAction(actionName);
        }

        // Extension method for Controller to add danger message
        public static RedirectToActionResult WithDanger(this Controller controller, string actionName, string message)
        {
            controller.TempData["ErrorMessage"] = message;
            return controller.RedirectToAction(actionName);
        }
    }
}