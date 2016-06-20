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
        public bool Aprovado { get; set; }
        public string UserNameAprovacao { get; set; }
        public string RazaoAprovacao { get; set; }
        public Motivo Motivo { get; set; }
    }
}