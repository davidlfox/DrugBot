namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class inventoryandschematweaks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InventoryItems",
                c => new
                    {
                        InventoryItemId = c.Int(nullable: false, identity: true),
                        DrugId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InventoryItemId)
                .ForeignKey("dbo.Drugs", t => t.DrugId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.DrugId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Drugs", "MinPrice", c => c.Int(nullable: false));
            AddColumn("dbo.Drugs", "MaxPrice", c => c.Int(nullable: false));
            AlterColumn("dbo.Users", "Wallet", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InventoryItems", "UserId", "dbo.Users");
            DropForeignKey("dbo.InventoryItems", "DrugId", "dbo.Drugs");
            DropIndex("dbo.InventoryItems", new[] { "UserId" });
            DropIndex("dbo.InventoryItems", new[] { "DrugId" });
            AlterColumn("dbo.Users", "Wallet", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.Drugs", "MaxPrice");
            DropColumn("dbo.Drugs", "MinPrice");
            DropTable("dbo.InventoryItems");
        }
    }
}
