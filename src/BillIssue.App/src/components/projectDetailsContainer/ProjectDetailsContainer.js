import { useParams } from "react-router-dom";
import "./ProjectDetailsContainer.css";
import { useEffect, useState } from "react";
import { useError } from "../../utils/errorHandling/ErrorProvider";
import { useForm } from "react-hook-form";
import XIcon from "../icons/XIcon";
import ProjectService from "../../services/ProjectService";

const ProjectDetailsContainer = () => {
  const { id } = useParams();
  const [projectDetails, setProjectDetails] = useState(null);
  const [loading, setLoading] = useState(true);

  const { showError } = useError();

  const defaultValues = {
    workspaceId: null,
    projectId: null,
    name: null,
    description: null,
  }

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm({
    defaultValues
  });

  const resetProjectData = () => {
    reset(projectDetails);
  }

  const loadProjectDetails = async () => {
    setLoading(true);
    const projectResult = await ProjectService.getProjectById(id, showError);
    setProjectDetails(projectResult);
    reset(projectResult);
    setLoading(false);
  }

  const handleWorkspaceDeleteClick = async () => {
    const result = await ProjectService.removeProject(id, showError);

    if (result) {
      window.location.href = "/project-management";
    }
  }
  
  const handleProjectWorktypeClick = (projectWorktypeId) => {
    window.location.href = `/project/${projectDetails.projectId}/project-worktype/${projectWorktypeId}`;
  }

  useEffect(() => {
    loadProjectDetails();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  const onSubmit = async (data) => {
    const result = await ProjectService.modifyProject({
      projectId: projectDetails.projectId,
      name: data.name,
      description: data.description
    }, showError);

    if (result) {
      window.location.reload();
    }
  }

  return (
    <>
      <div className='content-container'>
      {loading ? <>LOADING</> : (
        <div>
          <div className='row control-row '>
            <div className='breadcrumbs col-sm-6'>
              <a className="breadcrumb-link" href='/project-management'>Project management</a> / {projectDetails.name}
            </div>
            <div className='col-sm-6 d-flex justify-content-end'>
              <button onClick={() => handleWorkspaceDeleteClick()} type="submit" className="btn btn-danger"><XIcon size={12} classProperty="delete-button-icon"></XIcon> Delete Project</button>
            </div>
          </div>
          <hr/>
          <form onSubmit={handleSubmit(onSubmit)}>
          <div className="row controls-row">
            <div className="col-sm-12">
              <div className="d-flex justify-content-between">
                <div className="input-label">Name</div>
                <div className="required-label">Required</div>
              </div>
              <input className="form-control" {...register("name", { required: true })} placeholder="Project name..."></input>
              {errors.name && <span className="error-label">This field is required</span>}
            </div>
          </div>
          <div className="row controls-row">
            <div className="col-sm-12">
              <div className="d-flex justify-content-between">
                <div className="input-label">Description</div>
                <div className="optional-label">Optional</div>
              </div>
              <textarea
                {...register("description", {required: false})}
                className="textarea"
                rows={4}
                placeholder="Description..."/>
            </div>
          </div>
          <div className="row controls-row">
            <div className="col-md-12 d-flex flex-row-reverse">
              <button type="submit" className="btn btn-success">Save</button>
              <button onClick={() => resetProjectData()} type="button" className="btn btn-primary mx-2">Reset</button>
            </div>
          </div>
          </form>
          <div className='row control-row '>
            <hr/>
            <div className='breadcrumbs worktype-table-title'>
              Project worktypes
            </div>
            <hr/>
            {projectDetails?.projectWorktypes?.length > 0 ? (
              <table className='col-sm-12 styled-table'>
                <thead>
                  <tr>
                    <td>Name</td> 
                    <td>Description</td>
                    <td>Is Billable</td>
                    <td>Is deleted</td>
                  </tr>
                </thead>
                <tbody>
                  {projectDetails?.projectWorktypes.map((projectWorktype) => (
                    <tr key={projectWorktype.projectWorktypeId} onClick={() => handleProjectWorktypeClick(projectWorktype.projectWorktypeId)}>
                      <td>{projectWorktype.name}</td>
                      <td>{projectWorktype.description}</td>
                      <td>{projectWorktype.isBillable ? 'Yes' : 'No'}</td>
                      <td>{projectWorktype.isDeleted ? 'Yes' : 'No'}</td>
                  </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <h4 className="text-center col-sm-12 p-5">No Project Worktype data</h4>
            )}
          </div>
        </div>
      )}
      </div>
    </>
  )

}

export default ProjectDetailsContainer;