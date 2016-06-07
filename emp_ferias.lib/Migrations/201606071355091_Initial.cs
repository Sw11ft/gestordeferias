namespace emp_ferias.lib.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Marcacoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        DataPedido = c.DateTime(nullable: false),
                        DataInicio = c.DateTime(nullable: false),
                        DataFim = c.DateTime(nullable: false),
                        Observacoes = c.String(),
                        Aprovado = c.Boolean(nullable: false),
                        UserIdAprovacao = c.String(maxLength: 128),
                        RazaoAprovacao = c.String(),
                        Motivo = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserIdAprovacao)
                .Index(t => t.UserId)
                .Index(t => t.UserIdAprovacao);

            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Marcacoes", "UserIdAprovacao", "dbo.AspNetUsers");
            DropForeignKey("dbo.Marcacoes", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Marcacoes", new[] { "UserIdAprovacao" });
            DropIndex("dbo.Marcacoes", new[] { "UserId" });
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Marcacoes");
        }
    }
}
