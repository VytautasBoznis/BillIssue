import { React, useState } from 'react';
import { Outlet } from 'react-router-dom';
import { WorkspaceProvider } from '../../../utils/workspaceHandling/WorkspaceProvider';
import Header from '../header/Header';
import Sidebar from '../sidebar/Sidebar';

import './MainLayout.css';

const MainLayout = () => {
  const [isOpen, setIsOpen] = useState(true);

  return (
    <WorkspaceProvider>
    <div className='main-container'>
      <Header setIsOpen={setIsOpen} sidebarOpen={isOpen}/>
      <div className='d-flex outlet-container'>
        <div className='sidebar-container'>
          <Sidebar sidebarOpen={isOpen}/>
        </div>
        <div className='h-screen outlet'>
          <Outlet/>
        </div>
      </div>
    </div>
    </WorkspaceProvider>
  )
}

export default MainLayout;