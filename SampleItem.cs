using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionEditorEx
{
    internal class SampleItem
    {
        public string Title { get; set; }

        public SampleItem(string title)
        {
            Title = title;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Title))
            {
                return "名無し";
            }
            return Title;
        }
    }
}
