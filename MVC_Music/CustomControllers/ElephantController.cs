using Microsoft.AspNetCore.Mvc.Filters;
using MVC_Music.Utilities;
using System.Reflection.Metadata;

namespace MVC_Music.CustomControllers
{
    /// <summary>
    /// The Elephant Controller has a good memory to help
    /// persist the Index Sort, Filter and Paging parameters
    /// into a URL stored in ViewData
    /// WARNING: Depends on the following Utilities
    ///  - CookieHelper
    ///  - MaintainURL
    /// </summary>
    public class ElephantController : CognizantController
    {
        internal string[] ActionWithURL = new string[] { "Details", "Create", "Edit", "Delete", "Add", "Update", "Remove" };
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (ActionWithURL.Contains(ActionName()))
            {
                ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
            }
            base.OnActionExecuting(context);
        }

        public override Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (ActionWithURL.Contains(ActionName()))
            {
                ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
            }
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
