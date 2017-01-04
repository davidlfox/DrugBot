namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class requiredbotuserid : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "BotUserId", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "BotUserId", c => c.String());
        }
    }
}
