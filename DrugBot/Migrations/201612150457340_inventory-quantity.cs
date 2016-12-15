namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class inventoryquantity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InventoryItems", "Quantity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.InventoryItems", "Quantity");
        }
    }
}
