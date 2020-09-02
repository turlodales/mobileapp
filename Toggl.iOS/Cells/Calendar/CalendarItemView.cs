using CoreAnimation;
using CoreGraphics;
using Foundation;
using System;
using System.Reactive.Linq;
using System.Timers;
using Toggl.Core;
using Toggl.Core.Calendar;
using Toggl.Core.Extensions;
using Toggl.Core.UI.Transformations;
using Toggl.iOS.Extensions;
using Toggl.iOS.Transformations;
using Toggl.iOS.Views;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using UIKit;

namespace Toggl.iOS.Cells.Calendar
{
    public sealed partial class CalendarItemView : ReactiveCollectionViewCell<CalendarItem>
    {
        public static readonly NSString Key = new NSString(nameof(CalendarItemView));
        public static readonly UINib Nib;

        private CALayer stripesLayer;
        private CAShapeLayer borderLayer;
        private CAShapeLayer maskLayer;
        private CAShapeLayer badgeLayer;

        public CGRect TopDragTouchArea => TopDragIndicator.Frame.Inset(-20, -20);
        public CGRect BottomDragTouchArea => BottomDragIndicator.Frame.Inset(-20, -20);

        private CalendarProjectTaskClientToAttributedString projectTaskClientToAttributedString;

        private bool isEditing;
        public bool IsEditing
        {
            get => isEditing;
            set
            {
                isEditing = value;
                updateDragHandles();
                updateStripesPattern();
                updateBorders();
                updateShadows();
            }
        }

        public DurationFormat DurationFormat { get; set; }
        public ITimeService TimeService { get; set; }
        private IDisposable timerDisposable;

        static CalendarItemView()
        {
            Nib = UINib.FromName(nameof(CalendarItemView), NSBundle.MainBundle);
        }

        public CalendarItemView(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            prepareInitialConstraints();

            stripesLayer = new CALayer();
            borderLayer = new CAShapeLayer();
            maskLayer = new CAShapeLayer();
            badgeLayer = new CAShapeLayer();
            BackgroundColorView.Layer.AddSublayer(stripesLayer);
            BackgroundColorView.Layer.AddSublayer(borderLayer);
            ContentView.Layer.Mask = maskLayer;
            SyncStatusView.Layer.Mask = badgeLayer;
            TopContainerView.BackgroundColor = UIColor.Clear;
            BottomContainerView.BackgroundColor = UIColor.Clear;
            BottomShadowView.BackgroundColor = UIColor.Clear;

            var icTags = UIImage.FromBundle("icTags").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            HasTagsImageView.Image = icTags;

            var icBillable = UIImage.FromBundle("icBillable").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            IsBillableImageView.Image = icBillable;

            var icCalendar = UIImage.FromBundle("icCalendarSmall").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            IsEventImageView.Image = icCalendar;

            var badgePath = UIBezierPath.FromOval(new CGRect(0, 0, SyncStatusView.Bounds.Width * 2, SyncStatusView.Bounds.Height * 2));
            badgeLayer.Path = badgePath.CGPath;
        }

        private void prepareInitialConstraints()
        {
            // The following constraints are required because there's an undocumented change in iOS 12
            ContentView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
            ContentView.BottomAnchor.ConstraintEqualTo(BottomAnchor).Active = true;
            ContentView.LeadingAnchor.ConstraintEqualTo(LeadingAnchor).Active = true;
            ContentView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor).Active = true;
        }

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();
            timerDisposable?.Dispose();
            timerDisposable = null;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            CATransaction.Begin();
            CATransaction.AnimationDuration = 0;
            CATransaction.DisableActions = true;

            updateStripesPattern();
            updateBorders();
            updateShadows();
            updateElementsVisibility();

