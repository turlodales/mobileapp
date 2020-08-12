using System;

namespace Toggl.Shared.Models
{
    public enum Plan
    {
        Free,
        Starter,
        Premium,
        Enterprise,
    }

    public static class PlanExtensions
    {
        public static string Name(this Plan plan)
            => plan switch
            {
                Plan.Free => Resources.Free,
                Plan.Starter => Resources.Starter,
                _ => throw new InvalidOperationException()
            };

        public static bool IsAtLeast(this Plan self, Plan comparison)
            => self >= comparison;
    }
}
