using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attendance.Infrastructure.Entities;
using ClosedXML.Excel;

namespace Attendance.Infrastructure.Services;

public interface ICalendarParserService
{
    List<CourseCalendarEntry> ParseCalendarExcel(Stream excelStream, int courseId);
}

public class CalendarParserService : ICalendarParserService
{
    public List<CourseCalendarEntry> ParseCalendarExcel(Stream excelStream, int courseId)
    {
        var entries = new List<CourseCalendarEntry>();

        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            return entries;
        }

        var rows = worksheet.RowsUsed().Skip(1); // Skip header row
        var now = DateTime.UtcNow;

        foreach (var row in rows)
        {
            var dateCell = row.Cell(1);
            DateOnly date;

            if (dateCell.Value.IsDateTime)
            {
                date = DateOnly.FromDateTime(dateCell.GetDateTime());
            }
            else if (dateCell.Value.IsNumber)
            {
                date = DateOnly.FromDateTime(DateTime.FromOADate(dateCell.GetDouble()));
            }
            else
            {
                var dateStr = dateCell.GetValue<string>()?.Trim();
                if (string.IsNullOrEmpty(dateStr) || !DateTime.TryParse(dateStr, out var parsedDate))
                {
                    continue; // Skip rows without valid date
                }
                date = DateOnly.FromDateTime(parsedDate);
            }

            var module = row.Cell(4).GetValue<string>()?.Trim();
            var teacher = row.Cell(5).GetValue<string>()?.Trim();
            var room = row.Cell(6).GetValue<string>()?.Trim();

            bool isLective = false;
            string dayType = "NO LECTIVO";

            if (!string.IsNullOrEmpty(module))
            {
                if (module.Equals("FESTIVO", StringComparison.OrdinalIgnoreCase))
                {
                    dayType = "FESTIVO";
                    isLective = false;
                }
                else if (module.Equals("NO LECTIVO", StringComparison.OrdinalIgnoreCase))
                {
                    dayType = "NO LECTIVO";
                    isLective = false;
                }
                else
                {
                    dayType = "LECTIVO";
                    isLective = true;
                }
            }

            entries.Add(new CourseCalendarEntry
            {
                CourseId = courseId,
                Date = date,
                IsLective = isLective,
                DayType = dayType,
                Module = module,
                Teacher = teacher,
                Room = room,
                UploadedAt = now
            });
        }

        var groupedEntries = entries
            .GroupBy(e => e.Date)
            .Select(g =>
            {
                var isLective = g.Any(e => e.IsLective);
                
                string dayType;
                if (g.Any(e => e.DayType == "LECTIVO"))
                {
                    dayType = "LECTIVO";
                }
                else if (g.Any(e => e.DayType == "NO LECTIVO"))
                {
                    dayType = "NO LECTIVO";
                }
                else if (g.Any(e => e.DayType == "FESTIVO"))
                {
                    dayType = "FESTIVO";
                }
                else
                {
                    dayType = "NO LECTIVO";
                }

                var modules = g
                    .Select(e => e.Module?.Trim())
                    .Where(m => !string.IsNullOrEmpty(m) && 
                                !m.Equals("FESTIVO", StringComparison.OrdinalIgnoreCase) && 
                                !m.Equals("NO LECTIVO", StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .ToList();
                
                var module = modules.Any() ? string.Join(" / ", modules) : g.First().Module;

                var teachers = g
                    .Select(e => e.Teacher?.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .ToList();
                
                var teacher = teachers.Any() ? string.Join(", ", teachers) : g.First().Teacher;

                var rooms = g
                    .Select(e => e.Room?.Trim())
                    .Where(r => !string.IsNullOrEmpty(r))
                    .Distinct()
                    .ToList();
                
                var room = rooms.Any() ? string.Join(", ", rooms) : g.First().Room;

                return new CourseCalendarEntry
                {
                    CourseId = courseId,
                    Date = g.Key,
                    IsLective = isLective,
                    DayType = dayType,
                    Module = module,
                    Teacher = teacher,
                    Room = room,
                    UploadedAt = now
                };
            })
            .ToList();

        return groupedEntries;
    }
}
