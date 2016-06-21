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
            return db.Marcacoes.AsNoTracking().Include(x=> x.UserAprovacao).Include(x => x.User).ToList();
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

        public List<ExecutionResult> Reject (Marcacao m)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            Marcacao Rejecting = db.Marcacoes.Find(m.Id);

            if (Rejecting != null)
            {
                m.RazaoAprovacao = m.RazaoAprovacao.Trim();
                if (string.IsNullOrWhiteSpace(m.RazaoAprovacao) || m.RazaoAprovacao.Length > 100)
                    ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "The message may not be null, white spaces, or contain more than 100 characters." });

                foreach (var i in ExecutionResult)
                    if (i.MessageType == MessageType.Error)
                        return (ExecutionResult);

                Rejecting.Aprovado = false;
                Rejecting.RazaoAprovacao = m.RazaoAprovacao;
                Rejecting.UserIdAprovacao = _serviceLogin.GetUserID();
                db.SaveChanges();
            }
            else
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Cannot find the database entry." });
            }

            return (ExecutionResult);
        }   

        public List<Marcacao> GetHome(string SenderId)
        {
            return db.Marcacoes.AsNoTracking().Include(x => x.UserAprovacao).Include(x => x.User).Where(x => x.User.Id == SenderId && x.DataFim >= DateTime.Today.Date && x.UserIdAprovacao != null && x.Aprovado == true).ToList();
        }

        public List<Marcacao> GetUserMarcacoes(string SenderId)
        {
            return db.Marcacoes.AsNoTracking().Include(x => x.UserAprovacao).Include(x => x.User).Where(x => x.User.Id == SenderId).ToList();
        }

        public Array GetUserRazaoMarcacao(string SenderId, int DataSet, bool IncludeRejected)
        {
            var Ferias = 0;
            var Justificada = 0;
            var Injustificada = 0;

            if (DataSet == 1 && !IncludeRejected)
            {
                List<Marcacao> Marcacoes = db.Marcacoes.AsNoTracking().Include(x => x.User).Where(x => x.User.Id == SenderId && x.UserAprovacao != null && x.Aprovado == true).ToList();

                foreach (var i in Marcacoes)
                {
                    if (i.Motivo == Motivo.Ferias)
                    {
                        Ferias++;
                    }
                    else if (i.Motivo == Motivo.Justificada)
                    {
                        Justificada++;
                    }
                    else
                    {
                        Injustificada++;
                    }
                }
            }
            else if (DataSet == 1 && IncludeRejected)
            {
                List<Marcacao> Marcacoes = db.Marcacoes.AsNoTracking().Include(x => x.User).Where(x => x.User.Id == SenderId && x.UserAprovacao != null).ToList();

                foreach (var i in Marcacoes)
                {
                    if (i.Motivo == Motivo.Ferias)
                    {
                        Ferias++;
                    }
                    else if (i.Motivo == Motivo.Justificada)
                    {
                        Justificada++;
                    }
                    else
                    {
                        Injustificada++;
                    }
                }
            }
            else if (DataSet == 2 && !IncludeRejected)
            {
                List<Marcacao> Marcacoes = db.Marcacoes.AsNoTracking().Include(x => x.User).Where(x => x.User.Id == SenderId && x.UserAprovacao != null && x.Aprovado == true).ToList();

                foreach (var i in Marcacoes)
                {
                    if (i.Motivo == Motivo.Ferias)
                    {
                        Ferias += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays));
                    }
                    else if (i.Motivo == Motivo.Justificada)
                    {
                        Justificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays));
                    }
                    else
                    {
                        Injustificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays));
                    }
                }
            }
            else
            {
                List<Marcacao> Marcacoes = db.Marcacoes.AsNoTracking().Include(x => x.User).Where(x => x.User.Id == SenderId && x.UserAprovacao != null == true).ToList();

                foreach (var i in Marcacoes)
                {
                    if (i.Motivo == Motivo.Ferias)
                    {
                        Ferias += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays));
                    }
                    else if (i.Motivo == Motivo.Justificada)
                    {
                        Justificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays));
                    }
                    else
                    {
                        Injustificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays));
                    }
                }
            }
            
            return new[] { Ferias, Justificada, Injustificada };
        } 
    }
}
