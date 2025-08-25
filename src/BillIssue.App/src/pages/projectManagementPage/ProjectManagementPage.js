import React, { useEffect, useState } from 'react';
import { ProjectEditContainer } from '../../components';
import { useLocation } from 'react-router-dom';

const ProjectManagementPage = () => {
  const [projectData, setProjectData] = useState([]);
  const { state } = useLocation();

  useEffect(() => {
    if (state) {
      setProjectData(state);
      console.log('data', state);
    }
  }, []);

  return (
  <>
    <ProjectEditContainer />
  </>
  )
}

export default ProjectManagementPage;