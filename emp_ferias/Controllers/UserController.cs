using emp_ferias.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web;
using System.Net;
using System.Web.Mvc;
using MvcFlashMessages;

namespace emp_ferias.Controllers
{
    [System.Web.Mvc.Authorize]
    public class UserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
         
        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: User/Index
        public ActionResult Index()
        {
            List<ApplicationUser> users = UserManager.Users.ToList();
            ApplicationUser loggedUser = UserManager.FindById(User.Identity.GetUserId());
            IndexUserViewModel viewModel = new IndexUserViewModel();

            foreach (var user in users)
            {
                UserInfo MappedUser = new UserInfo();

                MappedUser.Id = user.Id;
                MappedUser.UserName = user.UserName;
                MappedUser.Email = user.Email;
                MappedUser.Role = UserManager.GetRoles(user.Id).FirstOrDefault();

                if (MappedUser.Role == "Administrador")
                    MappedUser.RoleTests.IsAdmin = true;
                else if (MappedUser.Role == "Moderador")
                    MappedUser.RoleTests.IsMod = true;
                else
                    MappedUser.RoleTests.IsUser = true;

                viewModel.UserList.Add(MappedUser);
            }
            viewModel.UserList = viewModel.UserList.OrderBy(x => x.Role).ToList();

            UserInfo MappedLoggedUser = new UserInfo();

            MappedLoggedUser.Id = loggedUser.Id;
            MappedLoggedUser.UserName = loggedUser.UserName;
            MappedLoggedUser.Email = loggedUser.Email;
            MappedLoggedUser.Role = UserManager.GetRoles(loggedUser.Id).FirstOrDefault();

            if (MappedLoggedUser.Role == "Administrador")
                MappedLoggedUser.RoleTests.IsAdmin = true;
            else if (MappedLoggedUser.Role == "Moderador")
                MappedLoggedUser.RoleTests.IsMod = true;
            else
                MappedLoggedUser.RoleTests.IsUser = true;

            viewModel.LoggedUser = MappedLoggedUser;

            return View(viewModel);
         }

        // GET: User/Edit
        [HttpGet]
        [Authorize(Roles = "Administrador, Moderador")]
        public ActionResult Edit(string id)
        {

            var user = UserManager.FindById(id);

            if (user == null)
            {
                this.Flash("error", "Utilizador não encontrado.");
                return RedirectToAction("Index");
            }

            if (UserManager.IsInRole(User.Identity.GetUserId(), "Moderador") && (UserManager.IsInRole(id, "Administrador") || UserManager.IsInRole(id, "Moderador")))
            {
                this.Flash("error", "Não tem permissões suficientes para efetuar essa operação.");
                return RedirectToAction("Index");
            }

            EditUserViewModel viewModel = GetViewModel(user);

            viewModel.LoggedUser.Id = User.Identity.GetUserId();
            viewModel.LoggedUser.UserName = User.Identity.GetUserName();

            if (UserManager.IsInRole(User.Identity.GetUserId(), "Administrador"))
                viewModel.LoggedUser.RoleTests.IsAdmin = true;
            else if (UserManager.IsInRole(User.Identity.GetUserId(), "Moderador"))
                viewModel.LoggedUser.RoleTests.IsMod = true;
            else
                viewModel.LoggedUser.RoleTests.IsUser = true;

            return View(viewModel);
        }

        public static EditUserViewModel GetViewModel(ApplicationUser user)
        {
            return new EditUserViewModel()
            {
                CurrentEmail = user.Email,
                CurrentUsername = user.UserName,
                id = user.Id,
                NewEmail = user.Email,
                NewUsername = user.UserName,
            };
        }

        //POST: /User/Edit
        [HttpPost]
        [Authorize(Roles="Administrador, Moderador")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserViewModel viewModel)
        {
            if (UserManager.IsInRole(User.Identity.GetUserId(), "Moderador") && (UserManager.IsInRole(viewModel.id, "Administrador") || UserManager.IsInRole(viewModel.id, "Moderador")))
            {
                this.Flash("error", "Não tem permissões suficientes para efetuar essa operação.");
                return RedirectToAction("Index");
            }

            if (viewModel.id == null)
            {
                this.Flash("error", "Ocorreu um erro. Utilizador não encontrado.");
                return RedirectToAction("Index");
            }
            

            var user =  UserManager.FindById(viewModel.id);

            var vm = GetViewModel(user);

            user.UserName = viewModel.NewUsername;
            user.Email = viewModel.NewEmail;
            
            IdentityResult result = UserManager.Update(user);

            if (UserManager.IsInRole(User.Identity.GetUserId(), "Administrador"))
            {
                if (UserManager.GetRoles(user.Id).FirstOrDefault() != null) //previne uma exception caso o utilizador não tenha role por alguma razão
                    UserManager.RemoveFromRole(user.Id, UserManager.GetRoles(user.Id).FirstOrDefault());

                UserManager.AddToRole(user.Id, viewModel.NewRole);
            }

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    this.Flash("error", error);

                return View(vm);
            }

            return RedirectToAction("Index");
        }

        // GET: User/Create
        [Authorize(Roles="Administrador")]
        public ActionResult Create()
        {
            return View();
        }


       // POST: /User/Create
       [System.Web.Mvc.HttpPost]
       [ValidateAntiForgeryToken]
       [Authorize(Roles="Administrador")]
       public async Task<ActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, model.Role);
                    return RedirectToAction("Index", "User");
                }
                foreach (var error in result.Errors)
                    this.Flash("error", error);
            }
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}