using BillIssue.Shared.Models.Enums.Schedule;

namespace BillIssue.Shared.Models.Response.Schedule.Dto
{
    public class DayScheduleDto
    {
        public WorkWeekDayEnum WorkWeekDay {  get; set; }
        public int StartHour { get; set; } = 8;
        public int EndHour { get; set; } = 17;
        public bool HasBreak { get; set; } = true;
        public int BreakTimeMinutes { get; set; } = 60;
        public int BreakStartHour {  get; set; } = 12;
    }
}
