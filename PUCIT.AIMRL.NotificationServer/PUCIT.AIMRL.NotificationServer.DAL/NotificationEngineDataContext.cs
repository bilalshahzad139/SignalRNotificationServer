using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace PUCIT.AIMRL.NotificationServer.DAL
{
    internal class NotificationEngineDataContext : DbContext
    {
        private static readonly string ConnectionString = DatabaseHelper.MainDBConnectionString;

        public NotificationEngineDataContext()
            : base(ConnectionString)
        {
            // We'll eager load entities whenever required.
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 3000;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    
    
    }
}



