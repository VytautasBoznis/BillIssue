import { useParams } from "react-router-dom";
import { useError } from "../../utils/errorHandling/ErrorProvider";
import PlusIcon from '../icons/PlusIcon';
import ProjectService from "../../services/ProjectService";
import { resolveUserProjectRoleName } from '../../utils/roleUtils';
import { useState, useEffect } from "react";
import UserAddModal from './userAddModal/UserAddModal';
import UserEditModal from './userEditModal/UserEditModal';
import WorkspaceService from "../../services/WorkspaceService";
import { PROJECT_USER_ROLE_ENUM, PROJECT_USER_ROLES } from "../../services/constants/ProjectConstants";

const ProjectUserManagementContainer = () => {
  const { projectid } = useParams();
  const { showError } = useError();

  const [loading, setLoading] = useState(true);
  const [projectDetails, setProjectDetails] = useState(null);
  const [modalLoading, setModalLoading] = useState(true);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);

  const [workspaceUserData, setWorkspaceUserData] = useState(null);
  const [editUserData, setEditUserData] = useState(null);

  const projectLink = `/project/${projectid}`;

  const loadProjectDetails = async () => {
    setLoading(true);
    const projectResult = await ProjectService.getProjectById(projectid, showError);
    setProjectDetails(projectResult);
    setLoading(false);
  }

  const handleAddUserClick = async () => {
    setShowAddModal(true);
    loadWorkspaceUsers();
  }

  const hadleUserAddModalClose = async (selectedUser) => {
    if(selectedUser){
      setLoading(true);
      if (await ProjectService.createUserAssignmentToProject({
        ProjectId: projectid,
        UserId: selectedUser.userId,
        Role: PROJECT_USER_ROLE_ENUM.Contributor,
      }, showError))
      window.location.reload();
    }
    setShowAddModal(false);
  }

  const loadWorkspaceUsers = async () => {
    setModalLoading(true);
    const workspaceUserData = await WorkspaceService.getAllWorkspaceUsers(projectDetails.workspaceId, showError);
    setWorkspaceUserData(workspaceUserData);
    setModalLoading(false);
  }

  const deleteUserAssingnmet = async (userAssingmentId) => {
    if(userAssingmentId){
      setLoading(true);
      if (await ProjectService.removeUserAssingnmentFromProject({
        projectId: projectid,
        projectUserAssignmentId: userAssingmentId
      }, showError))
      window.location.reload();
    }
  }

  const handleEditUserClick = async (userId) => {
    const selectedUser = projectDetails.projectUserAssignments?.find(userData => userData.userId === userId);
    setEditUserData(selectedUser);
    setShowEditModal(true);
  }

  const handleEditModalClose = async (result) => {
    if (result && result.confirmed && result.newRole > 0) {
      if (await ProjectService.modifyUserAssingment({
        projectId: projectid,
        projectUserAssignmentId: editUserData.userAssignmentId,
        role: parseInt(result.newRole)
      }, showError))
      window.location.reload();
    }
   setShowEditModal(false);
  }

  useEffect(() => {
    loadProjectDetails();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  return (
    <>
      <div className='content-container'>
      {loading ? <>LOADING</> : (
        <div>
          <div className='row control-row '>
            <div className='breadcrumbs col-sm-6'>
              <a className="breadcrumb-link" href='/project-management'>Project management</a> / 
              <a className="breadcrumb-link" href={projectLink}>{projectDetails.name}</a> / 
              User management
            </div>
            <div className='col-sm-6 d-flex justify-content-end'>
              <button onClick={() => handleAddUserClick()} type="submit" className="btn btn-success"><PlusIcon></PlusIcon>Add User</button>
            </div>
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
                {projectDetails.projectUserAssignments.map((user) => (
                    <tr>
                        <td>{user.email}</td>
                        <td>{user.firstName}</td>
                        <td>{user.lastName}</td>
                        <td>{resolveUserProjectRoleName(user.role)}</td>
                        <td>
                          <button onClick={() => deleteUserAssingnmet(user.userAssignmentId)} type="submit" className="btn btn-danger">Delete</button>
                          <button onClick={() => handleEditUserClick(user.userId)} type="submit" className="btn btn-warning">Edit</button>
                        </td>
                    </tr>
                ))}
            </tbody>
            </table>
        </div>
      )}
        <UserAddModal
          show={showAddModal}
          onHide={hadleUserAddModalClose}
          loading={modalLoading}
          workspaceUserData={workspaceUserData}
        />
        <UserEditModal
          show={showEditModal}
          onHide={handleEditModalClose}
          user={editUserData}
          ></UserEditModal>
      </div>
    </>
  )
}

export default ProjectUserManagementContainer;