import { useForm } from "react-hook-form";
import { Modal } from 'react-bootstrap';

const WorkspaceUserAddModal = (props) => {
  const defaultValues = {
    email: null,
  }

  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors },
    reset,
  } = useForm({
    defaultValues
  });

  const onSubmit = () => {
    props.onHide(getValues("email"));
    reset(defaultValues);
  }

  return (
    <Modal
      {...props}
      size="lg"
      centered
    >
      <Modal.Header closeButton>
        <Modal.Title id="contained-modal-title-vcenter">
          Add user to Workspace
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>        
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="row controls-row">
              <div className="col-sm-12">
                <div className="d-flex justify-content-between">
                    <div className="input-label">User email</div>
                    <div className="required-label">Required</div>
                </div>
                <input className="form-control" {...register("email", { required: true })} placeholder="User email..."></input>
                {errors.name && <span className="error-label">This field is required</span>}
              </div>
            </div>
          </form>
      </Modal.Body>
      <Modal.Footer>        
        <button onClick={() => onSubmit()}type="submit" className="btn btn-success">Save</button>
        <button onClick={() => props.onHide()} type="button" className="btn btn-danger">Close</button>
      </Modal.Footer>
    </Modal>
  );
}

export default WorkspaceUserAddModal;
