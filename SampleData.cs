using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace CollectionEditorEx
{
    internal class SampleData
    {
        [Editor(typeof(CollectionEditorEx), typeof(UITypeEditor))]
        public List<SampleItem> Samples { get; set; }

        public SampleData()
        {
            Samples = new List<SampleItem>();
            Samples.Add(new SampleItem("item1"));
            Samples.Add(new SampleItem("item2"));
            Samples.Add(new SampleItem("item3"));
        }
    }
}
