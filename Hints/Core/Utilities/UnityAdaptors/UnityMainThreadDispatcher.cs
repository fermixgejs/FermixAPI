namespace FermixAPI.Hints.Core.Utilities.UnityAdaptors
{
    using System;
    using FermixAPI.Hints.Core.Interface;

    internal class UnityMainThreadDispatcher : IMainThreadDispatcher
    {
        public void Dispatch(Action action)
        {
            global::MainThreadDispatcher.Dispatch(action);
        }
    }
}
