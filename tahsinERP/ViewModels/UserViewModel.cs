using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using tahsinERP.Models;

namespace tahsinERP.ViewModels
{
    public class UserViewModel
    {
        public int ID { get; set; }
        public string UName { get; set; }
        public string FullName { get; set; }
        [Required(ErrorMessage = "Emailni kiriting")]
        public string Email { get; set; }
        [DataType(DataType.Upload)]
        public HttpPostedFileBase File;
        public int companyID { get; set; }
        [Required(ErrorMessage = "Kalit so'zni kiriting")]
        public string Password { get; set; }
        public string Role { get; set; }
        public int RoleID { get; set; }
        public bool IsActive { get; set; }
        public bool KeepMeSigned { get; set; }
        public List<USER> userList { get; set; }
        public int SelectedRoleId { get; set; }
        public List<string> Roles { get; set; }
        public string IP_adrr { get; set; }
        public string MAC_adrr { get; set; }
        public DateTime datetime { get; set; }
        public List<USERLOG> logs { get; set; }

    }
}