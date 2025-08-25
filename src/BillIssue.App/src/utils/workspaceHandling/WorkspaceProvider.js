import React, { createContext, useContext, useEffect, useState } from 'react';
import WorkspaceService from '../../services/WorkspaceService';
import { useError } from '../errorHandling/ErrorProvider';

// Create a context for the workspace state
const WorkspaceContext = createContext();

export const WorkspaceProvider = ({ children }) => {
  const { showError } = useError();

  const [selectedWorkspace, setSelectedWorkspace] = useState(null);
  const [availableWorkspaces, setAvailableWorkspaces] = useState([]);
  const [workspaceLoading, setWorkspaceLoading] = useState(true);

  useEffect(() => {
    const loadAvailableWorkspaces = async () => {
      const loadedWorkspaces = await WorkspaceService.getWorkspaceData(showError);
      setAvailableWorkspaces(loadedWorkspaces);
      setSelectedWorkspace(loadedWorkspaces.length > 0 ? loadedWorkspaces[0] : []);
      setWorkspaceLoading(false);
    };

    loadAvailableWorkspaces();
  }, []);

  return (
    <WorkspaceContext.Provider value={{ selectedWorkspace, availableWorkspaces, setSelectedWorkspace, workspaceLoading }}>
      {children}
    </WorkspaceContext.Provider>
  );
};

export const useWorkspace = () => {
  return useContext(WorkspaceContext);
};
