namespace Logistics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderV2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "DeliveryAddress", c => c.String());
            AddColumn("dbo.Orders", "Province_IdProvince", c => c.String(maxLength: 128));
            CreateIndex("dbo.Orders", "Province_IdProvince");
            AddForeignKey("dbo.Orders", "Province_IdProvince", "dbo.Provinces", "IdProvince");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "Province_IdProvince", "dbo.Provinces");
            DropIndex("dbo.Orders", new[] { "Province_IdProvince" });
            DropColumn("dbo.Orders", "Province_IdProvince");
            DropColumn("dbo.Orders", "DeliveryAddress");
        }
    }
}
