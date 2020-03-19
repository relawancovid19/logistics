namespace Logistics.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Item_v1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "Images", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "Images");
        }
    }
}
