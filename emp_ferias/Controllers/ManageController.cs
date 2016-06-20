using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using emp_ferias.Models;
using MvcFlashMessages;
using emp_ferias.lib.Classes;
using System.Collections.Generic;
using emp_ferias.lib.Services;
using emp_ferias.Services;

namespace emp_ferias.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        ServiceMarcacoes serviceMarcacoes = new ServiceMarcacoes(new ServiceLogin());

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        private static List<IndexViewModel> MapManageViewModel(List<Marcacao> Marcacoes)
        {
            List<IndexViewModel> MappedViewModels = new List<IndexViewModel>();
            foreach (var i in Marcacoes)
            {
                var MappedViewModel = new IndexViewModel();

                MappedViewModel.id = i.Id;
                MappedViewModel.DataInicio = i.DataInicio;
                MappedViewModel.DataFim = i.DataFim;
                MappedViewModel.Motivo = i.Motivo;
                MappedViewModel.Aprovado = i.Aprovado;
                if (i.UserAprovacao != null)
                {
                    MappedViewModel.RazaoAprovacao = i.RazaoAprovacao;
                    MappedViewModel.UserNameAprovacao = i.UserAprovacao.UserName;
                }
                MappedViewModels.Add(MappedViewModel);
            }

            return MappedViewModels;
        }

        // GET: /Manage/Index
        public ActionResult Index(ManageMessageId? message)
        {
            if (message == ManageMessageId.ChangePasswordSuccess)
            {
                this.Flash("success", "A password foi alterada com sucesso.");
            }
            else if (message == ManageMessageId.Error)
            {
                this.Flash("error", "Ocorreu um erro a processar o pedido.");
            }

            return View(MapManageViewModel(serviceMarcacoes.GetUserMarcacoes(User.Identity.GetUserId())));
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }
#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            Error
        }

#endregion
    }
}