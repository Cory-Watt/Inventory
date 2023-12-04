using Inventory.Models;
using Inventory.ViewModels;

// The namespace groups related classes and services together
namespace Inventory.Services
{
    // SecurityService class provides methods for user authentication processes like registration and login
    public class SecurityService
    {
        // UserDAO instance for data access operations related to users
        private readonly UserDAO userDAO = new UserDAO();

        // Constructor for the SecurityService, currently empty
        public SecurityService()
        {
            // Constructor logic can be added here if needed in the future
        }

        // Method to handle the registration of a new user
        public bool Register(UserModel user)
        {
            // Delegates the call to the UserDAO's RegisterUser method
            // Returns true if registration is successful, false otherwise
            return userDAO.RegisterUser(user);
        }

        // Method to handle the login of an existing user
        public bool Login(LoginViewModel user)
        {
            // Delegates the call to the UserDAO's LoginUser method
            // Returns true if login is successful, false otherwise
            return userDAO.LoginUser(user);
        }
    }
}