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
            usercopy copy = new usercopy();

            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            copy.UserName = user.UserName;
            copy.Email = user.Email;

            ViewBag.CopyUserName = copy.UserName;
            ViewBag.CopyEmail = copy.Email;


            return View(user);
        }

        //POST: /User/Edit
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UserName, Email")] ApplicationUser formtarget, string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
            }
            usercopy copy = new usercopy();

            var user = await UserManager.FindByIdAsync(id);

            copy.UserName = user.UserName;
            copy.Email = user.Email;

            ViewBag.CopyUserName = copy.UserName;
            ViewBag.CopyEmail = copy.Email;

            user.UserName = formtarget.UserName;
            user.Email = formtarget.Email;

            IdentityResult result = UserManager.Update(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);

                return View(user);
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