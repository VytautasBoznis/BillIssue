import './Header.css';
import MenuIcon from './MenuIcon';
import UserIcon from '../../icons/UserIcon';
import Dropdown from 'react-bootstrap/Dropdown';
import NotificationsOverlay from '../../notificationsOverlay/NotificationsOverlay';
import XIcon from '../../icons/XIcon';
import { getUserSessionData, logout } from '../../../utils/sessionUtils';
import { useWorkspace } from '../../../utils/workspaceHandling/WorkspaceProvider';
import { forEach } from 'lodash';

const Header = ({setIsOpen, sidebarOpen}) => {
  const { availableWorkspaces, setSelectedWorkspace, workspaceLoading } = useWorkspace();

  const handleWorkspaceChange = (event) => {
    let selectedWorkspace = null;
    
    forEach(availableWorkspaces, aw => {
      aw.selected = false;

      if (aw.id === event.target.value) {
        selectedWorkspace = aw;
        aw.selected = true;
      }
    });

    setSelectedWorkspace(selectedWorkspace || {});
  };

  const userSessionData = getUserSessionData();

  return (
    <header className='header-content app-header'>
      <div className='logo-container' href="#">
          Bill Issue
      </div>
      <div className='menu-button-container'>
        <div className="mb-4 menu-button" onClick={() => setIsOpen(!sidebarOpen)}>
            {sidebarOpen && <XIcon/>}
            {!sidebarOpen && <MenuIcon/>}
        </div>
      </div>
      <div className='d-flex flex-row-reverse dropdown-container'>
        <Dropdown className='dropdown-button'>
          <Dropdown.Toggle className='stilized-dropdown'>
            <UserIcon/>
            <span className='name-holder'>{`${userSessionData.firstName || ''} ${userSessionData.lastName || ''}`}</span >
          </Dropdown.Toggle>
          <Dropdown.Menu>
            <Dropdown.Item href='/'>Time tracking</Dropdown.Item>
            <Dropdown.Item onClick={() => logout(true)}>Logout</Dropdown.Item>
          </Dropdown.Menu>
        </Dropdown>
        <NotificationsOverlay></NotificationsOverlay>
        {/* TODO: Add normal spinner for workspace load */}
        {workspaceLoading ? (<div>LOADING</div>) : (
          <select className="select" onChange={handleWorkspaceChange}>
            {availableWorkspaces.map(workspace => (<option key={workspace.id} value={workspace.id}>{workspace.name}</option>))}
          </select>)
        }
      </div>
    </header>
  );
}

export default Header;
