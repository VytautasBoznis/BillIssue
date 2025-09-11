import { Modal } from 'react-bootstrap';
import { resolveUserProjectRoleName } from '../../../utils/roleUtils';
import { useState } from 'react';
import { PROJECT_USER_ROLES } from '../../../services/constants/ProjectConstants';

const UserEditModal = (props) => {
  const [newRole, setNewRole] = useState(props?.user?.role);

  const handleNewRoleSelection = (newRole) => {
    setNewRole(newRole);
  }

  return (
    <Modal
      {...props}
      size="lg"
      centered
    >
      <Modal.Header closeButton>
        <Modal.Title id="contained-modal-title-vcenter">
          Edit user role in project
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
          <>
            <div>User email: {props?.user?.email} </div>
            <div>Current role: {resolveUserProjectRoleName(props?.user?.role)}</div>
            <div>Change user role <select
              id="new-role-selection"
              className="select" 
              onChange={(e) => handleNewRoleSelection(e.target.value)} 
              value={newRole || ''}>
                <option value="">New project role...</option>
                {PROJECT_USER_ROLES.map(newRoles => (<option key={newRoles.id} value={newRoles.id}>{newRoles.name}</option>))}
            </select></div>
          </>
      </Modal.Body>
      <Modal.Footer>
        <button onClick={() => props.onHide({ confirmed: true, newRole: newRole })} type="submit" className="btn btn-success">Save</button>
        <button onClick={() => props.onHide({ confirmed: false, newRole: -1 })} type="submit" className="btn btn-danger">Close</button>
      </Modal.Footer>
    </Modal>
  );
}

export default UserEditModal;
