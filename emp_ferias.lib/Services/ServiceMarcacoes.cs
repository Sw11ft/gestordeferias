using emp_ferias.lib.Classes;
using emp_ferias.lib.DAL;
using emp_ferias.lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace emp_ferias.lib.Services
{
    public class ServiceMarcacoes
    {
        private EmpFeriasDbContext db = new EmpFeriasDbContext();
        private IServiceLogin _serviceLogin;

        public ServiceMarcacoes(IServiceLogin serviceLogin)
        {
            _serviceLogin = serviceLogin;
        }

        public List<ExecutionResult> Create(Marcacao m)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            if (m.DataFim < m.DataInicio)
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "The end date must be after the start date."});
            }
            if (m.DataInicio <= DateTime.Now)
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "The start date must not be before or in the present day." });
            }
            if (!(Enum.IsDefined(typeof(Motivo),m.Motivo)))
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Invalid reason." });
            }



            foreach (var i in ExecutionResult)
                if (i.MessageType == MessageType.Error)
                    return(ExecutionResult);

            m.DataPedido = DateTime.Now;
            m.UserId = _serviceLogin.GetUserID();
            m.Aprovado = false;

            db.Marcacoes.Add(m);
            db.SaveChanges();
            return (ExecutionResult);

        }

        public void Update()
        {

        }

        public List<Marcacao> Get()
        {
            return db.Marcacoes.Include(x => x.User).ToList();
        }

        public List<ExecutionResult> Approve(int id)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            Marcacao Approving = db.Marcacoes.Find(id);

            if (Approving != null)
            {
                Approving.Aprovado = true;
                Approving.UserIdAprovacao = _serviceLogin.GetUserID();
                db.SaveChanges();
            }
            else
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Cannot find the database entry." });
            }

            return (ExecutionResult);

        }

        public List<ExecutionResult> Reject (int id, string reason)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            Marcacao Rejecting = db.Marcacoes.Find(id);

            if (Rejecting != null)
            {
                Rejecting.Aprovado = false;
                Rejecting.RazaoAprovacao = reason;
                Rejecting.UserIdAprovacao = _serviceLogin.GetUserID();
                db.SaveChanges();
            }
            else
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Cannot find the database entry." });
            }

            return (ExecutionResult);
        }   
    }
}
