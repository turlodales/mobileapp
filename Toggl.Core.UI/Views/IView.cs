namespace Toggl.Core.UI.Views
{
    public interface IView : IDialogProviderView, IPermissionRequester, IThirdPartyTokenProvider
    {
        void Close();
    }
}
