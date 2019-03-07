using System.Data.Entity;


namespace MCR_EMApp
{
    [DbConfigurationType(typeof(DBConfig))]
     class AppDBContext:DbContext
    {
        public AppDBContext() : base("meterApp") { }
        public DbSet<MeterModel> MeterModel { get; set; }
    }
}
