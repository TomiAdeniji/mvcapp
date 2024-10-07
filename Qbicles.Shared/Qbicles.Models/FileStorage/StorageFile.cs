using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.FileStorage
{
    public class StorageFile
    {
        public string Id { get; set; }

        public string Path { get; set; }

        public string Name { get; set; }

        public string Size { get; set; }
        [Column(TypeName = "bit")]
        public bool IsPublic { get; set; }
    }
}
