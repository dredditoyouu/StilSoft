using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using StilsoftIRS.Models;
using StilsoftIRS.Repositories;

namespace StilsoftIRS.Services
{
    internal sealed class ReportService
    {
        private readonly IIncidentRepository _incidents;
        private readonly IEventLogRepository _eventLogs;

        public ReportService(IIncidentRepository incidents, IEventLogRepository eventLogs)
        {
            _incidents = incidents;
            _eventLogs = eventLogs;
        }

        public ReportData BuildReport(DateTime dateFrom, DateTime dateTo)
        {
            var normalizedFrom = dateFrom.Date;
            var normalizedTo = dateTo.Date.AddDays(1).AddTicks(-1);
            var incidents = _incidents.GetByCreatedPeriod(normalizedFrom, normalizedTo);
            var logs = _eventLogs.GetAll();
            var incidentIds = new HashSet<int>(incidents.Select(item => item.Id));
            var incidentLogs = logs.Where(item => item.IncidentId.HasValue && incidentIds.Contains(item.IncidentId.Value)).ToList();

            var report = new ReportData
            {
                DateFrom = normalizedFrom,
                DateTo = normalizedTo,
                TotalIncidents = incidents.Count,
                EscalatedCount = incidentLogs
                    .Where(item => string.Equals(item.Action, "Эскалация", StringComparison.Ordinal))
                    .Select(item => item.IncidentId.Value)
                    .Distinct()
                    .Count()
            };

            foreach (var incident in incidents)
            {
                report.Incidents.Add(incident);
            }

            foreach (var item in incidents.GroupBy(incident => incident.Priority).OrderBy(group => group.Key))
            {
                report.PriorityBreakdown.Add(new ReportMetric { Name = item.Key, Count = item.Count() });
            }

            foreach (var item in incidents.GroupBy(incident => incident.CategoryName).OrderBy(group => group.Key))
            {
                report.CategoryBreakdown.Add(new ReportMetric { Name = item.Key, Count = item.Count() });
            }

            var reactionTimes = new List<TimeSpan>();
            foreach (var incident in incidents)
            {
                var reactionEntry = incidentLogs
                    .Where(item => item.IncidentId == incident.Id &&
                                   (string.Equals(item.Action, "Смена статуса", StringComparison.Ordinal) ||
                                    string.Equals(item.Action, "Эскалация", StringComparison.Ordinal)))
                    .OrderBy(item => item.OccurredAt)
                    .FirstOrDefault();

                if (reactionEntry != null)
                {
                    reactionTimes.Add(reactionEntry.OccurredAt - incident.CreatedAt);
                }
            }

            if (reactionTimes.Count > 0)
            {
                report.AverageReactionTime = TimeSpan.FromTicks(Convert.ToInt64(reactionTimes.Average(item => item.Ticks)));
            }

            var closureTimes = incidents
                .Where(item => item.ClosedAt.HasValue)
                .Select(item => item.ClosedAt.Value - item.CreatedAt)
                .ToList();

            if (closureTimes.Count > 0)
            {
                report.AverageClosureTime = TimeSpan.FromTicks(Convert.ToInt64(closureTimes.Average(item => item.Ticks)));
            }

            return report;
        }

        public void ExportToExcel(ReportData report, string filePath)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException("Не указан путь для экспорта отчёта.");
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var workbook = new XLWorkbook())
            {
                var summarySheet = workbook.Worksheets.Add("Сводка");
                summarySheet.Cell(1, 1).Value = "Период";
                summarySheet.Cell(1, 2).Value = report.DateFrom.ToString("dd.MM.yyyy") + " - " + report.DateTo.ToString("dd.MM.yyyy");
                summarySheet.Cell(2, 1).Value = "Всего инцидентов";
                summarySheet.Cell(2, 2).Value = report.TotalIncidents;
                summarySheet.Cell(3, 1).Value = "Эскалировано";
                summarySheet.Cell(3, 2).Value = report.EscalatedCount;
                summarySheet.Cell(4, 1).Value = "Среднее время реакции";
                summarySheet.Cell(4, 2).Value = FormatTimeSpan(report.AverageReactionTime);
                summarySheet.Cell(5, 1).Value = "Среднее время закрытия";
                summarySheet.Cell(5, 2).Value = FormatTimeSpan(report.AverageClosureTime);

                summarySheet.Cell(7, 1).Value = "Распределение по приоритетам";
                summarySheet.Cell(8, 1).Value = "Приоритет";
                summarySheet.Cell(8, 2).Value = "Количество";
                var priorityRow = 9;
                foreach (var metric in report.PriorityBreakdown)
                {
                    summarySheet.Cell(priorityRow, 1).Value = metric.Name;
                    summarySheet.Cell(priorityRow, 2).Value = metric.Count;
                    priorityRow++;
                }

                summarySheet.Cell(7, 4).Value = "Распределение по категориям";
                summarySheet.Cell(8, 4).Value = "Категория";
                summarySheet.Cell(8, 5).Value = "Количество";
                var categoryRow = 9;
                foreach (var metric in report.CategoryBreakdown)
                {
                    summarySheet.Cell(categoryRow, 4).Value = metric.Name;
                    summarySheet.Cell(categoryRow, 5).Value = metric.Count;
                    categoryRow++;
                }

                var incidentsSheet = workbook.Worksheets.Add("Инциденты");
                incidentsSheet.Cell(1, 1).Value = "ID";
                incidentsSheet.Cell(1, 2).Value = "Заголовок";
                incidentsSheet.Cell(1, 3).Value = "Приоритет";
                incidentsSheet.Cell(1, 4).Value = "Категория";
                incidentsSheet.Cell(1, 5).Value = "Статус";
                incidentsSheet.Cell(1, 6).Value = "Оператор";
                incidentsSheet.Cell(1, 7).Value = "Создан";
                incidentsSheet.Cell(1, 8).Value = "Закрыт";

                var row = 2;
                foreach (var incident in report.Incidents)
                {
                    incidentsSheet.Cell(row, 1).Value = incident.Id;
                    incidentsSheet.Cell(row, 2).Value = incident.Title;
                    incidentsSheet.Cell(row, 3).Value = incident.Priority;
                    incidentsSheet.Cell(row, 4).Value = incident.CategoryName;
                    incidentsSheet.Cell(row, 5).Value = incident.StatusName;
                    incidentsSheet.Cell(row, 6).Value = incident.OperatorName;
                    incidentsSheet.Cell(row, 7).Value = incident.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                    incidentsSheet.Cell(row, 8).Value = incident.ClosedAt.HasValue
                        ? incident.ClosedAt.Value.ToString("dd.MM.yyyy HH:mm")
                        : string.Empty;
                    row++;
                }

                summarySheet.Columns().AdjustToContents();
                incidentsSheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        public static string FormatTimeSpan(TimeSpan? value)
        {
            return value.HasValue ? string.Format("{0:%d} дн. {0:hh\\:mm\\:ss}", value.Value) : "н/д";
        }
    }
}
