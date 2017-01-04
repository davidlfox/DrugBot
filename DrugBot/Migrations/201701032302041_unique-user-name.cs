namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class uniqueusername : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Name", c => c.String(nullable: false, maxLength: 30));
            CreateIndex("dbo.Users", "Name", unique: true, name: "IX_UniqueName");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Users", "IX_UniqueName");
            AlterColumn("dbo.Users", "Name", c => c.String());
        }
    }
}
