namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dayofgame : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Games", "UserId", "dbo.Users");
            DropIndex("dbo.Games", new[] { "UserId" });
            AddColumn("dbo.Users", "DayOfGame", c => c.Int(nullable: false));
            DropTable("dbo.Games");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameId = c.Int(nullable: false, identity: true),
                        ConversationId = c.String(),
                        GameDay = c.Byte(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GameId);
            
            DropColumn("dbo.Users", "DayOfGame");
            CreateIndex("dbo.Games", "UserId");
            AddForeignKey("dbo.Games", "UserId", "dbo.Users", "UserId", cascadeDelete: true);
        }
    }
}
