namespace FermixAPI.Hints.Core.Interface
{
    using System;
    using System.Collections.Generic;

    internal interface ICoroutineRunner
    {
        ICoroutine StartCoroutine(IEnumerator<float> routine);

        ICoroutine CallAfter(TimeSpan time, Action action);
    }
}
