using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ATOEC
{
    /// <summary>
    /// A chain link that receives and outputs the same type of data by raising an event asyncnously. Implementing
    /// classes only need to implement the ProcessData(T data) method, all event handling is .. handled.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractLink<T> : AbstractLink<T, T> { }

    /// <summary>
    /// Operates like the other chain link's, except can aggregate multiple items of data into a single <seealso cref="ICollection&lt;TIn@gt;"/> that is passed to the
    /// implementations ProcessData(TCollection&lt;TIn&gt; data)"/> method.
    /// </summary>
    /// <typeparam name="TIn">The type that this link will receive</typeparam>
    /// <typeparam name="TOut">The type that this link will output</typeparam>
    /// <typeparam name="TCollection">The collection type that will be used to aggregate </typeparam>
    public abstract class AbstractLink<TIn, TOut, TCollection> : AbstractLink<TIn, TOut>
        where TCollection : ICollection<TIn>
    {
        private TCollection aggregate = default(TCollection);

        private int? subscribedCount = null;
        
        /// <summary>
        /// Creates a new instance of an AbstractLink that can aggregate multiple links into one output
        /// </summary>
        /// <param name="numSubscribedEvents">The number of events that this link is subscribed to</param>
        public AbstractLink(int numSubscribedEvents)
            : base()
        {
            subscribedCount = numSubscribedEvents;
        }

        /// <summary>
        /// Does not use ProcessData
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override TOut ProcessData(TIn data)
        {
            return default(TOut);
        }

        protected abstract TOut ProcessData(TCollection data);

        public override void HandleDataReady(object sender, EventArgs<TIn> e)
        {
            // See if we have a subscribed count, if not use the senders invocation list assuming we are
            // subscribed multiple times ... for some reason
            if (aggregate.Count < (subscribedCount ?? ((IDataSource<TIn>)sender).GetInvocationList().Count))
            {
                // still waiting for at least one value
                aggregate.Add(e.Value);
            }
            else
            {
                TOut result = ProcessData(aggregate);

                if (invo != null)
                {
                    // TODO : There must be a better method of specifying a asyncnous callback for multiple event handlers
                    // while still specifying the asyncnous result properly
                    // as (((AsyncResult)c).AsyncDelegate as DataReadyEventHandler<T, S>).EndInvoke(c) is just nasty

                    for (int i = 0; i < invo.Count; i++)
                    {
                        invo[i].BeginInvoke(
                            this,
                            new EventArgs<TOut>(result),
                            c => (((AsyncResult)c).AsyncDelegate as DataReadyEventHandler<TOut>).EndInvoke(c),
                            null);
                    }

                }
            }
        }
    }


    /// <summary>
    /// A link that receives one type of a data and outputs another by raising an event asyncnously. Implementing
    /// classes only need to implement the ProcessData(T data) method, all event handling is .. handled.
    /// </summary>
    public abstract class AbstractLink<TIn, TOut> : ATOEC.IDataReceiver<TIn>, ATOEC.IDataSource<TOut>
    {
        // Cache of event handlers so we don't need to use a multi delegate and thus don't have to
        // cast to DataReadyEventHandler each time we raise the vent
        protected List<DataReadyEventHandler<TOut>> invo = null;

        /// <summary>
        /// Performs the operation for this pipe on an input of T and outputs a object of type S
        /// </summary>
        /// <param name="data">The object to perform actions on</param>
        /// <returns>An instance of S</returns>
        protected abstract TOut ProcessData(TIn data);

        #region IDataSource and IDataReceiver implementation
        public virtual void HandleDataReady(object sender, EventArgs<TIn> e)
        {
            TOut result = ProcessData(e.Value);

            if (invo != null)
            {
                // TODO : There must be a better method of specifying a asyncnous callback for multiple event handlers
                // while still specifying the asyncnous result properly
                // as (((AsyncResult)c).AsyncDelegate as DataReadyEventHandler<T, S>).EndInvoke(c) is just nasty

                for (int i = 0; i < invo.Count; i++)
                {
                    invo[i].BeginInvoke(
                        this,
                        new EventArgs<TOut>(result),
                        c => (((AsyncResult)c).AsyncDelegate as DataReadyEventHandler<TOut>).EndInvoke(c),
                        null);
                }

            }
        }

        event DataReadyEventHandler<TOut> IDataSource<TOut>.DataReady
        {
            add
            {
                if (invo == null)
                    invo = new List<DataReadyEventHandler<TOut>>(1);
                invo.Add(value);
            }

            remove { invo.Remove(value); }
        }

        List<DataReadyEventHandler<TOut>> IDataSource<TOut>.GetInvocationList()
        {
            return invo;
        }
        #endregion

        /// <summary>
        /// Adds the link as the next link in the chain by registering it to the the DataReady event of this link
        /// </summary>
        /// <param name="link">A link to add as a listener</param>
        /// <returns>The link passed, to allow for easy link chaining</returns>
        public AbstractLink<TIn> Add(AbstractLink<TIn> link)
        {
            ((IDataSource<TIn>)this).DataReady += new DataReadyEventHandler<TIn>(((IDataReceiver<TIn>)link).HandleDataReady);
            return link;
        }

        /// <summary>
        /// Adds the link as the next link in the chain by registering it to the the DataReady event of this link
        /// </summary>
        /// <typeparam name="TOutNew">The type that the next link will output (unfortunately needed but not really useful)</typeparam>
        /// <param name="link">A link to add as a listener</param>
        /// <returns>The link passed, to allow for easy link chaining</returns>
        public AbstractLink<TOut, TOutNew> Add<TOutNew>(AbstractLink<TOut, TOutNew> link)
        {
            ((IDataSource<TOut>) this).DataReady += new DataReadyEventHandler<TOut>(((IDataReceiver<TOut>)link).HandleDataReady);
            return link;
        }

        /// <summary>
        /// Adds multiple links as listeners to this links DataReady event
        /// </summary>
        /// <param name="links"></param>
        public void AddMany(params AbstractLink<TIn>[] links)
        {
            foreach (var link in links)
            {
                ((IDataSource<TOut>)this).DataReady += new DataReadyEventHandler<TOut>(((IDataReceiver<TOut>)link).HandleDataReady);
            }
        }

        /// <summary>
        /// Asyncnously invokes a call handler, passing data in the event args.
        /// NOTE: if the handler is not being used as a multi-delegate then this method will only be able to call
        /// one method. <see cref="CallHandler<T>(T data, List<DataReadyEventhandler<T>> handlers"/> should be used instead,
        /// passing instead a cached list of event handlers
        /// </summary>
        /// <typeparam name="TType">Type that the handler's listeners will receive</typeparam>
        /// <param name="data">The data to pass to the </param>
        /// <param name="handler">The event handler to invoke</param>
        /// <returns>true if the handler was invoked, false if not</returns>
        public static bool CallHandler<TType>(object sender, TType data, DataReadyEventHandler<TType> handler)
        {
            if (handler != null)
            {
                handler.BeginInvoke(
                        sender,
                        new EventArgs<TType>(data),
                        c => handler.EndInvoke(c),
                        null);
            }
            else
            {
                return false;
            }
            return true;
        }
        
    }
}
