using emp_ferias.lib.Classes;
using emp_ferias.lib.Services;
using emp_ferias.Models;
using emp_ferias.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace emp_ferias.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        ServiceMarcacoes serviceMarcacoes = new ServiceMarcacoes(new ServiceLogin());

        private static List<HomeViewModel> MapHomeViewModel(List<Marcacao> Marcacoes)
        {
            List<HomeViewModel> MappedViewModels = new List<HomeViewModel>();
            foreach (var i in Marcacoes)
            {
                var MappedViewModel = new HomeViewModel();

                MappedViewModel.id = i.Id;
                MappedViewModel.DataInicio = i.DataInicio;
                MappedViewModel.DataFim = i.DataFim;
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

        public ActionResult Index()
        {
            return View(MapHomeViewModel(serviceMarcacoes.GetHome(User.Identity.GetUserId())));
        }

        private static List<Events> MapMarcacoesCalendar(List<Marcacao> Marcacoes, DateTime start, DateTime end)
        {
            List<Events> EventList = new List<Events>();
            foreach (var m in Marcacoes)
            {
                if (m.DataFim >= start && m.DataInicio <= end)
                {
                    Events newEvent = new Events
                    {
                        id = m.Id.ToString(),
                        title = "#" + m.Id + ": " + m.Motivo,
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
                        newEvent.color = "#2C93FF";
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
            return EventList;
        }

        public ActionResult GetMarcacoes(DateTime start, DateTime end)
        {

            List<Marcacao> Marcacoes = serviceMarcacoes.GetHome(User.Identity.GetUserId());

            var EventList = MapMarcacoesCalendar(Marcacoes, start, end);

            var EventArray = EventList.ToArray();

            return Json(EventArray, JsonRequestBehavior.AllowGet);
        }
    }
}