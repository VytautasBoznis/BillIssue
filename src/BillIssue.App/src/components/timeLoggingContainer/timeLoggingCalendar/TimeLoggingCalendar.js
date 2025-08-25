import { CaretLeft, CaretRight } from "../../icons/CaretIcons";
import "./TimeLoggingCalendar.css";
import { formatSecondsToHoursAndMinutes, getDisplayDateInISOFormat, getSecondsToHoursAndMinutesDisplay } from "../../../utils/timeFormatUtils";

const TimeLoggingCalendar = ({timelogDays, selectedWeek, changeWeekTarget, selectedDay, changeDayTarget}) => {
  const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  const hanldeWeekChange = (dayChange, reset) => {
    if (reset) {
      const newDate = new Date();
      changeWeekTarget(newDate);
      changeDayTarget(getDisplayDateInISOFormat(newDate.toJSON()));
      return;
    }

    const result = new Date(selectedWeek);
    result.setDate(result.getDate() + dayChange);

    changeWeekTarget(result);

    const dayTarget = new Date(result);
    const diffToMonday = (dayTarget.getDay() + 6) % 7;
    dayTarget.setDate(dayTarget.getDate() - diffToMonday)

    changeDayTarget(getDisplayDateInISOFormat(dayTarget.toJSON()));
  }


  const calcTotalSecondsLoggedInWeek = (entries) => {
    if(!entries){
      return 0;
    }

    let secondsSpent = 0;

    for(let i = 0, j = entries.length; i<j; i++){
      secondsSpent += entries[i].secondsLogged;
    }

    return secondsSpent;
  }

  const resolveDayName = (targetDay) => {
    const date = new Date(targetDay);
    const dayIndex = date.getDay();
    return dayNames[dayIndex];
  }

  return (
    <div>
      <div className="d-flex justify-content-between margin-spacer-bottom">
        <div className="btn-group" role="group">
          <button type="button" className="btn btn-theme" onClick={() => hanldeWeekChange(-7, false)}><CaretLeft/></button>
          <button type="button" className="btn btn-theme" onClick={() => hanldeWeekChange(0, true)}>This week</button>
          <button type="button" className="btn btn-theme" onClick={() => hanldeWeekChange(7, false)}><CaretRight/></button>
        </div>
        <div className="calendor-control-text">
          Total time logged in week: {getSecondsToHoursAndMinutesDisplay(calcTotalSecondsLoggedInWeek(timelogDays))}
        </div>
      </div>
      <div className="d-flex flex-wrap equal-cols calendar-container">
        { timelogDays?.map(target => (
          <div key={getDisplayDateInISOFormat(target.day)} onClick={() => changeDayTarget(getDisplayDateInISOFormat(target.day))} className={`flex-grow-1 calendar-cell ${selectedDay === getDisplayDateInISOFormat(target.day) ? 'selected-day': ''}`}>
            <div className="calendar-day">{getDisplayDateInISOFormat(target.day)}</div>
            <div className="calendar-weekday">{resolveDayName(target.day)}</div>
            <div className="d-flex align-items-end flex-column calendar-elements">
              <div className="badge badge-info mt-auto hour-badge">Total: {getSecondsToHoursAndMinutesDisplay(target.secondsLogged)}</div>
            </div>
          </div>))}
      </div>
    </div>
  ) 
}

export default TimeLoggingCalendar;