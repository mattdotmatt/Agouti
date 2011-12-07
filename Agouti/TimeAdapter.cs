using System.Threading;

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