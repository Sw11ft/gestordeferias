using emp_ferias.lib.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace emp_ferias.Models
{
    public class MarcacoesViewModels
    {
    }

    public class CreateMarcacaoViewModel
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Notas { get; set; }
        public Motivo Motivo { get; set; }
    }

    public class IndexMarcacaoViewModel
    {
        public UserInfo LoggedUser { get; set; } = new UserInfo();
        public List<Marcacao> Marcacoes { get; set; } = new List<Marcacao>();
    }
    
    
    public class ApproveMarcacaoViewModel
    {
        public int marcId { get; set; }
        public bool sendEmail { get; set; }
    }

    public class RejectMarcacaoViewModel
    {
        public int marcRejectId { get; set; }
        public bool sendEmail { get; set; }
        public string Razao { get; set; }
    }

    public class MyMarcacaoViewModel
    {
        public UserInfo UserInfo { get; set; } = new UserInfo();
        public List<Marcacao> Marcacoes { get; set; } = new List<Marcacao>();
    }

    public class Events
    {
        public string id { get; set; }
        public string title { get; set; }
        public string date { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string url { get; set; }
        public bool allDay { get; set; }
        public string color { get; set; }
        public string textColor { get; set; }
    }
}
