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
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "A data de fim tem de ser depois da data de início."});
            }
            if (m.DataInicio <= DateTime.Now)
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "A data de início não pode ser antes ou no dia de hoje." });
            }
            if (!(Enum.IsDefined(typeof(Motivo),m.Motivo)))
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Motivo inválido." });
            }

            foreach (var i in ExecutionResult)
                if (i.MessageType == MessageType.Error)
                    return(ExecutionResult);

            m.DataPedido = DateTime.Now;
            m.UserId = _serviceLogin.GetUserID();
            m.Status = Status.Pendente;

            db.Marcacoes.Add(m);
            db.SaveChanges();
            return (ExecutionResult);

        }

        public void Update()
        {

        }

        public List<Marcacao> Get()
        {
            return db.Marcacoes.AsNoTracking().Include(x=> x.ActionUser).Include(x => x.User).ToList();
        }

        public List<ExecutionResult> Approve(int id)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            Marcacao Approving = db.Marcacoes.Find(id);

            if (Approving != null)
            {
                Approving.Status = Status.Aprovado;
                Approving.ActionUserId = _serviceLogin.GetUserID();
                db.SaveChanges();
            }
            else
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Marcação não encontrada." });
            }

            return (ExecutionResult);

        }

        public List<ExecutionResult> Reject (Marcacao m)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            Marcacao Rejecting = db.Marcacoes.Find(m.Id);

            if (Rejecting != null)
            {
                m.RazaoRejeicao = m.RazaoRejeicao.Trim();
                if (string.IsNullOrWhiteSpace(m.RazaoRejeicao) || m.RazaoRejeicao.Length > 100)
                    ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "A razão não deve ser nula, espaços em branco ou ter mais de 100 caracteres." });

                foreach (var i in ExecutionResult)
                    if (i.MessageType == MessageType.Error)
                        return (ExecutionResult);

                Rejecting.Status = Status.Rejeitado;
                Rejecting.RazaoRejeicao = m.RazaoRejeicao;
                Rejecting.ActionUserId = _serviceLogin.GetUserID();
                db.SaveChanges();
            }
            else
            {
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Cannot find the database entry." });
            }

            return (ExecutionResult);
        }   

        //public List<ExecutionResult> Edit (int id)
        //{
        //    List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

        //    Marcacao m = db.Marcacoes.Find(id);


        //    if (m.DataFim < m.DataInicio)
        //    {
        //        ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "A data de fim tem de ser depois da data de início." });
        //    }
        //    if (m.DataInicio <= DateTime.Now)
        //    {
        //        ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "A data de início não pode ser antes ou no dia de hoje." });
        //    }
        //    if (!(Enum.IsDefined(typeof(Motivo), m.Motivo)))
        //    {
        //        ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Motivo inválido." });
        //    }
        //}

        public List<Marcacao> GetHome(string SenderId)
        {
            return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Include(x => x.User).Where(x => x.User.Id == SenderId && x.Status != Status.Rejeitado && x.Status != Status.Expirado).ToList();
        }

        public List<Marcacao> GetUserMarcacoes(string SenderId)
        {
            return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Include(x => x.User).Where(x => x.User.Id == SenderId).ToList();
        }

        public List<Marcacao> GetUserNotifications(string SenderId)
        {
            return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.User.Id == SenderId && !x.UserNotificado && x.Status != Status.Pendente).ToList(); 
        }

        public List<Marcacao> GetMyMarcacoes (string senderId, DateTime? fromDate, DateTime? toDate, fieldSelect fieldSelect)
        {
            if (fieldSelect == fieldSelect.Pedido)
            {
                if (fromDate == null && toDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.UserId == senderId).ToList();
                else if (fromDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataPedido <= toDate && x.UserId == senderId).ToList();
                else if (toDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataPedido >= fromDate && x.UserId == senderId).ToList();
                else
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataPedido >= fromDate && x.DataPedido <= toDate).ToList();
            }
            else if (fieldSelect == fieldSelect.PorDataInicio)
            {
                if (fromDate == null && toDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.UserId == senderId).ToList();
                else if (fromDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataInicio >= DateTime.MinValue && x.DataInicio <= toDate && x.UserId == senderId).ToList();
                else if (toDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataInicio >= fromDate && x.DataInicio <= DateTime.MaxValue && x.UserId == senderId).ToList();
                else
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataInicio >= fromDate && x.DataInicio <= toDate).ToList();
            }
            else if (fieldSelect == fieldSelect.PorDataFim)
            {
                if (fromDate == null && toDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.UserId == senderId).ToList();
                else if (fromDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataFim >= DateTime.MinValue && x.DataFim <= toDate && x.UserId == senderId).ToList();
                else if (toDate == null)
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataFim >= fromDate && x.DataFim <= DateTime.MaxValue && x.UserId == senderId).ToList();
                else
                    return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.DataFim >= fromDate && x.DataFim <= toDate).ToList();
            }        
            else
                return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Where(x => x.UserId == senderId).ToList(); //if something fails then just display this and pretend nothing has happened
        }

        public List<ExecutionResult> MarkAllAsRead(string SenderId)
        {
            List<Marcacao> Marcacoes = db.Marcacoes.Where(x => x.UserId == SenderId && !x.UserNotificado && x.Status != Status.Pendente).ToList();

            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            if (Marcacoes.Any())
            {
                foreach (var i in Marcacoes)
                {
                    i.UserNotificado = true;
                    db.SaveChanges();
                }
            }
            return ExecutionResult;
        }

        public List<ExecutionResult> MarkAsRead(string SenderId, int MarcId)
        {
            Marcacao Marcacao = db.Marcacoes.Where(x => x.UserId == SenderId && !x.UserNotificado && x.Status != Status.Pendente).FirstOrDefault();

            List<ExecutionResult> ExecutionResult = new List<Classes.ExecutionResult>();

            if (Marcacao != null)
            {
                Marcacao.UserNotificado = true;
                db.SaveChanges();
            }

            return ExecutionResult;
        }

        public int[] GetUserRazaoMarcacao(string SenderId, DataSet DataSet, bool IncludeRejected)
        {
            var Ferias = 0;
            var Justificada = 0;
            var Injustificada = 0;

            if (DataSet == DataSet.PorMarcacao && !IncludeRejected)
            {
                return (from m in db.Marcacoes
                            where m.User.Id == SenderId
                                && m.Status != Status.Rejeitado
                                && m.Status != Status.Expirado
                            group m by m.Motivo into grp
                            select grp.Count()).ToArray();
            }
            else if (DataSet == DataSet.PorMarcacao && IncludeRejected)
            {
                return (from m in db.Marcacoes
                        where m.User.Id == SenderId
                        group m by m.Motivo into grp
                        select grp.Count()).ToArray();
            }
            else if (DataSet == DataSet.PorTotalDeDias && !IncludeRejected)
            {
                List<Marcacao> Marcacoes = db.Marcacoes.AsNoTracking().Include(x => x.User).Where(x => x.User.Id == SenderId && x.ActionUser != null && x.Status != Status.Rejeitado).ToList();

                foreach (var i in Marcacoes)
                {
                    if (i.Motivo == Motivo.Ferias)
                    {
                        Ferias += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays)) + 1;
                    }
                    else if (i.Motivo == Motivo.Justificada)
                    {
                        Justificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays)) + 1;
                    }
                    else
                    {
                        Injustificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays)) + 1;
                    }
                }
            }
            else
            {
                List<Marcacao> Marcacoes = db.Marcacoes.AsNoTracking().Include(x => x.User).Where(x => x.User.Id == SenderId && x.ActionUser != null == true).ToList();

                foreach (var i in Marcacoes)
                {
                    if (i.Motivo == Motivo.Ferias)
                    {
                        Ferias += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays)) + 1;
                    }
                    else if (i.Motivo == Motivo.Justificada)
                    {
                        Justificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays)) + 1;
                    }
                    else
                    {
                        Injustificada += Convert.ToInt32(Math.Floor((i.DataFim - i.DataInicio).TotalDays)) + 1;
                    }
                }
            }
            
            return new[] { Ferias, Justificada, Injustificada };
        } 

        public Marcacao FindById(bool IncludeActionUser, int? MarcId)
        {
            if (IncludeActionUser)
                return db.Marcacoes.AsNoTracking().Include(x => x.ActionUser).Include(x => x.User).Where(x => x.Id == MarcId).FirstOrDefault();
            else
                return db.Marcacoes.AsNoTracking().Include(x => x.User).Where(x => x.Id == MarcId).FirstOrDefault();
        }

        public void RefreshStatus()
        {
            List<Marcacao> Marcacoes = db.Marcacoes.Where(x => x.Status == Status.Aprovado
                                                            || x.Status == Status.EmProgresso
                                                            || x.Status == Status.Pendente).ToList();
            foreach (var i in Marcacoes)
            {
                if (i.Status == Status.Pendente)
                {
                    if (i.DataInicio <= DateTime.Today.Date)
                        i.Status = Status.Expirado;
                }
                else if (i.Status == Status.EmProgresso)
                {
                    if (i.DataFim < DateTime.Today.Date)
                        i.Status = Status.Finalizado;
                }
                else
                {
                    if (i.DataInicio <= DateTime.Today.Date)
                        i.Status = Status.EmProgresso;
                }
            }
            db.SaveChanges();
        }
    }
}
