namespace ExBuddy.OrderBotTags.Fish
{
    using Clio.XmlEngine;
    using ExBuddy.Enumerations;
    using System.ComponentModel;

    [XmlElement("SurfaceSlap")]
    public class SurfaceSlap
    {
        [DefaultValue(SurfaceSlapAction.KeepAll)]
        [XmlAttribute("Action")]
        public SurfaceSlapAction Action { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return this.DynamicToString();
        }
    }
}