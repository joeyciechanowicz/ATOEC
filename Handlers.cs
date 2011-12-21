
namespace ATOEC
{
    /// <summary>
    /// Represents the method that will handle the event raised when a IPipe has data to pass down the chain
    /// </summary>
    /// <typeparam name="T">The type that the IPipe will receive and output</typeparam>
    /// <param name="sender">The object raising the event</param>
    /// <param name="e">The vent argument that contains an instance of T for further processing by the chain</param>
    public delegate void DataReadyEventHandler<T>(object sender, EventArgs<T> e);

    /// <summary>
    /// Represents the method that will handle the final result of a chain
    /// </summary>
    /// <typeparam name="T">The type of data to be received</typeparam>
    /// <param name="result">the actual result of the entire chain</param>
    public delegate void ChainResultHandler<T>(T result);
}
