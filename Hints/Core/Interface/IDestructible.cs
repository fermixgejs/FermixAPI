namespace FermixAPI.Hints.Core.Interface
{
    /// <summary>
    /// Defines a contract for objects that support explicit resource cleanup.
    /// </summary>
    public interface IDestructible
    {
        /// <summary>
        /// Releases all resources held by this object and performs any necessary cleanup.
        /// </summary>
        void Destruct();
    }
}
