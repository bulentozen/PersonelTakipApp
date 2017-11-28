using Microsoft.AspNet.Identity;
using PT.BLL.AccountRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PTWebMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var roleManager = MemberShipTools.NewRoleManager();
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new PT.Entitiy.IdentityModel.ApplicationRole() {Name="Admin",Description="Sistem Yöneticisi" });
            }
            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new PT.Entitiy.IdentityModel.ApplicationRole() { Name = "User", Description = "Sistem Kullanıcısı" });
            }
            if (!roleManager.RoleExists("Passive"))
            {
                roleManager.Create(new PT.Entitiy.IdentityModel.ApplicationRole() { Name = "Passive", Description = "E-Mail Aktivasyonu Gerekli" });
            }
        }
    }
}
