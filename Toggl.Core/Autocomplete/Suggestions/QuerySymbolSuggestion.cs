using System;
using Toggl.Shared;

namespace Toggl.Core.Autocomplete.Suggestions
{
    public sealed class QuerySymbolSuggestion : AutocompleteSuggestion
    {
        internal static QuerySymbolSuggestion[] Suggestions { get; } =
        {
            new QuerySymbolSuggestion(QuerySymbols.ProjectsString, Resources.Projects),
            new QuerySymbolSuggestion(QuerySymbols.TagsString, Resources.Tags)
        };

        public string Symbol { get; }

        public string Description { get; }

        private QuerySymbolSuggestion(string symbol, string suggestionName)
        {
            Symbol = symbol;
            Description = string.Format(Resources.SearchOrAddObject, suggestionName);
        }

        public override int GetHashCode()
            => HashCode.Combine(Symbol, Description);
    }

    public static class QuerySymbolSuggestionExtensions
    {
        public static string FormattedDescription(this QuerySymbolSuggestion querySymbolSuggestion)
            => $"{querySymbolSuggestion.Symbol} {querySymbolSuggestion.Description}";
    }
}
