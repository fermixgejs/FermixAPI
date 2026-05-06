namespace FermixAPI.Hints.Core.Utilities.UnityAdaptors
{
    using System;
    using System.Collections.Generic;
    using FermixAPI.Hints.Core.Interface;
    using MEC;

    internal class UnityCoroutineRunner : ICoroutineRunner
    {
        public ICoroutine StartCoroutine(IEnumerator<float> routine)
        {
            return new UnityCoroutine(Timing.RunCoroutine(WrapCoroutine(routine)));
        }

        public ICoroutine CallAfter(TimeSpan time, Action action)
        {
            return new UnityCoroutine(Timing.CallDelayed((float)time.TotalSeconds, action));
        }

        private IEnumerator<float> WrapCoroutine(IEnumerator<float> routine)
        {
            try
            {
                while (routine.MoveNext())
                {
                    float current = routine.Current;
                    if (float.IsNaN(current) || current <= 0)
                    {
                        yield return Timing.WaitForOneFrame;
                        continue;
                    }

                    yield return Timing.WaitForSeconds(current);
                }
            }
            finally
            {
                (routine as System.IDisposable)?.Dispose();
            }

            yield break;
        }
    }
}
