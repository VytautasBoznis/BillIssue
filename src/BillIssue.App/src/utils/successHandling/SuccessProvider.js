import { createContext, useContext, useState } from "react";

import "./SuccessProvider.css";

const SuccessContext = createContext();

export const SuccessProvider = ({ children }) => {
  const [successMessage, setSucessMessage] = useState(null);

  const showSuccess = (message) => {
    setSucessMessage(message);
    setTimeout(() => setSucessMessage(null), 3000);
  };

  return (
    <SuccessContext.Provider value={{ successMessage, showSuccess }}>
      {children}
      {successMessage && <div className="success-popup">{successMessage}</div>} 
    </SuccessContext.Provider>
  );
};

export const useSuccess = () => useContext(SuccessContext);