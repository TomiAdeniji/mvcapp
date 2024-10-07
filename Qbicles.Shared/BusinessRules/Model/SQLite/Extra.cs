using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class ExtraContext : SQLite.BaseService<Extra>
    {
        public ExtraContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class Extra
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Name { get; set; }
        [SQLite.DbColumnAttribute]
        public string Description { get; set; }
        [SQLite.DbColumnAttribute]
        public string SKU { get; set; }
        [SQLite.DbColumnAttribute]
        public string Barcode { get; set; }
        [SQLite.DbColumnAttribute]
        public string ImageUri { get; set; }
        [SQLite.DbColumnAttribute]
        public decimal NetValue { get; set; }
        [SQLite.DbColumnAttribute]
        public decimal GrossValue { get; set; }
        [SQLite.DbColumnAttribute]
        public decimal TaxAmount { get; set; }
        [SQLite.DbColumnAttribute]
        public int CategoryItem_Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Unit { get; set; }
    }
}
