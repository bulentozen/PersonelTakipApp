using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
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
            if (checkUser != null)
            {
                ModelState.AddModelError(string.Empty, "Bu kullanıcı zaten kayıtlı!");
                return View(model);
            }

            checkUser = userManager.FindByEmail(model.Email);
            if (checkUser != null)
            {
                ModelState.AddModelError(string.Empty, "Bu E posta adresi kullanılmakta");
                return View(model);
            }

            //register işlemi yapılır.
            var activitationCode = Guid.NewGuid().ToString();
            ApplicationUser user = new ApplicationUser()
            {
                Name = model.Name,
                SurName = model.SurName,
                Email = model.Email,
                UserName = model.UserName,
                ActivationCode = activitationCode

            };

            var response = userManager.Create(user, model.Password);
            if (response.Succeeded)
            {
                string SiteURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                if (userManager.Users.Count() == 1)
                {
                    userManager.AddToRole(user.Id, "Admin");
                    await SiteSettings.SendMail(new MailModel
                    {
                        To = user.Email,
                        Subject = "Hoşgeldin Sahip!",
                        Message = "Sitemizi yöneteceğin için çok mutluyuz ^^"

                    });
                }
                else
                {
                    userManager.AddToRole(user.Id, "Passive");
                    await SiteSettings.SendMail(new MailModel
                    {
                        To = user.Email,
                        Subject = "Personel Yönetimi - Aktivasyon",
                        Message = $"Merhaba {user.Name} {user.SurName} <br/> Sistemi kullanabilmeniz için <a href='{SiteURL}/Account/Activation?code={activitationCode}'>Aktivasyon Kodu</a>"
                    });
                }

                return RedirectToAction("Login", "Account");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Kayıt işleminde hata oluştu!");
                return View(model);
            }
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var UserManager = MemberShipTools.NewUserManager();
            var user = await UserManager.FindAsync(model.UserName, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Böyle bir kullanıcı bulunamadı");
                return View(model);
            }
            var authManager = HttpContext.GetOwinContext().Authentication;
            var userIdentity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            authManager.SignIn(new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            }, userIdentity);


            return RedirectToAction("Index", "Home");
        }
        [Authorize] //SignIn yapılmışmı ona bakıyor (giriş yapılmadan logout a geçirtmiyor yani)
        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut();
            return RedirectToAction("Login", "Account");
        }
        public async Task<ActionResult> Activation(string code)
        {
            var userStore = MemberShipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var sonuc = userStore.Context.Set<ApplicationUser>().FirstOrDefault(x => x.ActivationCode == code);
            if (sonuc == null)
            {
                ViewBag.sonuc = "Aktivasyon işlemi başarısız";
                return View();
            }
            sonuc.EmailConfirmed = true;
            await userStore.UpdateAsync(sonuc);
            await userStore.Context.SaveChangesAsync();

            userManager.RemoveFromRole(sonuc.Id, "Passive");
            userManager.AddToRole(sonuc.Id, "User");
            ViewBag.sonuc = $"Merhaba  {sonuc.Name} {sonuc.SurName} <br/> Aktivasyon işleminiz başarılı";

            await SiteSettings.SendMail(new MailModel()
            {
                To = sonuc.Email,
                Message = ViewBag.sonuc.ToString(),
                Subject = "Aktivasyon",
                Bcc = "poyildirim@gmail.com"
            });

            return View();
        }
        public ActionResult RecoverPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecoverPassword(string email)
        {
            var userStore = MemberShipTools.NewUserStore();
            var userManager = new UserManager<ApplicationUser>(userStore);
            var sonuc = userStore.Context.Set<ApplicationUser>().FirstOrDefault(x => x.Email == email);
            if (sonuc == null)
            {
                ViewBag.sonuc = "E mail adresiniz sisteme kayıtlı değil";
                return View();
            }
            var randomPass = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);
            await userStore.SetPasswordHashAsync(sonuc, userManager.PasswordHasher.HashPassword(randomPass));
            await userStore.UpdateAsync(sonuc);
            await userStore.Context.SaveChangesAsync();

            await SiteSettings.SendMail(new MailModel()
            {
                To = sonuc.Email,
                Subject = "Şifreniz Değişti",
                Message = $"Merhaba {sonuc.Name}  {sonuc.SurName}  <br/>Yeni Şifreniz: <b>{randomPass}</b>"
            });
            ViewBag.sonuc = "E mail adresinize yeni şifreniz gönderilmiştir";
            return View();
        }

        [Authorize]
        public ActionResult Profile()
        {
            var userManager = MemberShipTools.NewUserManager();
            var user = userManager.FindById(HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId());
            var model = new ProfilePasswordViewModel()
            {
                ProfileModel = new ProfileViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    SurName = user.SurName,
                    UserName = user.UserName
                }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Profile(ProfilePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            try
            {
                var userStore = MemberShipTools.NewUserStore();
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindById(model.ProfileModel.Id);
                user.Name = model.ProfileModel.Name;
                user.SurName = model.ProfileModel.SurName;
                if (user.Email != model.ProfileModel.Email)
                {
                    user.Email = model.ProfileModel.Email;
                    if (HttpContext.User.IsInRole("Admin"))
                    {
                        userManager.RemoveFromRole(user.Id, "Admin");
                    }
                    else if (HttpContext.User.IsInRole("User"))
                    {
                        userManager.RemoveFromRole(user.Id, "User");
                    }

                    userManager.AddToRole(user.Id, "Passive");
                    user.ActivationCode = Guid.NewGuid().ToString().Replace("-", "");
                    string siteURL = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    await SiteSettings.SendMail(new MailModel
                    {
                        To = user.Email,
                        Subject = "Personel Yönetimi - Aktivasyon",
                        Message = $"Merhaba {user.Name} {user.SurName} <br/> Email adresinizi <b> değiştirdiğiniz </b> için hesabınızı tekrar aktif etmelisiniz. <a href='{siteURL}/Account/Activation?code={user.ActivationCode}'>Aktivasyon Kodu </a>"
                    });

                }

                await userStore.UpdateAsync(user);
                await userStore.Context.SaveChangesAsync();
                var model1 = new ProfilePasswordViewModel()
                {
                    ProfileModel = new ProfileViewModel()
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Name = user.Name,
                        SurName = user.SurName,
                        UserName = user.UserName
                    }
                };
                ViewBag.sonuc = "Bilgileriniz güncelleştirilmiştir.";
                return View(model1);
            }
            catch (Exception exp)
            {

                ViewBag.sonuc = exp.Message;
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePassword(ProfilePasswordViewModel model)
        {
            if (model.PasswordModel.NewPassword !=model.PasswordModel.NewPasswordConfirm)
            {
                ModelState.AddModelError(string.Empty,"Şifreler Uyuşmuyor");
                return View("Profile", model);
            }
            try
            {
                var userStore = MemberShipTools.NewUserStore();
                var userManager = new UserManager<ApplicationUser>(userStore);
                var user = userManager.FindById(model.ProfileModel.Id);
                user = userManager.Find(user.UserName, model.PasswordModel.OldPassword);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Mevcut şifreniz yanlış girilmiştir.");
                    return View("Profile", model);
                }
                await userStore.SetPasswordHashAsync(user, userManager.PasswordHasher.HashPassword(model.PasswordModel.NewPassword));
                await userStore.UpdateAsync(user);
                await userStore.Context.SaveChangesAsync();

                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Profile");
            }
            catch (Exception exp)
            {
                ViewBag.sonuc = "Güncelleştirme işleminde bir hata oluştu!" + exp.Message;
                return View("Profile", model);
            }

        }
    }
}