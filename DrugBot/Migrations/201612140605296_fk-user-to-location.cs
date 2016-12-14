namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fkusertolocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "LocationId", c => c.Int());
            CreateIndex("dbo.Users", "LocationId");
            AddForeignKey("dbo.Users", "LocationId", "dbo.Locations", "LocationId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "LocationId", "dbo.Locations");
            DropIndex("dbo.Users", new[] { "LocationId" });
            DropColumn("dbo.Users", "LocationId");
        }
    }
}
