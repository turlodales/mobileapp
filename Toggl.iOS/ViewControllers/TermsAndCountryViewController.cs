using System;
using CoreGraphics;
using Foundation;
using Toggl.Core.UI.Helper;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Extensions;
using Toggl.iOS.Extensions.Reactive;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.ViewControllers
{
    public partial class TermsAndCountryViewController : ReactiveViewController<TermsAndCountryViewModel>
    {
        private const int fontSize = 15;
        private const int headerFontSize = 28;
        private const float headerLineHeight = 38;

        private readonly NSRange privacyPolicyRange;
        private readonly NSRange termsOfServiceTextRange;

        private readonly UIStringAttributes normalTextAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize),
            ForegroundColor = ColorAssets.Text
        };

        private readonly UIStringAttributes highlitedTextAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(fontSize),
            ForegroundColor = ColorAssets.Text,
            UnderlineStyle = NSUnderlineStyle.Single
        };

        private readonly UIStringAttributes headerTextAttributes = new UIStringAttributes
        {
            Font = UIFont.SystemFontOfSize(headerFontSize),
            ForegroundColor = ColorAssets.Text
        };

        public TermsAndCountryViewController(TermsAndCountryViewModel viewModel)
             : base(viewModel, nameof(TermsAndCountryViewController))
        {
            privacyPolicyRange = new NSRange(
                ViewModel.IndexOfPrivacyPolicy,
                Resources.PrivacyPolicy.Length);

            termsOfServiceTextRange = new NSRange(
                ViewModel.IndexOfTermsOfService,
                Resources.TermsOfService.Length);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.MinimumLineHeight = headerLineHeight;
            headerTextAttributes.ParagraphStyle = paragraphStyle;
            HeaderLabel.AttributedText = new NSAttributedString(Resources.ReviewTheTerms, headerTextAttributes);
            ConfirmButton.SetTitle(Resources.IAgree, UIControlState.Normal);
            ConfirmButton.Layer.MasksToBounds = true;
            ConfirmButton.Layer.CornerRadius = 6;

            PreferredContentSize = new CGSize(View.Frame.Width, View.Frame.Height);

            prepareTextView();

            ViewModel.CountryButtonTitle
                .Subscribe(CountrySelectionButton.Rx().Title())
                .DisposedBy(DisposeBag);

            ViewModel.IsCountryErrorVisible
                .Subscribe(CountrySelectionErrorView.Rx().IsVisible())
                .DisposedBy(DisposeBag);

            CountrySelectionButton.Rx()
                .BindAction(ViewModel.PickCountry)
                .DisposedBy(DisposeBag);

            ConfirmButton.Rx()
                .BindAction(ViewModel.Accept)
                .DisposedBy(DisposeBag);
        }

        private void prepareTextView()
        {
            TextView.TextContainerInset = UIEdgeInsets.Zero;
            TextView.TextContainer.LineFragmentPadding = 0;

            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.MinimumLineHeight = 24;
            paragraphStyle.ParagraphSpacing = 0;
            normalTextAttributes.ParagraphStyle = paragraphStyle;

            var text = new NSMutableAttributedString(ViewModel.FormattedDialogText, normalTextAttributes);
            text.AddAttributes(highlitedTextAttributes, termsOfServiceTextRange);
            text.AddAttributes(highlitedTextAttributes, privacyPolicyRange);
            TextView.AttributedText = text;

            TextView.AddGestureRecognizer(new UITapGestureRecognizer(onTextViewTapped));
        }

        private void onTextViewTapped(UITapGestureRecognizer recognizer)
        {
            var layoutManager = TextView.LayoutManager;
            var location = recognizer.LocationInView(TextView);
            location.X -= TextView.TextContainerInset.Left;
            location.Y -= TextView.TextContainerInset.Top;

            var characterIndex = layoutManager.GetCharacterIndex(location, TextView.TextContainer);

            if (termsOfServiceTextRange.ContainsNumber(characterIndex))
                ViewModel.ViewTermsOfService.Execute();

            if (privacyPolicyRange.ContainsNumber(characterIndex))
                ViewModel.ViewPrivacyPolicy.Execute();
        }
    }
}

