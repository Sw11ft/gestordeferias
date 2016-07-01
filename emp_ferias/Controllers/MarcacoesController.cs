using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using emp_ferias.lib.Classes;
using emp_ferias.lib.DAL;
using emp_ferias.Models;
using emp_ferias.lib.Services;
using emp_ferias.Services;
using MvcFlashMessages;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Net.Mail;
using System.Globalization;

namespace emp_ferias.Controllers
{
    [Authorize]
    public class MarcacoesController : Controller
    {
        ServiceMarcacoes serviceMarcacoes = new ServiceMarcacoes(new ServiceLogin());

        private ApplicationUserManager _userManager;
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

        public UserInfo LoggedUserInfo()
        {
            UserInfo UserInfo = new UserInfo();
            ApplicationUser LoggedUser = UserManager.FindById(User.Identity.GetUserId());

            UserInfo.Id = LoggedUser.Id;
            UserInfo.UserName = LoggedUser.UserName;
            UserInfo.Email = LoggedUser.Email;
            UserInfo.Role = UserManager.GetRoles(LoggedUser.Id).FirstOrDefault();
            if (UserInfo.Role == "Administrador")
                UserInfo.RoleTests.IsAdmin = true;
            else if (UserInfo.Role == "Moderador")
                UserInfo.RoleTests.IsMod = true;
            else
                UserInfo.RoleTests.IsUser = true;

            return UserInfo;
        }

        private IndexMarcacaoViewModel MapIndexMarcacaoViewModel(List<Marcacao> Marcacoes)
        {
            List<Marcacao> MappedMarcacoes = new List<Marcacao>();
            IndexMarcacaoViewModel viewModel = new IndexMarcacaoViewModel();
            foreach (var i in Marcacoes)
            {
                Marcacao MappedMarcacao = new Marcacao();

                MappedMarcacao.Id = i.Id;
                MappedMarcacao.User = i.User;
                MappedMarcacao.DataPedido = i.DataPedido;
                MappedMarcacao.DataInicio = i.DataInicio;
                MappedMarcacao.DataFim = i.DataFim;
                MappedMarcacao.Notas = i.Notas;
                MappedMarcacao.Motivo = i.Motivo;
                MappedMarcacao.Status = i.Status;
                MappedMarcacao.UserNotificado = i.UserNotificado;
                if (i.ActionUser != null)
                {
                    MappedMarcacao.RazaoRejeicao = i.RazaoRejeicao;
                    MappedMarcacao.ActionUser = i.ActionUser;
                }
                MappedMarcacoes.Add(MappedMarcacao);
            }

            viewModel.Marcacoes = MappedMarcacoes;
            viewModel.LoggedUser = LoggedUserInfo();

            return viewModel;
                 
        }

        // GET: Marcacoes
        public ActionResult Index()
        {
            var viewModel = MapIndexMarcacaoViewModel(serviceMarcacoes.Get());
            ApplicationUser LoggedUser = UserManager.FindById(User.Identity.GetUserId());

            viewModel.LoggedUser.Id = LoggedUser.Id;

            if (UserManager.IsInRole(LoggedUser.Id, "Administrador"))
                viewModel.LoggedUser.RoleTests.IsAdmin = true;
            else if (UserManager.IsInRole(LoggedUser.Id, "Moderador"))
                viewModel.LoggedUser.RoleTests.IsMod = true;
            else
                viewModel.LoggedUser.RoleTests.IsUser = true;

            return View(viewModel);
        }

        // GET: Marcacoes/Overview
        public ActionResult Overview()
        {
            return View(MapIndexMarcacaoViewModel(serviceMarcacoes.Get()));
        }

        [Authorize(Roles="Administrador")]
        public ActionResult Refresh()
        {
            serviceMarcacoes.RefreshStatus();
            this.Flash("success", "Marcações atualizadas.");
            return RedirectToAction("Index");
        }

