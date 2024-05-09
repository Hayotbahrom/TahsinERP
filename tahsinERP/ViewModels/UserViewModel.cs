using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class UserViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Loginni kiriting")]
        public string UName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "Input password")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public int companyID { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string RoleID { get; set; }
        public bool IsActive { get; set; }
        public bool KeepMeSigned { get; set; }
        public List<USER> userList { get; set; }
        public int SelectedRoleId { get; set; }
        public List<string> Roles { get; set; }

    }
}