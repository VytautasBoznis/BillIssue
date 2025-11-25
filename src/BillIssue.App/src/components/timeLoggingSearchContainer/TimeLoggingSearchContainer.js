import { useState } from "react";
import { useForm } from "react-hook-form";
import DatePicker from "react-datepicker";
import { useWorkspace } from "../../utils/workspaceHandling/WorkspaceProvider";
import { dateToTimestamp, getSecondsToHoursAndMinutesDisplay, timestampToDate } from "../../utils/timeFormatUtils";
import TimeLoggingHistoryItem from "../shared/TimeLoggingHistoryItem/TimeLoggingHistoryItem";
import TimeLoggingService from "../../services/TimeLoggingService";
import { useError } from "../../utils/errorHandling/ErrorProvider";
import { useSuccess } from "../../utils/successHandling/SuccessProvider";

const TimeLoggingSearchContainer = () => {
    
    const { showError } = useError();
    const { showSuccess } = useSuccess(); 

    const { selectedWorkspace, workspaceLoading } = useWorkspace();
    const [selectedProject, setSelectedProject] = useState(null);
    const [selectedWorktype, setSelectedWorktype] = useState(null);
    const [dateFrom, setDateFrom] = useState(new Date());
    const [dateTo, setDateTo] = useState(new Date());

    const [searching, setSearching] = useState(false);
    const [searchResult, setSearchResult] = useState([]);

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm({});

    const handleProjectSelection = (projectId) => {
        const selection = selectedWorkspace.projects.find(p => p.id === projectId);
        setSelectedProject(selection);
        setSelectedWorktype(null);
    };

    const handleWorkstypeSelection = (worktypeId) => {
        const selection = selectedProject.worktypes.find(w => w.id === worktypeId);
        setSelectedWorktype(selection);
    };

    const handleEditTimeEntry = async (timeEntry) => {
        await TimeLoggingService.editTimeEntry(timeEntry, showError);
        showSuccess("Entry edited successfully!")
        handleSubmit();
    }

    const handleDeleteTimeEntry = async (timeEntryId) => {
        await TimeLoggingService.deleteTimeEntry(timeEntryId, showError);
        showSuccess("Entry deleted successfully!")
        handleSubmit();
    }

    const onSubmit = async (data) => {
        setSearching(true);

        var searchRequest = {
            SearchContent: data.title, 
            workspaceId: selectedWorkspace.id,
            projectId: selectedProject ? selectedProject?.id : null,
            projectWorktypeId: selectedWorktype ? selectedWorktype?.id : null,
            startDate: dateToTimestamp(dateFrom),
            endDate: dateToTimestamp(dateTo),
        };

        var result = await TimeLoggingService.searchTimeLoggingEntries(searchRequest, showError);

        setSearchResult(result);
        setSearching(false);
    }

    return (
        <>
            <div className='content-container'>
                <form onSubmit={handleSubmit(onSubmit)}>
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
                        <div className="col-md-3 col-sm-12">
                            <div className="d-flex justify-content-between">
                                <div className="input-label">Project</div>
                            </div>
                            {/*TODO: add proper loading handling*/}
                            {workspaceLoading ? (<div>LOADING</div>) : (
                                <select 
                                id="project-selection"
                                className="select" 
                                onChange={(e) => handleProjectSelection(e.target.value)} 
                                value={selectedProject?.id || ''}>
                                    <option value="">Project selection...</option>
                                    {selectedWorkspace.projects.map(project => (<option key={project.id} value={project.id}>{project.name}</option>))}
                                </select>)
                            }
                        </div>
                        <div className="col-md-3 col-sm-12">
                            <div className="d-flex justify-content-between">
                                <div className="input-label">Worktype</div>
                            </div>
                            <select className="select" disabled={!selectedProject} onChange={(e) => handleWorkstypeSelection(e.target.value)} id="worktype-selection">
                                {
                                !selectedProject ? (<option value="">Project needs to be selected...</option>) : 
                                (<>
                                    <option value="">Select worktype...</option>
                                    {selectedProject.worktypes.map(worktype => (<option value={worktype.id}>{worktype.name}</option>))}
                                </>)
                                }
                            </select>
                        </div>
                        <div className="col-md-3 col-sm-12 input-bottom-spacer">
                            <div className="d-flex justify-content-between">
                                <div className="input-label">Date From</div>
                            </div>
                            <DatePicker
                                className="datepicker"
                                selected={dateFrom}
                                dateFormat="yyyy-MM-dd"
                                calendarStartDay={1}
                                onChange={(date) => setDateFrom(date)}
                            />
                        </div>
                        <div className="col-md-3 col-sm-12 input-bottom-spacer">
                            <div className="d-flex justify-content-between">
                                <div className="input-label">Date To</div>
                            </div>
                            <DatePicker
                                className="datepicker"
                                selected={dateTo}
                                dateFormat="yyyy-MM-dd"
                                calendarStartDay={1}
                                onChange={(date) => setDateTo(date)}
                            />
                        </div>
                    </div>
                    <div className="row controls-row">
                        <div className="col-md-12 d-flex flex-row-reverse">
                            <button type="submit" className="btn btn-success">Search</button>
                        </div>
                    </div>
                </form>
                <hr className='col-sm-12 p-1'/>
                <div>
                    {searching ? (<div>Searching</div>) : (
                        <>
                            {searchResult.length > 0 ? searchResult?.map(entry => (
                                <TimeLoggingHistoryItem key={entry.timeLogEntryId} timeEntry={entry} editTimeLogEntry={handleEditTimeEntry} deleteTimeLogEntry={handleDeleteTimeEntry} />
                            )) : <h4 className="text-center p-5">No time logging entries to display</h4>}
                        </>
                    )}
                    
                </div>
            </div>
        </>
    )
}

export default TimeLoggingSearchContainer;