        private static List<Events> MapMarcacoesCalendar(List<Marcacao> Marcacoes, DateTime start, DateTime end)
        {
            List<Events> EventList = new List<Events>();
            foreach (var m in Marcacoes)
            {
                if (m.Status != Status.Rejeitado && m.Status != Status.Expirado && m.Status != Status.Pendente) { 
                    if (m.DataFim >= start && m.DataInicio <= end)
                    {
                        Events newEvent = new Events
                        {
                            id = m.Id.ToString(),
                            title = "#" + m.Id + " - " + m.User.UserName + ", " + m.Motivo,
                            start = m.DataInicio.ToString("s"),
                            end = m.DataFim.AddDays(1).ToString("s"),
                            allDay = true,
                        
                        };
                        if (m.DataFim < DateTime.Today)
                        {
                            newEvent.color = "#777";
                            newEvent.textColor = "#ffffff";
                        }
                        else if (m.DataFim >= DateTime.Today && m.DataInicio <= DateTime.Today)
                        {
                            newEvent.color = "#337ab7";
                            newEvent.textColor = "#ffffff";
                        }
                        else
                        {
                            newEvent.color = "#5bc0de";
                            newEvent.textColor = "#ffffff";
                        }
                        EventList.Add(newEvent);
                    }
                }
            }
            return EventList;
        }

        public ActionResult GetMarcacoes (DateTime start, DateTime end)
        {
            List<Marcacao> Marcacoes = serviceMarcacoes.Get();

            var EventList = MapMarcacoesCalendar(Marcacoes, start, end);

            var EventArray = EventList.ToArray();

            return Json(EventArray, JsonRequestBehavior.AllowGet);
        }

        // GET: Marcacoes/Details/5
        public ActionResult Details(int? id)
        {
            return View();
        }

        // GET: Marcacoes/Create
        public ActionResult Create()
        {
            return View();
        }

        private static Marcacao MapViewModel(CreateMarcacaoViewModel UserInput)
        {
            return new Marcacao
            {
                DataInicio = UserInput.DataInicio,
                DataFim = UserInput.DataFim,
                Motivo = UserInput.Motivo,
                Notas = UserInput.Notas
            };
        }

        // POST: Marcacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateMarcacaoViewModel viewModel)
        {
            var ExecutionResult = serviceMarcacoes.Create(MapViewModel(viewModel));
            bool valid = true;
            foreach (var i in ExecutionResult)
                if (i.MessageType == MessageType.Error)
                {
                    this.Flash("error", i.Message);
                    valid = false;
                }

            if (valid)
                return RedirectToAction("Index");
            else 
                return View(viewModel);

        }

        //POST: Marcacoes/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(ApproveMarcacaoViewModel ApprInfo)
        {
            var ExecutionResult = serviceMarcacoes.Approve(ApprInfo.marcId);
            bool ExecutionValid = true;
            foreach (var i in ExecutionResult)
            {
                if (i.MessageType == MessageType.Error)
                {
                    this.Flash("error", i.Message);
                    ExecutionValid = false;
                }
            }
            
            if (!ExecutionValid)
            {
                return RedirectToAction("Index");
            }
                    
            if (ApprInfo.sendEmail)
            {
                Marcacao Marcacao = serviceMarcacoes.FindById(true, ApprInfo.marcId);

                if (Marcacao == null)
                {
                    this.Flash("error", "Marcação não encontrada.");
                    return RedirectToAction("Index");
                }

                var user = UserManager.FindById(Marcacao.UserId);

                var message = new MailMessage();
                message.From = (new MailAddress("notificacoes@test.com"));
                message.To.Add(new MailAddress(user.Email));
                message.Subject = "Marcação #" + ApprInfo.marcId + " aprovada";
                message.Body = "A sua marcação #" + ApprInfo.marcId + " foi aprovada por " + Marcacao.ActionUser.UserName + ".";

                using (var smtp = new SmtpClient())
                {
                    await smtp.SendMailAsync(message);
                    this.Flash("success", "Marcação aprovada e email enviado.");
                    return RedirectToAction("Index");
                }
            }

            this.Flash("Success", "Marcação aprovada.");
            return RedirectToAction("Index");
        }

