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
using MvcFlashMessages;

namespace emp_ferias.Controllers
{
    [Authorize]
    public class NotificationsPartialController : Controller
    {

        ServiceMarcacoes serviceMarcacoes = new ServiceMarcacoes(new ServiceLogin());

        private static List<NotificationViewModel> MapNotificationsPartialViewModel(List<Marcacao> Marcacoes)
        {
            List<NotificationViewModel> nvm = new List<NotificationViewModel>();

            foreach (var i in Marcacoes)
            {
                NotificationViewModel notification = new NotificationViewModel();
                notification.Id = i.Id;
                notification.DataInicio = i.DataInicio;
                notification.DataFim = i.DataFim;
                notification.Motivo = i.Motivo;
                notification.Status = i.Status;
                if (i.Status != Status.Expirado)
                {
                    notification.ActionUserName = i.ActionUser.UserName;
                }
                nvm.Add(notification);
            }

            return nvm;
        }

        // GET: Notifications
        public ActionResult Index()
        {
            return PartialView("NotificationsPartial",MapNotificationsPartialViewModel(serviceMarcacoes.GetUserNotifications(User.Identity.GetUserId())));
        }

        // GET: MarkAllAsRead
        public void MarkAllAsRead()
        {
            List<ExecutionResult> ExecutionResult = serviceMarcacoes.MarkAllAsRead(User.Identity.GetUserId());

            foreach (var i in ExecutionResult)
            {
                this.Flash("error", i.Message);
            }
        }

        // GET: MarkAsRead
        public void MarkAsRead(int MarcId)
        {
            List<ExecutionResult> ExecutionResult = serviceMarcacoes.MarkAsRead(User.Identity.GetUserId(), MarcId);

            foreach (var i in ExecutionResult)
                this.Flash("error", i.Message);
        }
    }
}