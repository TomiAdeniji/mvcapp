using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class PropertyContext : SQLite.BaseService<Property>
    {
        public PropertyContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class Property
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Name { get; set; }
        [SQLite.DbColumnAttribute]
        public int CategoryItem_Id { get; set; }
    }
}
