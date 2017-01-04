namespace DrugBot.Migrations
{
    using Common;
    using Data;
    using System;
    using System.Data.Entity.Migrations;

    public partial class inventorysize : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "InventorySize", c => c.Int(nullable: false, defaultValue: Common.Defaults.InventorySize));

            Sql("update users set dayofgame=1, locationid=1, wallet=1000 where 1=1");
            Sql("update inventoryitems set quantity=0 where 1=1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "InventorySize");
        }
    }
}
