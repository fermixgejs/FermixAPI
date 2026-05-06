namespace FermixAPI.Hints.Core.Utilities.UnityAdaptors
{
    using FermixAPI.Hints.Core.Interface;
    using MEC;

    internal class UnityCoroutine : ICoroutine
    {
        private readonly CoroutineHandle handle;

        internal UnityCoroutine(CoroutineHandle handle)
        {
            this.handle = handle;
        }

        public bool IsRunning => handle.IsRunning;

        public bool IsPaused => handle.IsAliveAndPaused;

        public void Kill()
        {
            Timing.KillCoroutines(handle);
        }

        public void Pause()
        {
            Timing.PauseCoroutines(handle);
        }

        public void Resume()
        {
            Timing.ResumeCoroutines(handle);
        }
    }
}
