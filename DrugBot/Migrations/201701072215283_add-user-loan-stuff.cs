namespace DrugBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adduserloanstuff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "LoanDebt", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "LoanRate", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "LoanRate");
            DropColumn("dbo.Users", "LoanDebt");
        }
    }
}
