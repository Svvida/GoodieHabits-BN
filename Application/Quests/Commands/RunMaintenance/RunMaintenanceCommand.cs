using Application.Common.Interfaces;

namespace Application.Quests.Commands.RunMaintenance
{
    /// <summary>
    /// Runs the daily maintenance pass for every profile that is behind. The per-user pass on the read path
    /// is the real mechanism (see <see cref="Services.UserMaintenanceService"/>); this is the sweep that
    /// catches users who have not opened the app, and it is what an external cron would call once the
    /// project has somewhere to run one.
    /// </summary>
    public record RunMaintenanceCommand() : ICommand<int>;
}
