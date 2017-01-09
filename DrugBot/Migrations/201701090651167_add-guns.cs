namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addguns : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Guns",
                c => new
                    {
                        GunId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Damage = c.Int(nullable: false),
                        Cost = c.Int(nullable: false),
                        MinimumDayOfGame = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GunId);
            
            AddColumn("dbo.Users", "GunId", c => c.Int());
            CreateIndex("dbo.Users", "GunId");
            AddForeignKey("dbo.Users", "GunId", "dbo.Guns", "GunId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "GunId", "dbo.Guns");
            DropIndex("dbo.Users", new[] { "GunId" });
            DropColumn("dbo.Users", "GunId");
            DropTable("dbo.Guns");
        }
    }
}
