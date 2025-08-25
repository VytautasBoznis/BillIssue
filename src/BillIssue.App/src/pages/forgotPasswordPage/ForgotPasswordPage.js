import { useState } from "react";
import { useForm } from "react-hook-form";
import AuthService from "../../services/AuthService";
import SpinnerIcon from "../../components/icons/SpinnerIcon";

import "./ForgotPasswordPage.css";
import CheckmarkIcon from "../../components/icons/CheckmarkIcon";

const ForgotPasswordPage = () => {
  const [message, setMessage] = useState("");
  const [reminderSent, setReminderSent] = useState(false);
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();

  const onSubmit = async (data) => {
    setLoading(true);
    try {
      const success = await AuthService.remindPassword(data.email, ()=>{});
      setLoading(false);
      setReminderSent(success);
    } catch (error) {
      setMessage(error.message);
      setLoading(false);
      setTimeout(() => setMessage(null), 3000);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div className="container">
        <div className="remind-password-container">
          <div className="text-center mb-4">
            <h4>Remind password</h4>
            {!reminderSent && (<div className="remind-password-header-text">Please enter an email address for the account you wish to get the password reminder sent to</div>)}
          </div>
          {!reminderSent && (
          <div>
            <div className="mb-3">
              <div className="d-flex justify-content-between">
                <div className="input-label">Email</div>
              </div>
              <input
                className="form-control col-sm-12"
                placeholder="Email"
                tabIndex={1}
                {...register("email", { required: true })}/>
                <div className="col-sm-12">
                  {errors.email && <span className="error-label">Email is required</span>}
                </div>
            </div>
            <div className="mb-3">
              <button type="submit" tabIndex={2} className="btn btn-primary remind-password-button">
                { loading ? (<SpinnerIcon classes="loader-spin"/>) : ("Remind password") }
              </button>
            </div>
            <div className="text-center error-text">
              {message && <p>{message}</p>}
            </div>
          </div>)}
          {reminderSent && (
            <div>
              <div className="mb-3"><CheckmarkIcon/></div>
              <div className="remind-password-header-text mb-3">An email has been sent to your specified email, open the link in the email and continue with the instructions to change your password</div>
              <div className="link-label"><a href="/login">Return to login</a></div>
            </div>
          )}
        </div>
      </div>
    </form>
  );
}

export default ForgotPasswordPage;