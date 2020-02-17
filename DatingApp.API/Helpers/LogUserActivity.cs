using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Helpers
{
  public class LogUserActivity : IAsyncActionFilter
  {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Exceute teh action and get access to the context.
        var resultContext = await next();

        // Get the userID
        var userId = int.Parse(resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

        // Get our repo, which is provided as a service so we ahve to request it as a service.
        // var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();
        // or
        var repo = (IDatingRepository)resultContext.HttpContext.RequestServices.GetService(typeof(IDatingRepository));

        var user = await repo.GetUser(userId, true);
        user.LastActive = DateTime.Now;
        await repo.SaveAll();
    }
  }
}