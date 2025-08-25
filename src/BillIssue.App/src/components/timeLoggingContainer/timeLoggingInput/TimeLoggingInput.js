import { useEffect, useState } from "react";
import { useWorkspace } from '../../../utils/workspaceHandling/WorkspaceProvider';
import { useForm } from "react-hook-form";
import DatePicker from "react-datepicker";
import "./TimeLoggingInput.css"
import "react-datepicker/dist/react-datepicker.css";
import { dateToTimestamp } from "../../../utils/timeFormatUtils";

const TimeLoggingInput = ({addTimeLogEntry}) => {

  const { selectedWorkspace, workspaceLoading } = useWorkspace();

  const [startDate, setStartDate] = useState(new Date());
  const [selectedProject, setSelectedProject] = useState(null);
  const [selectedWorktype, setSelectedWorktype] = useState(null);

  const defaultValues = {
    workspaceId: selectedWorkspace.id,
    projectId: null,
    projectWorktypeId: null,
    title: null,
    logDate: dateToTimestamp(new Date()),
    hourAmount: null,
    minuteAmount: null,
    workDescription: null
  }

  useEffect(() => {
    setSelectedProject(null);
    setSelectedWorktype(null);
  }, [selectedWorkspace]);

  const handleProjectSelection = (projectId) => {
    const selection = selectedWorkspace.projects.find(p => p.id === projectId);
    setSelectedProject(selection);
    setSelectedWorktype(null);
  };

  const handleWorkstypeSelection = (worktypeId) => {
    const selection = selectedProject.worktypes.find(w => w.id === worktypeId);
    setSelectedWorktype(selection);
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setError,
  } = useForm({
    defaultValues
  });

  const clearTimeLoggingEntry = () => {
    setStartDate(new Date());
    setSelectedProject(null);
    setSelectedWorktype(null);
    reset(defaultValues);
  }

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
      WorkspaceId: selectedWorkspace.id,
      ProjectId: selectedProject.id,
      ProjectWorktypeId: selectedWorktype.id,
      Title: data.title,
      LogDate: dateToTimestamp(startDate),
      HourAmount: data.hourAmount,
      MinuteAmount: data.minuteAmount,
      WorkDescription: data.workDescription
    };

    await addTimeLogEntry(timelogEntry);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      {/* <div className="controls-row">
        <div className="row">
          <label className="control-label input-label">Time entry mode</label>
        </div>
        <label className="radio inline right-spacer">
          <input type="radio" id="manual-entry-mode" name="manual-entry-mode" value="true" /> Manual
        </label>
        <label className="radio inline">
          <input type="radio" id="manual-entry-mode" name="manual-entry-mode" value="false" /> Timer
        </label>
      </div> */}
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
              onChange={(e) => handleProjectSelection(e.target.value)} 
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
          <select {...register("worktype", { required: true })} className="select" disabled={!selectedProject} onChange={(e) => handleWorkstypeSelection(e.target.value)} id="worktype-selection">
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
              <input className="form-control" {...register("hourAmount", { required: true })} placeholder="1..."></input>
              {errors.hours && <span className="error-label">This field is required</span>}
            </div>
            <div className="col-sm-12 col-md-5">
              <div className="d-flex justify-content-between">
                <div className="input-label">Time spent (minutes)</div>
                <div className="required-label">Required</div>
              </div>
              <input className="form-control" {...register("minuteAmount", { required: true })} placeholder="30..."></input>
              {errors.minutes && <span className="error-label">This field is required</span>}
            </div>
          </div>
        </div>
      </div>
      <div className="row controls-row">
        <div className="col-md-12 d-flex flex-row-reverse">
          <button type="submit" className="btn btn-success">Save</button>
          <button onClick={() => clearTimeLoggingEntry()} type="button" className="btn btn-danger mx-2">Clear</button>
        </div>
      </div>
    </form>
  );
}

export default TimeLoggingInput;