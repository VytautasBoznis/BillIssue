const CaretLeft = ({ size = 24, color = "#fff" }) => {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <polygon points="15,6 9,12 15,18" fill={color} />
    </svg>
  );
};

const CaretRight = ({ size = 24, color = "#fff" }) => {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <polygon points="9,6 15,12 9,18" fill={color} />
    </svg>
  );
};

const CaretUp = ({ size = 24, color = "#000" }) => {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <polygon points="6,15 12,9 18,15" fill={color} />
    </svg>
  );
};

const CaretDown = ({ size = 24, color = "#000" }) => {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <polygon points="6,9 12,15 18,9" fill={color} />
    </svg>
  );
};

export { CaretLeft, CaretRight, CaretUp, CaretDown };
