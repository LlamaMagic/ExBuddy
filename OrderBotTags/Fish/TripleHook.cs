namespace ExBuddy.OrderBotTags.Fish
{
    using Clio.XmlEngine;
    using ExBuddy.Enumerations;
    using System.ComponentModel;

    [XmlElement("TripleHook")]
    public class TripleHook
    {
        [DefaultValue(TripleHookAction.KeepAll)]
        [XmlAttribute("Action")]
        public DoubleHookAction Action { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return this.DynamicToString();
        }
    }
}