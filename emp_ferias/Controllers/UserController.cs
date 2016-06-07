using emp_ferias.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Web.Mvc;

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

            return View(users);
        }

        // GET: User/Edit
        public async Task<ActionResult> Edit(string id)
        {
            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            EditUserViewModel viewModel = GetViewModel(user);

            return View(viewModel);
        }

        private static EditUserViewModel GetViewModel(ApplicationUser user)
        {
            return new EditUserViewModel()
            {
                CurrentEmail = user.Email,
                CurrentUsername = user.UserName,
                id = user.Id,
                NewEmail = user.Email,
                NewUsername = user.UserName
            };
        }

        //POST: /User/Edit
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel viewModel)
        {
            if (viewModel.id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
            }

            var user = await UserManager.FindByIdAsync(viewModel.id);

            var vm = GetViewModel(user);

            user.UserName = viewModel.NewUsername;
            user.Email = viewModel.NewEmail;
            
            IdentityResult result = UserManager.Update(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);

                return View(vm);
            }

            return RedirectToAction("Index");
        }

        // GET: User/Create
        public ActionResult Create()
        {
            return View();
        }


       // POST: /User/Create
       [System.Web.Mvc.HttpPost]
       [System.Web.Mvc.AllowAnonymous]
       [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
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