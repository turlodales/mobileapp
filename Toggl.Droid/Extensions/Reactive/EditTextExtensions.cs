using Android.Widget;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Google.Android.Material.TextField;
using Toggl.Core.UI.Reactive;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Extensions.Reactive
{
    public static class EditTextExtensions
    {
        public static IObservable<Unit> EditorActionSent(this IReactive<EditText> reactive)
            => Observable
                .FromEventPattern<TextView.EditorActionEventArgs>(e => reactive.Base.EditorAction += e, e => reactive.Base.EditorAction -= e)
                .SelectUnit();

        public static Action<string> ErrorObserver(this IReactive<TextInputLayout> reactive)
        {
            return text =>
            {
                reactive.Base.Error = text;
                return;
            };
        }
    }
}
