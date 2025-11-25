import './App.css';
import React from 'react';
import { Routes } from './routes';
import 'bootstrap/dist/css/bootstrap.min.css';
import { ErrorProvider } from './utils/errorHandling/ErrorProvider';
import { SuccessProvider } from './utils/successHandling/SuccessProvider';

class App extends React.Component {
  render() {
    return (
      <ErrorProvider>
        <SuccessProvider>
          <Routes isAuthorized={true} />
        </SuccessProvider>
      </ErrorProvider>
    );
  }
}

export default App;
