using Pyramid2.GuiTests.Agouti;

namespace Agouti
{
    
    public static class BrowserFactory
    {
        public static IBrowser Browser
        {
            get
            {
                return new HtmlUnitAdapter(new TimeAdapter());
            }
        }
    }
}