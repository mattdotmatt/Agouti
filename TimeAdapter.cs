using System.Threading;
using Pyramid2.GuiTests.Agouti;

namespace Agouti
{
    
    public class TimeAdapter : ITime
    {
        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
    }
}