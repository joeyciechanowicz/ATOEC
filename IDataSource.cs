
using System.Collections.Generic;
namespace ATOEC
{
    /// <summary>
    /// Defines a class which can produce a stream of data by raising its DataReady event, so that
    /// links in the chain downstream can process the data
    /// </summary>        
    public interface IDataSource<TOut>
    {
        /// <summary>
        /// Event raised when this pipe has data ready to be processed by the next downstream pipe/s
        /// </summary>
        event DataReadyEventHandler<TOut> DataReady;

        List<DataReadyEventHandler<TOut>> GetInvocationList();
    }
}
