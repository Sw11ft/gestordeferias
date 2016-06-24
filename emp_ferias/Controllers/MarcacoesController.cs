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

namespace emp_ferias.Controllers
{
    [Authorize]
    public class MarcacoesController : Controller
    {
        ServiceMarcacoes serviceMarcacoes = new ServiceMarcacoes(new ServiceLogin());

        private EmpFeriasDbContext db = new EmpFeriasDbContext();

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

        private static List<IndexMarcacaoViewModel> MapIndexMarcacaoViewModel(List<Marcacao> Marcacoes)
        {
            List<IndexMarcacaoViewModel> MappedViewModels = new List<IndexMarcacaoViewModel>();
            foreach (var i in Marcacoes)
            {
                var MappedViewModel = new IndexMarcacaoViewModel();

                MappedViewModel.id = i.Id;
                MappedViewModel.UserName = i.User.UserName;
                MappedViewModel.DataPedido = i.DataPedido;
                MappedViewModel.DataInicio = i.DataInicio;
                MappedViewModel.DataFim = i.DataFim;
                MappedViewModel.Notas = i.Notas;
                MappedViewModel.Motivo = i.Motivo;
                MappedViewModel.Status = i.Status;
                if (i.ActionUser != null)
                {
                    MappedViewModel.RazaoRejeicao = i.RazaoRejeicao;
                    MappedViewModel.ActionUserName = i.ActionUser.UserName;
                }
                MappedViewModels.Add(MappedViewModel);
            }
            
            return MappedViewModels;
                 
        }

        // GET: Marcacoes
        public ActionResult Index()
        {
            return View(MapIndexMarcacaoViewModel(serviceMarcacoes.Get()));
        }

        // GET: Marcacoes/Overview
        public ActionResult Overview()
        {
            return View(MapIndexMarcacaoViewModel(serviceMarcacoes.Get()));
        }

        public ActionResult Refresh()
        {
            serviceMarcacoes.RefreshStatus();
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
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Marcacao marcacao = await db.Marcacoes.FindAsync(id);
            if (marcacao == null)
            {
                return HttpNotFound();
            }
            return View(marcacao);
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
                Marcacao Marcacao = serviceMarcacoes.FindById(ApprInfo.marcId);

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
                Marcacao Marcacao = serviceMarcacoes.FindById(RejectionInfo.marcRejectId);

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
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Marcacao marcacao = await db.Marcacoes.FindAsync(id);
            if (marcacao == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Id", marcacao.UserId);
            ViewBag.UserIdAprovacao = new SelectList(db.Users, "Id", "Id", marcacao.ActionUser);
            return View(marcacao);
        }
           
        // POST: Marcacoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,UserId,DataPedido,DataInicio,DataFim,Observacoes,Aprovado,UserIdAprovacao,RazaoAprovacao,Motivo")] Marcacao marcacao)
        {
            if (ModelState.IsValid)
            {
                db.Entry(marcacao).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "Id", marcacao.UserId);
            ViewBag.UserIdAprovacao = new SelectList(db.Users, "Id", "Id", marcacao.ActionUserId);
            return View(marcacao);
        }

        // GET: Marcacoes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Marcacao marcacao = await db.Marcacoes.FindAsync(id);
            if (marcacao == null)
            {
                return HttpNotFound();
            }
            return View(marcacao);
        }

        // POST: Marcacoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Marcacao marcacao = await db.Marcacoes.FindAsync(id);
            db.Marcacoes.Remove(marcacao);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
