namespace emp_ferias.lib.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class overhaul : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Marcacoes", name: "UserIdAprovacao", newName: "ActionUserId");
            RenameIndex(table: "dbo.Marcacoes", name: "IX_UserIdAprovacao", newName: "IX_ActionUserId");
            AddColumn("dbo.Marcacoes", "Notas", c => c.String());
            AddColumn("dbo.Marcacoes", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.Marcacoes", "RazaoRejeicao", c => c.String());
            AddColumn("dbo.Marcacoes", "UserNotificado", c => c.Boolean(nullable: false));
            DropColumn("dbo.Marcacoes", "Observacoes");
            DropColumn("dbo.Marcacoes", "Aprovado");
            DropColumn("dbo.Marcacoes", "RazaoAprovacao");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Marcacoes", "RazaoAprovacao", c => c.String());
            AddColumn("dbo.Marcacoes", "Aprovado", c => c.Boolean(nullable: false));
            AddColumn("dbo.Marcacoes", "Observacoes", c => c.String());
            DropColumn("dbo.Marcacoes", "UserNotificado");
            DropColumn("dbo.Marcacoes", "RazaoRejeicao");
            DropColumn("dbo.Marcacoes", "Status");
            DropColumn("dbo.Marcacoes", "Notas");
            RenameIndex(table: "dbo.Marcacoes", name: "IX_ActionUserId", newName: "IX_UserIdAprovacao");
            RenameColumn(table: "dbo.Marcacoes", name: "ActionUserId", newName: "UserIdAprovacao");
        }
    }
}
