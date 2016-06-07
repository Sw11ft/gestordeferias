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

namespace emp_ferias.Controllers
{
    public class MarcacoesController : Controller
    {
        private EmpFeriasDbContext db = new EmpFeriasDbContext();

        // GET: Marcacoes
        public async Task<ActionResult> Index()
        {
            var marcacoes = db.Marcacoes.Include(m => m.User).Include(m => m.UserAprovacao);
            return View(await marcacoes.ToListAsync());
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
            ViewBag.UserId = new SelectList(db.Users, "Id", "Id");
            ViewBag.UserIdAprovacao = new SelectList(db.Users, "Id", "Id");
            return View();
        }

        // POST: Marcacoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,UserId,DataPedido,DataInicio,DataFim,Observacoes,Aprovado,UserIdAprovacao,RazaoAprovacao,Motivo")] Marcacao marcacao)
        {
            if (ModelState.IsValid)
            {
                db.Marcacoes.Add(marcacao);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "Id", marcacao.UserId);
            ViewBag.UserIdAprovacao = new SelectList(db.Users, "Id", "Id", marcacao.UserIdAprovacao);
            return View(marcacao);
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
