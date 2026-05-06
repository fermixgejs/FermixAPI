namespace FermixAPI.Hints.Core.Interface
{
    using System;

    internal interface IPlayerContext : IEquatable<IPlayerContext>
    {
        /// <summary>
        /// Return if the player is still valid(i.e. not disconnected).
        /// </summary>
        /// <returns><see langword="true"/> if the player is still connected and valid; otherwise <see langword="false"/>.</returns>
        bool IsValid();
    }
}
