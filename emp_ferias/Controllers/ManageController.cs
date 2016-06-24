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

        private IndexViewModel MapIndexViewModel(List<Marcacao> Marcacoes)
        {

            var CurrentUser = UserManager.FindById(User.Identity.GetUserId());

            IndexViewModel IndexViewModel = new IndexViewModel();

            List<UserMarcacao> MappedMarcacoes = new List<UserMarcacao>();
            foreach (var i in Marcacoes)
            {
                var MappedMarcacao = new UserMarcacao();

                MappedMarcacao.id = i.Id;
                MappedMarcacao.DataInicio = i.DataInicio;
                MappedMarcacao.DataFim = i.DataFim;
                MappedMarcacao.Motivo = i.Motivo;
                MappedMarcacao.Status = i.Status;
                if (i.ActionUser != null)
                {
                    MappedMarcacao.RazaoRejeicao = i.RazaoRejeicao;
                    MappedMarcacao.ActionUserName = i.ActionUser.UserName;
                }
                MappedMarcacoes.Add(MappedMarcacao);
            }
            IndexViewModel.Email = CurrentUser.Email;
            IndexViewModel.UserId = CurrentUser.Id;
            IndexViewModel.UserName = CurrentUser.UserName;
            IndexViewModel.Marcacoes = MappedMarcacoes;

            return IndexViewModel;
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
            return View(MapIndexViewModel(serviceMarcacoes.GetUserMarcacoes(User.Identity.GetUserId())));
        }

        // GET: /Manage/ChartData
        public ActionResult ChartData(DataSet DataSet, bool IncludeRejected)
        {
            var RazaoMarcacoes = serviceMarcacoes.GetUserRazaoMarcacao(User.Identity.GetUserId(), DataSet, IncludeRejected);
            return Json(RazaoMarcacoes,JsonRequestBehavior.AllowGet);
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
            if (model.NewPassword != model.ConfirmPassword)
            {
                this.Flash("error", "A nova password e a confirmação não correspondem.");
                return View(model);
            }

            var VerifyUser = await UserManager.FindAsync(User.Identity.Name, model.OldPassword);
            if (VerifyUser == null)
            {
                this.Flash("error", "Password atual incorreta.");
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
            this.Flash("error", "Ocorreu um erro.");
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