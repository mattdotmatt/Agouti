
namespace Agouti
{
    
    public class XPathSelector
    {
        public string XPath { get; private set; }

        public XPathSelector(string xPath)
        {
            XPath = xPath;
        }

        public XPathSelector(string formatString, params object[] args)
            : this(string.Format(formatString, args))
        {
        }

        public override string ToString()
        {
            return XPath;
        }
    }
}