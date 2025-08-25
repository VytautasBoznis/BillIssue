import { useState } from "react";
import { useForm } from "react-hook-form";
import AuthService from "../../services/AuthService";
import SpinnerIcon from "../../components/icons/SpinnerIcon";

import "./LoginPage.css";

const LoginPage = () => {
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();

  const onSubmit = async (data) => {
    setLoading(true);
    try {
      await AuthService.login(data.email, data.password, () => {} );
      setLoading(false);
      window.location.href = '/';
    } catch (error) {
      setLoading(false);
      setMessage(error.message);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div className="container">
        <div className="login-container">
          <div className="text-center mb-4">
            <h4>Login</h4>
          </div>
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
            <div className="d-flex justify-content-between">
              <div className="input-label">Password</div>
              <div className="link-label"><a tabIndex={4} href="/forgot-password">Forgot password?</a></div>
            </div>
            <input
              type="password"
              placeholder="Password"
              className="form-control col-sm-12"
              tabIndex={2}
              {...register("password", { required: true })}
            />
            <div className="col-sm-12">
              {errors.password && <span className="error-label">Password is required</span>}
            </div>
          </div>
          <div className="mb-3">
            <button type="submit" tabIndex={3} className="btn btn-primary login-button" disabled={loading}>
              { loading ? (<SpinnerIcon classes="loader-spin"/>) : ("Login") }
            </button>
          </div>
          <div className="mb-3">
            <div className="text-center">
              <div className="subheader-label">Don't have an account? <a tabIndex={5} href="/register">Register</a></div>
            </div>
          </div>
          <div className="text-center error-text">
            {message && <p>{message}</p>}
          </div>
        </div>
      </div>
    </form>
  );
}

export default LoginPage;
