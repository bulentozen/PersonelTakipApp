﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Entitiy.ViewModel
{
    public class ProfilePasswordViewModel
    {
        public ProfileViewModel ProfileModel { get; set; } = new ProfileViewModel();
        public PasswordViewModel PasswordModel { get; set; } = new PasswordViewModel();
    }
   public class ProfileViewModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name ="Ad")]
        [StringLength(25)]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Soyad")]
        [StringLength(35)]
        public string SurName { get; set; }
        [Required]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        


    }
    public class PasswordViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Eski Şifre")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Şifreniz en az 5 karakter olmalıdır!")]
        public string OldPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Şifreniz en az 5 karakter olmalıdır!")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("NewPassword", ErrorMessage = "Şifreler Uyuşmuyor")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Şifreniz en az 5 karakter olmalıdır!")]
        public string NewPasswordConfirm { get; set; }
    }
}
