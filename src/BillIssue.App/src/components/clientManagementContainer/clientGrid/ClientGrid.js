import React, { useState } from 'react';
import ClientDataRow from '../clientDataRow/ClientDataRow';
import './ClientGrid.css';

const ClientGrid = () => {

  const clientList = [
    {
      id: 'asdasd',
      name: 'Test client',
      colorCode: '#eb4034',
      defaultBillingPrice: 0.0,
      projects: [
        {
          id: 'asdxzc',
          name: 'Project 1',
          colorCode: '#e28743',
          isBilled: true,
          billingPrice: 0.0,
        },
        {
          id: 'zxcasd',
          name: 'Project 2',
          colorCode: '#76b5c5',
          isBilled: true,
          billingPrice: 15.0,
        }
      ]
    },
    {
      id: 'qwe',
      name: 'Test client 2',
      colorCode: '#eeeee4',
      defaultBillingPrice: 0.0,
      projects: [
        {
          id: 'asd',
          name: 'Project 1',
          colorCode: '#abdbe3',
          isBilled: true,
          billingPrice: 0.0,
        },
        {
          id: 'xcv',
          name: 'Project 2',
          colorCode: '#063970',
          isBilled: true,
          billingPrice: 15.0,
        }
      ]
    }
  ];

  return <div className="container">
    <div className="row client-header">
      <h3 className="col-sm-10">Clients</h3>
      <div className="btn btn-primary col-sm-2">Add Client</div>
    </div>
    {clientList.map(client => (
      <ClientDataRow item={client}/>
    ))}
  </div>
}

export default ClientGrid;