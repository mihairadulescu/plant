using System;

namespace Plant.Core
{
    public class BluePrintEventArgs : EventArgs
    {
        private readonly object _objectConstructed;

        public BluePrintEventArgs(object objectConstructed)
        {
            _objectConstructed = objectConstructed;
        }

        public object ObjectConstructed
        {
            get
            {
                return _objectConstructed;
            }
        }
    }
}