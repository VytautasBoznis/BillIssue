import React from 'react';
import { Outlet } from 'react-router-dom';

import "./AnonymousLayout.css";

const AnonymousLayout = () => {
  return (
    <div className="anonymous-layout">
      <Outlet />
    </div>
  )
}

export default AnonymousLayout;