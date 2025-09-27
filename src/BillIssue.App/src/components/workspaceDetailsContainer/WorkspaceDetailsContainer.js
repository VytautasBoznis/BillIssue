import { useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import WorkspaceService from "../../services/WorkspaceService";
import { useError } from "../../utils/errorHandling/ErrorProvider";
import { useState, useEffect } from "react";
import XIcon from "../icons/XIcon";

import "./WorkspaceDetailsContainer.css";
import UserIcon from "../icons/UserIcon";

const WorkspaceDetailsContainer = () => {
  const { id } = useParams();
  const [workspaceDetails, setWorkspaceDetails] = useState(null);
  const [loading, setLoading] = useState(true);

  const { showError } = useError();

  const defaultValues = {
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

  const resetWorkspaceData = () => {
    reset(workspaceDetails);
  }

  const loadWorkspaceDetails = async () => {
    setLoading(true);
    const workspaceResult = await WorkspaceService.getWorkspaceById(id, false, showError);
    setWorkspaceDetails(workspaceResult);
    reset(workspaceResult);
    setLoading(false);
  }

  const handleWorkspaceDeleteClick = async () => {
    const result = await WorkspaceService.removeWorkspace(id, showError);

    if (result) {
      window.location.href = "/workspace-management";
    }
  }

  const handleProjectUsersClick = () => {
    window.location.href = `/workspace/${id}/workspace-users`;
  }

  useEffect(() => {
    loadWorkspaceDetails();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  const onSubmit = async (data) => {
    const result = await WorkspaceService.updateWorkspace({
      workspaceId: workspaceDetails.id,
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
              <a className="breadcrumb-link" href='/workspace-management'>Workspace management</a> / {workspaceDetails.name}
            </div>
            <div className='col-sm-6 d-flex justify-content-end'>
              <button onClick={() => handleProjectUsersClick()} type="submit" className="color-white btn btn-primary"><UserIcon size={12} fill="#fff" classProperty="user-management-button-icon"></UserIcon> User Management</button>
              <button onClick={() => handleWorkspaceDeleteClick()} type="submit" className="btn btn-danger spacer-left"><XIcon size={12} classProperty="delete-button-icon"></XIcon> Delete workspace</button>
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
              <input className="form-control" {...register("name", { required: true })} placeholder="Workspace name..."></input>
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
              <button onClick={() => resetWorkspaceData()} type="button" className="btn btn-primary mx-2">Reset</button>
            </div>
          </div>
          </form>
        </div>
      )}
      </div>
    </>
  )
}

export default WorkspaceDetailsContainer;