using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using emp_ferias.lib.Classes;

namespace emp_ferias.Models
{
    public class IndexViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<UserMarcacao> Marcacoes { get; set; }
    }

    public class UserMarcacao
    {
        public int id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFim { get; set; }
        public Motivo Motivo { get; set; }
        public Status Status { get; set; }
        public string ActionUserName { get; set; }
        public string RazaoRejeicao { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password atual")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "Password nova")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar password nova")]
        public string ConfirmPassword { get; set; }
    }
}