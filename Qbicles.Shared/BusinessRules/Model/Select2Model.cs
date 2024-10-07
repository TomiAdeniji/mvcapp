using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class Select2GroupedModel
    {
        public List<Select2GroupedOption> results { get; set; }
        public Select2Pagination pagination { get; set; }
    }
    public class Select2Option
    {
        public string id { get; set; }
        public string text { get; set; }
        public bool selected { get; set; } = false;
    }
    public class Select2GroupedOption
    {
        public string text { get; set; }
        public List<Select2Option> children { get; set; }
    }
    public class Select2Pagination
    {
        public bool more { get; set; }
    }
}
