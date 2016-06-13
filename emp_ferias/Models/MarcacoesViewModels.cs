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
        public string Observacoes { get; set; }
        public Motivo Motivo { get; set; }
    }

    public class IndexMarcacaoViewModel
    {
        public int id { get; set; }
        public string UserName { get; set; }
        public DateTime DataPedido { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFim { get; set; } 
        public string Observacoes { get; set; } 
        public bool Aprovado { get; set; } 
        public string UserNameAprovacao { get; set; } 
        public string RazaoAprovacao { get; set; }
        public Motivo Motivo { get; set; }
    }
}