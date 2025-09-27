import { useError } from "../../utils/errorHandling/ErrorProvider";
import { useEffect, useState } from "react";
import { resolveUserWorkspaceRoleName } from "../../utils/roleUtils";
import { useParams } from "react-router-dom";

import PlusIcon from "../icons/PlusIcon";
import WorkspaceService from "../../services/WorkspaceService";
import './WorkspaceUserManagementContainer.css';
import WorkspaceUserAddModal from "./workspaceUserAddModal/workspaceUserAddModal";
import WorkspaceUserEditModal from "./workspaceUserEditModal/workspaceUserEditModal";

const WorkspaceUserManagementContainer = () => {
  const { id } = useParams();

  const [workspaceDetails, setWorkspaceDetails] = useState(null);
  const [editUserData, setEditUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);

  const { showError } = useError();

  const workspaceLink = `/workspace/${id}`;

  const loadWorkspaceDetails = async () => {
    setLoading(true);
    const workspaceResult = await WorkspaceService.getWorkspaceById(id, true, showError);
    setWorkspaceDetails(workspaceResult);
    setLoading(false);
  }

  const handleAddUserClick = async () => {
    setShowAddModal(true);
  }

  const handleAddUserModalClose = async (email) => {
    setShowAddModal(false);

    if (!email) {
      return;
    }

    if (await WorkspaceService.addWorkspaceUser({
          workspaceId: workspaceDetails.id,
          NewUserEmail: email
        }, showError)) 
    {
      window.location.reload();
    }
  }

  const handleEditUserClick = async (userId) => {
    const selectedUser = workspaceDetails.workspaceUsers?.find(userData => userData.userId === userId);
    setEditUserData(selectedUser);
    setShowEditModal(true);
  }

  const handleEditModalClose = async (result) => {
    if (result && result.confirmed && result.newRole > 0) {
      if (await WorkspaceService.modifyWorkspaceUser({
        workspaceId: id,
        userId: editUserData.userId,
        newUserRole: parseInt(result.newRole)
      }, showError))
      window.location.reload();
    }
   setShowEditModal(false);
  }

  const deleteUserAssingnmet = async (userId) => {
    if(userId){
      setLoading(true);
      if (await WorkspaceService.removeWorkspaceUser({
        workspaceId: id,
        userId: userId
      }, showError))
      window.location.reload();
    }
  }

  useEffect(() => {
    loadWorkspaceDetails();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  return (
    <>
      <div className='content-container'>
      {loading ? <>LOADING</> : (
        <div>
          <div className='row control-row '>
            <div className='breadcrumbs col-sm-6'>
              <a className="breadcrumb-link" href='/workspace-management'>Workspace management</a> / 
              <a className="breadcrumb-link" href={workspaceLink}>{workspaceDetails.name}</a> / 
              User management
            </div>
            {<div className='col-sm-6 d-flex justify-content-end'>
              <button onClick={() => handleAddUserClick()} type="submit" className="btn btn-success"><PlusIcon></PlusIcon>Add User</button>
            </div>}
          </div>
          <hr className='col-sm-12 p-1'/>
          <table className='col-sm-12 styled-table'>
            <thead>
              <tr>
                <td>Email</td> 
                <td>First name</td>
                <td>Last name</td>
                <td>Role in project</td>
                <td>Actions</td>
              </tr>
            </thead>
            <tbody>
                {workspaceDetails?.workspaceUsers?.map((user) => (
                    <tr>
                        <td>{user.email}</td>
                        <td>{user.firstName}</td>
                        <td>{user.lastName}</td>
                        <td>{resolveUserWorkspaceRoleName(user.role)}</td>
                        <td>
                          <button onClick={() => deleteUserAssingnmet(user.userId)} type="submit" className="btn btn-danger">Delete</button>
                          <button onClick={() => handleEditUserClick(user.userId)} type="submit" className="btn btn-warning">Edit</button> 
                        </td>
                    </tr>
                ))}
            </tbody>
            </table>
        </div>
      )}
        <WorkspaceUserAddModal
          show={showAddModal}
          onHide={handleAddUserModalClose}
        />
        <WorkspaceUserEditModal
          show={showEditModal}
          onHide={handleEditModalClose}
          user={editUserData}
          />
      </div>
    </>
  )
}

export default WorkspaceUserManagementContainer;