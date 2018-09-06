﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Toggl.Giskard.Adapters;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Extensions
{
    public static class UIExtensions
    {
        public static IObservable<Unit> Tapped(this View button)
            => Observable
                .FromEventPattern(e => button.Click += e, e => button.Click -= e)
                .SelectUnit();

        public static IObservable<Unit> Refreshed(this SwipeRefreshLayout swipeLayout)
            => Observable
                .FromEventPattern(e => swipeLayout.Refresh += e, e => swipeLayout.Refresh -= e)
                .SelectUnit();

        public static IObservable<string> Text(this TextView textView)
            => Observable
                .FromEventPattern<TextChangedEventArgs>(e => textView.TextChanged += e, e => textView.TextChanged -= e)
                .Select(args => ((EditText)args.Sender).Text);

        public static IObservable<ICharSequence> TextFormatted(this TextView textView)
            => Observable
                .FromEventPattern<TextChangedEventArgs>(e => textView.TextChanged += e, e => textView.TextChanged -= e)
                .Select(args => ((EditText)args.Sender).TextFormatted);

        public static IObservable<Unit> EditorActionSent(this EditText text)
            => Observable
                .FromEventPattern<TextView.EditorActionEventArgs>(e => text.EditorAction += e, e => text.EditorAction -= e)
                .SelectUnit();

        public static Action<bool> BindIsVisible(this View view, bool useGone = false)
            => isVisible => view.Visibility = isVisible.ToVisibility(useGone);

        public static Action<string> BindText(this TextView textView)
            => text => textView.Text = text;

        public static Action<bool> BindChecked(this CompoundButton compoundButton)
            => isChecked => compoundButton.Checked = isChecked;

        public static Action<IList<T>> BindItems<T>(this BaseRecyclerAdapter<T> adapter)
            => collection => adapter.Items = collection;

        public static Action<bool> BindEnabled(this View view)
            => enabled => view.Enabled = enabled;
    }
}
