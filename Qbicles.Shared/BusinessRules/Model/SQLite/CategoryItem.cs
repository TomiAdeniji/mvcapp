using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class CategoryItemContext : SQLite.BaseService<CategoryItem>
    {
        public CategoryItemContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class CategoryItem
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Name { get; set; }
        [SQLite.DbColumnAttribute]
        public string Description { get; set; }
        [SQLite.DbColumnAttribute]
        public string ImageUri { get; set; }
        [SQLite.DbColumnAttribute]
        public int DefaultVariant { get; set; }
        [SQLite.DbColumnAttribute]
        public int Category_Id { get; set; }
    }
}
