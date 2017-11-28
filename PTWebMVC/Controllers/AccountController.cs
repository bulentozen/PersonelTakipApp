using Microsoft.AspNet.Identity;
using PT.BLL.AccountRepository;
using PT.BLL.Settings;
using PT.Entitiy.IdentityModel;
using PT.Entitiy.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PTWebMVC.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var userManager = MemberShipTools.NewUserManager();
            var checkUser = userManager.FindByName(model.UserName);
            if (checkUser !=null)
            {
                ModelState.AddModelError(string.Empty,"Bu kullanıcı zaten kayıtlı!");
                return View(model);
            }

            //register işlemi yapılır.
            var activitationCode = Guid.NewGuid().ToString();
            ApplicationUser user = new ApplicationUser()
            {
                Name = model.Name,
                SurName=model.SurName,
                Email=model.Email,
                UserName=model.UserName,
                ActivationCode=activitationCode
                
            };

            var response = userManager.Create(user,model.Password);
            if (response.Succeeded)
            {
                string SiteURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                if (userManager.Users.Count() == 1)
                {
                    userManager.AddToRole(user.Id, "Admin");
                    await SiteSettings.SendMail(new MainModel
                    {
                        To = user.Email,
                        Subject = "Hoşgeldin Sahip!",
                        Message = "Sitemizi yöneteceğin için çok mutluyuz ^^"

                    });
                }
                else
                {
                    userManager.AddToRole(user.Id, "Passive");
                    await SiteSettings.SendMail(new MainModel
                    {
                        To = user.Email,
                        Subject = "Personel Yönetimi - Aktivasyon",
                        Message = $"Merhaba {user.Name} {user.SurName} <br/> Sistemi kullanabilmeniz için <a href='{SiteURL}/Account/Activation?code={activitationCode}'>Aktivasyon Kodu</a>"
                    });
                }

                return RedirectToAction("Login","Account");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Kayıt işleminde hata oluştu!");
                return View(model);
            }
        }
    }
}