        private static Marcacao MapRejectViewModel(RejectMarcacaoViewModel RejectionInfo)
        {
            return new Marcacao
            {
                Id = RejectionInfo.marcRejectId,
                RazaoRejeicao = RejectionInfo.Razao
            };
        }

        //POST: Marcacoes/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reject(RejectMarcacaoViewModel RejectionInfo)
        {
            var ExecutionResult = serviceMarcacoes.Reject(MapRejectViewModel(RejectionInfo));
            bool ExecutionValid = true;
            foreach (var i in ExecutionResult)
            {
                if (i.MessageType == MessageType.Error)
                {
                    this.Flash("error", i.Message);
                    ExecutionValid = false;
                }
            }

            if (!ExecutionValid)
            {
                return RedirectToAction("Index");
            }

            if (RejectionInfo.sendEmail)
            {
                Marcacao Marcacao = serviceMarcacoes.FindById(true, RejectionInfo.marcRejectId);

                if (Marcacao == null)
                {
                    this.Flash("error", "Marcação não encontrada.");
                    return RedirectToAction("Index");
                }

                var user = UserManager.FindById(Marcacao.UserId);

                var message = new MailMessage();
                message.From = (new MailAddress("notificacoes@test.com"));
                message.To.Add(new MailAddress(user.Email));
                message.Subject = "Marcação #" + RejectionInfo.marcRejectId + " rejeitada";
                message.Body = "A sua marcação #" + RejectionInfo.marcRejectId + " foi rejeitada por " + Marcacao.ActionUser.UserName + " com a razão '" + RejectionInfo.Razao + "'.";

                using (var smtp = new SmtpClient())
                {
                    await smtp.SendMailAsync(message);
                    this.Flash("warning", "Marcação rejeitada e email enviado.");
                    return RedirectToAction("Index");
                }
            }

            this.Flash("warning", "Marcação rejeitada.");
            return RedirectToAction("Index");
        }

        // GET: Marcacoes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            Marcacao marcacao = serviceMarcacoes.FindById(false, id);

            if (marcacao == null)
            {
                this.Flash("error", "Marcação não encontrada");
                return RedirectToAction("Index");
            }

            return View(marcacao);
        }

        // POST: Marcacoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // GET: Marcacoes/Delete/5
        public ActionResult Delete(int? id)
        {

            return View();
        }

        // POST: Marcacoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            return View();
        }


        // GET: Marcacoes/My
        public ActionResult My()
        {
            return View(MapIndexMarcacaoViewModel(serviceMarcacoes.GetUserMarcacoes(User.Identity.GetUserId())));
        }

        // GET: Marcacoes/MyTableData
        public ActionResult MyTableData(string fromDate, string toDate, fieldSelect fieldSelect)
        {
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime parsedFromDate = DateTime.Parse(fromDate.ToString(), new CultureInfo("pt-PT"));
                DateTime parsedToDate = DateTime.Parse(toDate.ToString(), new CultureInfo("pt-PT"));
                return PartialView("_MyTableData", MapIndexMarcacaoViewModel(serviceMarcacoes.GetMyMarcacoes(User.Identity.GetUserId(), parsedFromDate, parsedToDate, fieldSelect)));
            }
            else if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime parsedFromDate = DateTime.Parse(fromDate.ToString(), new CultureInfo("pt-PT"));
                return PartialView("_MyTableData", MapIndexMarcacaoViewModel(serviceMarcacoes.GetMyMarcacoes(User.Identity.GetUserId(), parsedFromDate, null, fieldSelect)));
            }
            else if (!string.IsNullOrEmpty(toDate))
            {
                DateTime parsedToDate = DateTime.Parse(toDate.ToString(), new CultureInfo("pt-PT"));
                return PartialView("_MyTableData", MapIndexMarcacaoViewModel(serviceMarcacoes.GetMyMarcacoes(User.Identity.GetUserId(), null, parsedToDate, fieldSelect)));
            }
            else
                return PartialView("_MyTableData", MapIndexMarcacaoViewModel(serviceMarcacoes.GetMyMarcacoes(User.Identity.GetUserId(), null, null, fieldSelect)));
        }
    }
}
