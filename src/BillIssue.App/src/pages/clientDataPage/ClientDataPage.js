import React, { useEffect, useState } from 'react';
import { ClientEditContainer } from '../../components';
import { useLocation } from 'react-router-dom';

const ClientDataPage = () => {
  const [clientData, setClientData] = useState([]);
  const { state } = useLocation();

  useEffect(() => {
    if (state) {
      setClientData(state);
      console.log('data', state);
    }
  }, []);

  return (
    <>
      Client Id: {clientData.id }
      <ClientEditContainer />
    </>
  )
}

export default ClientDataPage;