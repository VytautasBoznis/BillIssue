import { useForm } from "react-hook-form";
import WorkspaceService from "../../services/WorkspaceService";
import { useError } from "../../utils/errorHandling/ErrorProvider";

const WorkspaceCreationContainer = () => {
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

  const clearWorkspaceData = () => {
    reset(defaultValues);
  }

  const onSubmit = async (data) => {
    const result = await WorkspaceService.createWorkspace(data, showError);

    if (result) {
      window.location.href = "/workspace-management";
    }
  }

  return (
    <>
      <div className='content-container'>
        <div className='row control-row '>
          <div className='breadcrumbs col-sm-12'>
            <a className="breadcrumb-link" href='/workspace-management'>Workspace management</a> / Add workspace
          </div>
        </div>
        <hr/>
        <div className="disclaimer">Please note: to be able to switch to a newly created workspace a re-login is required</div>
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
            <button onClick={() => clearWorkspaceData()} type="button" className="btn btn-danger mx-2">Clear</button>
          </div>
        </div>
        </form>
      </div>
    </>
  )
}

export default WorkspaceCreationContainer;