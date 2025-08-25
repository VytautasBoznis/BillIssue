import { useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import ProjectWorktypeService from "../../services/ProjectWorktypeService";
import { useError } from "../../utils/errorHandling/ErrorProvider";
import { useState, useEffect } from "react";
import XIcon from "../icons/XIcon";

const ProjectWorktypeDetailsContainer = () => {
  const { id, projectid } = useParams();
  const { showError } = useError();
  
  const [projectWorktypeData, setProjectWorktypeData] = useState(null);
  const [loading, setLoading] = useState(true);

  const defaultValues = {
    name: null,
    description: null,
    isBillable: false,
  }

  const projectLink = `/project/${projectid}`;

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm({
    defaultValues
  });

  const loadProjectWorktypeData = async () => {
    setLoading(true);
    const projectWorktypeResult = await ProjectWorktypeService.getProjectById(id, showError);
    setProjectWorktypeData(projectWorktypeResult);
    reset(projectWorktypeResult);
    setLoading(false);
  };

  const resetProjectWorktypeData = () => {
    reset(projectWorktypeData);
  }

  const handleProjectWorktypeDeleteClick = async () => {
    const result = await ProjectWorktypeService.removeProjectWorktype(projectid, id, showError);

    if (result) {
      window.location.href = projectLink;
    }
  }

  useEffect(() => {
      loadProjectWorktypeData();
      // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  const onSubmit = async (data) => {
    const result = await ProjectWorktypeService.modifyProjectWorktype({
      projectId: projectid,
      projectWorktypeId: id,
      name: data.name,
      description: data.description,
      isBillable: data.isBillable,
    }, showError);

    if (result) {
      window.location.href = projectLink;
    }
  }

  return (
    <>
      <div className='content-container'>
      {loading ? <>LOADING</> : (
        <div>
          <div className='row control-row '>
            <div className='breadcrumbs col-sm-6'>
              <a className="breadcrumb-link" href='/project-management'>Project management</a> / <a className="breadcrumb-link" href={projectLink}>{projectWorktypeData.projectName}</a> / {projectWorktypeData.name}
            </div>
            <div className='col-sm-6 d-flex justify-content-end'>
              <button onClick={() => handleProjectWorktypeDeleteClick()} type="submit" className="btn btn-danger"><XIcon size={12} classProperty="delete-button-icon"></XIcon> Delete project worktype</button>
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
              <input className="form-control" {...register("name", { required: true })} placeholder="Project worktype name..."></input>
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
            <div className="col-sm-3">
              <div className="d-flex justify-content-between label-more-padding">
                <div className="input-label">Is Billable</div>
              </div>
              <input
                {...register("isBillable", {required: false})}
                type="checkbox"/>
            </div>
          </div>
          <div className="row controls-row">
            <div className="col-md-12 d-flex flex-row-reverse">
              <button type="submit" className="btn btn-success">Save</button>
              <button onClick={() => resetProjectWorktypeData()} type="button" className="btn btn-primary mx-2">Reset</button>
            </div>
          </div>
          </form>
        </div>
      )}
      </div>
    </>
  )
}

export default ProjectWorktypeDetailsContainer;