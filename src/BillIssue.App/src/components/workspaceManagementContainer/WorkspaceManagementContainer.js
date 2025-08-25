import React from 'react';
import { useError } from '../../utils/errorHandling/ErrorProvider';

import './WorkspaceManagementContainer.css';
import { useEffect, useState } from 'react';
import WorkspaceService from '../../services/WorkspaceService';
import PlusIcon from '../icons/PlusIcon';
import { resolveUserWorkspaceRoleName } from '../../utils/roleUtils';

const WorkspaceManagementContainer = () => {
  const { showError } = useError();

  const [workspaceData, setWorkspaceData] = useState([]);
  const [loading, setLoading] = useState(true);

  const loadWorkspaceData = async () => {
    setLoading(true);
    const workspaceResult = await WorkspaceService.loadWorkspacesForUser(showError);
    setWorkspaceData(workspaceResult);
    setLoading(false);
  };

  const handleAddWorkspaceClick = () => {
    window.location.href = "/workspace-creation";
  };

  const handleWorkspaceClick = (workspaceId) => {
    window.location.href = `/workspace/${workspaceId}`;
  }

  useEffect(() => {
    loadWorkspaceData();
  }, [])

  return (
    <>
      <div className='content-container'>
        <div className='row control-row '>
          <div className='breadcrumbs col-sm-6'>
            Workspace management
          </div>
          <div className='col-sm-6 d-flex justify-content-end'>
            <button onClick={() => handleAddWorkspaceClick()} type="submit" className="btn btn-success"><PlusIcon></PlusIcon>Add workspace</button>
          </div>
        </div>
        <hr className='col-sm-12 p-1'/>
        {loading ? <>LOADING</> : workspaceData?.length > 0 ? (
          <table className='col-sm-12 styled-table'>
            <thead>
              <tr>
                <td>Name</td> 
                <td>Description</td>
                <td>Role in workspace</td>
                <td>Is deleted</td>
              </tr>
            </thead>
            <tbody>
              {workspaceData.map((workspace) => (
                <tr key={workspace.id} onClick={() => handleWorkspaceClick(workspace.id)}>
                  <td>{workspace.name}</td>
                  <td>{workspace.description}</td>
                  <td>{resolveUserWorkspaceRoleName(workspace.userRole)}</td>
                  <td>{workspace.isDeleted ? 'Yes' : 'No'}</td>
              </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <h4 className="text-center col-sm-12 p-5">No Workspace data</h4>
        )}
      </div>
    </>
  )
}

export default WorkspaceManagementContainer;
