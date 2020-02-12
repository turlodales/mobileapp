using System;

namespace Toggl.Core.UI.Views
{
    public interface IAppleTokenProvider
    {
        IObservable<string> GetAppleToken();
    }
}
