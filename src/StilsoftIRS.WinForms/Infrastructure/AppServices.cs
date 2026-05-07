using StilsoftIRS.Repositories;
using StilsoftIRS.Services;

namespace StilsoftIRS.Infrastructure
{
    internal sealed class AppServices
    {
        public AppServices()
        {
            Users = new UserRepository();
            Categories = new CategoryRepository();
            Statuses = new StatusRepository();
            Incidents = new IncidentRepository();
            Resources = new ResourceRepository();
            IncidentResources = new IncidentResourceRepository();
            EventLogs = new EventLogRepository();

            EventLogService = new EventLogService(EventLogs);
            UserService = new UserService(Users);
            IncidentService = new IncidentService(Incidents, Statuses, EventLogService, DbConnectionFactory.CreateConnection);
            ResourceService = new ResourceService(Resources, IncidentResources, Incidents, EventLogService, DbConnectionFactory.CreateConnection);
            ReportService = new ReportService(Incidents, EventLogs);
        }

        public IUserRepository Users { get; }

        public ICategoryRepository Categories { get; }

        public IStatusRepository Statuses { get; }

        public IIncidentRepository Incidents { get; }

        public IResourceRepository Resources { get; }

        public IIncidentResourceRepository IncidentResources { get; }

        public IEventLogRepository EventLogs { get; }

        public UserService UserService { get; }

        public IncidentService IncidentService { get; }

        public ResourceService ResourceService { get; }

        public EventLogService EventLogService { get; }

        public ReportService ReportService { get; }
    }
}
