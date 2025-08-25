import { createContext, useContext, useState } from "react";

import "./ErrorProvider.css";

const ErrorContext = createContext();

export const ErrorProvider = ({ children }) => {
  const [error, setError] = useState(null);

  const showError = (message) => {
    setError(message);
    setTimeout(() => setError(null), 3000);
  };

  return (
    <ErrorContext.Provider value={{ error, showError }}>
      {children}
      {error && <div className="error-popup">{error}</div>} 
    </ErrorContext.Provider>
  );
};

export const useError = () => useContext(ErrorContext);