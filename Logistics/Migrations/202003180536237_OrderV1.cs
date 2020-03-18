namespace Logistics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderV1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Deliveries",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        ETA = c.DateTimeOffset(nullable: false, precision: 7),
                        TrackingNumber = c.String(),
                        Service = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Order_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.Order_Id)
                .Index(t => t.Order_Id);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Created = c.DateTimeOffset(nullable: false, precision: 7),
                        Title = c.String(),
                        Descriptions = c.String(),
                        Priority = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Amount = c.Int(nullable: false),
                        Unit = c.Int(nullable: false),
                        Item_Id = c.String(maxLength: 128),
                        Order_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Items", t => t.Item_Id)
                .ForeignKey("dbo.Orders", t => t.Order_Id)
                .Index(t => t.Item_Id)
                .Index(t => t.Order_Id);
            
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Descriptions = c.String(),
                        Unit = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Deliveries", "Order_Id", "dbo.Orders");
            DropForeignKey("dbo.Orders", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.OrderItems", "Order_Id", "dbo.Orders");
            DropForeignKey("dbo.OrderItems", "Item_Id", "dbo.Items");
            DropIndex("dbo.OrderItems", new[] { "Order_Id" });
            DropIndex("dbo.OrderItems", new[] { "Item_Id" });
            DropIndex("dbo.Orders", new[] { "User_Id" });
            DropIndex("dbo.Deliveries", new[] { "Order_Id" });
            DropTable("dbo.Items");
            DropTable("dbo.OrderItems");
            DropTable("dbo.Orders");
            DropTable("dbo.Deliveries");
        }
    }
}
