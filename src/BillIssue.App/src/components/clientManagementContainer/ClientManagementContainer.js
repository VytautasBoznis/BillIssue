import React from 'react';
import './ClientManagementContainer.css';
import ClientGrid from './clientGrid/ClientGrid';

const ClientManagementContainer = () => {

  return <div className="task-entry-container">
    <ClientGrid />
  </div>
}

export default ClientManagementContainer;