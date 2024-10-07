using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class MenuContext : SQLite.BaseService<Menu>
    {
        public MenuContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class Menu
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Name { get; set; }
    }
}
