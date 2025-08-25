import { useError } from '../../utils/errorHandling/ErrorProvider';
import { useWorkspace } from '../../utils/workspaceHandling/WorkspaceProvider';
import TimeLoggingCalendar from './timeLoggingCalendar/TimeLoggingCalendar';
import TimeLoggingHistory from './timeLoggingHistory/TimeLoggingHistory';
import TimeLoggingInput from './timeLoggingInput/TimeLoggingInput';
import TimeLoggingService from '../../services/TimeLoggingService';

import './TimeLoggingContainer.css';
import { useEffect, useState } from 'react';
import { getCurrentDateInISOFormat, getDisplayDateInISOFormat } from '../../utils/timeFormatUtils';

const TimeLoggingContainer = () => {
  const { showError } = useError();
  const { selectedWorkspace, workspaceLoading } = useWorkspace();

  const [ weeksTimeLogEntries, setWeeksTimeLogEntries ] = useState([]);
  const [ selectedWeek, setSelectedWeek ] = useState(new Date());
  const [ selectedDay, setSelectedDay ] = useState(getCurrentDateInISOFormat());
  const [ currentDayTimeEntries, setCurrentDayTimeEntries] = useState([]);

  const handleAddTimeEntry = async (timeEntry) => {
    await TimeLoggingService.logTimeEntry(timeEntry, showError);
    await loadTimeLogEntries();
  };

  const handleEditTimeEntry = async (timeEntry) => {
    await TimeLoggingService.editTimeEntry(timeEntry, showError);
    await loadTimeLogEntries();
  }

  const handleDeleteTimeEntry = async (timeEntryId) => {
    await TimeLoggingService.deleteTimeEntry(timeEntryId, showError);
    await loadTimeLogEntries();
  }

  const handleChangeWeekTarget = (newTargetDate) => {
    setSelectedWeek(newTargetDate);
  }

  const handleChangeDayTarget = (newSelectedDay) => {
    setSelectedDay(newSelectedDay);

    const currentTimeEntries = weeksTimeLogEntries?.find(timeEntryDay => getDisplayDateInISOFormat(timeEntryDay.day) === newSelectedDay);
    setCurrentDayTimeEntries(currentTimeEntries?.timeLogEntries || []);
  }

  const loadTimeLogEntries = async () => {
    const timeEntryResult = await TimeLoggingService.loadWeeksTimeEntries({
      workspaceId: selectedWorkspace.id,
      targetDay: selectedWeek,
    }, showError);

    setWeeksTimeLogEntries(timeEntryResult);
  };

  useEffect(() => {  
    if (selectedWorkspace != null) {
      loadTimeLogEntries();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedWorkspace, selectedWeek])

  useEffect(() => {
    const currentTimeEntries = weeksTimeLogEntries?.find(timeEntryDay => getDisplayDateInISOFormat(timeEntryDay.day) === selectedDay);
    setCurrentDayTimeEntries(currentTimeEntries?.timeLogEntries || []);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [weeksTimeLogEntries])

  return (
    <>
      {workspaceLoading ? (<div>LOADING</div>) : (
      <div className='content-container'>
        <TimeLoggingInput addTimeLogEntry={handleAddTimeEntry}/>
        <hr/>
        <TimeLoggingCalendar timelogDays={weeksTimeLogEntries} selectedWeek={selectedWeek} changeWeekTarget={handleChangeWeekTarget} selectedDay={selectedDay} changeDayTarget={handleChangeDayTarget}/>
        <hr/>
        <TimeLoggingHistory timeLoggingEntries={currentDayTimeEntries} editTimeLogEntry={handleEditTimeEntry} deleteTimeLogEntry={handleDeleteTimeEntry}/>
      </div>)}
    </>
  )
}

export default TimeLoggingContainer;