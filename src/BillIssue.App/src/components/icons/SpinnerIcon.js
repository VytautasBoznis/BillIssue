import React from 'react';

const SpinnerIcon = ({classes = "loader-spin"}) => {
    return (
      <svg
          className={classes}
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 50 50"
          fill="none"
          stroke="currentColor"
          strokeWidth="4"
        >
          <circle cx="25" cy="25" r="20" strokeDasharray="31.4 31.4" strokeLinecap="round" />
      </svg>
    );
}

export default SpinnerIcon;