const PlusIcon = ({ size = 24, color = "#fff" }) => {
  return (
    <svg
      width={size}
      height={size}
      viewBox="-2 -3 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
        <path d="M10 1H6V6L1 6V10H6V15H10V10H15V6L10 6V1Z" fill={color}/>
    </svg>
  );
};


export default PlusIcon;