using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace emp_ferias.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;

        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class IndexUserViewModel
    {
        public UserInfo LoggedUser { get; set; } = new UserInfo();
        public List<UserInfo> UserList { get; set; } = new List<UserInfo>();
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Role { get; set; }
        public RoleTests RoleTests { get; set; } = new RoleTests();
    }

    public class RoleTests
    {
        public bool IsAdmin { get; set; } = false;
        public bool IsMod { get; set; } = false;
        public bool IsUser { get; set; } = false;
    }

    public class EditUserViewModel
    {
        public UserInfo LoggedUser { get; set; } = new UserInfo();
        public string CurrentUsername { get; set; }
        [EmailAddress]
        public string CurrentEmail { get; set; }
        public string id { get; set; }
        [Required(ErrorMessage = "O campo para o novo nome de utilizador é obrigatório.")]
        public string NewUsername { get; set; }
        [Required(ErrorMessage = "O campo para o novo endereço de Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Endereço Email inválido")]
        public string NewEmail { get; set; }
        [Required(ErrorMessage = "O campo para a nova função é obrigatório.")]
        public string NewRole { get; set; }
    }

    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required]
        [Display(Name="RoleName")]
        public string Name { get; set; }
    }
    
    
}