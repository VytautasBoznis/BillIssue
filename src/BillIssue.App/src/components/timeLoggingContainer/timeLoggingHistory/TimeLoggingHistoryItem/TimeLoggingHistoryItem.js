import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import DatePicker from "react-datepicker";
import { CaretDown, CaretUp } from "../../../icons/CaretIcons";

import "./TimeLoggingHistoryItem.css";
import "react-datepicker/dist/react-datepicker.css";
import { dateToTimestamp, getSecondsToHoursAndMinutesDisplay, timestampToDate } from "../../../../utils/timeFormatUtils";
import { useWorkspace } from "../../../../utils/workspaceHandling/WorkspaceProvider";

const TimeLoggingHistoryItem = ({ timeEntry, editTimeLogEntry, deleteTimeLogEntry }) => {
  const { selectedWorkspace, workspaceLoading } = useWorkspace();
  const [startDate, setStartDate] = useState(timestampToDate(timeEntry.logDate));
  const [isExpanded, setIsExpanded] = useState(false);

  const [selectedProject, setSelectedProject] = useState(null);
  const [selectedWorktype, setSelectedWorktype] = useState(null);

  const handleExpand = () => {
    setIsExpanded(!isExpanded);
  };

  const handleProjectSelection = (projectId) => {
    if (!selectedWorkspace) return;

    const selection = selectedWorkspace.projects.find(p => p.id === projectId);
    setSelectedProject(selection);
  };

  const handleWorkstypeSelection = (worktypeId) => {
    if (!selectedProject) return;

    const selection = selectedProject.worktypes.find(w => w.id === worktypeId);
    setSelectedWorktype(selection);
  };

  useEffect(() => {
    handleProjectSelection(timeEntry.projectId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedWorkspace, timeEntry.projectId]);

  useEffect(() => {
    if (selectedProject) {
      handleWorkstypeSelection(timeEntry.projectWorktypeId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedProject, timeEntry.projectWorktypeId]);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setError,
  } = useForm({
    defaultValues: timeEntry
  });

  const onSubmit = async (data) => {
    if (!startDate) {
      setError("startDate", { type: "manual", message: "This field is required" });
      return;
    }

    if (!selectedProject) {
      setError("project", { type: "manual", message: "This field is required" });
      return;
    }

    if (!selectedWorktype) {
      setError("worktype", { type: "manual", message: "This field is required" });
      return;
    }

    const timelogEntry = {
      TimeLogEntryId: timeEntry.timeLogEntryId,
      WorkspaceId: selectedWorkspace.id,
      ProjectId: selectedProject.id,
      ProjectWorktypeId: selectedWorktype.id,
      Title: data.title,
      LogDate: dateToTimestamp(startDate),
      HourAmount: data.hourAmount,
      MinuteAmount: data.minuteAmount,
      WorkDescription: data.workDescription
    };

    await editTimeLogEntry(timelogEntry);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div className="history-item-container">
        <div className="d-flex justify-content-between history-item-header" onClick={handleExpand}>
          <div className="history-item-title">{timeEntry.title}</div>
          <div>
            <div className="badge badge-info mt-auto hour-badge">{getSecondsToHoursAndMinutesDisplay(timeEntry.secondsTotalAmount)}</div>
            {!isExpanded && <CaretUp size={"16"}/>}
            {isExpanded && <CaretDown size={"16"}/>}
          </div>
        </div>
        {isExpanded && <div className="history-item-content">
            <div className="row controls-row">
              <div className="col-sm-12">
                <div className="d-flex justify-content-between">
                  <div className="input-label">Title</div>
                  <div className="required-label">Required</div>
                </div>
                <input className="form-control" {...register("title", { required: true })} placeholder="Work item title..."></input>
                {errors.title && <span className="error-label">This field is required</span>}
              </div>
            </div>
            <div className="row controls-row">
              <div className="col-md-6 col-sm-12">
                <div className="d-flex justify-content-between">
                  <div className="input-label">Project</div>
                  <div className="required-label">Required</div>
                </div>
                {/*TODO: add proper loading handling*/}
                {workspaceLoading ? (<div>LOADING</div>) : (
                  <select 
                    {...register("project", { required: true })}
                    id="project-selection"
                    className="select" 
                    onChange={e => handleProjectSelection(e.target.value)} 
                    value={selectedProject?.id || ''}>
                      <option value="">Project selection...</option>
                      {selectedWorkspace.projects.map(project => (<option key={project.id} value={project.id}>{project.name}</option>))}
                  </select>)
                }
                {errors.project && <span  className="error-label">This field is required</span>}
              </div>
              <div className="col-md-6 col-sm-12">
                <div className="d-flex justify-content-between">
                  <div className="input-label">Worktype</div>
                  <div className="required-label">Required</div>
                </div>
                <select 
                  {...register("worktype", { required: true })}
                  id="worktype-selection"
                  className="select"
                  disabled={!selectedProject}
                  value={selectedWorktype?.id || ''}
                  onChange={e => handleWorkstypeSelection(e.target.value)}>
                {
                  !selectedProject ? (<option value="">Project needs to be selected...</option>) : 
                  (<>
                    <option value="">Select worktype...</option>
                    {selectedProject.worktypes.map(worktype => (<option value={worktype.id}>{worktype.name}</option>))}
                  </>)
                }
                </select>
                {errors.worktype && <span className="error-label">This field is required</span>}
              </div>
            </div>
            <div className="row controls-row">
              <div className="col-md-6 col-sm-12">
                <div className="d-flex justify-content-between">
                  <div className="input-label">Detailed description</div>
                  <div className="optional-label">Optional</div>
                </div>
                <textarea 
                  {...register("workDescription", {required: false})}
                  className="textarea"
                  rows={4}
                  placeholder="Detailed description..."/>
              </div>
              <div className="col-md-6 col-sm-12">
                <div className="input-bottom-spacer">
                  <div className="d-flex justify-content-between">
                    <div className="input-label">Date</div>
                    <div className="required-label">Required</div>
                  </div>
                  <DatePicker
                    className="datepicker"
                    selected={startDate}
                    dateFormat="yyyy-MM-dd"
                    calendarStartDay={1}
                    onChange={(date) => setStartDate(date)}
                  />
                  {errors.startDate && <span className="error-label">This field is required</span>}
                </div>
                <div className="d-flex justify-content-between col-sm-12 col-md-12">
                  <div className="col-sm-12 col-md-5">
                    <div className="d-flex justify-content-between">
                      <div className="input-label">Time spent (hours)</div>
                      <div className="required-label">Required</div>
                    </div>
                    <input className="form-control" {...register("hourAmount", { required: true })} placeholder="1"></input>
                    {errors.hours && <span className="error-label">This field is required</span>}
                  </div>
                  <div className="col-sm-12 col-md-5">
                    <div className="d-flex justify-content-between">
                      <div className="input-label">Time spent (minutes)</div>
                      <div className="required-label">Required</div>
                    </div>
                    <input className="form-control" {...register("minuteAmount", { required: true })} placeholder="30"></input>
                    {errors.minutes && <span className="error-label">This field is required</span>}
                  </div>
                </div>
              </div>
            </div>
            <div className="row controls-row">
              <div className="col-md-12 d-flex flex-row-reverse ">
                <button type="submit" className="btn btn-success">Save</button>
                <button onClick={() => deleteTimeLogEntry(timeEntry.timeLogEntryId)} type="button" className="btn btn-danger mx-2">Delete</button>
              </div>
            </div>
        </div>}
      </div>
    </form>
  );
};

export default TimeLoggingHistoryItem;