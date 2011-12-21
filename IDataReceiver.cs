using System;
using System.Collections.Generic;
namespace ATOEC
{
    
    /// <summary>
    /// Represents a process which receives data of type T and outputs data of type S using asynchronous events
    /// </summary>
    /// <typeparam name="TIn">Type that the IPipe receives</typeparam>
    public interface IDataReceiver<TIn>
    {
        /// <summary>
        /// Handles another pipes DataReady event
        /// </summary>
        /// <remarks>
        ///  For the AsyncCallback ( result => ...) we first need to cast the IAsyncResult to a AsyncResult so that we
        /// can retrieve the AsyncDelegate which generated the IAsyncResult. Then cast to a 
        /// EventHandler which allows us to call EndInvoke with the matching IAsyncResult.
        /// If we do provide the correct IAsyncResult to EndInvoke then an error is thrown
        /// </remarks>
        /// <param name="data"></param>
        void HandleDataReady(object sender, EventArgs<TIn> e);
    }
}
