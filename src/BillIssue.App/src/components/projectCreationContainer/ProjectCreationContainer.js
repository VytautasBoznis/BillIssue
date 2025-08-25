import { useForm } from "react-hook-form";
import { useError } from "../../utils/errorHandling/ErrorProvider";
import ProjectService from "../../services/ProjectService";
import { useWorkspace } from "../../utils/workspaceHandling/WorkspaceProvider";

const ProjectCreationContainer = () => {
  const { showError } = useError();
  const { selectedWorkspace, workspaceLoading } = useWorkspace();

  const defaultValues = {
    workspaceId: null,
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

  const clearProjectData = () => {
    reset(defaultValues);
  }

  const onSubmit = async (data) => {
    data.workspaceId = selectedWorkspace.id;
    const result = await ProjectService.createProject(data, showError);

    if (result) {
      window.location.href = "/project-management";
    }
  }

  return (
    <>
      { workspaceLoading ? <>LOADING</> : (
        <div className='content-container'>
          <div className='row control-row '>
            <div className='breadcrumbs col-sm-12'>
              <a className="breadcrumb-link" href='/project-management'>Project management</a> / Add Project
            </div>
          </div>
          <hr/>
          <div className="disclaimer">Please note: to be able to log time entries to newly created project a re-login is required</div>
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
              <button onClick={() => clearProjectData()} type="button" className="btn btn-danger mx-2">Clear</button>
            </div>
          </div>
          </form>
        </div>
      )}
    </>
  )
}

export default ProjectCreationContainer;