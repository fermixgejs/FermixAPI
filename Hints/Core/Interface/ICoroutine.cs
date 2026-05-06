namespace FermixAPI.Hints.Core.Interface
{
    internal interface ICoroutine
    {
        bool IsRunning { get; }

        bool IsPaused { get; }

        void Kill();

        void Pause();

        void Resume();
    }
}
