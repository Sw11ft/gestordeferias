using emp_ferias.lib.Classes;
using emp_ferias.lib.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emp_ferias.lib.Services
{
    public class ServiceMarcacoes
    {
        private EmpFeriasDbContext db = new EmpFeriasDbContext();

        public void Create(Marcacao m)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            if (m.DataFim < m.DataInicio)
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "The end date must be after the start date."});
            }
            if (!(Enum.IsDefined(typeof(Motivo),m.Motivo)))
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Invalid reason." });
            }
            if (ExecutionResult.Any())
            {
                m.DataPedido = DateTime.Now;
               // m.UserId = 
            }
        }

        public void Update()
        {

        }

        public List<Marcacao> Get()
        {
            return db.Marcacoes.ToList();
        }
    }
}
