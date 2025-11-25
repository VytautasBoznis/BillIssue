import "./TimeLoggingHistory.css";
import TimeLoggingHistoryItem from "../../shared/TimeLoggingHistoryItem/TimeLoggingHistoryItem";

const TimeLoggingHistory = ({ timeLoggingEntries, editTimeLogEntry, deleteTimeLogEntry }) => {

  return (
    <>
      {timeLoggingEntries.length > 0 ? timeLoggingEntries?.map(entry => (
        <TimeLoggingHistoryItem key={entry.timeLogEntryId} timeEntry={entry} editTimeLogEntry={editTimeLogEntry} deleteTimeLogEntry={deleteTimeLogEntry} />
      )) : <h4 className="text-center p-5">No time logging entries to display</h4>}
    </>
  );
}

export default TimeLoggingHistory;