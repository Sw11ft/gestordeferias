//using System.Web.Mvc;
//using System.Threading.Tasks;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
//using System.Collections.Generic;
//using System.Web;
//using System.Linq;
//using emp_ferias.Models;
//using System.Web.Security;
//using System.Data.Entity.Validation;
//using System.Net;
//using System.Data.Entity;

//namespace emp_ferias.Controllers
//{
//    [Authorize]
//    public class RoleController : Controller
//    {
//        ApplicationDbContext context;

//        public RoleController()
//        {
//            context = new ApplicationDbContext();
//        }

//        // GET: Role
//        public ActionResult Index()
//        {

//            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
//            List<IdentityRole> roles = roleManager.Roles.ToList();

//            return View(roles);

//        }

//        // GET: /Role/Edit
//        public ActionResult Edit(string id)
//        {

//            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());

//            if (id == null)
//            {
//                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
//            }

//            IdentityRole role = roleManager.FindById(id);

//            if (role == null)
//            {
//                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
//            }
//            return View(role);
//        }

//        // POST: /Role/Edit
//        [System.Web.Mvc.HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Edit([Bind(Include = "Name")] IdentityRole targetrole, string id)
//        {
//            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
//            }

//            var role = await roleManager.FindByIdAsync(id);

//            role.Name = targetrole.Name;

//            IdentityResult result = roleManager.Update(role);

//            if (!result.Succeeded)
//            {
//                foreach (var error in result.Errors)
//                    ModelState.AddModelError("", error);
//                return View(role);
//            }
//            return RedirectToAction("Index");
//        }   

//        // GET: /Role/Create
//        public ActionResult Create()
//        {
//            return View();
//        }

//        // POST: /Role/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Create(IdentityRole Role)
//        {

//            context.Roles.Add(Role);

//            List<DbEntityValidationResult> teste = context.GetValidationErrors().ToList();

//            if (teste.Any())
//            {
//                foreach (var item in teste[0].ValidationErrors)
//                {
//                    ModelState.AddModelError("", item.ErrorMessage);
//                }
//                return View();
//            }
//            context.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        // GET: /Role/Delete
//        [HttpGet]
//        public ActionResult Delete(string id)
//        {
//            if (id == null)
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

//            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());


//            IdentityRole role = roleManager.FindById(id);

//            if (role == null)
//                return HttpNotFound();

//            return View(role);
//        }
//        // POST: /Role/Delete
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Delete(IdentityRole role)
//        {

//            if (role.Id == null)
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

//           var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());


//          //  var deleteRole = await roleManager.FindByNameAsync(role.Id);
//            var result = await roleManager.DeleteAsync(roleManager.FindById(role.Id));

//            if (!result.Succeeded)
//            {
//                foreach (var error in result.Errors)
//                    ModelState.AddModelError("", error);
//            }
//            return RedirectToAction("Index");
//        }
//    }
//}
