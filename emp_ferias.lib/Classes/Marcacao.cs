using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace emp_ferias.lib.Classes
{
    public enum Motivo
    {
        Ferias,
        Justificada,
        Injustificada
    }

    [Table("Marcacoes")]
    public class Marcacao
    {
        [Key]
        public int Id { get; set; } //id da marcação

        [Required]
        public string UserId { get; set; } //id do utilizador que efetuou a marcação
        public DateTime DataPedido { get; set; } //data de quando foi efetuada a marcação
        public DateTime DataInicio { get; set; } //data pretendida pelo utilizador de quando as férias vão começar
        public DateTime DataFim { get; set; } //data pretendida pelo utilizador de quando as férias vão acabar
        public string Observacoes { get; set; } // ... 
        public bool Aprovado { get; set; } //marcação aprovada ou não (apenas por utilizadores com permissões para tal)
        public string UserIdAprovacao { get; set; } //id de quem aprovou a marcação
        public string RazaoAprovacao { get; set; } //preenchido caso não seja aprovado
        public Motivo Motivo { get; set; } //motivo da marcação, enum

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("UserIdAprovacao")]
        public User UserAprovacao { get; set; }
    }

    [Table("AspNetUsers")]
    public class User
    {
        public string Id { get; set; }
    }
}