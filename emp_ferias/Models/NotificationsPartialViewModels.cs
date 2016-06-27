using emp_ferias.lib.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace emp_ferias.Models
{
    public class NotificationsPartialViewModels
    {
    }

    public class NotificationViewModel
    {
        public int Id { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public Motivo Motivo { get; set; }
        public Status Status { get; set; }
        public string ActionUserName { get; set; }
    }
}