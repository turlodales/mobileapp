using System;
using Toggl.Core.Helper;

namespace Toggl.Core.UI.Views
{
    public interface IThirdPartyTokenProvider
    {
        IObservable<string> GetToken(ThirdPartyLoginProvider provider);
    }
}
