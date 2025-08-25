import { useParams } from "react-router-dom";

const WorkspaceProjectContainer = () => {
  const { id } = useParams();

  return (
    <div> Details page id: { id } </div>
  )
}

export default WorkspaceProjectContainer;