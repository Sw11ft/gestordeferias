using emp_ferias.lib.Classes;
using emp_ferias.lib.DAL;
using emp_ferias.lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Runtime.Caching;
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
            if (m.DataInicio <= DateTime.UtcNow)
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

            m.DataPedido = DateTime.UtcNow;
            m.UserId = _serviceLogin.GetUserID();
            m.Status = Status.Pendente;

            db.Marcacoes.Add(m);
            db.SaveChanges();
            return (ExecutionResult);

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
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Marcação não encontrada." });

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
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Marcação não encontrada." });

            return (ExecutionResult);
        }   

        public List<ExecutionResult> Edit (Marcacao m)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            if (m.DataFim < m.DataInicio)
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "A data de fim tem de ser depois da data de início." });
            if (m.DataInicio <= DateTime.UtcNow)
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "A data de início não pode ser antes ou no dia de hoje." });
            if (!(Enum.IsDefined(typeof(Motivo), m.Motivo)))
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Motivo inválido." });
            if (!string.IsNullOrEmpty(m.Notas))
                if (m.Notas.Length > 100)
                    ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "O número de caracteres nas notas é demasiado grande." });

            foreach (var i in ExecutionResult)
                if (i.MessageType == MessageType.Error)
                    return (ExecutionResult);

            Marcacao Editing = db.Marcacoes.Find(m.Id);

            if (Editing != null)
            {
                Editing.DataInicio = m.DataInicio;
                Editing.DataFim = m.DataFim;
                Editing.Notas = m.Notas;
                Editing.Motivo = m.Motivo;
                db.SaveChanges();
            }
            else
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Marcação não encontrada." });

            return ExecutionResult;
        }

        public List<ExecutionResult> Delete (string SenderId, int id)
        {
            List<ExecutionResult> ExecutionResult = new List<ExecutionResult>();

            Marcacao Deleting = db.Marcacoes.Find(id);

            if (Deleting != null)
            {
                
                if (SenderId != Deleting.UserId)
                {
                    ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Não tem permissões para efetuar esta operação." });
                    return ExecutionResult;
                }

                if (Deleting.Status != Status.Pendente)
                {
                    ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Apenas é possível alterar marcações pendentes." });
                    return ExecutionResult;
                }

                db.Marcacoes.Remove(Deleting);
                db.SaveChanges();
            }
            else
                ExecutionResult.Add(new ExecutionResult() { MessageType = MessageType.Error, Message = "Marcação não encontrada." });

            return (ExecutionResult);
        }

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

        public List<Marcacao> GetMyMarcacoes (string SenderId, int? id, Motivo? Motivo, Status? Status, DateTime? pedido_fromDate, DateTime? pedido_toDate, DateTime? inicio_fromDate, DateTime? inicio_toDate, DateTime? fim_fromDate, DateTime? fim_toDate)
        {
            var query = db.Marcacoes.AsQueryable().Include(x => x.ActionUser); 

            if (!string.IsNullOrEmpty(SenderId))
                query = query.Where(x => x.UserId == SenderId);

            if (id.HasValue)
                query = query.Where(x => x.Id == id);

            if (Motivo.HasValue)
                query = query.Where(x => x.Motivo == Motivo);

            if (Status.HasValue)
                query = query.Where(x => x.Status == Status);
             
            if (pedido_fromDate.HasValue)
                query = query.Where(x => x.DataPedido >= pedido_fromDate);

            if (pedido_toDate.HasValue)
                query = query.Where(x => x.DataPedido <= pedido_toDate);

            if (inicio_fromDate.HasValue)
                query = query.Where(x => x.DataInicio >= inicio_fromDate);

            if (inicio_toDate.HasValue)
                query = query.Where(x => x.DataInicio <= inicio_toDate);

            if (fim_fromDate.HasValue)
                query = query.Where(x => x.DataFim >= fim_fromDate);

            if (fim_toDate.HasValue)
                query = query.Where(x => x.DataFim <= fim_toDate);

            return query.ToList();
        }

        public List<Marcacao> GetIndexMarcacoes(string SenderId, int? id, string userName, Motivo? Motivo, Status? Status, DateTime? pedido_fromDate, DateTime? pedido_toDate, DateTime? inicio_fromDate, DateTime? inicio_toDate, DateTime? fim_fromDate, DateTime? fim_toDate)
        {
            var query = db.Marcacoes.AsQueryable().Include(x => x.ActionUser);

            if (!string.IsNullOrEmpty(SenderId))
                query = query.Where(x => x.UserId == SenderId);

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(x => x.User.UserName.Contains(userName.Trim()));

            if (id.HasValue)
                query = query.Where(x => x.Id == id);

            if (Motivo.HasValue)
                query = query.Where(x => x.Motivo == Motivo);

            if (Status.HasValue)
                query = query.Where(x => x.Status == Status);

            if (pedido_fromDate.HasValue)
                query = query.Where(x => x.DataPedido >= pedido_fromDate);

            if (pedido_toDate.HasValue)
                query = query.Where(x => x.DataPedido <= pedido_toDate);

            if (inicio_fromDate.HasValue)
                query = query.Where(x => x.DataInicio >= inicio_fromDate);

            if (inicio_toDate.HasValue)
                query = query.Where(x => x.DataInicio <= inicio_toDate);

            if (fim_fromDate.HasValue)
                query = query.Where(x => x.DataFim >= fim_fromDate);

            if (fim_toDate.HasValue)
                query = query.Where(x => x.DataFim <= fim_toDate);

            return query.ToList();
        }

        public List<Marcacao> GetMyPendingMarcacoes(string SenderId)
        {
            return db.Marcacoes.AsNoTracking().Where(x => x.User.Id == SenderId && x.Status == Status.Pendente).ToList();
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

        public void RefreshMarcStatus()
        {
            if (!MemoryCache.Default.Contains("RefreshMarcStatus"))
                RefreshStatus();
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

            if (!MemoryCache.Default.Contains("RefreshMarcStatus"))
                MemoryCache.Default.Add("RefreshMarcStatus", "", DateTimeOffset.Now.AddHours(6));
        }

        public int[] CalcDays(string UserId, bool IncludeSaturday, Motivo Motivo) //retorna o total de férias MARCADAS (e não apenas as gozadas)!!!
        {
            int[] Total = new int[4];
            Total[0] = 0; Total[1] = 0; Total[2] = 0; Total[3] = 0;
            //Total[0] = dias uteis num ano
            //Total[1] = dias uteis no total
            //Total[2] = total de dias num ano
            //Total[3] = total de dias (no total)

            List<Marcacao> Marcacoes = db.Marcacoes.Where(x => x.UserId == UserId && x.Motivo == Motivo && x.Status != Status.Expirado && x.Status != Status.Pendente && x.Status != Status.Rejeitado).ToList();

            foreach (var i in Marcacoes)
            {
                int DUA = 0; //dias uteis no ano
                int DUT = 0; //dias uteis no total
                int TA = 0; //total de dias num ano
                int TT = 0; //total de dias (no total)

                for (var day = i.DataInicio.Date; day.Date <= i.DataFim.Date; day = day.AddDays(1)) //passa por cada dia entre a data de inicio e a data de fim da marcação
                {
                    if (day.Year == DateTime.UtcNow.Year)
                        TA++; //se o dia pertencer a este ano, adiciona 1 ao total de dias no ano

                    if (day.DayOfWeek != DayOfWeek.Sunday && day.DayOfWeek != DayOfWeek.Saturday)
                    {
                        if (day.Year == DateTime.UtcNow.Year)
                            DUA++; //se for um dia util (exc sáb e dom) e neste ano, adiciona um ao total de dias uteis no ano
                        DUT++; //se for um dia util (exc sáb e dom), adiciona um ao total de dias uteis
                    }
                    else
                    {
                        if (IncludeSaturday && day.DayOfWeek == DayOfWeek.Saturday) 
                        {
                            if (day.Year == DateTime.UtcNow.Year)
                                DUA++; //se o util incluir o sáb como dia util e o dia for neste ano, adiciona um ao total de dias uteis no ano
                            DUT++; //se o util incluir o sáb como dia util, adiciona um ao total de dias uteis
                        }
                    }
                    TT++; //adiciona um ao total de dias por cada dia que passa
                }

                //adiciona os valores calculados ao array que vai ser retornado
                Total[0] += DUA;
                Total[1] += DUT;
                Total[2] += TA;
                Total[3] += TT;
            }

            return Total;
        }

        public int[] CalcStatus(string UserId)
        {
            int[] Total = new int[4];
            Total[0] = 0; Total[1] = 0; Total[2] = 0; Total[3] = 0;
            //Total[0] -> Aceites
            //Total[1] -> Rejeitadas
            //Total[2] -> Expiradas
            //Total[3] -> EVERYTHING!!
            var Marcacoes = db.Marcacoes.Where(x => x.UserId == UserId && x.Status != Status.Pendente).ToList();
            foreach (var i in Marcacoes)
            {
                if (i.Status == Status.Expirado)
                    Total[2]++;
                else if (i.Status == Status.Rejeitado)
                    Total[1]++;
                else
                    Total[0]++;
                System.Diagnostics.Debug.WriteLine(i.Status);
                Total[3]++; 
            }
            return Total;
        }
    }
}
