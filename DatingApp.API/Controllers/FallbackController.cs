using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    // need to derive from Contrlller becuuse we need access to views
    public class FallbackController: Controller
    {
        public IActionResult Index() 
        {
            // return a physical file from wwwroot with index.html name and return it as text/HTML
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}