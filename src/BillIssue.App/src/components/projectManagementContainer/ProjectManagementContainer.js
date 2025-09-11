import React, { useState, useEffect } from 'react';
import { useError } from '../../utils/errorHandling/ErrorProvider';
import { useWorkspace } from '../../utils/workspaceHandling/WorkspaceProvider';

import './ProjectManagementContainer.css';
import PlusIcon from '../icons/PlusIcon';
import { resolveUserProjectRoleName } from '../../utils/roleUtils';
import ProjectService from '../../services/ProjectService';

const ProjectManagementContainer = () => {
  const { showError } = useError();

  const { selectedWorkspace, workspaceLoading } = useWorkspace();
  const [projectData, setProjectData] = useState(null);
  const [loading, setLoading] = useState(true);

  const loadProjectData = async () => {
    setLoading(true);
    const projectResult = await ProjectService.getUserProjectsInWorkspace(selectedWorkspace.id, showError);
    setProjectData(projectResult);
    setLoading(false);
  };

  const handleAddProjectClick = () => {
    window.location.href = "/project-creation";
  }

  const handleProjectClick = (projectId) => {
    window.location.href = `/project/${projectId}`;
  }
  
  useEffect(() => {
    if (workspaceLoading) {
      return;
    }

    setLoading(true);
    loadProjectData();
    setLoading(false);
  }, [selectedWorkspace]);

  return (<>
    <div className='content-container'>
      <div className='row control-row '>
        <div className='breadcrumbs col-sm-6'>
          Project management
        </div>
        <div className='col-sm-6 d-flex justify-content-end'>
          <button onClick={() => handleAddProjectClick()} type="submit" className="btn btn-success"><PlusIcon></PlusIcon>Add Project</button>
        </div>
      </div>
      <hr className='col-sm-12 p-1'/>
      {loading || workspaceLoading ? <>LOADING</> : projectData?.length > 0 ? (
        <table className='col-sm-12 styled-table'>
          <thead>
            <tr>
              <td>Name</td> 
              <td>Description</td>
              <td>Role in project</td>
              <td>Is deleted</td>
            </tr>
          </thead>
          <tbody>
            {projectData?.map((project) => (
              <tr key={project.projectId} onClick={() => handleProjectClick(project.projectId)}>
                <td>{project.name}</td>
                <td>{project.description}</td>
                <td>{resolveUserProjectRoleName(project.userRole)}</td>
                <td>{project.isDeleted ? 'Yes' : 'No'}</td>
            </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <h4 className="text-center col-sm-12 p-5">No Project data</h4>
      )}
    </div>
  </>);
}

export default ProjectManagementContainer;