namespace MCR_EMApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class metermodel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MeterModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MeterID = c.Int(nullable: false),
                        IB = c.Int(nullable: false),
                        IR = c.Int(nullable: false),
                        IY = c.Int(nullable: false),
                        KWH = c.Int(nullable: false),
                        MVAR = c.Int(nullable: false),
                        MW = c.Int(nullable: false),
                        VB = c.Int(nullable: false),
                        VR = c.Int(nullable: false),
                        VY = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MeterModels");
        }
    }
}
