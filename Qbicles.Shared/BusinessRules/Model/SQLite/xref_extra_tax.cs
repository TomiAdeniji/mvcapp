using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class ExtraTaxContext : SQLite.BaseService<xref_extra_tax>
    {
        public ExtraTaxContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class xref_extra_tax
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Extra_Id { get; set; }
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Tax_Id { get; set; }
        [SQLite.DbColumnAttribute]
        public decimal TaxValue { get; set; }
    }
}
