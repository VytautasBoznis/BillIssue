import { useEffect, useState } from 'react';
import { Modal } from 'react-bootstrap';

const UserAddModal = (props) => {
  const [selectedUser, setSelectedUser] = useState(null);

  const selectUser = (userId) => {
    if (userId) {
      const selectedUser = props.workspaceUserData?.find(userData => userData.userId === userId);
      setSelectedUser(selectedUser);
    }
  }

  useEffect(() => {
    setSelectedUser(null);
  },[]);

  return (
    <Modal
      {...props}
      size="lg"
      centered
    >
      <Modal.Header closeButton>
        <Modal.Title id="contained-modal-title-vcenter">
          Add user from Workspace
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {props.loading ? <>LOADING</> : (
          <table className='col-sm-12 styled-table'>
            <thead>
              <tr>
                <td>Selected</td>
                <td>Email</td> 
                <td>First name</td>
                <td>Last name</td>
              </tr>
            </thead>
            <tbody>
              {props.workspaceUserData?.map((user) => (
                <tr key={user.userId} onClick={() => selectUser(user.userId)}>
                  <td>{selectedUser?.userId === user.userId ? '✓' : ''}</td>
                  <td>{user.email}</td>
                  <td>{user.firstName}</td>
                  <td>{user.lastName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Modal.Body>
      <Modal.Footer>
        <button onClick={() => props.onHide(selectedUser)} type="submit" className="btn btn-success">Add</button>
        <button onClick={() => props.onHide()} type="submit" className="btn btn-danger">Close</button>
      </Modal.Footer>
    </Modal>
  );
}

export default UserAddModal;
