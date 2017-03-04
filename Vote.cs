using System.Xml.Serialization;

namespace Arechi.CallVote
{
    public sealed class Vote
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Alias")]
        public string Alias;

        [XmlAttribute("Enabled")]
        public bool Enabled;

        public Vote(string name, string alias, bool enabled)
        {
            Name = name;
            Alias = alias;
            Enabled = enabled;
        }

        public Vote()
        {
            Name = "";
            Alias = "";
            Enabled = true;
        }
    }
}
