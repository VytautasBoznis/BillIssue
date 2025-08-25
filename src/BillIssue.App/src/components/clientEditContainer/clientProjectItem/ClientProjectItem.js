import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './ClientProjectItem.css';

const ClientProjectItem = (item) => {
  const [project] = useState(item.item);
  const navigate = useNavigate();

  const navigateToView = () => {
    navigate('/project-management', { state: { id: project.id, clientId: project.clientId } });
  }

  return <div className="container client-container">
    <div className="row client-data-row">
      <div className="col-sm-9 client-name">{project.name}</div>
      <div className="col-sm-2 btn-group client-buttons">
        <div className="btn btn-primary margin-left-button" onClick={navigateToView}>View</div>
        <div className="btn btn-outline-danger">Delete</div>
      </div>
    </div>
  </div>
}

export default ClientProjectItem;