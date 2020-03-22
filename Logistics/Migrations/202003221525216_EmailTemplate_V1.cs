namespace Logistics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmailTemplate_V1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmailTemplates",
                c => new
                    {
                        IdEmailTemplate = c.String(nullable: false, maxLength: 128),
                        Subject = c.String(),
                        Content = c.String(),
                        CreatedBy_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.IdEmailTemplate)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedBy_Id)
                .Index(t => t.CreatedBy_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmailTemplates", "CreatedBy_Id", "dbo.AspNetUsers");
            DropIndex("dbo.EmailTemplates", new[] { "CreatedBy_Id" });
            DropTable("dbo.EmailTemplates");
        }
    }
}
