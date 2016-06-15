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

namespace emp_ferias.Controllers
{
    [Authorize]
    public class MarcacoesController : Controller
    {
        ServiceMarcacoes serviceMarcacoes = new ServiceMarcacoes(new ServiceLogin());

        private EmpFeriasDbContext db = new EmpFeriasDbContext();

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
                MappedViewModel.Observacoes = i.Observacoes;
                MappedViewModel.Motivo = i.Motivo;
                MappedViewModel.Aprovado = i.Aprovado;
                if (i.Aprovado)
                {
                    MappedViewModel.RazaoAprovacao = i.RazaoAprovacao;
                    MappedViewModel.UserNameAprovacao = i.UserAprovacao.UserName;
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
                Observacoes = UserInput.Observacoes
            };
        }

        // POST: Marcacoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateMarcacaoViewModel viewModel)
        {
            var ExecutionResult = serviceMarcacoes.Create(MapViewModel(viewModel));

            foreach (var i in ExecutionResult)
                if (i.MessageType == MessageType.Error)
                    ModelState.AddModelError("", i.Message);

            if (ModelState.IsValid)
                return RedirectToAction("Index");
            else 
                return View(viewModel);

        }

        //POST: Marcacoes/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            var ExecutionResult = serviceMarcacoes.Approve(id);

            foreach (var i in ExecutionResult)
                if (i.MessageType == MessageType.Error)
                    ModelState.AddModelError("", i.Message);

            return RedirectToAction("Index");


        }

        //POST: Marcacoes/Reject

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
            ViewBag.UserIdAprovacao = new SelectList(db.Users, "Id", "Id", marcacao.UserIdAprovacao);
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
            ViewBag.UserIdAprovacao = new SelectList(db.Users, "Id", "Id", marcacao.UserIdAprovacao);
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