            CATransaction.Commit();
        }

        protected override void UpdateView()
        {
            CATransaction.Begin();
            CATransaction.AnimationDuration = 0;
            CATransaction.DisableActions = true;

            var color = new Color(Item.Color).ToNativeColor();
            setBackgroundColor(color, Item.IsPlaceholder);
            updateStripesPattern();

            setBorderColor(color);
            updateBorders();

            setSyncStatus();

            DescriptionLabel.Text = Item.Description;

            projectTaskClientToAttributedString = new CalendarProjectTaskClientToAttributedString(12);
            ProjectTaskClientLabel.AttributedText = projectTaskClientToAttributedString.Convert(Item);

            if (Item.Duration.HasValue)
            {
                DurationLabel.Text = DurationAndFormatToString.Convert(Item.Duration.Value, DurationFormat);
            }
            else
            {
                timerDisposable?.Dispose();
                timerDisposable = null;
                timerDisposable = TimeService.CurrentDateTimeObservable
                    .Select(now => now - Item.StartTime)
                    .Select(duration => DurationAndFormatToString.Convert(duration, DurationFormat.Improved))
                    .AsDriver("", IosDependencyContainer.Instance.SchedulerProvider)
                    .Subscribe(formattedDuration => DurationLabel.Text = formattedDuration);
            }

            HasTagsImageView.Hidden = !Item.HasTags;
            IsBillableImageView.Hidden = !Item.IsBillable;
            IsEventImageView.Hidden = Item.IconKind != CalendarIconKind.Event;

            CATransaction.Commit();

            SetNeedsLayout();
            LayoutIfNeeded();
        }

        private void setBackgroundColor(UIColor color, bool isPlaceholder)
        {
            if (isPlaceholder)
            {
                BackgroundColorView.BackgroundColor = UIColor.Clear;
                return;
            }

            stripesLayer.Hidden = !Item.IsRunningTimeEntry();
            if (Item.Source == CalendarItemSource.Calendar || !Item.IsRunningTimeEntry())
            {
                var alpha = IsEditing ? (nfloat)0.34 : (nfloat)0.25;
                BackgroundColorView.BackgroundColor = color.ColorWithAlpha(alpha);
            }
            else
            {
                BackgroundColorView.BackgroundColor = color.ColorWithAlpha((nfloat)0.08);
                stripesLayer.BackgroundColor = createStripesPattern(color).CGColor;
            }
        }

        private void setBorderColor(UIColor color)
        {
            borderLayer.Hidden = !Item.IsRunningTimeEntry();
            borderLayer.FillColor = UIColor.Clear.CGColor;
            borderLayer.StrokeColor = color.ColorWithAlpha((nfloat)0.5).CGColor;
            borderLayer.LineWidth = 1f;
            borderLayer.LineCap = CAShapeLayer.CapRound;
            borderLayer.LineDashPattern = new NSNumber[] { 4, 4 };
        }

        private UIColor createStripesPattern(UIColor color)
        {
            var patternTint = color.ColorWithAlpha((nfloat)0.08);
            var patternTemplate = UIImage.FromBundle("stripes")
                .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)
                .ApplyTintColor(patternTint);

            UIGraphics.BeginImageContextWithOptions(patternTemplate.Size, false, patternTemplate.CurrentScale);
            UIGraphics.GetCurrentContext().ScaleCTM(1, -1);
            UIGraphics.GetCurrentContext().TranslateCTM(0, -patternTemplate.Size.Height);
            patternTemplate.Draw(new CGRect(0, 0, patternTemplate.Size.Width, patternTemplate.Size.Height));

            var pattern = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return UIColor.FromPatternImage(pattern);
        }

        private void setSyncStatus()
        {
            switch (Item.IconKind)
            {
                case CalendarIconKind.Unsynced:
                    SyncStatusView.Hidden = false;
                    SyncStatusView.BackgroundColor = ColorAssets.CustomGray;
                    break;
                case CalendarIconKind.Unsyncable:
                    SyncStatusView.Hidden = false;
                    SyncStatusView.BackgroundColor = ColorAssets.StopButton;
                    break;
                default:
                    SyncStatusView.Hidden = true;
                    break;
            }
        }

        private void updateStripesPattern()
        {
            stripesLayer.Frame = ContentView.Bounds;
        }

        private void updateShadows()
        {
            if (isEditing)
            {
                var shadowPath = UIBezierPath.FromRect(Bounds);
                Layer.ShadowPath = shadowPath.CGPath;

                Layer.CornerRadius = 2;
                Layer.ShadowRadius = 4;
                Layer.ShadowOpacity = 0.1f;
                Layer.MasksToBounds = false;
                Layer.ShadowOffset = new CGSize(0, 4);
                Layer.ShadowColor = UIColor.Black.CGColor;
            }
            else
            {
                Layer.ShadowOpacity = 0;
            }
        }

        private void updateBorders()
        {
            var radius = 4;

            var backgroundCorners = Item.IsRunningTimeEntry() ? UIRectCorner.TopLeft | UIRectCorner.TopRight : UIRectCorner.AllCorners;
            var backgroundPath = UIBezierPath.FromRoundedRect(ContentView.Bounds, backgroundCorners, new CGSize(radius, radius));
            borderLayer.Frame = ContentView.Bounds;
            borderLayer.Path = backgroundPath.CGPath;
            maskLayer.Path = backgroundPath.CGPath;
        }

        private void updateElementsVisibility()
        {
            var height = ContentView.Bounds.Height;

            if (height < 20)
            {
                DescriptionLabel.Hidden = true;
                ProjectTaskClientLabel.Hidden = true;
                BottomContainerView.Hidden = true;
            }
            else if (height < 40)
            {
                DescriptionLabel.Hidden = false;
                ProjectTaskClientLabel.Hidden = true;
                BottomContainerView.Hidden = true;
            }
            else if (height < 64)
            {
                DescriptionLabel.Hidden = false;
                ProjectTaskClientLabel.Hidden = false;
                BottomContainerView.Hidden = true;
            }
            else
            {
                DescriptionLabel.Hidden = false;
                ProjectTaskClientLabel.Hidden = false;
                BottomContainerView.Hidden = false;
            }

            TopContainerView.ClipsToBounds = !BottomContainerView.Hidden;

            if (!BottomContainerView.Hidden && ProjectTaskClientLabel.Frame.GetMaxY() >= BottomContainerView.Frame.GetMinY())
            {
                var shadowPath = UIBezierPath.FromRect(BottomShadowView.Bounds);
                BottomShadowView.Layer.ShadowPath = shadowPath.CGPath;

                BottomShadowView.Layer.ShadowRadius = 1;
                BottomShadowView.Layer.ShadowOpacity = 0.1f;
                BottomShadowView.Layer.MasksToBounds = false;
                BottomShadowView.Layer.ShadowOffset = new CGSize(0, -2);
                BottomShadowView.Layer.ShadowColor = UIColor.Black.CGColor;
            }
            else
            {
                BottomShadowView.Layer.ShadowOpacity = 0;
            }

            IconsContainerView.Hidden = IconsContainerView.Frame.GetMinX() <= DurationLabel.Frame.GetMaxX();
        }

        private void updateDragHandles()
        {
            TopDragIndicator.Hidden = !IsEditing;
            BottomDragIndicator.Hidden = !IsEditing;
        }
    }
}
