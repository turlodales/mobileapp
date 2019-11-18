﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.UI.Views;
using Toggl.Shared;

namespace Toggl.Core.UI.ViewModels.Reports
{
    public struct ReportFilter
    {
        public IThreadSafeWorkspace Workspace { get; private set; }
        public DateTimeOffsetRange TimeRange { get; private set; }

        private ReportFilter(IThreadSafeWorkspace workspace, DateTimeOffsetRange timeRange)
        {
            Workspace = workspace;
            TimeRange = timeRange;
        }

        public static ReportFilter Create(IThreadSafeWorkspace workspace, DateTimeOffsetRange timeRange)
            => new ReportFilter(workspace, timeRange);
    }
}
