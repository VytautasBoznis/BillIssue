import './App.css';
import React from 'react';
import { Routes } from './routes';
import 'bootstrap/dist/css/bootstrap.min.css';
import { ErrorProvider } from './utils/errorHandling/ErrorProvider';

class App extends React.Component {
  render() {
    return (
      <ErrorProvider>
        <Routes isAuthorized={true} />
      </ErrorProvider>
    );
  }
}

export default App;
