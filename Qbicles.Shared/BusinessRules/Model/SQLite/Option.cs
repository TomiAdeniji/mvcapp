using Qbicles.BusinessRules.Helper;

namespace Qbicles.BusinessRules.Model.SQLite
{
    public class OptionContext : SQLite.BaseService<Option>
    {
        public OptionContext()
            : base(ConfigManager.SQLiteConnectionString)
        {

        }

    }
    public class Option
    {
        [SQLite.DbColumnAttribute(IsPrimary = true)]
        public int Id { get; set; }
        [SQLite.DbColumnAttribute]
        public string Name { get; set; }
        [SQLite.DbColumnAttribute]
        public int Property_Id { get; set; }
    }
}
