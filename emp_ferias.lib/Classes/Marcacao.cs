using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace emp_ferias.lib.Classes
{
    #region declaração de enums
    public enum Motivo
    {
        [Display(Name="Férias")]
        Ferias,
        Justificada,
        Injustificada
    }

    public enum Status
    {
        Pendente,
        Rejeitado,
        Expirado,
        Finalizado,
        [Display(Name="Em Progresso")]
        EmProgresso,
        Aprovado
    }

    public enum DataSet
    {
        PorMarcacao,
        PorTotalDeDias
    }
    #endregion

    [Table("Marcacoes")]
    public class Marcacao
    {
        [Key]
        public int Id { get; set; } //id da marcação

        [Required]
        public string UserId { get; set; } //id do utilizador que efetuou a marcação
        public DateTime DataPedido { get; set; } //data de quando foi efetuada a marcação
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; } //data pretendida pelo utilizador de quando as férias vão começar
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFim { get; set; } //data pretendida pelo utilizador de quando as férias vão acabar
        public Motivo Motivo { get; set; } //motivo da marcação, enum
        public string Notas { get; set; } //observações
        public Status Status { get; set; } //estado da marcação, enum
        public string ActionUserId { get; set; } //id de quem tomou uma ação sobre a marcação
        public string RazaoRejeicao { get; set; } //preenchido caso não seja aprovado
        public bool UserNotificado { get; set; } //bool para saber se o utilizador já foi notificado e viu a notif
        

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ActionUserId")]
        public User ActionUser { get; set; }
    }
    [Table("AspNetUsers")]
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }
}