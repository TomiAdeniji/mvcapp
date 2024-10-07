using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class CategoryContext : SQLite.BaseService<Category>
    {
        public CategoryContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class Category
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Name { get; set; }
        [SQLite.DbColumnAttribute]
        public int IsVisible { get; set; }
        [SQLite.DbColumnAttribute]
        public int Menu_Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Description { get; set; }
    }
}
