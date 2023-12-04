using Microsoft.AspNetCore.Mvc;
using Inventory.Models;
using Inventory.Services;

// Define the namespace that groups related controllers together
namespace Inventory.Controllers
{
    // Define the RegistrationController class which inherits from the base Controller class
    public class RegistrationController : Controller
    {
        // A private instance of SecurityService to handle business logic related to user registration
        private readonly SecurityService _securityService;

        // Constructor for the controller initializing the SecurityService instance
        public RegistrationController()
        {
            _securityService = new SecurityService();
        }

        // Action method responding to a GET request to serve the registration page
        public IActionResult Index()
        {
            // Returns the registration view along with a new empty UserModel to be filled out by the user
            return View(new UserModel());
        }

        // Action method to process the user registration
        // Responds to POST requests when the user submits the registration form
        [HttpPost]
        [ValidateAntiForgeryToken] // Attribute to prevent cross-site request forgery attacks
        public IActionResult ProcessRegistration(UserModel user)
        {
            // Checks if the form data meets all validation rules defined in the UserModel
            if (ModelState.IsValid)
            {
                // Attempts to register the user using the SecurityService
                if (_securityService.Register(user))
                {
                    // On successful registration, renders the RegistrationSuccess view with the user's data
                    return View("RegistrationSuccess", user);
                }
                else
                {
                    // If registration fails, adds an error message to the ModelState to display to the user
                    ModelState.AddModelError("", "Registration Failed. User could not be added.");
                }
            }
            // If the form data is invalid, re-renders the registration form with the user's input and validation messages
            return View("Index", user);
        }
    }
}