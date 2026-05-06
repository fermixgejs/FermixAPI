namespace FermixAPI.Hints.Core.Utilities.UnityAdaptors
{
    using System;
    using FermixAPI.Hints.Core.Interface;
    using FermixAPI.Hints.Core.Models.Arguments;
    using FermixAPI.Hints.Core.Utilities.Tools;
    using Mirror;

    internal class ScpslDisplayOutput(NetworkConnection connectionToPlayer) : IDisplayOutput
    {
        private readonly NetworkConnection? connectionToPlayer = connectionToPlayer;

        public void ShowHint(DisplayOutputArg ev)
        {
            try
            {
                if (connectionToPlayer is not { isReady: true })
                    return;

                global::Hints.HintMessage hintMessageTemplate = new(new global::Hints.TextHint(ev.Content, [new global::Hints.StringHintParameter(string.Empty)], [new global::Hints.AlphaEffect(1)], 99999f));
                connectionToPlayer.Send(hintMessageTemplate);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
        }
    }
}
