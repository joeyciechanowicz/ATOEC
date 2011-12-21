using System.Collections.Generic;
using System;

namespace ATOEC
{

    /// <summary>
    /// Generic event args used to pass data between async runs of pipes
    /// </summary>
    public class EventArgs<T> : EventArgs
    {
        private T value;

        public EventArgs(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get { return value; }
        }
    }
}
