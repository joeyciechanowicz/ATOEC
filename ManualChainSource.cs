using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ATOEC
{

    /// <summary>
    /// A  IDataSource implementation that allows manual insertion of objects into a chain
    /// </summary>
    public class ManualChainSource<S> : IDataSource<S>
    {
        // Cache of event handlers so we don't need to use a multi delegate and thus don't have to
        // cast to DataReadyEventHandler each time we raise the vent
        private List<DataReadyEventHandler<S>> invo = null;

        public void AddData(S data)
        {
            if (invo != null)
            {
                // TODO : There must be a better method of specifying a asyncnous callback for multiple event handlers
                // while still specifying the asyncnous result properly
                // as (((AsyncResult)c).AsyncDelegate as DataReadyEventHandler<T, S>).EndInvoke(c) is just nasty
                // Call all registered handlers of the event asynchronously so we don't block this thread
                // Also use an AsyncCallback to call EndInvoke so that all the worker threads get returned to the 
                // thread pool, otherwise we will cause a memory leak!
                for (int i = 0; i < invo.Count; i++)
                {
                    invo[i].BeginInvoke(
                        this,
                        new EventArgs<S>(data),
                        c => (((AsyncResult)c).AsyncDelegate as DataReadyEventHandler<S>).EndInvoke(c),
                        null);
                }

            }
        }

        event DataReadyEventHandler<S> IDataSource<S>.DataReady
        {
            add
            {
                if (invo == null)
                    invo = new List<DataReadyEventHandler<S>>(1);
                invo.Add(value);
            }

            remove { invo.Remove(value); }
        }


        List<DataReadyEventHandler<S>> IDataSource<S>.GetInvocationList()
        {
            return invo;
        }
    }
}
