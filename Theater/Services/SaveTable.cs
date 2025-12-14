using System.Collections.Generic;

namespace Theater.Services
{
    public class SaveTable
    {
        public string Title { get; set; } = "";
        public List<string> Columns { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }
}
