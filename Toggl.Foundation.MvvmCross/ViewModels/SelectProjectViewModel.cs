﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectProjectViewModel
        : MvxViewModel<(long? projectId, long? taskId, long workspaceId), (long? projectId, long? taskId)>
    {
        private readonly ITogglDataSource dataSource;
        private readonly IMvxNavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly Subject<string> infoSubject = new Subject<string>();
        private long workspaceId;
        private long? projectId;
        private long? taskId;

        public string Text { get; set; } = "";

        public IMvxCommand<ProjectSuggestion> ToggleTaskSuggestionsCommand { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxCommand<AutocompleteSuggestion> SelectProjectCommand { get; }

        public MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>> Suggestions { get; }
            = new MvxObservableCollection<WorkspaceGroupedCollection<AutocompleteSuggestion>>();

        public SelectProjectViewModel(
            ITogglDataSource dataSource, IMvxNavigationService navigationService, IDialogService dialogService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));

            this.dataSource = dataSource;
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            CloseCommand = new MvxAsyncCommand(close);
            ToggleTaskSuggestionsCommand = new MvxCommand<ProjectSuggestion>(toggleTaskSuggestions);
            SelectProjectCommand = new MvxCommand<AutocompleteSuggestion>(selectProject);
        }

        public override void Prepare((long? projectId, long? taskId, long workspaceId) parameter)
        {
            taskId = parameter.taskId;
            projectId = parameter.projectId;
            workspaceId = parameter.workspaceId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            infoSubject.AsObservable()
                       .StartWith(Text)
                       .SelectMany(text => dataSource.AutocompleteProvider.Query(text, AutocompleteSuggestionType.Projects))
                       .Select(suggestions => suggestions.Cast<ProjectSuggestion>())
                       .Subscribe(onSuggestions);
        }

        private void OnTextChanged()
        {
            infoSubject.OnNext(Text);
        }

        private void onSuggestions(IEnumerable<ProjectSuggestion> suggestions)
        {
            Suggestions.Clear();

            suggestions
                .GroupByWorkspaceAddingNoProject()
                .ForEach(Suggestions.Add);
        }

        private Task close()
            => navigationService.Close(this, (projectId, taskId));

        private void selectProject(AutocompleteSuggestion suggestion)
        {
            if (suggestion.WorkspaceId == workspaceId || suggestion.WorkspaceId == 0)
            {
                setProject(suggestion);
                return;
            }

            dialogService.Confirm(
                Resources.DifferentWorkspaceAlertTitle,
                Resources.DifferentWorkspaceAlertMessage,
                Resources.Ok,
                Resources.Cancel,
                () => setProject(suggestion),
                null
            );
        }

        private void setProject(AutocompleteSuggestion suggestion)
        {
            switch (suggestion)
            {
                case ProjectSuggestion projectSuggestion:
                    projectId = projectSuggestion
                        .ProjectId == 0 ? null : (long?)projectSuggestion.ProjectId;
                    taskId = null;
                    break;

                case TaskSuggestion taskSuggestion:
                    projectId = taskSuggestion.ProjectId;
                    taskId = taskSuggestion.TaskId;
                    break;

                default:
                    throw new ArgumentException($"{nameof(suggestion)} must be either of type {nameof(ProjectSuggestion)} or {nameof(TaskSuggestion)}.");
            }

            navigationService.Close(this, (projectId, taskId));
        }

        private void toggleTaskSuggestions(ProjectSuggestion projectSuggestion)
        {
            var grouping = Suggestions.FirstOrDefault(s => s.WorkspaceName == projectSuggestion.WorkspaceName);
            if (grouping == null) return;

            var suggestionIndex = grouping.IndexOf(projectSuggestion);
            if (suggestionIndex < 0) return;

            projectSuggestion.TasksVisible = !projectSuggestion.TasksVisible;

            var groupingIndex = Suggestions.IndexOf(grouping);
            Suggestions.Remove(grouping);
            Suggestions.Insert(groupingIndex,
                new WorkspaceGroupedCollection<AutocompleteSuggestion>(
                    grouping.WorkspaceName, getSuggestionsWithTasks(grouping)
                )
            );
        }

        private IEnumerable<AutocompleteSuggestion> getSuggestionsWithTasks(
            IEnumerable<AutocompleteSuggestion> suggestions)
        {
            foreach (var suggestion in suggestions)
            {
                if (suggestion is TaskSuggestion) continue;

                yield return suggestion;

                if (suggestion is ProjectSuggestion projectSuggestion && projectSuggestion.TasksVisible)
                    foreach (var taskSuggestion in projectSuggestion.Tasks)
                        yield return taskSuggestion;
            }
        }
    }
}
