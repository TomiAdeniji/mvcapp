using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class TaxRateContext : SQLite.BaseService<TaxRate>
    {
        public TaxRateContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class TaxRate
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Name { get; set; }
        [SQLite.DbColumnAttribute]
        public decimal Rate { get; set; }
        [SQLite.DbColumnAttribute]
        public string Description { get; set; }
    }
}
