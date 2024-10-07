using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class VariantTaxContext : SQLite.BaseService<xref_variant_tax>
    {
        public VariantTaxContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class xref_variant_tax
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Variant_Id { get; set; }
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Tax_Id { get; set; }
        [SQLite.DbColumnAttribute]
        public decimal TaxValue { get; set; }
    }
}
