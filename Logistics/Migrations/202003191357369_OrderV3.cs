namespace Logistics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderV3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Deliveries", "Order_Id", "dbo.Orders");
            DropIndex("dbo.Deliveries", new[] { "Order_Id" });
            AddColumn("dbo.Orders", "Notes", c => c.String());
            AddColumn("dbo.Orders", "Delivery_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Orders", "Delivery_Id");
            AddForeignKey("dbo.Orders", "Delivery_Id", "dbo.Deliveries", "Id");
            DropColumn("dbo.Deliveries", "Order_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Deliveries", "Order_Id", c => c.String(maxLength: 128));
            DropForeignKey("dbo.Orders", "Delivery_Id", "dbo.Deliveries");
            DropIndex("dbo.Orders", new[] { "Delivery_Id" });
            DropColumn("dbo.Orders", "Delivery_Id");
            DropColumn("dbo.Orders", "Notes");
            CreateIndex("dbo.Deliveries", "Order_Id");
            AddForeignKey("dbo.Deliveries", "Order_Id", "dbo.Orders", "Id");
        }
    }
}
