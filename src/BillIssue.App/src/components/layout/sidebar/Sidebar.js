import { React } from 'react';
import './Sidebar.css';
import SandClockIcon from '../../icons/SandClockIcon';
import WorkspaceIcon from '../../icons/WorkspaceIcon';
import ProjectIcon from '../../icons/ProjectIcon';
import ReportIcon from '../../icons/ReportIcon';

const Sidebar = ({sidebarOpen}) => {

  return (
    <div className={`h-screen ${sidebarOpen ? "extended" : "minimized"} transition-all duration-300 p-2`}>
      <nav className="d-flex flex-column space-y-4">
        <a href="/" className="d-flex items-center space-x-2 p-2 sidebar-menu-button">
          <SandClockIcon className='sidebar-menu-image'></SandClockIcon>
          {sidebarOpen && <div className='sidebar-menu-text'>Time logging</div>}
        </a>
        <a href="/workspace-management" className="d-flex items-center space-x-2 p-2 sidebar-menu-button">
          <WorkspaceIcon className='sidebar-menu-image'></WorkspaceIcon>
          {sidebarOpen && <span className='sidebar-menu-text'>Workspace</span>}
        </a>
        <a href="/project-management" className="d-flex items-center space-x-2 p-2 sidebar-menu-button">
          <ProjectIcon className='sidebar-menu-image'></ProjectIcon>
          {sidebarOpen && <span className='sidebar-menu-text'>Projects</span>}
        </a>
        <a href="/reports" className="d-flex items-center space-x-2 p-2 sidebar-menu-button">
          <ReportIcon className='sidebar-menu-image'></ReportIcon>
          {sidebarOpen && <span className='sidebar-menu-text'>Reports</span>}
        </a>
      </nav>
    </div>
  );
}

export default Sidebar;