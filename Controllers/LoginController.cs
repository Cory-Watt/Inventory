using Inventory.Services;
using Microsoft.AspNetCore.Mvc;
using Inventory.Extensions;
using Inventory.ViewModels;
using Microsoft.AspNetCore.Identity;

// Namespace is defined to organize controllers
namespace Inventory.Controllers
{
    
    // LoginController inherits from the base Controller class provided by ASP.NET MVC
    public class LoginController : Controller
    {
        // SecurityService instance to handle the login logic
        private readonly SecurityService _securityService;

        // Constructor initializes a new instance of SecurityService
        public LoginController()
        {
            _securityService = new SecurityService();
        }

        // Action method for the default view of the login page
        // This method responds to the GET request and displays the login form
        public IActionResult Index()
        {
            // Returns the Index view with a new instance of LoginViewModel
            return View(new LoginViewModel());
        }

        // Action method to process the login
        // This method responds to POST requests when the login form is submitted
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents Cross-Site Request Forgery attacks
        public IActionResult ProcessLogin(LoginViewModel user)
        {
            // Checks if the submitted form data adheres to the model validation rules
            if (ModelState.IsValid)
            {
                if (_securityService.Login(user))
                {
                    // Set the username in session
                    HttpContext.Session.SetString("username", user.UserName);

                    // Set no-cache headers for the response
                    Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                    Response.Headers["Pragma"] = "no-cache";
                    Response.Headers["Expires"] = "0";

                    return View("LoginSuccess");
                }
                else
                {
                    // If login fails, add an error message to ModelState and return to the login form
                    ModelState.AddModelError("", "Login Failed.");
                }
            }

            // If ModelState is not valid, return to the Index view with the user model to display validation errors
            return View("Index", user);
        }

        // Action to logout the user
        public IActionResult Logout()
        {
            // Clear the existing session
            HttpContext.Session.Clear();

            // Redirect to the login/home page after logout 
            return RedirectToAction("Index", "Login");
        }
    }
}