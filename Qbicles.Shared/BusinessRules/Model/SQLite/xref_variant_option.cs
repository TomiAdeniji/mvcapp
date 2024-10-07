using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class VariantOptionContext : SQLite.BaseService<xref_variant_option>
    {
        public VariantOptionContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class xref_variant_option
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Variant_Id { get; set; }
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Option_Id { get; set; }
    }
}
