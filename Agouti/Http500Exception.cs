using System;

namespace Agouti
{
    public class Http500Exception : Exception
    {
        public Http500Exception(string message) : base(String.Format("HTTP 500: {0}",message))
        {
        }
    }
}