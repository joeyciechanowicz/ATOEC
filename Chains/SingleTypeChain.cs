using System;
using System.Collections.Generic;

namespace ATOEC.Chains
{
    /// <summary>
    /// A chain that uses only one type of object
    /// </summary>
    /// <typeparam name="T">The incoming and outgoing type of all links in the chain</typeparam>
    public class SingleTypeChain<T>
    {
        private IDataSource<T> source = null;

        /// <summary>
        /// Gets a reference to the chains source object
        /// </summary>
        public IDataSource<T> Source
        {
            get { return source; }
        }

        // The list of data receivers we have
        private List<IDataReceiver<T>> links;

        // Final method to call when the last data receiver returns
        private ChainResultHandler<T> finishedHandler;

        public SingleTypeChain(ChainResultHandler<T> ResultHandler)
        {
            this.finishedHandler = ResultHandler;
            links = new List<IDataReceiver<T>>();
        }

        /// <summary>
        /// Adds a pipe as the last pipe in the chain, automatically registering it for the
        /// DataReady event of the pipe before it
        /// </summary>
        /// <param name="pipe"></param>
        public void AddLink(IDataReceiver<T> pipe)
        {
            if (links.Count == 0)
            {
                // First element, link to the source
                if (source == null)
                    throw new InvalidOperationException("A pipe can not be added without a start pipe");

                links.Add(pipe);
                ((IDataSource<T>)source).DataReady += new DataReadyEventHandler<T>(links[0].HandleDataReady);
            }
            else
            {
                links.Add(pipe);
                ((IDataSource<T>)links[links.Count - 2]).DataReady += new DataReadyEventHandler<T>(links[links.Count - 1].HandleDataReady);
            }
        }

        /// <summary>
        /// Adds the IDataSource that is responsible for producing the data that this chain uses
        /// </summary>
        /// <param name="source">The data source from which to receive data</param>
        public void AddSource(IDataSource<T> source)
        {
            this.source = source;
        }
        
        /// <summary>
        /// Adds the last pipe to the chain and registers its event handler to the ChainResultHandler<T> passed to the constructor
        /// </summary>
        /// <param name="pipe">The pipe to add as the last link of the chain</param>
        public void AddLast(IDataReceiver<T> pipe)
        {
            AddLink(pipe);
            ((IDataSource<T>)links[links.Count - 1]).DataReady += new DataReadyEventHandler<T>(this.HandleResult);
        }

        /// <summary>
        /// Handles the result of the chain
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleResult(object sender, EventArgs<T> e)
        {
            this.finishedHandler(e.Value);
        }
    }
}
