import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './ClientDataRow.css';

const ClientDataRow = (item) => {
  const [client] = useState(item.item);
  const navigate = useNavigate();

  const navigateToView = () => {
    navigate('/client-data', { state: { id: client.id } });
  }

  return <div className="container client-container">
    <div className="row client-data-row">
      <div className="col-sm-9 client-name">{client.name}</div>
      <div className="col-sm-2 btn-group client-buttons">
        <div className="btn btn-primary margin-left-button" onClick={navigateToView}>View</div>
        <div className="btn btn-outline-danger">Delete</div>
      </div>
    </div>
  </div>
}

export default ClientDataRow;