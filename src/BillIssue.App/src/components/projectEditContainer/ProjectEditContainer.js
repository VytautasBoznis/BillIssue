import React from 'react';
import './ProjectEditContainer.css';

const ProjectEditContainer = () => {

  return <div className="task-entry-container">
      <h2 className="history-title">Project</h2>
      <div className='row row-padded-top'>
      <div className='col-sm-9'></div>
      <div className='col-sm-3 '>
        <div className="btn btn-outline-danger pull-right">Back</div>
      </div>
      </div>
      <div className='row row-padded-top'>
        <div className='col-sm-8'>
          <input
              type="text"
              className="form-control"
              placeholder="Name"
              onChange={(e) => console.log(e)}
              value=""/>
        </div>
        <div className='col-sm-2'>
          <input
              type="text"
              className="form-control"
              placeholder="Default billing rate"
              onChange={(e) => console.log(e)}
              value=""/>
        </div>
        <div className='col-sm-2'>
          <input
              type="text"
              className="form-control"
              placeholder="Color code"
              onChange={(e) => console.log(e)}
              value=""/>
        </div>
    </div>
    <div className='row'>
      <div className='col-sm-12 row-padded-top'>
        <textarea className='form-control' placeholder="Additional comments" value=""></textarea>
      </div>
    </div>
    <div className='row row-padded-top'>
      <div className="col-sm-2 btn-group client-buttons">
        <div className="btn btn-primary margin-left-button" onClick={console.log('test')}>Save</div>
        <div className="btn btn-outline-danger">Cancel</div>
      </div>
    </div>
  </div>
}

export default ProjectEditContainer;