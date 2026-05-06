namespace FermixAPI.Hints.Core.Interface
{
    using FermixAPI.Hints.Core.Models.Arguments;

    /// <summary>
    /// Defines an adaptor that adapt other hints into HSM through a compatibility layer.
    /// </summary>
    public interface ICompatibilityAdaptor
    {
        /// <summary>
        /// Adapt a hint into HSM using the provided compatibility adaptor arguments.
        /// </summary>
        /// <param name="ev">The arguments containing hint display data.</param>
        void ShowHint(CompatibilityAdaptorArg ev);
    }
}
