import React from 'react';
import './ClientEditContainer.css';
import ClientProjectItem from './clientProjectItem/ClientProjectItem';

const ClientEditContainer = () => {

  const clientData = {
    id: 'asdasd',
    name: 'Test client',
    colorCode: '#eb4034',
    defaultBillingPrice: 0.5,
    comments: 'Test',
    projects: [
      {
        id: 'asdxzc',
        clientId: 'asdasdaszxcxcqwer',
        name: 'Project 1',
        colorCode: '#e28743',
        isBilled: true,
        billingPrice: 0.0,
      },
      {
        id: 'zxcasd',
        clientId: 'asdasdasdasd',
        name: 'Project 2',
        colorCode: '#76b5c5',
        isBilled: true,
        billingPrice: 15.0,
      }
    ]
  };

  return <div className="task-entry-container">
    <h2 className="history-title">Client</h2>
    <div className='row row-padded-top'>
      <div className='col-sm-8'>
        <input
            type="text"
            className="form-control"
            placeholder="Name"
            onChange={(e) => console.log(e)}
            value={clientData.name}/>
      </div>
      <div className='col-sm-2'>
        <input
            type="text"
            className="form-control"
            placeholder="Default billing rate"
            onChange={(e) => console.log(e)}
            value={clientData.defaultBillingPrice}/>
      </div>
      <div className='col-sm-2'>
        <input
            type="text"
            className="form-control"
            placeholder="Color code"
            onChange={(e) => console.log(e)}
            value={clientData.colorCode}/>
      </div>
    </div>
    <div className='row'>
      <div className='col-sm-12 row-padded-top'>
        <textarea className='form-control' placeholder="Additional comments" value={clientData.comments}></textarea>
      </div>
    </div>
    <div className='row row-padded-top'>
      <div className="col-sm-2 btn-group client-buttons">
        <div className="btn btn-primary margin-left-button" onClick={console.log('test')}>Save</div>
        <div className="btn btn-outline-danger">Cancel</div>
      </div>
    </div>
    <div className='row row-padded-top'>
      <h3 className="history-title col-sm-10">Client projects</h3>
      <div className="btn btn-primary col-sm-2">Add Project</div>
    </div>
    <div className='row row-padded-top'>
      {clientData.projects.map(item => (
        <ClientProjectItem item={item}/>
      ))}
    </div>
  </div>
}

export default ClientEditContainer;