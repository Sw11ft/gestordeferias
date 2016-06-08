using emp_ferias.lib.Classes;
using System;
using System.Collections.Generic;
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
}