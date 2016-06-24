using emp_ferias.lib.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace emp_ferias.Models
{
    public class HomeViewModel
    {
        public int id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFim { get; set; }
        public Motivo Motivo { get; set; }
        public Status Status { get; set; }
        public string ActionUserName { get; set; }
        public string RazaoRejeicao { get; set; }
    }
}