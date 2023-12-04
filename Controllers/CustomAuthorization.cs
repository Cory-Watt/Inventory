using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inventory.Controllers
{
    // CustomAuthorizationAttribute: Custom attribute for authorization handling.
    // It implements IAuthorizationFilter to integrate with ASP.NET Core's filter pipeline.
    public class CustomAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        // OnAuthorization: The method that gets called when the authorization filter is invoked.
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Retrieve the username from the session.
            // This assumes that the username is stored in the session upon successful login.
            string userName = context.HttpContext.Session.GetString("username");

            // Check if the userName is null, which would indicate that the user is not logged in.
            if (userName == null)
            {
                // If userName is not found, redirect the user to the login page.
                // This prevents unauthorized access to protected resources.
                context.Result = new RedirectResult("/login");
            }
            else
            {
                // If the username exists in the session, do nothing and let the request proceed.
                // This means the user is authorized to access the resource.
            }
        }
    }
}